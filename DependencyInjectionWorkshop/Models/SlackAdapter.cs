using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public interface INotification
    {
        void Notify(string accountId, string message);
    }

    public class SlackAdapter : INotification
    {
        public SlackAdapter()
        {
        }

        public void Notify(string accountId, string message)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(messageResponse => { }, "my channel", message, "my bot name");
        }
    }
}