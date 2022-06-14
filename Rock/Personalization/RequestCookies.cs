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
        /// Expires after X days.
        /// <para>
        /// This will be the <see cref="Rock.Data.IEntity.IdKey">IdKey</see> of a <see cref="Rock.Model.PersonAlias">PersonAlias</see> record.
        /// </para>
        /// </summary>
        public static readonly string ROCK_VISITOR_KEY = $".ROCK_VISITOR_KEY";

        /// <summary>
        /// The cookie key for .ROCK_VISITOR_CREATED_DATETIME.
        /// Expires after X days.
        /// <para>
        /// The value is the UTC DateTime of when the Visitor PersonAlias was created ( ISO-8601 format ).
        /// </para>
        /// </summary>
        public static readonly string ROCK_VISITOR_CREATED_DATETIME = $".ROCK_VISITOR_CREATED_DATETIME";

        /// <summary>
        /// The cookie ey for .ROCK_VISITOR_LASTSEEN.
        /// Expires after X days
        ///  Tracks the last page load of the visitor. This is used to determine how long since they were last here.
        ///  On Session End, this will be used to update the <see cref="Rock.Model.PersonAlias.LastVisitDateTime" />
        /// </summary>
        public static readonly string ROCK_VISITOR_LASTSEEN = $".ROCK_VISITOR_LASTSEEN";

        /// <summary>
        /// The cookie key for .ROCK_FIRSTTIME_VISITOR.
        /// Expires after X days.
        /// <para>
        /// This will have a value of True if this is the first time the visitor has visited the site.
        /// </para>
        /// </summary>
        public static readonly string ROCK_FIRSTTIME_VISITOR = $".ROCK_FIRSTTIME_VISITOR";

        /// <summary>
        /// The cookie key for .ROCK_SESSION_START_DATETIME. Cookie is only valid for session.
        /// <para>
        /// The value is the UTC DateTime of when the Session started for the CurrentPerson/Visitor ( ISO-8601 format ).
        /// </para>
        /// </summary>
        public static readonly string ROCK_SESSION_START_DATETIME = $".ROCK_SESSION_START_DATETIME";

        /// <summary>
        /// The cookie key for .ROCK_SEGMENT_FILTERS. Cookie is only valid for session.
        /// <para>
        /// A comma-delimited list of <seealso cref="Rock.Model.PersonalizationSegment">PersonalizationSegment</seealso> Ids that the current
        /// person/visitor meets the filter for.
        /// </para>
        /// </summary>
        public static readonly string ROCK_SEGMENT_FILTERS = $".ROCK_SEGMENT_FILTERS";
    }
}
