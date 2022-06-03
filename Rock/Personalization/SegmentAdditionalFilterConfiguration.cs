using System.Collections.Generic;

using Rock.Model;
using Rock.Personalization.SegmentFilters;

namespace Rock.Personalization
{
    /// <summary>
    /// Configuration class for Additional Filters for Personalization Segments.
    /// </summary>
    public class SegmentAdditionalFilterConfiguration
    {
        /// <summary>
        /// Gets or sets <see cref="FilterExpressionType"/>
        /// </summary>
        /// <value>The type of the session filter expression.</value>
        public FilterExpressionType SessionFilterExpressionType { get; set; } = FilterExpressionType.GroupAll;

        /// <summary>
        /// Gets or sets the session segment filters.
        /// </summary>
        /// <value>The session segment filters.</value>
        public List<SessionCountSegmentFilter> SessionSegmentFilters { get; set; } = new List<SessionCountSegmentFilter>();

        /// <summary>
        /// Gets or sets the type of the page view filter expression.
        /// </summary>
        /// <value>The type of the page view filter expression.</value>
        public FilterExpressionType PageViewFilterExpressionType { get; set; } = FilterExpressionType.GroupAll;

        /// <summary>
        /// Gets or sets the page view segment filters.
        /// </summary>
        /// <value>The page view segment filters.</value>
        public List<PageViewSegmentFilter> PageViewSegmentFilters { get; set; } = new List<PageViewSegmentFilter>();

        /// <summary>
        /// Gets or sets the type of the interaction filter expression.
        /// </summary>
        /// <value>The type of the interaction filter expression.</value>
        public FilterExpressionType InteractionFilterExpressionType { get; set; } = FilterExpressionType.GroupAll;

        /// <summary>
        /// Gets or sets the interaction segment filters.
        /// </summary>
        /// <value>The interaction segment filters.</value>
        public List<InteractionSegmentFilter> InteractionSegmentFilters { get; set; } = new List<InteractionSegmentFilter>();
    }
}
