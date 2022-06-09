using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Rock.Utility;
using Rock.Web.UI;

namespace Rock.Personalization
{
    public class IPAddressRequestFilter : RequestFilter
    {
        #region Configuration

        public RangeType MatchType { get; set; }

        public string BeginningIPAddress { get; set; }
        public string EndingIPAddress { get; set; }

        #endregion Configuration

        /// <summary>
        /// Determines whether the specified HTTP request meets the criteria of this filter.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <returns><c>true</c> if the specified HTTP request is match; otherwise, <c>false</c>.</returns>
        public override bool IsMatch( HttpRequest httpRequest )
        {
            var isInRange = WebRequestHelper.IsIPAddressInRange( new HttpRequestWrapper( httpRequest ), BeginningIPAddress, EndingIPAddress );
            if ( MatchType == RangeType.InRange )
            {
                return isInRange;
            }
            else
            {
                return !isInRange;
            }
        }

        public enum RangeType
        {
            InRange,
            NotInRange
        }
    }
}
