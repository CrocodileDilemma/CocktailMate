using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace UltimateCocktails.Utilities
{
    public class EncryptedQueryString : System.Collections.Specialized.StringDictionary
    {
        public EncryptedQueryString()
        {
            // Nothing to do here
        }

        public EncryptedQueryString(string encryptedData)
        {
            // Decrypt data passed in using DPAPI
            byte[] rawData = HexEncoding.GetBytes(encryptedData);
            //byte[] clearRawData = ProtectedData.Unprotect(rawData, null, DataProtectionScope.LocalMachine);
            string clearRawData = Encoding.UTF8.GetString(rawData);
            string stringData = clearRawData.Aggregate("", (c, a) => c + (char)(a + 2));

            // Split the data and add the contents
            int index;
            string[] splittedData = stringData.Split(new char[] { '&' });

            foreach (string singleData in splittedData)
            {
                index = singleData.IndexOf('=');
                base.Add(HttpUtility.UrlDecode(singleData.Substring(0, index)),
                HttpUtility.UrlDecode(singleData.Substring(index + 1)));
            }
        }

        public override string ToString()
        {
            StringBuilder content = new StringBuilder();

            // Build a typical query string based on the contents
            foreach (string key in base.Keys)
            {

                content.Append(HttpUtility.UrlEncode(key));
                content.Append("=");
                content.Append(HttpUtility.UrlEncode(base[key]));
                content.Append("&");
            }

            // Remove the last ‘&’
            content.Remove(content.Length - 1, 1);

            // Encrypt the contents using DPAPI
            ////byte[] encryptedData = ProtectedData.Protect(Encoding.UTF8.GetBytes(content.ToString()), null, 
            ////    DataProtectionScope.LocalMachine);
            byte[] encryptedData = Encoding.UTF8.GetBytes(content.ToString()
                .Aggregate("", (c, a) => c + (char)(a - 2)));
            // Convert encrypted byte array to a URL-legal string.
            // Check that data is not larger than typical 4 KB query string
            return HexEncoding.GetString(encryptedData);
        }
    }
}
