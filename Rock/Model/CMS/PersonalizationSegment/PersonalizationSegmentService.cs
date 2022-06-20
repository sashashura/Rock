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
        /// Calculates the Segments that the PersonAliasId in is based on the configuration of active <see cref="Rock.Model.PersonalizationSegment">PersonalizationSegments</see>,
        /// return returns the list of Ids of the PersonalizationSegment that meet the criteria.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns>System.Int32[].</returns>
        public int[] CalculatePersonalizationSegmentIdsForPersonAliasId( int personAliasId )
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

        /// <summary>
        /// Gets the person alias personalization query that have the specified PersonalizationType.
        /// For example, <see cref="Rock.Model.PersonalizationType.Segment" />.
        /// </summary>
        /// <returns>IQueryable&lt;Rock.Model.PersonAliasPersonalization&gt;.</returns>
        public IQueryable<Rock.Model.PersonAliasPersonalization> GetPersonAliasPersonalizationQuery( PersonalizationType personalizationType )
        {
            return ( this.Context as RockContext ).PersonAliasPersonalizations.Where( a => a.PersonalizationType == personalizationType );
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
    }
}