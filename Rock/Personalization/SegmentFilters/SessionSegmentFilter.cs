using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Rock.Model;

namespace Rock.Personalization.SegmentFilters
{
    /// <summary>
    /// Class SessionSegmentFilter.
    /// Implements the <see cref="Rock.Personalization.Filter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.Filter" />
    public class SessionSegmentFilter : Rock.Personalization.Filter
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
        /// Gets or sets the sliding date range <see cref="Rock.Web.UI.Controls.SlidingDateRangePicker.DelimitedValues"/>
        /// </summary>
        /// <value>The sliding date range delimited values.</value>
        public string SlidingDateRangeDelimitedValues { get; set; }

        public override Expression GetWherePersonAliasExpression( PersonAliasService personAliasService, ParameterExpression parameterExpression )
        {
            throw new NotImplementedException();
        }
    }
}
