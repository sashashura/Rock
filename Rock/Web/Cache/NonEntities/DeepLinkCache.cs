// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System.Collections.Generic;

using Rock.Common.Mobile;

namespace Rock.Web.Cache
{
    internal static class DeepLinkCache
    {
        private const string RoutesCacheKey = "DeepLinkCache:Routes";

        private const string ApplePayloadCacheKey = "DeepLinkCache:ApplePayload";

        private const string AndroidPayloadCacheKey = "DeepLinkCache:AndroidPayload";

        public static List<DeepLinkRoute> GetDeepLinksForPrefix( string prefix )
        {
            var routeTable = RockCache.GetOrAddExisting( RoutesCacheKey, BuildDeepLinkRoutes ) as Dictionary<string, List<DeepLinkRoute>>;

            if ( routeTable.TryGetValue( prefix.ToLower(), out var routes ) )
            {
                return routes;
            }
            {
                return null;
            }
        }

        public static string GetApplePayload()
        {
        }

        public static string GetAndroidPayload()
        {
        }

        public static void Flush()
        {
            RockCache.Remove( RoutesCacheKey );
            RockCache.Remove( ApplePayloadCacheKey );
            RockCache.Remove( AndroidPayloadCacheKey );
        }

        private static Dictionary<string, List<DeepLinkRoute>> BuildDeepLinkRoutes()
        {
        }
    }
}