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

        #endregion Configuration

        /// <summary>
        /// Determines whether the specified HTTP request meets the criteria of this filter.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <returns><c>true</c> if the specified HTTP request is match; otherwise, <c>false</c>.</returns>
        public override bool IsMatch( HttpRequest httpRequest )
        {
            var cookieValue = httpRequest.Cookies.Get( this.Key )?.Value ?? string.Empty;
            var comparisonValue = ComparisonValue ?? string.Empty;

            return cookieValue.CompareTo( comparisonValue, ComparisonType );
        }
    }
}
