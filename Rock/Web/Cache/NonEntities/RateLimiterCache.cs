using System;
using System.Runtime.Serialization;

namespace Rock.Web.Cache
{
    /// <summary>
    /// This class is used to rate limit calls or block to help prevent them from being spammed.
    /// </summary>
    /// <seealso cref="Rock.Web.Cache.ItemCache{Rock.Web.Cache.RateLimiterCache}" />
    [DataContract]
    public class RateLimiterCache : ItemCache<RateLimiterCache>, IHasLifespan
    {
        [DataMember]
        private int PeriodActionCount { get; set; }

        [DataMember]
        private int PeriodMaxActionCount { get; set; }

        [DataMember]
        private DateTime LastAction { get; set; }

        [DataMember]
        private TimeSpan MinTimeBetweenActions { get; set; }

        [DataMember]
        public TimeSpan? Lifespan { get; private set; }

        private RateLimiterCache( TimeSpan lifespan, int maxActionsInPeriod, TimeSpan? minTimeBetweenActions = null )
        {
            PeriodActionCount = 0;
            LastAction = DateTime.MinValue;
            Lifespan = lifespan;

            PeriodMaxActionCount = maxActionsInPeriod;

            if ( minTimeBetweenActions == null )
                MinTimeBetweenActions = new TimeSpan( 0 );
            else
                MinTimeBetweenActions = minTimeBetweenActions.Value;
        }

        public static bool Check( string actionName, string requestIdentifier, TimeSpan period, int maxActionsInPeriod, TimeSpan? minTimeBetweenActions = null )
        {
            var limiter = GetRateLimiter( actionName, requestIdentifier, period, maxActionsInPeriod, minTimeBetweenActions );
            if ( limiter.CanPerformAction() )
            {
                limiter.PerformAction();
                return true;
            }
            return false;
        }

        private void PerformAction()
        {
            PeriodActionCount++;
            LastAction = DateTime.Now;
        }
        private bool CanPerformAction()
        {
            if ( PeriodActionCount >= PeriodMaxActionCount )
                return false;
            if ( LastAction.Add( MinTimeBetweenActions ) > DateTime.Now )
                return false;
            return true;
        }

        private static RateLimiterCache GetRateLimiter( string actionName, string requestIdentifier, TimeSpan period, int maxActionsInPeriod, TimeSpan? minTimeBetweenActions = null )
        {
            var cacheKey = GetRateLimiterCacheKey( actionName, requestIdentifier );
            return GetOrAddExisting( cacheKey, () => InitializeNewRateLimiterCache( period, maxActionsInPeriod, minTimeBetweenActions ) );
        }

        private static RateLimiterCache InitializeNewRateLimiterCache( TimeSpan period, int maxActionsInPeriod, TimeSpan? minTimeBetweenActions = null )
        {
            return new RateLimiterCache( period, maxActionsInPeriod, minTimeBetweenActions );
        }

        private static string GetRateLimiterCacheKey( string actionName, string requestIdentifier )
        {
            return actionName + "." + requestIdentifier;
        }
    }
}
