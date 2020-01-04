using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string accountId, string password, string otp)
        {
            string dbPassword;
            using (var connection = new SqlConnection("my connection string"))
            {
                dbPassword = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hashStringBuilder = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hashStringBuilder.Append(theByte.ToString("x2"));
            }
            var hashedPassword = hashStringBuilder.ToString();

            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var response = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            var currentOtp = response.Content.ReadAsAsync<string>().Result;

            if (hashedPassword == dbPassword && otp == currentOtp)
            {
                return true;
            }

            var message = $"account:{accountId} try to login failed.";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
            return false;
        }
    }
}