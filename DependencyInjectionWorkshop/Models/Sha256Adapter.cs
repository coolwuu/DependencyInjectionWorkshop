using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public interface IHash
    {
        string Compute(string password);
    }

    public class Sha256Adapter : IHash
    {
        public Sha256Adapter()
        {
        }

        public string Compute(string password)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hashStringBuilder = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hashStringBuilder.Append(theByte.ToString("x2"));
            }

            return hashStringBuilder.ToString();
        }
    }
}