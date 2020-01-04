using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly INotification _notification;
        private readonly ICounter _counter;
        private readonly ILogger _logger;

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            _notification = new SlackAdapter();
            _counter = new FailedCounter();
            _logger = new NLogAdapter();
        }

        public AuthenticationService(IProfile profile, IHash hash, IOtpService otpService, INotification notification, ICounter counter, ILogger logger)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
            _notification = notification;
            _counter = counter;
            _logger = logger;
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isLocked = _counter.GetAccountIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }

            var dbPassword = _profile.GetPassword(accountId);
            var hashedPassword = _hash.Compute(password);
            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (hashedPassword == dbPassword && otp == currentOtp)
            {
                _counter.Reset(accountId);
                return true;
            }
            else
            {
                _counter.Add(accountId);
                LogFailedCount(accountId);
                _notification.Notify(accountId);
                return false;
            }
        }

        private void LogFailedCount(string accountId)
        {
            var failedCount = _counter.GetFailedCount(accountId);
            _logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}