using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public void is_valid()
        {
            var profile = Substitute.For<IProfile>();
            var hash = Substitute.For<IHash>();
            var otpService = Substitute.For<IOtpService>();
            var logger = Substitute.For<ILogger>();
            var notification = Substitute.For<INotification>();
            var failedCounter = Substitute.For<IFailedCounter>();

            var authenticationService =
                new AuthenticationService(profile, hash, otpService, notification, failedCounter, logger);

            profile.GetPassword("Wuu").Returns("1234qwer");
            hash.Compute("55688").Returns("1234qwer");
            otpService.GetCurrentOtp("Wuu").Returns("56789");

            var isValid = authenticationService.Verify("Wuu", "55688", "56789");
            Assert.IsTrue(isValid);
        }
    }
}