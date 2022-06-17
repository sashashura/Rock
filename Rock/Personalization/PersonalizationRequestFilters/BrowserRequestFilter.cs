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
        public BrowserFamilyEnum BrowserFamily { get; set; }

        /// <summary>
        /// Gets or sets the type of the version comparison.
        /// </summary>
        /// <value>The type of the version comparison.</value>
        public ComparisonType VersionComparisonType { get; set; }

        /// <summary>
        /// The major version
        /// </summary>
        public int MajorVersion { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>The unique identifier.</value>
        public Guid Guid { get; set; }

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

            var filteredBrowserFamily = this.BrowserFamily.ConvertToString(); 

            if ( !detectedFamily.Equals( filteredBrowserFamily, StringComparison.OrdinalIgnoreCase ) )
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
            /// <summary>
            /// Other Browser
            /// </summary>
            [Description( "Other" )]
            Other = 0,

            /// <summary>
            /// Chrome Browser
            /// </summary>
            [Description( "Chrome" )]
            Chrome = 1,

            /// <summary>
            /// Chrome Mobile Browser
            /// </summary>
            [Description( "Chrome Mobile" )]
            ChromeMobile = 2,

            /// <summary>
            /// Firefox Browser
            /// </summary>
            [Description( "Firefox" )]
            Firefox = 3,

            /// <summary>
            /// Firefox Mobile Browser
            /// </summary>
            [Description( "Firefox Mobile" )]
            FirefoxMobile = 4,

            /// <summary>
            /// Safari Browser
            /// </summary>
            [Description( "Safari" )]
            Safari = 5,

            /// <summary>
            /// Opera Browser
            /// </summary>
            [Description( "Opera" )]
            Opera = 6,

            /// <summary>
            /// Opera Mini Browser
            /// </summary>
            [Description( "Opera Mini" )]
            OperaMini = 7,

            /// <summary>
            /// Edge Browser
            /// </summary>
            [Description( "Edge" )]
            Edge = 8,

            /// <summary>
            /// Internet Explorer Browser
            /// </summary>
            [Description( "IE" )]
            IE = 9
        }
    }
}
