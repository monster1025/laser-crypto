using System;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace keygen
{
    class Program
    {
        private static readonly string PasswordHash = "P@sS_x_w0rd";
		private static readonly string SaltKey = "S@LT_&_KEY";
		private static readonly string VIKey = "@1B2c3D4_e5F6g7H8@";

		public static string Encrypt(string plainText)
		{
			byte[] bytes1 = Encoding.UTF8.GetBytes(plainText);
			byte[] bytes2 = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(32);
			RijndaelManaged rijndaelManaged = new RijndaelManaged();
			rijndaelManaged.Mode = CipherMode.CBC;
			rijndaelManaged.Padding = PaddingMode.Zeros;
			ICryptoTransform encryptor = rijndaelManaged.CreateEncryptor(bytes2, Encoding.ASCII.GetBytes(VIKey));
			byte[] array;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
				{
					cryptoStream.Write(bytes1, 0, bytes1.Length);
					cryptoStream.FlushFinalBlock();
					array = memoryStream.ToArray();
					cryptoStream.Close();
				}
				memoryStream.Close();
			}
			return Convert.ToBase64String(array);
		}

		public static string Decrypt(string encryptedText)
		{
			byte[] buffer = Convert.FromBase64String(encryptedText);
			byte[] bytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(32);
			RijndaelManaged rijndaelManaged = new RijndaelManaged();
			rijndaelManaged.Mode = CipherMode.CBC;
			rijndaelManaged.Padding = PaddingMode.None;
			ICryptoTransform decryptor = rijndaelManaged.CreateDecryptor(bytes, Encoding.ASCII.GetBytes(VIKey));
			MemoryStream memoryStream = new MemoryStream(buffer);
			CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read);
			byte[] numArray = new byte[buffer.Length];
			int count = cryptoStream.Read(numArray, 0, numArray.Length);
			memoryStream.Close();
			cryptoStream.Close();
			return Encoding.UTF8.GetString(numArray, 0, count).TrimEnd("\0".ToCharArray());
		}

		public static string GetHDDSerialNumber(string drive)
		{
			if (string.IsNullOrEmpty(drive) || drive == null)
				drive = "C";
			ManagementObject managementObject = new ManagementObject("Win32_LogicalDisk.DeviceID=\"" + drive + ":\"");
			managementObject.Get();
			return managementObject["VolumeSerialNumber"].ToString();
		}


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


		static string GetLicenceForCutter()
		{
			var serial = GetHDDSerialNumber("C");

			var array_LicenseLineItemsEncrypted = new string[16];
			array_LicenseLineItemsEncrypted[0] = Encrypt("Paid");
			array_LicenseLineItemsEncrypted[1] = Encrypt(serial);
			array_LicenseLineItemsEncrypted[2] = Encrypt("1");
			array_LicenseLineItemsEncrypted[3] = Encrypt("00.00.0000");

			var sb = new StringBuilder();
            sb.Append(array_LicenseLineItemsEncrypted[0] + "\r\n");
			sb.Append(array_LicenseLineItemsEncrypted[1] + "\r\n");
			sb.Append(array_LicenseLineItemsEncrypted[2] + "\r\n");
			sb.Append(array_LicenseLineItemsEncrypted[3]);

			return sb.ToString();
		}

        static void Main(string[] args)
        {
			Console.WriteLine("You cutter license key is: ");
			Console.WriteLine(GetLicenceForCutter());
			Console.WriteLine("----------------------------- COPY UNTIL THIS LINE -------------------------");

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
