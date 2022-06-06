using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Personalization.SegmentFilters
{
    /// <summary>
    /// Class PageViewSegmentFilter.
    /// Implements the <see cref="Rock.Personalization.SegmentFilter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.SegmentFilter" />
    public class PageViewSegmentFilter : Rock.Personalization.SegmentFilter
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

        private SiteCache[] GetSelectedSites() => SiteGuids?.Select( a => SiteCache.Get( a ) ).Where( a => a != null ).ToArray() ?? new SiteCache[0];
        private PageCache[] GetSelectedPages() => PageGuids?.Select( a => PageCache.Get( a ) ).Where( a => a != null ).ToArray() ?? new PageCache[0];

        /// <summary>
        /// Gets the description based on how the filter is configured.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string GetDescription()
        {
            ComparisonType comparisonType = this.ComparisonType;
            var comparisonValue = this.ComparisonValue;
            string comparisonPhrase;

            if ( comparisonType == ComparisonType.IsBlank )
            {
                comparisonPhrase = "Has had no page views";
            }
            else if ( comparisonType == ComparisonType.IsNotBlank )
            {
                comparisonPhrase = "Has had page views";
            }
            else
            {
                comparisonPhrase = $"Has had {comparisonType.GetFriendlyDescription()} {ComparisonValue} page views";
            }

            var siteNames = GetSelectedSites().Select( a => a.Name ).ToList();
            string onTheSites = siteNames.AsDelimited( ", ", " or " ) + " website";

            string inTheDateRange = SlidingDateRangePicker.FormatDelimitedValues( SlidingDateRangeDelimitedValues );
            var pageNames = GetSelectedPages().Select( a => a.ToString() ).ToList();
            string limitedToPages = null;
            if ( pageNames.Any() )
            {
                limitedToPages = $"limited to the {pageNames.AsDelimited( ",", " and " )} pages.";
            }

            var description = $"{comparisonPhrase} {onTheSites} {inTheDateRange} {limitedToPages}";
            return description.Trim() + ".";
        }

        /// <summary>
        /// Gets the where person alias expression.
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

            var selectedPageIds = GetSelectedPages().Select( a => a.Id ).ToArray();

            IQueryable<Interaction> pageViewsInteractionsQuery;

            var rockContext = personAliasService.Context as RockContext;

            if ( selectedPageIds.Any() )
            {
                pageViewsInteractionsQuery = new InteractionService( rockContext ).GetPageViewsByPage( siteIds, selectedPageIds ).Where( a => a.PersonAliasId.HasValue );
            }
            else
            {
                pageViewsInteractionsQuery = new InteractionService( rockContext ).GetPageViewsBySite( siteIds ).Where( a => a.PersonAliasId.HasValue );
            }

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
            var personAliasCompareEqualQuery = personAliasQuery.Where( p =>
                pageViewsInteractionsQuery.Where( i => i.PersonAliasId == p.Id ).Count() == comparisonValue );

            BinaryExpression compareEqualExpression = FilterExpressionExtractor.Extract<Rock.Model.PersonAlias>( personAliasCompareEqualQuery, parameterExpression, "p" ) as BinaryExpression;
            BinaryExpression result = FilterExpressionExtractor.AlterComparisonType( comparisonType, compareEqualExpression, null );
            return result;
        }
    }
}