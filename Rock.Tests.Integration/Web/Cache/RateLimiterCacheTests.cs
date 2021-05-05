using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Bus;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Web.Cache
{
    [TestClass]
    public class RateLimiterCacheTests
    {
        [ClassInitialize]
        public static void InitializeClass(TestContext testContext)
        {
            RockMessageBus.IsRockStarted = true;
            RockMessageBus.StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void Check_ShouldResetAfterPeriodExpires()
        {
            var result = RateLimiterCache.Check(
                "RockTest",
                nameof( Check_ShouldResetAfterPeriodExpires ),
                TimeSpan.FromSeconds( 5 ),
                1,
                null );

            Assert.IsTrue( result );

            Thread.Sleep( 6000 );

            result = RateLimiterCache.Check(
                "RockTest",
                nameof( Check_ShouldResetAfterPeriodExpires ),
                TimeSpan.FromSeconds( 5 ),
                1,
                null );

            Assert.IsTrue( result );
        }

        [TestMethod]
        public void Check_ShouldReturnFalseIfCallCountExceeded()
        {
            var result = RateLimiterCache.Check(
                "RockTest",
                nameof( Check_ShouldReturnFalseIfCallCountExceeded ),
                TimeSpan.FromSeconds( 5 ),
                1,
                null );

            Assert.IsTrue( result );

            result = RateLimiterCache.Check(
                "RockTest",
                nameof( Check_ShouldReturnFalseIfCallCountExceeded ),
                TimeSpan.FromSeconds( 5 ),
                1,
                null );

            Assert.IsFalse( result );
        }

        [TestMethod]
        public void Check_ShouldReturnTrueIfCallCountNotExceeded()
        {
            var maxCount = 100;

            for ( var i = 0; i < maxCount; i++ )
            {
                var result = RateLimiterCache.Check(
                    "RockTest",
                    nameof( Check_ShouldReturnTrueIfCallCountNotExceeded ),
                    TimeSpan.FromSeconds( 5 ),
                    maxCount,
                    null );

                Assert.IsTrue( result );
            }

            var failedResult = RateLimiterCache.Check(
                "RockTest",
                nameof( Check_ShouldReturnTrueIfCallCountNotExceeded ),
                TimeSpan.FromSeconds( 5 ),
                maxCount,
                null );

            Assert.IsFalse( failedResult );
        }
    }
}
