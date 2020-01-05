using DependencyInjectionWorkshop.Models;
using System;
using System.Runtime.InteropServices;
using Autofac;
using Autofac.Extras.DynamicProxy;

namespace MyConsole
{
    internal class Program
    {
        private static IContainer _container;

        private static void Main(string[] args)
        {
            RegisterContainer();
            Login("Wuu's Agent");
            var authentication = _container.Resolve<IAuthentication>();
            var isValid = authentication.Verify("joey", "abc", "wrong otp");
            Console.WriteLine($"result:{isValid}");
        }

        private static void Login(string userName)
        {
            var context = _container.Resolve<IContext>();
            context.SetUser(userName);
        }

        private static void RegisterContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<FakeProfile>().As<IProfile>();
            builder.RegisterType<FakeOtp>().As<IOtpService>();
            builder.RegisterType<FakeHash>().As<IHash>();
            builder.RegisterType<FakeLogger>().As<ILogger>();
            builder.RegisterType<FakeSlack>().As<INotification>();
            builder.RegisterType<FakeFailedCounter>().As<IFailedCounter>().EnableInterfaceInterceptors()
                .InterceptedBy(typeof(AuditLogInterceptor)); ;
            builder.RegisterType<FakeContext>().As<IContext>().SingleInstance();
            builder.RegisterType<AuditLogInterceptor>();

            builder.RegisterType<AuthenticationService>().As<IAuthentication>()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(AuditLogInterceptor));
            builder.RegisterDecorator<FailedCounterDecorator, IAuthentication>();
            builder.RegisterDecorator<LogDecorator, IAuthentication>();
            builder.RegisterDecorator<NotificationDecorator, IAuthentication>();
            //builder.RegisterDecorator<LogMethodInfoDecorator, IAuthentication>();
            //builder.RegisterDecorator<AuditLogDecorator, IAuthentication>();
            _container = builder.Build();
        }
    }

    public class FakeContext : IContext
    {
        private User _user;

        public User GetUser()
        {
            return _user;
        }

        public void SetUser(string userName)
        {
            _user = new User()
            {
                Name = userName
            };
        }
    }

    public interface IContext
    {
        User GetUser();
        void SetUser(string userName);
    }

    public class User
    {
        public string Name { get; set; }
    }

    internal class FakeLogger : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine($"logger: {message}");
        }
    }

    internal class FakeSlack : INotification
    {
        public void PushMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void Notify(string accountId, string message)
        {
            PushMessage($"{nameof(Notify)}, accountId:{accountId}, message:{message}");
        }
    }

    internal class FakeFailedCounter : IFailedCounter
    {
        public void Reset(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Reset)}({accountId})");
        }

        public void AddFailedCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(AddFailedCount)}({accountId})");
        }

        public bool GetAccountIsLocked(string accountId)
        {
            return IsAccountLocked(accountId);
        }

        public int GetFailedCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(GetFailedCount)}({accountId})");
            return 91;
        }

        public bool IsAccountLocked(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(IsAccountLocked)}({accountId})");
            return false;
        }
    }

    internal class FakeOtp : IOtpService
    {
        public string GetCurrentOtp(string accountId)
        {
            Console.WriteLine($"{nameof(FakeOtp)}.{nameof(GetCurrentOtp)}({accountId})");
            return "123456";
        }
    }

    internal class FakeHash : IHash
    {
        public string Compute(string plainText)
        {
            Console.WriteLine($"{nameof(FakeHash)}.{nameof(Compute)}({plainText})");
            return "my hashed password";
        }
    }

    internal class FakeProfile : IProfile
    {
        public string GetPassword(string accountId)
        {
            Console.WriteLine($"{nameof(FakeProfile)}.{nameof(GetPassword)}({accountId})");
            return "my hashed password";
        }
    }
}