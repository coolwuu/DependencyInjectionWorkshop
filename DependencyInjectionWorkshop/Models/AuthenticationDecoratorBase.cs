namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationDecoratorBase : IAuthentication
    {
        private readonly IAuthentication _authentication;

        public AuthenticationDecoratorBase(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        public virtual bool Verify(string accountId, string password, string otp)
        {
            return _authentication.Verify(accountId, password, otp);
        }
    }
}