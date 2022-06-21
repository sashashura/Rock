using System;
using System.ComponentModel;
using System.Linq;
using System.Web;

using Rock.Model;

namespace Rock.Personalization
{
    /// <summary>
    /// Class DeviceTypeRequestFilter.
    /// Implements the <see cref="Rock.Personalization.PersonalizationRequestFilter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.PersonalizationRequestFilter" />
    public class DeviceTypeRequestFilter : PersonalizationRequestFilter
    {
        #region Configuration

        /// <summary>
        /// Gets or sets the device types.
        /// </summary>
        /// <value>The device types.</value>
        public DeviceType[] DeviceTypes { get; set; } = new DeviceType[0];

        private string[] DeviceTypeStrings => DeviceTypes?.Select( x => x.ToString() ).ToArray();

        #endregion Configuration        

        /// <summary>
        /// Determines whether the specified HTTP request meets the criteria of this filter.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <returns><c>true</c> if the specified HTTP request is match; otherwise, <c>false</c>.</returns>
        public override bool IsMatch( HttpRequest httpRequest )
        {
            var clientType = InteractionDeviceType.GetClientType( httpRequest.UserAgent );

            return DeviceTypeStrings.Contains( clientType, StringComparer.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Enum DeviceType
        /// </summary>
        public enum DeviceType
        {
            /// <summary>
            /// The desktop
            /// </summary>
            [Description( "Desktop" )]
            Desktop = 0,

            /// <summary>
            /// The tablet
            /// </summary>
            [Description( "Tablet" )]
            Tablet = 1,

            /// <summary>
            /// The mobile
            /// </summary>
            [Description( "Mobile" )]
            Mobile = 2
        }
    }
}
