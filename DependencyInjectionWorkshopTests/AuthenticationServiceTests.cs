using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string DefaultAccountId = "Wuu";
        private IProfile _profile;
        private IHash _hash;
        private IOtpService _otpService;
        private ILogger _logger;
        private INotification _notification;
        private IFailedCounter _failedCounter;
        private AuthenticationService _authenticationService;

        [SetUp]
        public void Setup()
        {
            _profile = Substitute.For<IProfile>();
            _hash = Substitute.For<IHash>();
            _otpService = Substitute.For<IOtpService>();
            _logger = Substitute.For<ILogger>();
            _notification = Substitute.For<INotification>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _authenticationService = new AuthenticationService(_profile, _hash, _otpService, _notification, _failedCounter, _logger);
        }

        [Test]
        public void is_valid()
        {
            GivenPasswordFromDb(DefaultAccountId, "1234qwer");
            GivenHashedPassword("55688", "1234qwer");
            GivenOtp(DefaultAccountId, "ABCD1234");
            ShouldBeValid(DefaultAccountId, "55688", "ABCD1234");
        }

        [Test]
        public void is_invalid()
        {
            GivenPasswordFromDb(DefaultAccountId, "1234qwer");
            GivenHashedPassword("55688", "1234qwer");
            GivenOtp(DefaultAccountId, "ABCD1234");
            ShouldBeInvalid(DefaultAccountId, "55688", "wrong");
        }

        private void ShouldBeInvalid(string accountId, string password, string otp)
        {
            var isInvalid = _authenticationService.Verify(accountId, password, otp);
            Assert.IsFalse(isInvalid);
        }

        private void ShouldBeValid(string accountId, string password, string otp)
        {
            var isValid = _authenticationService.Verify(accountId, password, otp);
            Assert.IsTrue(isValid);
        }

        private void GivenOtp(string accountId, string currentOtp)
        {
            _otpService.GetCurrentOtp(accountId).Returns(currentOtp);
        }

        private void GivenHashedPassword(string password, string passwordFromDb)
        {
            _hash.Compute(password).Returns(passwordFromDb);
        }

        private void GivenPasswordFromDb(string accountId, string passwordFromDb)
        {
            _profile.GetPassword(accountId).Returns(passwordFromDb);
        }
    }
}