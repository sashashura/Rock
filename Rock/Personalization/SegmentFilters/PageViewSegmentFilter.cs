using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Personalization.SegmentFilters
{
    /// <summary>
    /// Class PageViewSegmentFilter.
    /// Implements the <see cref="Rock.Personalization.Filter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.Filter" />
    public class PageViewSegmentFilter : Rock.Personalization.Filter
    {
        /// <summary>
        /// Gets or sets the type of the comparison.
        /// </summary>
        /// <value>The type of the comparison.</value>
        public ComparisonType ComparisonType { get; set; } = ComparisonType.GreaterThanOrEqualTo;

        /// <summary>
        /// Gets or sets the comparison value.
        /// The Number of Sessions. 
        /// </summary>
        /// <value>The comparison value.</value>
        public int ComparisonValue { get; set; } = 4;

        /// <summary>
        /// If this is a ComparisonType of Between, this is the upper value.
        /// </summary>
        /// <value>The comparison value between upper.</value>
        public int? ComparisonValueBetweenUpper { get; set; }

        /// <summary>
        /// List of <see cref="Rock.Model.Site">sites</see> that apply to this filter (Required)
        /// </summary>
        /// <value>The site guids.</value>
        public List<Guid> SiteGuids { get; set; } = new List<Guid>();

        /// <summary>
        /// List of <see cref="Rock.Model.Page">pages</see> that apply to this filter (Optional)
        /// </summary>
        /// <value>The site guids.</value>
        public List<Guid> PageGuids { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets the sliding date range <see cref="Rock.Web.UI.Controls.SlidingDateRangePicker.DelimitedValues"/>
        /// </summary>
        /// <value>The sliding date range delimited values.</value>
        public string SlidingDateRangeDelimitedValues { get; set; }

        /// <summary>
        /// Gets the where person alias expression.
        /// </summary>
        /// <param name="personAliasService">The person alias service.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>Expression.</returns>
        public override Expression GetWherePersonAliasExpression( PersonAliasService personAliasService, ParameterExpression parameterExpression )
        {
            if ( !SiteGuids.Any() )
            {
                return null;
            }

            var siteIds = SiteGuids.Select( a => SiteCache.GetId( a ) ).Where( a => a.HasValue ).Select( a => a.Value ).ToArray();

            var rockContext = personAliasService.Context as RockContext;
            var interactionsQuery = new InteractionService( rockContext ).Queryable().Where( a => a.PersonAliasId.HasValue );

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( SlidingDateRangeDelimitedValues );
            if ( dateRange?.Start != null )
            {
                interactionsQuery = interactionsQuery.Where( a => a.InteractionDateTime >= dateRange.Start );
            }

            if ( dateRange?.End != null )
            {
                interactionsQuery = interactionsQuery.Where( a => a.InteractionDateTime < dateRange.End );
            }

            IQueryable<InteractionComponent> componentsQuery;

            if ( PageGuids.Any() )
            {
                var pageIds = PageGuids.Select( a => PageCache.GetId( a ) ).Where( a => a != null ).Select( a => a.Value ).ToArray();
                componentsQuery = new InteractionComponentService( rockContext ).QueryByPages( siteIds, pageIds );
            }
            else
            {
                componentsQuery = new InteractionComponentService( rockContext ).QueryBySites( siteIds );
            }

            interactionsQuery = interactionsQuery.Where( a => componentsQuery.Any( c => c.Id == a.InteractionComponentId ) );
            
            var personAliasQuery = personAliasService.Queryable();

            var comparisonType = this.ComparisonType;
            var comparisonValue = this.ComparisonValue;
            if ( comparisonType == ComparisonType.Between && !this.ComparisonValueBetweenUpper.HasValue )
            {
                comparisonType = ComparisonType.GreaterThanOrEqualTo;
            }

            if ( comparisonType == ComparisonType.Between )
            {
                var comparisonValueBetweenUpper = this.ComparisonValueBetweenUpper.Value;
                var personAliasBetweenQuery = personAliasQuery.Where( p =>
                    interactionsQuery.Where( i => i.PersonAliasId == p.Id )
                        .GroupBy( a => a.InteractionSessionId.Value ).Count() >= comparisonValue
                        &&
                    interactionsQuery.Where( i => i.PersonAliasId == p.Id )
                        .GroupBy( a => a.InteractionSessionId.Value ).Count() <= comparisonValueBetweenUpper
                        );

                return FilterExpressionExtractor.Extract<Rock.Model.PersonAlias>( personAliasBetweenQuery, parameterExpression, "p" );
            }
            else
            {
                var personAliasCompareEqualQuery = personAliasQuery.Where( p =>
                    interactionsQuery.Where( i => i.PersonAliasId == p.Id ).GroupBy( a => a.InteractionSessionId ).Count() == comparisonValue );

                BinaryExpression compareEqualExpression = FilterExpressionExtractor.Extract<Rock.Model.PersonAlias>( personAliasCompareEqualQuery, parameterExpression, "p" ) as BinaryExpression;
                BinaryExpression result = FilterExpressionExtractor.AlterComparisonType( comparisonType, compareEqualExpression, null );
                return result;
            }

        }
    }
}
