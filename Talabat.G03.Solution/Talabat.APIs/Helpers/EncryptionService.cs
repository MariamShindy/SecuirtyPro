using System.Security.Cryptography;
using System.Text;

namespace Talabat.APIs.Helpers
{
	public class EncryptionService
	{
		//private const string EncryptionKey = "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF9439340FDHFGVKDFHH"; // You should generate a strong key

		public static readonly byte[] Key = new byte[]
		{
			0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF,
			0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10,
			0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67
		};

		public static string Encrypt(string plainText)
		{
			using (var des = new TripleDESCryptoServiceProvider())
			{
				des.Key = Key;
				des.Mode = CipherMode.ECB; // Electronic Codebook (ECB) mode for simplicity, consider using other modes for better security
				des.Padding = PaddingMode.PKCS7; // Padding mode

				using (var encryptor = des.CreateEncryptor())
				{
					byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
					byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
					return Convert.ToBase64String(encryptedBytes);
				}
			}
		}

		public static string Decrypt(string encryptedText)
		{
			using (var des = new TripleDESCryptoServiceProvider())
			{
				des.Key = Key;
				des.Mode = CipherMode.ECB;
				des.Padding = PaddingMode.PKCS7;

				using (var decryptor = des.CreateDecryptor())
				{
					byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
					byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
					return Encoding.UTF8.GetString(decryptedBytes);
				}
			}
		}
	}

	public class KeyGenerator
	{
		public static byte[] GenerateTripleDesKey()
		{
			using (var des = new TripleDESCryptoServiceProvider())
			{
				// Generate a random key
				des.GenerateKey();
				return des.Key;
			}
		}
	}
}
