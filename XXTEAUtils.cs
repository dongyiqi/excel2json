using System;
using System.Text;

/*
 * 需要注意的是，这里加密的是字节数组，而不是字符串。
 * 因为 C# 字符串是按照 Unicode 编码保存的。
 * 要加密字符串的话，需要用 System.Text.Encoding.UTF8 （或者其他编码器）的 GetBytes 方法转化为字节数组，然后才能对其加密，密钥也是一样的。
 * 密钥长度是 128 位，也就是 16 个元素的字节数组，不过少于 16 个字节或多于 16 个字节都可以正常工作，
 * 少于 16 个字节时，会自动通过补零来充填到 16 个字节，多于 16 个字节之后的元素会被忽略。
 * 另外还需要注意一点，加密以后的内容也是字节数组，但是你不能用 System.Text.Encoding.UTF8 把它转化为字符串，否则会造成信息丢失。
 */

public class XXTEAUtils
{
	
// 	public static string Encrypt(string text, string key)
// 	{
// 		Byte[] encryptBytes = Encrypt(Encoding.UTF8.GetBytes(text), Encoding.UTF8.GetBytes(key));
// 		return Convert.ToBase64String(encryptBytes);
// 	}
// 
// 	public static string Decrypt(string text, string key)
// 	{
// 		Byte[] decryptBytes = Decrypt(Convert.FromBase64String(text), Encoding.UTF8.GetBytes(key));
// 		return Encoding.UTF8.GetString(decryptBytes);
// 	}
	

	public static Byte[] Encrypt(Byte[] Data, Byte[] Key)
	{
		if (Data.Length == 0)
		{
			return null;
		}

		//data uint32[]
		int intNumber = (Data.Length >> 2);
		if ((Data.Length & 0x03) != 0)
		{
			intNumber++;
		}
		Byte[] tempData = new Byte[intNumber * 4];
		Array.Copy(Data, tempData, Data.Length);

		intNumber++;
		UInt32[] DataIntArr = new UInt32[intNumber];
		DataIntArr[intNumber - 1] = (UInt32)Data.Length;  //加入数据长度
		for (int m = 0; m < intNumber - 1; ++m)
		{
			DataIntArr[m] = BitConverter.ToUInt32(tempData, m * 4);
		}

		//key uint32[]
		int keyNumber = (Key.Length >> 2);
		if ((Key.Length & 0x03) != 0)
		{
			keyNumber++;
		}
		Byte[] tempKey = new Byte[keyNumber * 4];
		Array.Copy(Key, tempKey, Key.Length);

		UInt32[] KeyIntArr = new UInt32[keyNumber];
		for (Int32 i = 0; i < keyNumber; i++)
		{
			KeyIntArr[i] = BitConverter.ToUInt32(tempKey, i * 4);
		}

        //UnityEngine.Debug.Log("!!!!!!!!!:xxtea:" + KeyIntArr.Length);

        InnerEncrypt(DataIntArr, KeyIntArr);


		int n = intNumber << 2;
		Byte[] Result = new Byte[n];

		for (int j = 0; j < intNumber; ++j)
		{
			Byte[] tempByte = BitConverter.GetBytes(DataIntArr[j]);
			Array.Copy(tempByte, 0, Result, j * 4, 4);
		}
		return Result;
	}

	public static Byte[] Decrypt(Byte[] Data, Byte[] Key)
	{
		if (Data.Length % 4 != 0)
		{
			return null;
		}

		//data uint32[]
		int intNumber = (Data.Length >> 2);
		UInt32[] DataIntArr = new UInt32[intNumber];
		for (int m = 0; m < intNumber; ++m)
		{
			DataIntArr[m] = BitConverter.ToUInt32(Data, m * 4);
		}

		//key uint32[]
		int keyNumber = (Key.Length >> 2);
		if ((Key.Length & 0x03) != 0)
		{
			keyNumber++;
		}
		Byte[] tempKey = new Byte[keyNumber * 4];
		Array.Copy(Key, tempKey, Key.Length);

		UInt32[] KeyIntArr = new UInt32[keyNumber];
		for (Int32 i = 0; i < keyNumber; i++)
		{
			KeyIntArr[i] = BitConverter.ToUInt32(tempKey, i * 4);
		}

		InnerDecrypt(DataIntArr, KeyIntArr);

		UInt32 n = DataIntArr[intNumber - 1];
		if (n > (intNumber - 1) * 4)
		{
			return null;
		}

		Byte[] tempData = new Byte[intNumber * 4];
		for (int j = 0; j < intNumber; ++j)
		{
			Byte[] tempByte = BitConverter.GetBytes(DataIntArr[j]);
			Array.Copy(tempByte, 0, tempData, j * 4, 4);
		}

		Byte[] Result = new Byte[n];
		Array.Copy(tempData, (int)0, Result, (int)0, (int)n);
		return Result;
	}

	private static UInt32[] InnerEncrypt(UInt32[] v, UInt32[] k)
	{
		Int32 n = v.Length - 1;
		if (n < 1)
		{
			return v;
		}
		if (k.Length < 4)
		{
            UInt32[] Key = new UInt32[4] { 0, 0, 0, 0 };
			k.CopyTo(Key, 0);
			k = Key;
		}

//        UnityEngine.Debug.Log("------------------------------------------");

//         string key_str = "";
//         for (int xx = 0; xx < k.Length; xx++ )
//         {
//             key_str += k[xx].ToString("X8") + ",";
//         }
//         UnityEngine.Debug.Log("key=[" + key_str + "]");
// 
//         for (int xx = 0; xx < v.Length; xx++ )
//         {
//             UnityEngine.Debug.Log("input["+ xx +"] = " + v[xx].ToString("X8"));
//         }

        

		UInt32 z = v[n], y = v[0], delta = 0x9E3779B9, sum = 0, e;
		UInt32 p, q = 6 + 52 / ((UInt32)n + 1);
		while (q-- > 0)
		{
			sum = unchecked(sum + delta);
			e = sum >> 2 & 3;
			for (p = 0; p < (UInt32)n; p++)
			{
				y = v[p + 1];
				z = unchecked(v[p] += (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z));
			}
			y = v[0];
			z = unchecked(v[n] += (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z));
		}

//         for (int xx = 0; xx < v.Length; xx++)
//         {
//             UnityEngine.Debug.Log("output[" + xx + "] = " + v[xx].ToString("X8"));
//         }
		return v;
	}

	private static UInt32[] InnerDecrypt(UInt32[] v, UInt32[] k)
	{
		Int32 n = v.Length - 1;
		if (n < 1)
		{
			return v;
		}
		if (k.Length < 4)
		{
			UInt32[] Key = new UInt32[4]{0, 0, 0, 0};
			k.CopyTo(Key, 0);
			k = Key;
		}
		UInt32 z = v[n], y = v[0], delta = 0x9E3779B9, sum, e;
		UInt32 p, q = 6 + 52 / ((UInt32)n + 1);
		sum = unchecked((UInt32)(q * delta));
		while (sum != 0)
		{
			e = sum >> 2 & 3;
			for (p = (UInt32)n; p > 0; p--)
			{
				z = v[p - 1];
				y = unchecked(v[p] -= (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z));
			}
			z = v[n];
			y = unchecked(v[0] -= (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z));
			sum = unchecked(sum - delta);
		}
		return v;
	}

}


