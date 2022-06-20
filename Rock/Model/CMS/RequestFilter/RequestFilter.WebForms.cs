using System.Web;

using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class RequestFilter
    {
        /// <summary>
        /// Requests the meets criteria.
        /// </summary>
        /// <param name="requestFilterId">The request filter identifier.</param>
        /// <param name="request">The request.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool RequestMeetsCriteria( int requestFilterId, HttpRequest request )
        {
            var requestFilterConfiguration = RequestFilterCache.Get( requestFilterId )?.FilterConfiguration;
            if ( requestFilterConfiguration == null )
            {
                // somehow null so return false
                return false;
            }

            /* All of these are AND'd so, if any are false we can return false */

            // Check against Previous Activity
            var previousActivityFilter = requestFilterConfiguration.PreviousActivityRequestFilter;
            if ( !previousActivityFilter.IsMatch( request ) )
            {
                return false;
            }

            // Check against Device Type
            var deviceTypeFilter = requestFilterConfiguration.DeviceTypeRequestFilter;
            if ( !deviceTypeFilter.IsMatch( request ) )
            {
                return false;
            }

            // Check against Query String
            bool queryStringRequestFiltersMatch = true;
            foreach ( var queryStringRequestFilter in requestFilterConfiguration.QueryStringRequestFilters )
            {
                var isMatch = queryStringRequestFilter.IsMatch( request );
                if ( requestFilterConfiguration.QueryStringRequestFilterExpressionType == FilterExpressionType.GroupAll )
                {
                    queryStringRequestFiltersMatch = queryStringRequestFiltersMatch && isMatch;
                    if ( !queryStringRequestFiltersMatch )
                    {
                        // If these are and'd and one of them is false, then we can break out of the loop
                        break;
                    }
                }
                else
                {
                    queryStringRequestFiltersMatch = queryStringRequestFiltersMatch || isMatch;
                }
            }

            if ( !queryStringRequestFiltersMatch )
            {
                return false;
            }

            // Check against Cookie values
            bool cookieFiltersMatch = true;
            foreach ( var cookieRequestFilter in requestFilterConfiguration.CookieRequestFilters )
            {
                var isMatch = cookieRequestFilter.IsMatch( request );
                if ( requestFilterConfiguration.CookieRequestFilterExpressionType == FilterExpressionType.GroupAll )
                {
                    cookieFiltersMatch = cookieFiltersMatch && isMatch;
                    if ( !cookieFiltersMatch )
                    {
                        // If these are and'd and one of them is false, then we can break out of the loop
                        break;
                    }
                }
                else
                {
                    cookieFiltersMatch = cookieFiltersMatch || isMatch;
                }
            }

            if ( !cookieFiltersMatch )
            {
                return false;
            }

            // Check against Browser type and version
            bool browserFiltersMatch = true;
            foreach ( var browserRequestFilter in requestFilterConfiguration.BrowserRequestFilters )
            {
                var isMatch = browserRequestFilter.IsMatch( request );
                browserFiltersMatch = browserFiltersMatch || isMatch;
            }

            if ( !browserFiltersMatch )
            {
                return false;
            }

            // Check based on IPAddress Range
            bool ipAddressFiltersMatch = true;
            foreach ( var ipAddressRequestFilter in requestFilterConfiguration.IPAddressRequestFilters )
            {
                var isMatch = ipAddressRequestFilter.IsMatch( request );
                ipAddressFiltersMatch = ipAddressFiltersMatch || isMatch;
            }

            if ( !ipAddressFiltersMatch )
            {
                return false;
            }

            // if none of the filters return false, then return true;
            return true;
        }
    }
}
