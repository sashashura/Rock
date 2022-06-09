using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Rock.Model;

namespace Rock.Personalization
{
    public class BrowserRequestFilter : RequestFilter
    {
        private static UAParser.Parser uaParser = UAParser.Parser.GetDefault();

        #region Configuration

        public string BrowserFamily { get; set; }
        public ComparisonType VersionComparisonType { get; set; }
        public int MajorVersion;

        #endregion Configuration

        /// <summary>
        /// Determines whether the specified HTTP request meets the criteria of this filter.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <returns><c>true</c> if the specified HTTP request is match; otherwise, <c>false</c>.</returns>
        public override bool IsMatch( HttpRequest httpRequest )
        {
            var ua = uaParser.ParseUserAgent( httpRequest.UserAgent );
            var family = ua.Family;

            if ( !family.Equals( BrowserFamily, StringComparison.OrdinalIgnoreCase ) )
            {
                return false;
            }

            var majorVersion = ua.Major;
            if ( majorVersion.IsNullOrWhiteSpace() )
            {
                return false;
            }

            return majorVersion.CompareTo( MajorVersion.ToString(), VersionComparisonType );
        }
    }
}
