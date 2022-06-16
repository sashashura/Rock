using System;
using System.Linq;
using System.Web;

namespace Rock.Personalization
{
    /// <summary>
    /// Class PreviousActivityRequestFilter.
    /// Implements the <see cref="Rock.Personalization.PersonalizationRequestFilter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.PersonalizationRequestFilter" />
    public class PreviousActivityRequestFilter : PersonalizationRequestFilter
    {
        #region Configuration

        /// <summary>
        /// Gets or sets the previous activity types.
        /// </summary>
        /// <value>The previous activity types.</value>
        public PreviousActivityType[] PreviousActivityTypes { get; set; }

        #endregion Configuration

        /// <summary>
        /// Determines whether the specified HTTP request meets the criteria of this filter.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <returns><c>true</c> if the specified HTTP request is match; otherwise, <c>false</c>.</returns>
        public override bool IsMatch( HttpRequest httpRequest )
        {
            if ( PreviousActivityTypes.Length == 0 || PreviousActivityTypes.Length == 2 )
            {
                // If nothing is selected, return true.
                // If both are selected, we can also return true because a previous activity can only be New or Returning.
                return true;
            }

            var firstTimeVisitorCookie = httpRequest.Cookies.Get( Rock.Personalization.RequestCookieKey.ROCK_FIRSTTIME_VISITOR );

            // Only count them as a First Time visitor if we know for sure they are. Which means the cookie has to exist and
            // have a value of True.
            var isFirstTimeVisitor = firstTimeVisitorCookie != null && firstTimeVisitorCookie.Value.AsBoolean();

            if ( isFirstTimeVisitor )
            {
                return PreviousActivityTypes.Contains( PreviousActivityType.New );
            }
            else
            {
                return PreviousActivityTypes.Contains( PreviousActivityType.Return );
            }
        }

        /// <summary>
        /// Options on whether filter on New or Returning visitor
        /// </summary>
        public enum PreviousActivityType
        {
            /// <summary>
            /// The new
            /// </summary>
            New = 0,

            /// <summary>
            /// The returning
            /// </summary>
            Return = 1
        }
    }
}
