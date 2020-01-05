using Castle.DynamicProxy;
using DependencyInjectionWorkshop.Models;
using System;
using System.Linq;

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
            if (!(Attribute.GetCustomAttribute(invocation.Method, typeof(AuditLogAttribute)) is AuditLogAttribute))
            {
                invocation.Proceed();
            }
            else
            {
                var methodName = invocation.Method.Name;
                _logger.Info($"[AuditInterceptor.{methodName}] User:{_context.GetUser().Name} parameters: {string.Join("|", invocation.Arguments.Select(x => (x ?? "").ToString()))}");
                invocation.Proceed();
                var returnValue = invocation.ReturnValue;
                _logger.Info($"[AuditInterceptor.{methodName}] ReturnValue:{returnValue}");
            }
        }
    }
}