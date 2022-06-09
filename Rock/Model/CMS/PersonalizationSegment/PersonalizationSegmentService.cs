using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Class PersonalizationSegmentService.
    /// </summary>
    public partial class PersonalizationSegmentService
    {
        /// <summary>
        /// Gets the personalization segment ids for the specified PersonAliasId
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns>System.Int32[].</returns>
        public int[] GetPersonalizationSegmentIdsForPersonAliasId( int personAliasId )
        {
            var segmentList = PersonalizationSegmentCache.All().Where( a => a.IsActive );
            var rockContext = this.Context as RockContext;
            var personAliasService = new PersonAliasService( rockContext );
            var parameterExpression = personAliasService.ParameterExpression;
            var segmentIds = new List<int>();
            foreach ( var segment in segmentList )
            {
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

                var isInSegment = personAliasQuery.Any( a => a.Id == personAliasId );

                if ( isInSegment )
                {
                    segmentIds.Add( segment.Id );
                }
            }

            return segmentIds.ToArray();
        }
    }
}