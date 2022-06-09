using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Rock.Personalization
{
    /// <summary>
    /// Class PreviousActivityRequestFilter.
    /// Implements the <see cref="Rock.Personalization.RequestFilter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.RequestFilter" />
    public class PreviousActivityRequestFilter : RequestFilter
    {
        #region Configuration

        // #TODO#

        #endregion Configuration

        /// <summary>
        /// Determines whether the specified HTTP request meets the criteria of this filter.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <returns><c>true</c> if the specified HTTP request is match; otherwise, <c>false</c>.</returns>
        public override bool IsMatch( HttpRequest httpRequest )
        {
            throw new NotImplementedException();
        }
    }
}
