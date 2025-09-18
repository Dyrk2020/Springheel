using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class CryptoEngine
{
	private const int AES_KEY_SIZE = 32;

	public static string AesEncrypt(string data, string key = "McQfTjWnZr4u7x!A%D*G-KaPdRgUkXp2")
	{
		try
		{
			return AesEncrypt(data, Encoding.UTF8.GetBytes(key));
		}
		catch (Exception)
		{
		}
		return null;
	}

	public static string AesDecrypt(string data, string key = "McQfTjWnZr4u7x!A%D*G-KaPdRgUkXp2")
	{
		try
		{
			return AesDecrypt(data, Encoding.UTF8.GetBytes(key));
		}
		catch (Exception)
		{
		}
		return null;
	}

	private static string AesEncrypt(string data, byte[] key)
	{
		return Convert.ToBase64String(AesEncrypt(Encoding.UTF8.GetBytes(data), key));
	}

	private static string AesDecrypt(string data, byte[] key)
	{
		return Encoding.UTF8.GetString(AesDecrypt(Convert.FromBase64String(data), key));
	}

	private static byte[] AesEncrypt(byte[] data, byte[] key)
	{
		if (data == null || data.Length == 0)
		{
			throw new ArgumentNullException("data cannot be empty");
		}
		if (key == null || key.Length != 32)
		{
			throw new ArgumentException(string.Format("{0} must be length of {1}", "key", 32));
		}
		using AesCryptoServiceProvider aesCryptoServiceProvider = new AesCryptoServiceProvider
		{
			KeySize = 256,
			BlockSize = 128,
			Key = key,
			Mode = CipherMode.CBC,
			Padding = PaddingMode.PKCS7
		};
		aesCryptoServiceProvider.GenerateIV();
		byte[] iV = aesCryptoServiceProvider.IV;
		using ICryptoTransform transform = aesCryptoServiceProvider.CreateEncryptor(aesCryptoServiceProvider.Key, iV);
		using MemoryStream memoryStream = new MemoryStream();
		using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
		{
			using BinaryWriter binaryWriter = new BinaryWriter(cryptoStream);
			memoryStream.Write(iV, 0, iV.Length);
			binaryWriter.Write(data);
			cryptoStream.FlushFinalBlock();
		}
		return memoryStream.ToArray();
	}

	private static byte[] AesDecrypt(byte[] data, byte[] key)
	{
		if (data == null || data.Length == 0)
		{
			throw new ArgumentNullException("data cannot be empty");
		}
		if (key == null || key.Length != 32)
		{
			throw new ArgumentException(string.Format("{0} must be length of {1}", "key", 32));
		}
		using AesCryptoServiceProvider aesCryptoServiceProvider = new AesCryptoServiceProvider
		{
			KeySize = 256,
			BlockSize = 128,
			Key = key,
			Mode = CipherMode.CBC,
			Padding = PaddingMode.PKCS7
		};
		byte[] array = new byte[aesCryptoServiceProvider.BlockSize / 8];
		Array.Copy(data, 0, array, 0, array.Length);
		using MemoryStream memoryStream = new MemoryStream();
		using (CryptoStream output = new CryptoStream(memoryStream, aesCryptoServiceProvider.CreateDecryptor(aesCryptoServiceProvider.Key, array), CryptoStreamMode.Write))
		{
			using BinaryWriter binaryWriter = new BinaryWriter(output);
			binaryWriter.Write(data, array.Length, data.Length - array.Length);
		}
		return memoryStream.ToArray();
	}
}
