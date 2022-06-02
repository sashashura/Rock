using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Model;
using Rock.Personalization.SegmentFilters;

namespace Rock.Personalization
{
    public class SegmentAdditionalFilter
    {
        public FilterExpressionType SessionFilterExpressionType { get; set; } = FilterExpressionType.GroupAll;
        public List<SessionSegmentFilter> SessionSegmentFilters { get; set; } = new List<SessionSegmentFilter>();

        public FilterExpressionType PageViewFilterExpressionType { get; set; } = FilterExpressionType.GroupAll;
        public List<PageViewSegmentFilter> PageViewSegmentFilters { get; set; } = new List<PageViewSegmentFilter>();

        public FilterExpressionType InteractionFilterExpressionType { get; set; } = FilterExpressionType.GroupAll;

        public List<InteractionSegmentFilter> InteractionSegmentFilters { get; set; } = new List<InteractionSegmentFilter>();
    }
}
