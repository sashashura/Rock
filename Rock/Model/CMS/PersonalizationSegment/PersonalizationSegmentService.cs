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
using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Class PersonalizationSegmentService.
    /// </summary>
    public partial class PersonalizationSegmentService
    {
        /// <summary>
        /// Gets a Queryable of PersonAlias that meet the criteria of the Segment
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <returns></returns>
        public IQueryable<PersonAlias> GetPersonAliasQueryForSegment( PersonalizationSegmentCache segment )
        {
            var rockContext = this.Context as RockContext;
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

            return personAliasQuery;
        }

        /// <summary>
        /// Gets a Queryable of <see cref="PersonAliasPersonalization"/> that have a <see cref="PersonAliasPersonalization.PersonalizationType"/> of <see cref="PersonalizationType.Segment"/>
        /// </summary>
        public IQueryable<Rock.Model.PersonAliasPersonalization> GetPersonAliasPersonalizationSegmentQuery()
        {
            return ( this.Context as RockContext ).PersonAliasPersonalizations.Where( a => a.PersonalizationType == PersonalizationType.Segment );
        }

        /// <summary>
        /// Gets the person alias personalization query for the specified segment.
        /// </summary>
        /// <param name="personalizationSegment">The personalization segment.</param>
        public IQueryable<Rock.Model.PersonAliasPersonalization> GetPersonAliasPersonalizationSegmentQuery( PersonalizationSegmentCache personalizationSegment )
        {
            return GetPersonAliasPersonalizationSegmentQuery().Where( a => a.PersonalizationTypeId == personalizationSegment.Id );
        }

        /// <inheritdoc cref="UpdatePersonAliasPersonalizationDataForSegment(PersonalizationSegmentCache)"/>
        public void UpdatePersonAliasPersonalizationData( PersonalizationSegmentCache segment )
        {
            this.UpdatePersonAliasPersonalizationDataForSegment( segment );
        }

        /// <summary>
        /// Updates the data in <see cref="Rock.Model.PersonAliasPersonalization"/> table based on the specified segment's criteria.
        /// </summary>
        /// <param name="segment">The segment.</param>
        internal SegmentUpdateResults UpdatePersonAliasPersonalizationDataForSegment( PersonalizationSegmentCache segment )
        {
            var rockContext = this.Context as RockContext;
            rockContext.SqlLogging( true );
            var personAliasService = new PersonAliasService( rockContext );
            var parameterExpression = personAliasService.ParameterExpression;

            var whereExpression = segment.GetPersonAliasFiltersWhereExpression( personAliasService, parameterExpression );

            var personAliasQueryForSegment = personAliasService.Get( parameterExpression, whereExpression );

            var dataViewFilterId = segment.FilterDataViewId;
            if ( dataViewFilterId.HasValue )
            {
                var args = new DataViewGetQueryArgs { DbContext = rockContext };
                var dataView = new DataViewService( rockContext ).Get( dataViewFilterId.Value );

                var personDataViewQuery = new PersonService( rockContext ).GetQueryUsingDataView( dataView );
                personAliasQueryForSegment = personAliasQueryForSegment.Where( pa => personDataViewQuery.Any( person => person.Aliases.Any( alias => alias.Id == pa.Id ) ) );
            }

            var personAliasIdsInSegmentQry = personAliasQueryForSegment.Select( a => a.Id );
            var personAliasPersonalizationQry = this.GetPersonAliasPersonalizationSegmentQuery( segment );

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

            rockContext.SqlLogging( false );

            return new SegmentUpdateResults( countAddedToSegment, countRemovedFromSegment );
        }

        internal struct SegmentUpdateResults
        {
            public int CountAddedSegment;
            public int CountRemovedFromSegment;

            public SegmentUpdateResults( int countAddedSegment, int countRemovedFromSegment )
            {
                CountAddedSegment = countAddedSegment;
                CountRemovedFromSegment = countRemovedFromSegment;
            }
        }

        /// <summary>
        /// Gets the personalization segment identifier keys for person alias identifier.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns>System.String[].</returns>
        public string[] GetPersonalizationSegmentIdKeysForPersonAliasId( int personAliasId )
        {
            var qry = ( this.Context as RockContext ).PersonAliasPersonalizations
                .Where( a => a.PersonalizationType == PersonalizationType.Segment && a.PersonAliasId == personAliasId );

            var segmentIds = qry.Select( a => a.PersonalizationTypeId ).ToArray();
            var segmentIdKeys = segmentIds.Select( a => IdHasher.Instance.GetHash( a ) ).ToArray();

            return segmentIdKeys;
        }

        /// <summary>
        /// Bulk updates the <see cref="PersonAliasPersonalization.PersonAliasId" /> for this person to the person's PrimaryAliasId
        /// if there is any Personalization data that uses any of their non-primary alias ids.
        /// Returns the number of PersonAliasPersonalization records updated.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        public int MergePersonAliasPersonalizationToPrimaryAliasId( int personId )
        {
            var rockContext = this.Context as RockContext;
            var anonymousVisitorPersonId = new PersonService( rockContext ).GetOrCreateAnonymousVisitorPersonId();
            if ( personId == anonymousVisitorPersonId )
            {
                // don't merge PersonAliasPersonalization into the AnonymousVisitor record
                return 0;
            }

            var primaryAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( personId );
            if ( !primaryAliasId.HasValue )
            {
                // shouldn't happen, but just in case
                return 0;
            }

            var qryPersonAliasPersonalization = rockContext.PersonAliasPersonalizations as IQueryable<PersonAliasPersonalization>;
            var qryNonPrimaryAliasData = qryPersonAliasPersonalization.Where( a => a.PersonAlias.PersonId == personId && a.PersonAliasId != primaryAliasId );
            var recordsUpdated = rockContext.BulkUpdate( qryNonPrimaryAliasData, p => new PersonAliasPersonalization { PersonAliasId = primaryAliasId.Value } );
            return recordsUpdated;
        }

        /// <summary>
        /// Bulk updates the <see cref="PersonAliasPersonalization.PersonAliasId" /> to the primary alias id of each person.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int MergePersonAliasPersonalizationToPrimaryAliasId()
        {
            var rockContext = this.Context as RockContext;
            var anonymousVisitorPersonId = new PersonService( rockContext ).GetOrCreateAnonymousVisitorPersonId();

            var qryPersonAliasPersonalization = rockContext.PersonAliasPersonalizations as IQueryable<PersonAliasPersonalization>;
            var primaryAliasIdQry = new PersonAliasService( rockContext ).GetPrimaryAliasQuery().Select( a => a.Id );
            var qryNonPrimaryAliasData = qryPersonAliasPersonalization.Where( a => !primaryAliasIdQry.Contains( a.PersonAliasId ) && a.PersonAlias.PersonId != anonymousVisitorPersonId );
            var personIdsWithNonPrimaryAliasData = qryNonPrimaryAliasData.Select( a => a.PersonAlias.PersonId ).Distinct();
            int totalRecordsUpdated = 0;
            foreach ( var personId in personIdsWithNonPrimaryAliasData )
            {
                var recordsUpdated = MergePersonAliasPersonalizationToPrimaryAliasId( personId );
                totalRecordsUpdated += recordsUpdated;
            }

            return totalRecordsUpdated;

        }
    }
}