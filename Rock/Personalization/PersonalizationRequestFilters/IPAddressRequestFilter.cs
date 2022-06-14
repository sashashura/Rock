using System;
using System.Web;

using Rock.Utility;

namespace Rock.Personalization
{
    /// <summary>
    /// Class IPAddressRequestFilter.
    /// Implements the <see cref="Rock.Personalization.PersonalizationRequestFilter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.PersonalizationRequestFilter" />
    public class IPAddressRequestFilter : PersonalizationRequestFilter
    {
        #region Configuration

        /// <summary>
        /// Gets or sets the type of the match.
        /// </summary>
        /// <value>The type of the match.</value>
        public RangeType MatchType { get; set; }

        /// <summary>
        /// Gets or sets the beginning ip address.
        /// </summary>
        /// <value>The beginning ip address.</value>
        public string BeginningIPAddress { get; set; }
        /// <summary>
        /// Gets or sets the ending ip address.
        /// </summary>
        /// <value>The ending ip address.</value>
        public string EndingIPAddress { get; set; }

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

        /// <summary>
        /// Enum RangeType
        /// </summary>
        public enum RangeType
        {
            /// <summary>
            /// The in range
            /// </summary>
            InRange,
            /// <summary>
            /// The not in range
            /// </summary>
            NotInRange
        }
    }
}
