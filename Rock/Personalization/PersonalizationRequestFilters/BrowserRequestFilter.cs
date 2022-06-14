using System;
using System.ComponentModel;
using System.Web;

using Rock.Model;

namespace Rock.Personalization
{
    /// <summary>
    /// Class BrowserRequestFilter.
    /// Implements the <see cref="Rock.Personalization.PersonalizationRequestFilter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.PersonalizationRequestFilter" />
    public class BrowserRequestFilter : PersonalizationRequestFilter
    {
        /// <summary>
        /// The ua parser
        /// </summary>
        private readonly static UAParser.Parser uaParser = UAParser.Parser.GetDefault();

        #region Configuration

        /// <summary>
        /// Gets or sets the browser family. Configured UI should only present browsers from <seealso cref="SupportedBrowserFamilyOptions"/>.
        /// </summary>
        /// <value>The browser family.</value>
        public string BrowserFamily { get; set; }

        /// <summary>
        /// Gets or sets the type of the version comparison.
        /// </summary>
        /// <value>The type of the version comparison.</value>
        public ComparisonType VersionComparisonType { get; set; }

        /// <summary>
        /// The major version
        /// </summary>
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
            var detectedFamily = ua.Family;

            if ( !detectedFamily.Equals( BrowserFamily, StringComparison.OrdinalIgnoreCase ) && BrowserFamily != "Other" )
            {
                // If the detected family doesn't match the BrowserFamily for this filter,
                // and the selected BrowerFamily is not Other, then
                // return false since both the BrowserFamily AND MajorVersion condition must be met.
                return false;
            }

            var majorVersion = ua.Major;
            if ( majorVersion.IsNullOrWhiteSpace() )
            {
                return false;
            }

            return majorVersion.CompareTo( MajorVersion.ToString(), VersionComparisonType );
        }

        /// <summary>
        /// The supported browser family options that the Configuration UI for this filter should show,
        /// </summary>
        public enum BrowserFamilyEnum
        {
            [Description( "Chrome" )]
            Chrome,

            [Description( "Chrome Mobile" )]
            ChromeMobie,
                
            [Description( "Firefox" )]
            Firefox,

            [Description( "Firefox Mobile" )]
            FirefoxMobile,

            [Description( "Safari" )]
            Safari,

            [Description( "Opera" )]
            Opera,

            [Description( "Opera Mini" )]
            OperaMini,

            [Description( "Edge" )]
            Edge,

            [Description( "IE" )]
            IE,

            [Description( "Other" )]
            Other
        }
    }
}
