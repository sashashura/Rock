using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web;

using Newtonsoft.Json;

using Rock.Data;
using Rock.Mobile;
using Rock.Model;
namespace Rock.Web.HttpModules
{
    /// <summary>
    /// 
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
            if ( !path.StartsWith( "/.well-known" ) )
            {
                return;
            }

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
            var appLinks = new AASAResponsePOCO();
            var detailsList = new List<AASADeepLinkDetailsPOCO>();

            // We're going to loop through each site and do a couple of things.
            // 1. Construct the AppId and Path for each application, if unclear please refer to link above.
            // 2. Add to parent list of details.
            foreach ( var site in mobileSites )
            {
                var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();

                var appDetails = new AASADeepLinkDetailsPOCO
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

                var appDetails = new AssetLinksTargetDetailsPOCO
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
        private class AASAResponsePOCO
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
            public List<AASADeepLinkDetailsPOCO> DetailsList { get; set; }
        }

        /// <summary>
        /// POCO for the deep link details information. 
        /// </summary>
        private class AASADeepLinkDetailsPOCO
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
        private class AssetLinksTargetDetailsPOCO
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
