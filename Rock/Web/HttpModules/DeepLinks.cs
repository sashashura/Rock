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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web;

using Newtonsoft.Json;

using Rock.Common.Mobile;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;

namespace Rock.Web.HttpModules
{
    /// <summary>
    /// An HTTP module used to handle incoming Deep Links. 
    /// </summary>
    /// <seealso cref="Rock.Web.HttpModules.HttpModuleComponent" />
    [Description( "A HTTP Module that handles deep link requests for mobile applications." )]
    [Export( typeof( HttpModuleComponent ) )]
    [ExportMetadata( "ComponentName", "Deep Links" )]
    public class DeepLinks : HttpModuleComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeepLinks"/> class.
        /// </summary>
        public DeepLinks()
        {
        }

        ///<inheritdoc />
        public override void Dispose()
        {
        }

        ///<inheritdoc />
        public override void Init( HttpApplication context )
        {
            context.BeginRequest += ( new EventHandler( Application_BeginRequest ) );
        }

        /// <summary>
        /// Handles the BeginRequest event of the Application control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Application_BeginRequest( Object source, EventArgs e )
        {
            // Create HttpApplication and HttpContext objects to access
            // request and response properties.
            HttpApplication application = ( HttpApplication ) source;
            HttpContext context = application.Context;

            string path = HttpContext.Current.Request.Url.AbsolutePath;

            // Early outs in case the requested link isn't in regards to deep linking.
            if ( path.StartsWith( "/.well-known/" ) )
            {
                ProcessWellKnownRequest( context, path );
                return;
            }

            var segments = path.SplitDelimitedValues( "/" ).ToList();
            segments.RemoveAt( 0 );

            // TODO: Cache the deep links. These are dummy deep links, but we need to fetch all the mobile sites that have deep linking enabled \
            // and if the path prefix is the same as the beginning segment of the URL.

            DeepLinkRoute matchedRoute = null;

            var dynamicParameters = new Dictionary<string, string>();
            using ( var rockContext = new RockContext() )
            {
                var siteService = new SiteService( rockContext );
                var sites = siteService.Queryable().AsEnumerable().Where( x =>
                x.AdditionalSettings?.FromJsonOrNull<AdditionalSiteSettings>().IsDeepLinkingEnabled == true
                );

                foreach ( var site in sites )
                {
                    if ( path.StartsWith( $"/{site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>().DeepLinkPathPrefix}/" ) )
                    {
                        var (route, dynamicParams) = FindRouteWithParams( site, path );
                        matchedRoute = route;

                        if ( dynamicParams != null )
                        {
                            dynamicParameters = dynamicParams;
                        }

                        break;
                    }
                }
            }

            if ( matchedRoute == null )
            {
                return;
            }

            string query = HttpContext.Current.Request.Url.Query;

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if(query.IsNotNullOrWhiteSpace())
            {
                query = query.Remove( 0, 1 );
                parameters = QuerystringToParms( query );
            }

            if ( dynamicParameters.Count() > 0 )
            {
                try
                {
                    dynamicParameters.ToList().ForEach( q => parameters.Add( q.Key, q.Value ) );
                }
                catch ( ArgumentException ex )
                {
                    System.Diagnostics.Debug.WriteLine( "Please make sure you do not have duplicate query parameters." );
                    System.Diagnostics.Debug.WriteLine( $"{ex.InnerException?.ToStringSafe() ?? string.Empty}" );
                    throw;
                }
            }

            // If the route uses a Url as a fallback, we will redirect them as such.
            if ( matchedRoute.UsesUrlAsFallback )
            {
                HttpContext.Current.Response.StatusCode = 303;
                HttpContext.Current.Response.Redirect( matchedRoute.WebFallbackPageUrl );
                HttpContext.Current.Response.End();
            }
            // The route falls back to a page, so let's build the route to that page 
            else
            {
                using ( var rockContext = new RockContext() )
                {
                    var pageService = new PageService( rockContext );

                    var pageSequence = pageService.Queryable().Where( x => x.Guid == matchedRoute.WebFallbackPageGuid );
                    var page = pageSequence.Any() ? pageSequence.First() : null;

                    if ( page == null )
                    {
                        return;
                    }

                    var pageReference = new PageReference( page.Id, page.PageRoutes.First().Id, parameters );

                    var routeUrl = pageReference.BuildUrl();

                    HttpContext.Current.Response.StatusCode = 301;
                    HttpContext.Current.Response.Redirect( $"{routeUrl}" );
                    HttpContext.Current.Response.End();
                }
            }
        }

        /// <summary>
        /// Takes a query string Var1=ValueA&Var2=ValueB&Var3=ValueC and returns a Dictionary of parameters.
        /// </summary>
        /// <param name="str">The querystring.</param>
        /// <returns></returns>
        private static Dictionary<string, string> QuerystringToParms( string str )
        {
            var parms = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

            if ( str.IsNullOrWhiteSpace() )
            {
                return parms;
            }

            var queryStringParms = str.Split( new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries );

            foreach ( var keyValue in queryStringParms )
            {
                string[] splitKeyValue = keyValue.Split( new char[] { '=' } );

                {
                    string unencodedKey = Uri.UnescapeDataString( splitKeyValue[0] ).Trim();
                    string unencodedValue = Uri.UnescapeDataString( splitKeyValue[1] ).Trim();
                    parms.AddOrReplace( unencodedKey, unencodedValue );
                }
            }

            return parms;
        }

        /// <summary>
        /// Finds the route with parameters.
        /// </summary>
        /// <param name="site">The site to cross reference the routes from.</param>
        /// <param name="path">The path.</param>
        /// <returns>&lt;DeepLinkRoute, Dictionary&lt;string, string&gt;&gt;.</returns>
        public (DeepLinkRoute route, Dictionary<string, string> dynamicParams) FindRouteWithParams( Site site, string path )
        {
            var dynamicParameters = new Dictionary<string, string>();
            var routes = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>().DeepLinkRoutes;

            for ( int routeIndex = 0; routeIndex < routes.Count(); routeIndex++ )
            {
                var route = routes[routeIndex];
                var absoluteRoute = route.Route;

                var routeSegments = absoluteRoute.Split( '/' ).ToList();
                var pathSegments = path.Split( '/' ).ToList();

                pathSegments.RemoveAt( 0 );
                pathSegments.RemoveAt( 0 );

                if( pathSegments.Count() != routeSegments.Count() )
                {
                    continue;
                }

                for ( int routeSegmentIndex = 0; routeSegmentIndex <= routeSegments.Count(); routeSegmentIndex++ )
                {
                    var pathSegmentIndex = routeSegmentIndex;

                    if ( routeSegmentIndex == routeSegments.Count() )
                    {
                        return (route, dynamicParameters);
                    }

                    // The segments we are comparing. (requested path e.g. '/u/christmas/32/notes' to route path e.g. '/u/christmas/{christmasId}/notes'
                    var routeSegment = routeSegments[routeSegmentIndex];
                    var pathSegment = pathSegments[pathSegmentIndex];

                    // If this route is a dynamic segment, we need to take those values and put them into the parameters dictionary that will go to the page.
                    if ( routeSegment.StartsWith( "{" ) && routeSegment.EndsWith( "}" ) )
                    {
                        var key = routeSegment.Trim( new char[] { '{', '}' } );
                        dynamicParameters.Add( key, pathSegment );
                        continue;
                    }

                    // The route is not a match. On to the next route.
                    if ( routeSegment != pathSegment )
                    {
                        dynamicParameters.Clear();
                        break;
                    }
                }
            }

            return (null, null);
        }

        /// <summary>
        /// Processes a .well-known folder request to see if we are requesting either the AASA (iOS) file or the assetlinks.json (Android) file.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="path">The path.</param>
        private void ProcessWellKnownRequest( HttpContext context, string path )
        {
            var shouldGetAssetLinks = path.Contains( "assetlinks.json" );
            var shouldGetAASA = path.Contains( "apple-app-site-association" );

            // If the .well-known request is not requesting the assetlinks or AASA file.
            if ( !shouldGetAssetLinks && !shouldGetAASA )
            {
                return;
            }

            context.Response.StatusCode = 200;
            context.Response.Headers.Set( "content-type", "application/json" );

            // Get either the AASA or the AssetLinks response, and write it.
            var response = shouldGetAASA ? GenerateAASAResponse() : GenerateAssetLinksResponse();
            context.Response.Write( response );
            HttpContext.Current.Response.End();
        }

        /// <summary>
        /// Generates the AASA response.
        /// </summary>
        /// <returns>A JSON string containing the app link data.</returns>
        /// <seealso href="https://developer.apple.com/documentation/xcode/supporting-associated-domains"/>
        private string GenerateAASAResponse()
        {
            // Fetching all of our mobile sites.
            var mobileSites = GetDeepLinkSites();


            // In this area, we are mostly working on constructing our data in a way that easily converts into the
            // required format of the AASA file. See: https://gist.github.com/mat/e35393e9dfd9d7fb0972
            var appLinks = new AASAResponse();
            var detailsList = new List<AASADeepLinkDetails>();

            // We're going to loop through each site and do a couple of things.
            // 1. Construct the AppId and Path for each application, if unclear please refer to link above.
            // 2. Add to parent list of details.
            foreach ( var site in mobileSites )
            {
                var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();

                var appDetails = new AASADeepLinkDetails
                {
                    AppId = $"{additionalSettings.TeamIdentifier}.{additionalSettings.BundleIdentifier}",
                    Paths = new List<string> { $"/{additionalSettings.DeepLinkPathPrefix}/*" }
                };

                detailsList.Add( appDetails );
            }

            appLinks.DetailsList = detailsList;

            // Setting the parent key as 'applinks'.
            var aasaResponse = new Dictionary<string, object>
            {
                ["applinks"] = appLinks
            };

            return JsonConvert.SerializeObject( aasaResponse, Formatting.Indented );
        }

        /// <summary>
        /// Generates the asset links response.
        /// </summary>
        /// <returns>A JSON string containing the asset links data.</returns>
        /// <seealso href="https://developer.android.com/training/app-links"/>
        private string GenerateAssetLinksResponse()
        {
            // Fetching all of our mobile sites.
            var mobileSites = GetDeepLinkSites();
            using ( var context = new RockContext() )
            {
                var siteService = new SiteService( context );
                var qry = siteService.Queryable();
                mobileSites = qry.Where( x => x.SiteType == SiteType.Mobile ).ToList();
            }

            // In this area, we are focusing on constructing our data to easily convert to the assetlinks.json file that is
            // required for deep linking. See: https://developer.android.com/training/app-links/verify-site-associations#web-assoc.
            var assetLinks = new List<object>();

            foreach ( var site in mobileSites )
            {
                var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();

                if ( !additionalSettings.IsDeepLinkingEnabled )
                {
                    continue;
                }

                var appDetails = new AssetLinksTargetDetails
                {
                    PackageName = additionalSettings.PackageName,
                    CertificateFingerprint = additionalSettings.CertificateFingerprint
                };

                var linkData = new Dictionary<string, object>
                {
                    ["relation"] = new List<string> { "delegate_permission/common.handle_all_urls" },
                    ["target"] = appDetails
                };

                assetLinks.Add( linkData );
            }

            return JsonConvert.SerializeObject( assetLinks, Formatting.Indented );
        }

        /// <summary>
        /// Gets a list of mobile sites with deep linking enabled.
        /// </summary>
        /// <returns></returns>
        private List<Site> GetDeepLinkSites()
        {
            using ( var context = new RockContext() )
            {
                var siteService = new SiteService( context );
                var qry = siteService.Queryable();
                return qry.Where( x =>
                            x.SiteType == SiteType.Mobile )
                    .AsEnumerable()
                    .Where( x => x.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>().IsDeepLinkingEnabled == true )
                    .ToList();
            }
        }

        /// <summary>
        /// POCO for the entire AASA response information.
        /// </summary>
        private class AASAResponse
        {
            /// <summary>
            /// Gets or sets the apps identifier, in our use case, we leave this list empty. 
            /// </summary>
            /// <value>
            /// The apps.
            /// </value>
            [JsonProperty( "apps" )]
            public List<string> Apps { get; set; } = new List<string>();

            /// <summary>
            /// Gets or sets the deep link details list.
            /// </summary>
            /// <value>
            /// The deep link details list.
            /// </value>
            [JsonProperty( "details" )]
            public List<AASADeepLinkDetails> DetailsList { get; set; }
        }

        /// <summary>
        /// POCO for the deep link details information. 
        /// </summary>
        private class AASADeepLinkDetails
        {
            /// <summary>
            /// Gets or sets the application identifier.
            /// </summary>
            /// <value>
            /// The application identifier.
            /// </value>
            [JsonProperty( "appID" )]
            public string AppId { get; set; }

            /// <summary>
            /// Gets or sets the paths.
            /// </summary>
            /// <value>
            /// The paths.
            /// </value>
            [JsonProperty( "paths" )]
            public List<string> Paths { get; set; }
        }

        /// <summary>
        /// POCO for the deep link details information.
        /// </summary>
        private class AssetLinksTargetDetails
        {
            /// <summary>
            /// Gets or sets the namespace.
            /// </summary>
            /// <value>
            /// The namespace.
            /// </value>
            [JsonProperty( "namespace" )]
            public string Namespace { get; set; } = "android_app";

            /// <summary>
            /// Gets or sets the name of the package.
            /// </summary>
            /// <value>
            /// The name of the package.
            /// </value>
            [JsonProperty( "package_name" )]
            public string PackageName { get; set; }

            /// <summary>
            /// Gets or sets the certificate fingerprint.
            /// </summary>
            /// <value>
            /// The certificate fingerprint.
            /// </value>
            [JsonProperty( "sha256_cert_fingerprints" )]
            public string CertificateFingerprint { get; set; }
        }
    }
}
