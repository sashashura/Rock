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
        /// Gets a Queryable of PersonAlias for PersonAlias's that meet the criteria of the Segment
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