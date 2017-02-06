using System;
using System.Security.Cryptography;
using System.Text;

namespace keygen
{
    class Program
    {
        private static string GetCodedKey(string userHwk)
        {
            string input = userHwk.Substring(12, 4) + "T" + userHwk.Substring(24, 4) + "H" + userHwk.Substring(8, 4) + "O" + userHwk.Substring(16, 4) + "M" + userHwk.Substring(28, 4) + "S" + userHwk.Substring(4, 4) + "O" + userHwk.Substring(20, 4) + "N" + userHwk.Substring(0, 4);
            using (MD5 md5Hash = MD5.Create())
                return GetMd5Hash(md5Hash, input);
        }

        public static string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] hash = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder stringBuilder = new StringBuilder();
            int num1 = 0;
            int num2 = checked(hash.Length - 1);
            int index = num1;
            while (index <= num2)
            {
                stringBuilder.Append(hash[index].ToString("x2"));
                checked { ++index; }
            }
            return stringBuilder.ToString();
        }

        static void Main(string[] args)
        {
            Console.Write("Please enter hardware ID: ");
            var key = Console.ReadLine();
            if (key == null)
            {
                Console.WriteLine("Key cannot be null or empty.");
                Console.ReadLine();
                return;
            }
            key = key.Replace("\n", "").Replace("-", "");

            var encoded = GetCodedKey(key);
            Console.WriteLine("You registration key is: " + encoded);
            Console.ReadLine();
        }
    }
}
