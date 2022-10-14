using System.Text;

namespace UltimateCocktails.Utilities
{
    public static class HexEncoding
    {
        public static string GetString(byte[] data)
        {
            StringBuilder results = new StringBuilder();

            foreach (byte b in data)
            {
                results.Append(b.ToString("X2"));
            }

            return results.ToString();
        }

        public static byte[] GetBytes(string data)
        {
            // GetString encodes the hex numbers with two digits
            byte[] results = new byte[data.Length / 2];

            for (int i = 0; i < data.Length; i += 2)
            {
                results[i / 2] = Convert.ToByte(data.Substring(i, 2), 16);
            }

            return results;
        }
    }
}
