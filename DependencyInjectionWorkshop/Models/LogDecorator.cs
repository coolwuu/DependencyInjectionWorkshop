using NLog.Fluent;

namespace DependencyInjectionWorkshop.Models
{
    public class LogDecorator : AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;

        public LogDecorator(IAuthentication authenticationService, IFailedCounter failedCounter, ILogger logger) : base(authenticationService)
        {
            _failedCounter = failedCounter;
            _logger = logger;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = base.Verify(accountId, password, otp);
            if (!isValid)
            {
                LogFailedCount(accountId);
            }
            return isValid;
        }

        private void LogFailedCount(string accountId)
        {
            var failedCount = _failedCounter.GetFailedCount(accountId);
            _logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}