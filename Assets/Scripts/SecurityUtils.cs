using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace VoyagerController
{
    public class SecurityUtils
    {
        public static string WPA_PSK(string ssid, string password)
        {
            byte[] ssidBytes = Encoding.ASCII.GetBytes(ssid);
            byte[] passwordBytes = Encoding.ASCII.GetBytes(password);
            Rfc2898DeriveBytes pbkdf2;
            //little magic here
            //Rfc2898DeriveBytes class has restriction of salt size to >= 8
            //but rfc2898 not (see http://www.ietf.org/rfc/rfc2898.txt)
            //we use Reflection to setup private field to avoid this restriction
            if (ssid.Length >= 8)
            {
                pbkdf2 = new Rfc2898DeriveBytes(passwordBytes, ssidBytes, 4096);
            }
            else
            {
                //use dummy salt here, we replace it later vie reflection
                pbkdf2 = new Rfc2898DeriveBytes(password, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 4096);
                var saltField = typeof(Rfc2898DeriveBytes).GetField("m_salt", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
                saltField.SetValue(pbkdf2, ssidBytes);
            }
            //get 256 bit PMK key
            byte[] resultBytes = pbkdf2.GetBytes(32);
            return BitConverter.ToString(resultBytes).Replace("-", "");
        }
    }
}
