using System;
using System.Linq;
using System.Web;

using Rock.Model;

namespace Rock.Personalization
{
    /// <summary>
    /// Class CookieRequestFilter.
    /// Implements the <see cref="Rock.Personalization.PersonalizationRequestFilter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.PersonalizationRequestFilter" />
    public class CookieRequestFilter : PersonalizationRequestFilter
    {
        #region Configuration

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>The key.</value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the type of the comparison.
        /// </summary>
        /// <value>The type of the comparison.</value>
        public ComparisonType ComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the comparison value.
        /// </summary>
        /// <value>The comparison value.</value>
        public string ComparisonValue { get; set; }

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
            // Note that httpRequest.Cookies.Get will create the cookie if it doesn't exist, so make sure to check if it exists first.
            var cookieExists = httpRequest.Cookies.AllKeys.Contains( this.Key );
            string cookieValue;
            if ( cookieExists )
            {
                cookieValue = httpRequest.Cookies.Get( this.Key )?.Value ?? string.Empty;
            }
            else
            {
                cookieValue = string.Empty;
            } 

            var comparisonValue = ComparisonValue ?? string.Empty;

            return cookieValue.CompareTo( comparisonValue, ComparisonType );
        }
    }
}
