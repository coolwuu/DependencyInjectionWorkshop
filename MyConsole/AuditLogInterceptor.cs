using System.Linq;
using Castle.DynamicProxy;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    public class AuditLogInterceptor : IInterceptor
    {
        private readonly ILogger _logger;
        private readonly IContext _context;

        public AuditLogInterceptor(ILogger logger, IContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void Intercept(IInvocation invocation)
        {
            var username = _context.GetUser().Name;
            var methodName = invocation.Method.Name;
            _logger.Info($"[AuditInterceptor.{methodName}] User:{username} parameters: {string.Join("|", invocation.Arguments.Select(x => x ?? "").ToString())}");
            invocation.Proceed();
            var returnValue = invocation.ReturnValue;
            _logger.Info($"[AuditInterceptor.{methodName}] ReturnValue:{returnValue}");
        }
    }
}