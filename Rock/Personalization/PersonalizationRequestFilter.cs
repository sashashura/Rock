using System.Web;

namespace Rock.Personalization
{
    /// <summary>
    /// Filter that determines if a Browser request meets criteria.
    /// </summary>
    public abstract class PersonalizationRequestFilter
    {
        /// <summary>
        /// Determines whether the specified HTTP request meets the criteria of this filter.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <returns><c>true</c> if the specified HTTP request is match; otherwise, <c>false</c>.</returns>
        public abstract bool IsMatch( HttpRequest httpRequest );
    }
}
