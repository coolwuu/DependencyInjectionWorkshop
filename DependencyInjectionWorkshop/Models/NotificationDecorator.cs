﻿namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : AuthenticationDecoratorBase
    {
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authentication, INotification notification) : base(authentication)
        {
            _notification = notification;
        }

        private void Notify(string accountId)
        {
            _notification.Notify(accountId, $"account:{accountId} try to login failed.");
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isVerified = base.Verify(accountId, password, otp);
            if (!isVerified)
            {
                Notify(accountId);
            }
            return isVerified;
        }
    }
}