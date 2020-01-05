using System;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthentication
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
        }

        public AuthenticationService(IProfile profile, IHash hash, IOtpService otpService)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var dbPassword = _profile.GetPassword(accountId);
            var hashedPassword = _hash.Compute(password);
            var currentOtp = _otpService.GetCurrentOtp(accountId);

            return hashedPassword == dbPassword && otp == currentOtp;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}