using System.Web;

using Rock.Model;

namespace Rock.Personalization
{
    public class CookieRequestFilter : RequestFilter
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
            var cookieValue = httpRequest.Cookies.Get( this.Key )?.Value ?? string.Empty;
            var comparisonValue = ComparisonValue ?? string.Empty;

            return cookieValue.CompareTo( comparisonValue, ComparisonType );
        }
    }
}
