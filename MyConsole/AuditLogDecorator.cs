using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    public class AuditLogDecorator : AuthenticationDecoratorBase
    {
        private readonly ILogger _logger;
        private readonly IContext _context;

        public AuditLogDecorator(IAuthentication authentication, ILogger logger, IContext context) : base(authentication)
        {
            _logger = logger;
            _context = context;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var userName = _context.GetUser().Name;
            _logger.Info($"[AUDIT] user: {userName} | parameters : {accountId} | {password} | {otp}");
            var isValid = base.Verify(accountId, password, otp);
            _logger.Info($"[AUDIT] result : {isValid}");
            return isValid;
        }
    }
}