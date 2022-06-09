using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI;

namespace Rock.Personalization
{
    public class QueryStringRequestFilter : RequestFilter
    {
        #region Configuration

        public string Key { get; set; }
        public ComparisonType ComparisonType { get; set; }
        public string ComparisonValue { get; set; }

        #endregion Configuration

        /// <summary>
        /// Determines whether the specified HTTP request meets the criteria of this filter.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <returns><c>true</c> if the specified HTTP request is match; otherwise, <c>false</c>.</returns>
        public override bool IsMatch( HttpRequest httpRequest )
        {
            var queryString = httpRequest?.QueryString;
            if ( queryString == null )
            {
                return false;
            }

            var queryStringValue = queryString[Key] ?? string.Empty;
            var comparisonValue = ComparisonValue ?? string.Empty;

            return queryStringValue.CompareTo( comparisonValue, ComparisonType );
        }
    }
}
