using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Rock.Model;

namespace Rock.Personalization
{
    public class DeviceTypeRequestFilter : RequestFilter
    {
        #region Configuration

        public string[] DeviceTypes { get; set; }

        #endregion Configuration

        /// <summary>
        /// Determines whether the specified HTTP request meets the criteria of this filter.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <returns><c>true</c> if the specified HTTP request is match; otherwise, <c>false</c>.</returns>
        public override bool IsMatch( HttpRequest httpRequest )
        {
            var clientType = InteractionDeviceType.GetClientType( httpRequest.UserAgent );

            return DeviceTypes.Contains( clientType, StringComparer.OrdinalIgnoreCase );
        }
    }
}
