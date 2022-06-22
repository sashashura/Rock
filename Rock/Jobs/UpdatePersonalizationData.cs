// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisplayName( "Update Personalization Data" )]
    [Description( "Job that updates Personalization Data." )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeoutSeconds,
        Description = "Maximum amount of time (in seconds) to wait for the sql operations to complete. Leave blank to use the default for this job (180).",
        IsRequired = false,
        DefaultIntegerValue = 60 * 3,
        Order = 1 )]

    [DisallowConcurrentExecution]
    public class UpdatePersonalizationData : IJob
    {
        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string CommandTimeoutSeconds = "CommandTimeoutSeconds";
        }

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public UpdatePersonalizationData()
        {
        }

        /// <summary>
        /// Executes the UpdatePersonalizationData Job
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            var includeSegmentsWithNonPersistedDataViews = false;
            var segmentList = PersonalizationSegmentCache.GetActiveSegments( includeSegmentsWithNonPersistedDataViews );
            var resultsBuilder = new StringBuilder();

            foreach ( var segment in segmentList )
            {
                context.UpdateLastStatusMessage( $"Updating {segment.Name}..." );
                SegmentUpdateResults segmentUpdateResults = UpdatePersonAliasPersonalizationDataForSegment( segment );
                if ( segmentUpdateResults.CountAddedSegment == 0 && segmentUpdateResults.CountRemovedFromSegment == 0 )
                {
                    resultsBuilder.AppendLine( $"{segment.Name} - No changes needed." );
                }
                else
                {
                    resultsBuilder.AppendLine( $"{segment.Name} - {segmentUpdateResults.CountAddedSegment} added and {segmentUpdateResults.CountRemovedFromSegment} removed." );
                }
            }

            context.UpdateLastStatusMessage ( resultsBuilder.ToString() );
        }

        private static SegmentUpdateResults UpdatePersonAliasPersonalizationDataForSegment( PersonalizationSegmentCache segment )
        {
            var rockContext = new RockContext();
            var personAliasService = new PersonAliasService( rockContext );
            var parameterExpression = personAliasService.ParameterExpression;

            var whereExpression = segment.GetPersonAliasFiltersWhereExpression( personAliasService, parameterExpression );

            var personAliasQuery = personAliasService.Get( parameterExpression, whereExpression );

            var dataViewFilterId = segment.FilterDataViewId;
            if ( dataViewFilterId.HasValue )
            {
                var args = new DataViewGetQueryArgs { DbContext = rockContext };
                var dataView = new DataViewService( rockContext ).Get( dataViewFilterId.Value );

                var personDataViewQuery = new PersonService( rockContext ).GetQueryUsingDataView( dataView );
                personAliasQuery = personAliasQuery.Where( pa => personDataViewQuery.Any( person => person.Aliases.Any( alias => alias.Id == pa.Id ) ) );
            }

            var personAliasIdsInSegmentQry = personAliasQuery.Select( a => a.Id );
            var personalizationSegmentService = new PersonalizationSegmentService( rockContext );
            var personAliasPersonalizationQry = personalizationSegmentService.GetPersonAliasPersonalizationQuery( PersonalizationType.Segment );

            // Delete PersonAliasIds that are no longer in the segment
            var personAliasToDeleteFromSegment = personAliasPersonalizationQry.Where( a => !personAliasIdsInSegmentQry.Contains( a.PersonAliasId ) );
            var countRemovedFromSegment = rockContext.BulkDelete( personAliasToDeleteFromSegment );

            // Add PersonAliasIds that are new in the segment.
            var personAliasIdsToAddToSegment = personAliasIdsInSegmentQry
                .Where( personAliasId => !personAliasPersonalizationQry.Any( pp => pp.PersonAliasId == personAliasId ) )
                .ToList();

            List<PersonAliasPersonalization> personAliasPersonalizationsToInsert = personAliasIdsToAddToSegment
                .Select( personAliasId => new PersonAliasPersonalization
                {
                    PersonAliasId = personAliasId,
                    PersonalizationType = PersonalizationType.Segment,
                    PersonalizationTypeId = segment.Id
                } ).ToList();

            var countAddedToSegment = personAliasPersonalizationsToInsert.Count();
            if ( countAddedToSegment > 0 )
            {
                rockContext.BulkInsert( personAliasPersonalizationsToInsert );
            }

            return new SegmentUpdateResults( countAddedToSegment, countRemovedFromSegment );
        }

        private struct SegmentUpdateResults
        {
            public int CountAddedSegment;
            public int CountRemovedFromSegment;

            public SegmentUpdateResults( int countAddedSegment, int countRemovedFromSegment )
            {
                CountAddedSegment = countAddedSegment;
                CountRemovedFromSegment = countRemovedFromSegment;
            }
        }
    }
}
