using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpService _otpService;
        private readonly SlackAdapter _slackAdapter;
        private readonly FailedCounter _failedCounter;
        private readonly NLogAdapter _nLogAdapter;

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _slackAdapter = new SlackAdapter();
            _failedCounter = new FailedCounter();
            _nLogAdapter = new NLogAdapter();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var isLocked = _failedCounter.GetAccountIsLocked(accountId, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }

            var dbPassword = _profileDao.GetPasswordFromDb(accountId);
            var hashedPassword = _sha256Adapter.GetHashedPassword(password);
            var currentOtp = _otpService.GetCurrentOtp(accountId, httpClient);

            if (hashedPassword == dbPassword && otp == currentOtp)
            {
                _failedCounter.Reset(accountId, httpClient);
                return true;
            }
            else
            {
                _failedCounter.Add(accountId, httpClient);
                LogFailedCount(accountId, httpClient);
                _slackAdapter.Notify(accountId);
                return false;
            }
        }

        private void LogFailedCount(string accountId, HttpClient httpClient)
        {
            var failedCount = _failedCounter.GetFailedCount(accountId, httpClient);
            _nLogAdapter.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}