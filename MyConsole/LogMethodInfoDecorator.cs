using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    public class LogMethodInfoDecorator : AuthenticationDecoratorBase
    {
        private readonly ILogger _logger;

        public LogMethodInfoDecorator(IAuthentication authentication, ILogger logger) : base(authentication)
        {
            _logger = logger;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            _logger.Info($"parameters : {accountId} | {password} | {otp}");
            var isValid = base.Verify(accountId, password, otp);
            _logger.Info($"result : {isValid}");
            return isValid;
        }
    }
}