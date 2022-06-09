using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Personalization
{
    public static class RequestCookieKey
    {
        public static readonly string ROCK_VISITOR_KEY = $".ROCK_VISITOR_KEY";

        public static readonly string ROCK_VISITOR_CREATED_DATETIME = $".ROCK_VISITOR_CREATED_DATETIME";

        public static readonly string ROCK_FIRSTTIME_VISITOR = $".ROCK_FIRSTTIME_VISITOR";

        public static readonly string ROCK_SESSION_START_DATETIME = $".ROCK_SESSION_START_DATETIME";

        public static readonly string ROCK_SEGMENT_FILTERS = $".ROCK_SEGMENT_FILTERS";
    }
}
