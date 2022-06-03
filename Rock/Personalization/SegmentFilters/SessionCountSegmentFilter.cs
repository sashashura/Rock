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
    /// Class SessionSegmentFilter.
    /// Implements the <see cref="Rock.Personalization.SegmentFilter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.SegmentFilter" />
    public class SessionCountSegmentFilter : Rock.Personalization.SegmentFilter
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

        private SiteCache[] GetSelectedSites() => SiteGuids?.Select( a => SiteCache.Get( a ) ).Where( a => a != null ).ToArray() ?? new SiteCache[0];

        /// <summary>
        /// Gets or sets the sliding date range <see cref="Rock.Web.UI.Controls.SlidingDateRangePicker.DelimitedValues"/>
        /// </summary>
        /// <value>The sliding date range delimited values.</value>
        public string SlidingDateRangeDelimitedValues { get; set; }

        /// <summary>
        /// Gets the description based on how the filter is configured.
        /// </summary>
        /// <returns>System.String.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override string GetDescription()
        {
            ComparisonType comparisonType = this.ComparisonType;
            var comparisonValue = this.ComparisonValue;
            string comparisonPhrase;

            if ( comparisonType == ComparisonType.IsBlank )
            {
                comparisonPhrase = "Has had no sessions";
            }
            else if ( comparisonType == ComparisonType.IsNotBlank )
            {
                comparisonPhrase = "Has had no sessions";
            }
            else if ( comparisonType == ComparisonType.Between )
            {
                comparisonPhrase = $"Has had between {ComparisonValue} and {ComparisonValueBetweenUpper} sessions";
            }
            else
            {
                comparisonPhrase = $"Has had {comparisonType.GetFriendlyDescription()} {ComparisonValue} sessions";
            }

            var siteNames = GetSelectedSites().Select( a => a.Name ).ToList();
            string onTheSites = siteNames.AsDelimited( ", ", " or " ) + " website";

            string inTheDateRange = SlidingDateRangePicker.FormatDelimitedValues( SlidingDateRangeDelimitedValues ).ToLower();

            var description = $"{comparisonPhrase} {onTheSites} {inTheDateRange}.";
            return description;
        }

        /// <summary>
        /// Gets Expression that will be used as one of the WHERE clauses for the PersonAlias query.
        /// </summary>
        /// <param name="personAliasService">The person alias service.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>Expression.</returns>
        public override Expression GetWherePersonAliasExpression( PersonAliasService personAliasService, ParameterExpression parameterExpression )
        {
            var siteIds = GetSelectedSites().Select( a => a.Id ).ToArray();

            if ( !siteIds.Any() )
            {
                return null;
            }

            var rockContext = personAliasService.Context as RockContext;
            var pageViewsInteractionsQuery = new InteractionService( rockContext ).GetPageViewsBySite( siteIds ).Where( a => a.PersonAliasId.HasValue );

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( SlidingDateRangeDelimitedValues );
            if ( dateRange?.Start != null )
            {
                pageViewsInteractionsQuery = pageViewsInteractionsQuery.Where( a => a.InteractionDateTime >= dateRange.Start );
            }

            if ( dateRange?.End != null )
            {
                pageViewsInteractionsQuery = pageViewsInteractionsQuery.Where( a => a.InteractionDateTime < dateRange.End );
            }

            var personAliasQuery = personAliasService.Queryable();

            var comparisonType = this.ComparisonType;
            var comparisonValue = this.ComparisonValue;
            if ( comparisonType == ComparisonType.Between && !this.ComparisonValueBetweenUpper.HasValue )
            {
                comparisonType = ComparisonType.GreaterThanOrEqualTo;
            }

            // Filter by the SessionCount of the Page Views
            if ( comparisonType == ComparisonType.Between )
            {
                var comparisonValueBetweenUpper = this.ComparisonValueBetweenUpper.Value;
                var personAliasBetweenQuery = personAliasQuery.Where( p =>
                    pageViewsInteractionsQuery.Where( i => i.PersonAliasId == p.Id )
                        .GroupBy( a => a.InteractionSessionId.Value ).Count() >= comparisonValue
                        &&
                    pageViewsInteractionsQuery.Where( i => i.PersonAliasId == p.Id )
                        .GroupBy( a => a.InteractionSessionId.Value ).Count() <= comparisonValueBetweenUpper
                        );

                return FilterExpressionExtractor.Extract<Rock.Model.PersonAlias>( personAliasBetweenQuery, parameterExpression, "p" );
            }
            else
            {
                var personAliasCompareEqualQuery = personAliasQuery.Where( p =>
                    pageViewsInteractionsQuery.Where( i => i.PersonAliasId == p.Id ).GroupBy( a => a.InteractionSessionId ).Count() == comparisonValue );

                BinaryExpression compareEqualExpression = FilterExpressionExtractor.Extract<Rock.Model.PersonAlias>( personAliasCompareEqualQuery, parameterExpression, "p" ) as BinaryExpression;
                BinaryExpression result = FilterExpressionExtractor.AlterComparisonType( comparisonType, compareEqualExpression, null );
                return result;
            }

        }
    }
}
