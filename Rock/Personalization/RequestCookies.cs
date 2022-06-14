using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Personalization
{
    /// <summary>.
    /// The Cookie Keys for Rock Personalization.
    /// </summary>
    public static class RequestCookieKey
    {
        /// <summary>
        /// The cookie key for .ROCK_VISITOR_KEY.
        /// This will be the <see cref="Rock.Data.IEntity.IdKey">IdKey</see> of a <see cref="Rock.Model.PersonAlias">PersonAlias</see> record.
        /// </summary>
        public static readonly string ROCK_VISITOR_KEY = $".ROCK_VISITOR_KEY";

        /// <summary>
        /// The cookie key for .ROCK_VISITOR_CREATED_DATETIME.
        /// The value is the UTC DateTime of when the Visitor PersonAlias was created ( ISO-8601 format ).
        /// </summary>
        public static readonly string ROCK_VISITOR_CREATED_DATETIME = $".ROCK_VISITOR_CREATED_DATETIME";

        /// <summary>
        /// The cookie key for .ROCK_FIRSTTIME_VISITOR.
        /// This will have a value of True if this is the first time the visitor has visited the site.
        /// </summary>
        public static readonly string ROCK_FIRSTTIME_VISITOR = $".ROCK_FIRSTTIME_VISITOR";

        /// <summary>
        /// The cookie key for .ROCK_SESSION_START_DATETIME.
        /// </summary>
        public static readonly string ROCK_SESSION_START_DATETIME = $".ROCK_SESSION_START_DATETIME";

        /// <summary>
        /// The cookie key for .ROCK_SEGMENT_FILTERS.
        /// </summary>
        public static readonly string ROCK_SEGMENT_FILTERS = $".ROCK_SEGMENT_FILTERS";
    }
}
