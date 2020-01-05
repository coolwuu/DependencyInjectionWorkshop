using System;
using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string DefaultAccountId = "Wuu";
        private const int DefaultFailedCount = 100;
        private IProfile _profile;
        private IHash _hash;
        private IOtpService _otpService;
        private ILogger _logger;
        private INotification _notification;
        private IFailedCounter _failedCounter;
        private IAuthentication _authentication;

        [SetUp]
        public void Setup()
        {
            _profile = Substitute.For<IProfile>();
            _hash = Substitute.For<IHash>();
            _otpService = Substitute.For<IOtpService>();
            _logger = Substitute.For<ILogger>();
            _notification = Substitute.For<INotification>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _authentication = new AuthenticationService(_profile, _hash, _otpService);

            _authentication = new FailedCounterDecorator(_authentication, _failedCounter, _logger);
            _authentication = new NotificationDecorator(_authentication, _notification);

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
        public void reset_failed_count_when_valid()
        {
            WhenValid();
            ShouldResetFailedCount(DefaultAccountId);
        }

        private void ShouldResetFailedCount(string accountId)
        {
            _failedCounter.Received(1).Reset(accountId);
        }

        private void WhenValid()
        {
            GivenPasswordFromDb(DefaultAccountId, "1234qwer");
            GivenHashedPassword("55688", "1234qwer");
            GivenOtp(DefaultAccountId, "ABCD1234");
            _authentication.Verify(DefaultAccountId, "55688", "ABCD1234");
        }

        [Test]
        public void is_invalid()
        {
            GivenPasswordFromDb(DefaultAccountId, "1234qwer");
            GivenHashedPassword("55688", "1234qwer");
            GivenOtp(DefaultAccountId, "ABCD1234");
            ShouldBeInvalid(DefaultAccountId, "55688", "wrong");
        }

        [Test]
        public void add_failed_count_when_invalid()
        {
            WhenInvalid();
            ShouldAddFailedCountWhenInvalid(DefaultAccountId);
        }

        [Test]
        public void log_failed_count_when_invalid()
        {
            GivenFailedCount(DefaultFailedCount);
            WhenInvalid();
            ShouldLogWhenInvalid(DefaultAccountId, DefaultFailedCount.ToString());
        }

        [Test]
        public void account_is_locked()
        {
            GivenAccountIsLocked(DefaultAccountId, true);
            ShouldThrow<FailedTooManyTimesException>();
        }


        private void ShouldThrow<TException>() where TException : Exception
        {
            TestDelegate action = () => _authentication.Verify(DefaultAccountId, "1234", "123456");
            Assert.Throws<TException>(action);
        }

        private void GivenAccountIsLocked(string accountId, bool isLocked)
        {
            _failedCounter.GetAccountIsLocked(accountId).Returns(isLocked);
        }

        [Test]
        public void notify_when_invalid()
        {
            WhenInvalid();
            ShouldNotifyWhenInvalid(DefaultAccountId);
        }

        private void ShouldNotifyWhenInvalid(string accountId)
        {
            _notification.Received(1).Notify(
                Arg.Is<string>(x => x.Contains(accountId)),
                Arg.Is<string>(x => x.Contains(accountId)));
        }

        private void ShouldLogWhenInvalid(string accountId, string failedCount)
        {
            _logger.Received(1)
                .Info(Arg.Is<string>(x => x.Contains(accountId) && x.Contains(failedCount)));
        }

        private void GivenFailedCount(int failedCount)
        {
            _failedCounter.GetFailedCount(DefaultAccountId).Returns(failedCount);
        }

        private void ShouldAddFailedCountWhenInvalid(string accountId)
        {
            _failedCounter.Received(1).Add(accountId);
        }

        private void WhenInvalid()
        {
            GivenPasswordFromDb(DefaultAccountId, "1234qwer");
            GivenHashedPassword("55688", "1234qwer");
            GivenOtp(DefaultAccountId, "ABCD1234");
            _authentication.Verify(DefaultAccountId, "55688", "wrong");
        }

        private void ShouldBeInvalid(string accountId, string password, string otp)
        {
            var isInvalid = _authentication.Verify(accountId, password, otp);
            Assert.IsFalse(isInvalid);
        }

        private void ShouldBeValid(string accountId, string password, string otp)
        {
            var isValid = _authentication.Verify(accountId, password, otp);
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