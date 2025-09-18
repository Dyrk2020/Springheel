using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Crosstales.Common.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Crosstales;

public static class ExtensionMethods
{
	private static readonly Vector3 FLAT_VECTOR = new Vector3(1f, 0f, 1f);

	public static string CTToTitleCase(this string str)
	{
		if (str == null)
		{
			return str;
		}
		return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
	}

	public static string CTReverse(this string str)
	{
		if (str == null)
		{
			return str;
		}
		char[] array = str.ToCharArray();
		Array.Reverse(array);
		return new string(array);
	}

	public static string CTReplace(this string str, string oldString, string newString, StringComparison comp = StringComparison.OrdinalIgnoreCase)
	{
		if (str == null)
		{
			return str;
		}
		if (oldString == null)
		{
			return str;
		}
		if (newString == null)
		{
			return str;
		}
		bool flag;
		do
		{
			int num = str.IndexOf(oldString, comp);
			flag = num >= 0;
			if (flag)
			{
				str = str.Remove(num, oldString.Length);
				str = str.Insert(num, newString);
			}
		}
		while (flag);
		return str;
	}

	public static string CTRemoveChars(this string str, params char[] removeChars)
	{
		if (str == null)
		{
			return str;
		}
		if (removeChars == null)
		{
			return str;
		}
		return removeChars.Aggregate(str, (string current, char rmChar) => current.Replace($"{rmChar}", string.Empty));
	}

	public static bool CTEquals(this string str, string toCheck, StringComparison comp = StringComparison.OrdinalIgnoreCase)
	{
		return str?.Equals(toCheck, comp) ?? false;
	}

	public static bool CTContains(this string str, string toCheck, StringComparison comp = StringComparison.OrdinalIgnoreCase)
	{
		if (str == null)
		{
			return false;
		}
		return str.IndexOf(toCheck, comp) >= 0;
	}

	public static bool CTContainsAny(this string str, string searchTerms, char splitChar = ' ')
	{
		if (str == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(searchTerms))
		{
			return true;
		}
		char[] separator = new char[1] { splitChar };
		return searchTerms.Split(separator, StringSplitOptions.RemoveEmptyEntries).Any((string searchTerm) => str.CTContains(searchTerm));
	}

	public static bool CTContainsAll(this string str, string searchTerms, char splitChar = ' ')
	{
		if (str == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(searchTerms))
		{
			return true;
		}
		char[] separator = new char[1] { splitChar };
		return searchTerms.Split(separator, StringSplitOptions.RemoveEmptyEntries).All((string searchTerm) => str.CTContains(searchTerm));
	}

	public static string CTRemoveNewLines(this string str, string replacement = "#nl#", string newLine = null)
	{
		return str?.Replace(string.IsNullOrEmpty(newLine) ? Environment.NewLine : newLine, replacement);
	}

	public static string CTAddNewLines(this string str, string replacement = "#nl#", string newLine = null)
	{
		return str?.CTReplace(replacement, string.IsNullOrEmpty(newLine) ? Environment.NewLine : newLine);
	}

	[Obsolete("Please use 'CTIsNumeric' instead.")]
	public static bool CTisNumeric(this string str)
	{
		return str.CTIsNumeric();
	}

	public static bool CTIsNumeric(this string str)
	{
		double result;
		if (str != null)
		{
			return double.TryParse(str, out result);
		}
		return false;
	}

	[Obsolete("Please use 'CTIsInteger' instead.")]
	public static bool CTisInteger(this string str)
	{
		return str.CTIsInteger();
	}

	public static bool CTIsInteger(this string str)
	{
		if (str == null)
		{
			return false;
		}
		long result;
		if (!str.Contains("."))
		{
			return long.TryParse(str, out result);
		}
		return false;
	}

	[Obsolete("Please use 'CTIsEmail' instead.")]
	public static bool CTisEmail(this string str)
	{
		return str.CTIsEmail();
	}

	public static bool CTIsEmail(this string str)
	{
		if (str != null)
		{
			return BaseConstants.REGEX_EMAIL.IsMatch(str);
		}
		return false;
	}

	[Obsolete("Please use 'CTIsWebsite' instead.")]
	public static bool CTisWebsite(this string str)
	{
		return str.CTIsWebsite();
	}

	public static bool CTIsWebsite(this string str)
	{
		if (str != null)
		{
			return BaseConstants.REGEX_URL_WEB.IsMatch(str);
		}
		return false;
	}

	[Obsolete("Please use 'CTIsCreditcard' instead.")]
	public static bool CTisCreditcard(this string str)
	{
		return str.CTIsCreditcard();
	}

	public static bool CTIsCreditcard(this string str)
	{
		if (str != null)
		{
			return BaseConstants.REGEX_CREDITCARD.IsMatch(str);
		}
		return false;
	}

	[Obsolete("Please use 'CTIsIPv4' instead.")]
	public static bool CTisIPv4(this string str)
	{
		return str.CTIsIPv4();
	}

	public static bool CTIsIPv4(this string str)
	{
		if (str != null)
		{
			return NetworkHelper.isIPv4(str);
		}
		return false;
	}

	[Obsolete("Please use 'CTIsAlphanumeric' instead.")]
	public static bool CTisAlphanumeric(this string str)
	{
		return str.CTIsAlphanumeric();
	}

	public static bool CTIsAlphanumeric(this string str)
	{
		if (str != null)
		{
			return BaseConstants.REGEX_ALPHANUMERIC.IsMatch(str);
		}
		return false;
	}

	[Obsolete("Please use 'CTHasLineEndings' instead.")]
	public static bool CThasLineEndings(this string str)
	{
		return str.CTHasLineEndings();
	}

	public static bool CTHasLineEndings(this string str)
	{
		if (str != null)
		{
			return BaseConstants.REGEX_LINEENDINGS.IsMatch(str);
		}
		return false;
	}

	[Obsolete("Please use 'CTHasInvalidChars' instead.")]
	public static bool CThasInvalidChars(this string str)
	{
		return str.CTHasInvalidChars();
	}

	public static bool CTHasInvalidChars(this string str)
	{
		if (str != null)
		{
			return BaseConstants.REGEX_INVALID_CHARS.IsMatch(str);
		}
		return false;
	}

	public static bool CTStartsWith(this string str, string toCheck, StringComparison comp = StringComparison.OrdinalIgnoreCase)
	{
		if (str == null)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(toCheck))
		{
			return str.StartsWith(toCheck, comp);
		}
		return true;
	}

	public static bool CTEndsWith(this string str, string toCheck, StringComparison comp = StringComparison.OrdinalIgnoreCase)
	{
		if (str == null)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(toCheck))
		{
			return str.EndsWith(toCheck, comp);
		}
		return true;
	}

	public static int CTLastIndexOf(this string str, string toCheck, StringComparison comp = StringComparison.OrdinalIgnoreCase)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		if (!string.IsNullOrEmpty(toCheck))
		{
			return str.LastIndexOf(toCheck, comp);
		}
		return 0;
	}

	public static int CTIndexOf(this string str, string toCheck, StringComparison comp = StringComparison.OrdinalIgnoreCase)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		if (!string.IsNullOrEmpty(toCheck))
		{
			return str.IndexOf(toCheck, comp);
		}
		return 0;
	}

	public static int CTIndexOf(this string str, string toCheck, int startIndex, StringComparison comp = StringComparison.OrdinalIgnoreCase)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		if (!string.IsNullOrEmpty(toCheck))
		{
			return str.IndexOf(toCheck, startIndex, comp);
		}
		return 0;
	}

	public static string CTToBase64(this string str, Encoding encoding = null)
	{
		if (str == null)
		{
			return null;
		}
		Encoding encoding2 = Encoding.UTF8;
		if (encoding != null)
		{
			encoding2 = encoding;
		}
		return encoding2.GetBytes(str).CTToBase64();
	}

	public static string CTFromBase64(this string str, Encoding encoding = null)
	{
		if (str == null)
		{
			return null;
		}
		Encoding encoding2 = Encoding.UTF8;
		if (encoding != null)
		{
			encoding2 = encoding;
		}
		return encoding2.GetString(str.CTFromBase64ToByteArray());
	}

	public static byte[] CTFromBase64ToByteArray(this string str)
	{
		if (str != null)
		{
			return Convert.FromBase64String(str);
		}
		return null;
	}

	public static string CTToHex(this string str, bool addPrefix = false)
	{
		if (str == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (addPrefix)
		{
			stringBuilder.Append("0x");
		}
		byte[] bytes = Encoding.Unicode.GetBytes(str);
		foreach (byte b in bytes)
		{
			stringBuilder.Append(b.ToString("X2"));
		}
		return stringBuilder.ToString();
	}

	public static string CTHexToString(this string hexString)
	{
		if (hexString == null)
		{
			return null;
		}
		string text = hexString;
		if (text.StartsWith("0x"))
		{
			text = text.Substring(2);
		}
		if (hexString.Length % 2 != 0)
		{
			throw new FormatException("String seems to be an invalid hex-code: " + hexString);
		}
		byte[] array = new byte[text.Length / 2];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
		}
		return Encoding.Unicode.GetString(array);
	}

	public static Color32 CTHexToColor32(this string hexString)
	{
		if (hexString == null)
		{
			throw new ArgumentNullException("hexString");
		}
		string text = hexString;
		if (text.StartsWith("0x"))
		{
			text = text.Substring(2);
		}
		if (text.StartsWith("#"))
		{
			text = text.Substring(1);
		}
		if (text.Length != 6 && text.Length != 8)
		{
			throw new FormatException("String seems to be an invalid color: " + text);
		}
		byte r = Convert.ToByte(text.Substring(0, 2), 16);
		byte g = Convert.ToByte(text.Substring(2, 2), 16);
		byte b = Convert.ToByte(text.Substring(4, 2), 16);
		byte a = byte.MaxValue;
		if (text.Length == 8)
		{
			a = Convert.ToByte(text.Substring(6, 2), 16);
		}
		return new Color32(r, g, b, a);
	}

	public static Color CTHexToColor(this string hexString)
	{
		return hexString.CTHexToColor32();
	}

	public static byte[] CTToByteArray(this string str, Encoding encoding = null)
	{
		if (str == null)
		{
			return null;
		}
		Encoding encoding2 = Encoding.UTF8;
		if (encoding != null)
		{
			encoding2 = encoding;
		}
		return encoding2.GetBytes(str);
	}

	public static string CTClearTags(this string str)
	{
		if (str == null)
		{
			return null;
		}
		return BaseConstants.REGEX_CLEAN_TAGS.Replace(str, string.Empty).Trim();
	}

	public static string CTClearSpaces(this string str)
	{
		if (str == null)
		{
			return null;
		}
		return BaseConstants.REGEX_CLEAN_SPACES.Replace(str, " ").Trim();
	}

	public static string CTClearLineEndings(this string str)
	{
		if (str == null)
		{
			return null;
		}
		return BaseConstants.REGEX_LINEENDINGS.Replace(str, string.Empty).Trim();
	}

	public static void CTShuffle<T>(this T[] array, int seed = 0)
	{
		if (array == null || array.Length == 0)
		{
			throw new ArgumentNullException("array");
		}
		System.Random random = ((seed == 0) ? new System.Random() : new System.Random(seed));
		int num = array.Length;
		while (num > 1)
		{
			int num2 = random.Next(num--);
			int num3 = num;
			int num4 = num2;
			T val = array[num2];
			T val2 = array[num];
			array[num3] = val;
			array[num4] = val2;
		}
	}

	public static string CTDump<T>(this T[] array, string prefix = "", string postfix = "", bool appendNewLine = true, string delimiter = "; ")
	{
		if (array == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (T val in array)
		{
			if (0 < stringBuilder.Length)
			{
				stringBuilder.Append(appendNewLine ? Environment.NewLine : delimiter);
			}
			stringBuilder.Append(prefix);
			stringBuilder.Append(val);
			stringBuilder.Append(postfix);
		}
		return stringBuilder.ToString();
	}

	public static string CTDump(this Quaternion[] array)
	{
		if (array == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			Quaternion quaternion = array[i];
			if (0 < stringBuilder.Length)
			{
				stringBuilder.Append(Environment.NewLine);
			}
			stringBuilder.Append(quaternion.x);
			stringBuilder.Append(", ");
			stringBuilder.Append(quaternion.y);
			stringBuilder.Append(", ");
			stringBuilder.Append(quaternion.z);
			stringBuilder.Append(", ");
			stringBuilder.Append(quaternion.w);
		}
		return stringBuilder.ToString();
	}

	public static string CTDump(this Vector2[] array)
	{
		if (array == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			Vector2 vector = array[i];
			if (0 < stringBuilder.Length)
			{
				stringBuilder.Append(Environment.NewLine);
			}
			stringBuilder.Append(vector.x);
			stringBuilder.Append(", ");
			stringBuilder.Append(vector.y);
		}
		return stringBuilder.ToString();
	}

	public static string CTDump(this Vector3[] array)
	{
		if (array == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 vector = array[i];
			if (0 < stringBuilder.Length)
			{
				stringBuilder.Append(Environment.NewLine);
			}
			stringBuilder.Append(vector.x);
			stringBuilder.Append(", ");
			stringBuilder.Append(vector.y);
			stringBuilder.Append(", ");
			stringBuilder.Append(vector.z);
		}
		return stringBuilder.ToString();
	}

	public static string CTDump(this Vector4[] array)
	{
		if (array == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			Vector4 vector = array[i];
			if (0 < stringBuilder.Length)
			{
				stringBuilder.Append(Environment.NewLine);
			}
			stringBuilder.Append(vector.x);
			stringBuilder.Append(", ");
			stringBuilder.Append(vector.y);
			stringBuilder.Append(", ");
			stringBuilder.Append(vector.z);
			stringBuilder.Append(", ");
			stringBuilder.Append(vector.w);
		}
		return stringBuilder.ToString();
	}

	public static string[] CTToStringArray<T>(this T[] array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		string[] array2 = new string[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = ((array[i] == null) ? "null" : array[i].ToString());
		}
		return array2;
	}

	public static float[] CTToFloatArray(this byte[] array, int count = 0)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int num = count;
		if (num <= 0)
		{
			num = array.Length;
		}
		float[] array2 = new float[num / 2];
		int num2 = 0;
		for (int i = 0; i < num; i += 2)
		{
			array2[num2] = bytesToFloat(array[i], array[i + 1]);
			num2++;
		}
		return array2;
	}

	public static byte[] CTToByteArray(this float[] array, int count = 0)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int num = count;
		if (num <= 0)
		{
			num = array.Length;
		}
		byte[] array2 = new byte[num * 2];
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			short num3 = (short)(array[i] * 32767f);
			array2[num2] = (byte)(num3 & 0xFF);
			array2[num2 + 1] = (byte)((num3 >> 8) & 0xFF);
			num2 += 2;
		}
		return array2;
	}

	public static Texture2D CTToTexture(this byte[] data, Texture2D supportTexture = null)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		Texture2D texture2D = supportTexture;
		if (texture2D == null)
		{
			texture2D = new Texture2D(1, 1);
		}
		texture2D.LoadImage(data);
		return texture2D;
	}

	public static Sprite CTToSprite(this byte[] data, Texture2D supportTexture = null)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (supportTexture == null)
		{
			Texture2D texture2D = data.CTToTexture();
			return Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(texture2D.width / 2, texture2D.height / 2));
		}
		supportTexture = data.CTToTexture(supportTexture);
		return Sprite.Create(supportTexture, new Rect(0f, 0f, supportTexture.width, supportTexture.height), new Vector2(supportTexture.width / 2, supportTexture.height / 2));
	}

	public static string CTToString(this byte[] data, Encoding encoding = null)
	{
		if (data == null)
		{
			return null;
		}
		return (encoding ?? Encoding.UTF8).GetString(data);
	}

	public static string CTToBase64(this byte[] data)
	{
		if (data != null)
		{
			return Convert.ToBase64String(data);
		}
		return null;
	}

	public static T[] GetColumn<T>(this T[,] matrix, int columnNumber)
	{
		return (from x in Enumerable.Range(0, matrix.GetLength(0))
			select matrix[x, columnNumber]).ToArray();
	}

	public static T[] GetRow<T>(this T[,] matrix, int rowNumber)
	{
		return (from x in Enumerable.Range(0, matrix.GetLength(1))
			select matrix[rowNumber, x]).ToArray();
	}

	public static void CTShuffle<T>(this IList<T> list, int seed = 0)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		System.Random random = ((seed == 0) ? new System.Random() : new System.Random(seed));
		int count = list.Count;
		while (count > 1)
		{
			int num = random.Next(count--);
			int index = count;
			int index2 = num;
			T val = list[num];
			T val2 = list[count];
			T val3 = (list[index] = val);
			val3 = (list[index2] = val2);
		}
	}

	public static string CTDump<T>(this IList<T> list, string prefix = "", string postfix = "", bool appendNewLine = true, string delimiter = "; ")
	{
		if (list == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (T item in list)
		{
			if (0 < stringBuilder.Length)
			{
				stringBuilder.Append(appendNewLine ? Environment.NewLine : delimiter);
			}
			stringBuilder.Append(prefix);
			stringBuilder.Append(item);
			stringBuilder.Append(postfix);
		}
		return stringBuilder.ToString();
	}

	public static string CTDump(this IList<Quaternion> list)
	{
		if (list == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Quaternion item in list)
		{
			if (0 < stringBuilder.Length)
			{
				stringBuilder.Append(Environment.NewLine);
			}
			stringBuilder.Append(item.x);
			stringBuilder.Append(", ");
			stringBuilder.Append(item.y);
			stringBuilder.Append(", ");
			stringBuilder.Append(item.z);
			stringBuilder.Append(", ");
			stringBuilder.Append(item.w);
		}
		return stringBuilder.ToString();
	}

	public static string CTDump(this IList<Vector2> list)
	{
		if (list == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Vector2 item in list)
		{
			if (0 < stringBuilder.Length)
			{
				stringBuilder.Append(Environment.NewLine);
			}
			stringBuilder.Append(item.x);
			stringBuilder.Append(", ");
			stringBuilder.Append(item.y);
		}
		return stringBuilder.ToString();
	}

	public static string CTDump(this IList<Vector3> list)
	{
		if (list == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Vector3 item in list)
		{
			if (0 < stringBuilder.Length)
			{
				stringBuilder.Append(Environment.NewLine);
			}
			stringBuilder.Append(item.x);
			stringBuilder.Append(", ");
			stringBuilder.Append(item.y);
			stringBuilder.Append(", ");
			stringBuilder.Append(item.z);
		}
		return stringBuilder.ToString();
	}

	public static string CTDump(this IList<Vector4> list)
	{
		if (list == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Vector4 item in list)
		{
			if (0 < stringBuilder.Length)
			{
				stringBuilder.Append(Environment.NewLine);
			}
			stringBuilder.Append(item.x);
			stringBuilder.Append(", ");
			stringBuilder.Append(item.y);
			stringBuilder.Append(", ");
			stringBuilder.Append(item.z);
			stringBuilder.Append(", ");
			stringBuilder.Append(item.w);
		}
		return stringBuilder.ToString();
	}

	public static List<string> CTToString<T>(this IList<T> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		List<string> list2 = new List<string>(list.Count);
		list2.AddRange(list.Select((T element) => (element != null) ? element.ToString() : "null"));
		return list2;
	}

	public static string CTDump<K, V>(this IDictionary<K, V> dict, string prefix = "", string postfix = "", bool appendNewLine = true, string delimiter = "; ")
	{
		if (dict == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<K, V> item in dict)
		{
			if (0 < stringBuilder.Length)
			{
				stringBuilder.Append(appendNewLine ? Environment.NewLine : delimiter);
			}
			stringBuilder.Append(prefix);
			stringBuilder.Append("Key = ");
			stringBuilder.Append(item.Key);
			stringBuilder.Append(", Value = ");
			stringBuilder.Append(item.Value);
			stringBuilder.Append(postfix);
		}
		return stringBuilder.ToString();
	}

	public static void CTAddRange<K, V>(this IDictionary<K, V> dict, IDictionary<K, V> collection)
	{
		if (dict == null)
		{
			throw new ArgumentNullException("dict");
		}
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		foreach (KeyValuePair<K, V> item in collection)
		{
			if (!dict.ContainsKey(item.Key))
			{
				dict.Add(item.Key, item.Value);
			}
			else
			{
				Debug.LogWarning($"Duplicate key found: {item.Key}");
			}
		}
	}

	public static byte[] CTReadFully(this Stream input)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		using MemoryStream memoryStream = new MemoryStream();
		input.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	public static string CTToHexRGB(this Color32 input)
	{
		return ((Color)input).CTToHexRGB();
	}

	public static string CTToHexRGB(this Color input)
	{
		return ColorUtility.ToHtmlStringRGB(input);
	}

	public static string CTToHexRGBA(this Color32 input)
	{
		return ((Color)input).CTToHexRGBA();
	}

	public static string CTToHexRGBA(this Color input)
	{
		return ColorUtility.ToHtmlStringRGBA(input);
	}

	public static Vector3 CTVector3(this Color32 color)
	{
		return ((Color)color).CTVector3();
	}

	public static Vector3 CTVector3(this Color color)
	{
		return new Vector3(color.r, color.g, color.b);
	}

	public static Vector4 CTVector4(this Color32 color)
	{
		return ((Color)color).CTVector4();
	}

	public static Vector4 CTVector4(this Color color)
	{
		return new Vector4(color.r, color.g, color.b, color.a);
	}

	public static Vector2 CTMultiply(this Vector2 a, Vector2 b)
	{
		return new Vector2(a.x * b.x, a.y * b.y);
	}

	public static Vector3 CTMultiply(this Vector3 a, Vector3 b)
	{
		return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
	}

	public static Vector3 CTFlatten(this Vector3 a)
	{
		return a.CTMultiply(FLAT_VECTOR);
	}

	public static Quaternion CTQuaternion(this Vector3 eulerAngle)
	{
		return Quaternion.Euler(eulerAngle);
	}

	public static Color CTColorRGB(this Vector3 rgb, float alpha = 1f)
	{
		return new Color(Mathf.Clamp01(rgb.x), Mathf.Clamp01(rgb.y), Mathf.Clamp01(rgb.z), Mathf.Clamp01(alpha));
	}

	public static Vector4 CTMultiply(this Vector4 a, Vector4 b)
	{
		return new Vector4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
	}

	public static Quaternion CTQuaternion(this Vector4 angle)
	{
		return new Quaternion(angle.x, angle.y, angle.z, angle.w);
	}

	public static Color CTColorRGBA(this Vector4 rgba)
	{
		return new Color(Mathf.Clamp01(rgba.x), Mathf.Clamp01(rgba.y), Mathf.Clamp01(rgba.z), Mathf.Clamp01(rgba.w));
	}

	public static Vector3 CTVector3(this Quaternion angle)
	{
		return angle.eulerAngles;
	}

	public static Vector4 CTVector4(this Quaternion angle)
	{
		return new Vector4(angle.x, angle.y, angle.z, angle.w);
	}

	public static Vector3 CTCorrectLossyScale(this Canvas canvas)
	{
		if (canvas == null)
		{
			throw new ArgumentNullException("canvas");
		}
		if (!Application.isPlaying)
		{
			return Vector3.one;
		}
		if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
		{
			CanvasScaler component = canvas.GetComponent<CanvasScaler>();
			if ((bool)component && component.enabled)
			{
				component.enabled = false;
				Vector3 lossyScale = canvas.GetComponent<RectTransform>().lossyScale;
				component.enabled = true;
				Vector3 lossyScale2 = canvas.GetComponent<RectTransform>().lossyScale;
				return new Vector3(lossyScale2.x / lossyScale.x, lossyScale2.y / lossyScale.y, lossyScale2.z / lossyScale.z);
			}
			return Vector3.one;
		}
		return canvas.GetComponent<RectTransform>().lossyScale;
	}

	public static void CTGetLocalCorners(this RectTransform transform, Vector3[] fourCornersArray, Canvas canvas, float inset = 0f, bool corrected = false)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		if (fourCornersArray == null)
		{
			throw new ArgumentNullException("fourCornersArray");
		}
		if (canvas == null)
		{
			throw new ArgumentNullException("canvas");
		}
		transform.GetLocalCorners(fourCornersArray);
		if (corrected)
		{
			Vector3 vector = canvas.CTCorrectLossyScale();
			fourCornersArray[0].x /= vector.x;
			fourCornersArray[0].y /= vector.y;
			fourCornersArray[1].x /= vector.x;
			fourCornersArray[1].y /= vector.y;
			fourCornersArray[2].x /= vector.x;
			fourCornersArray[2].y /= vector.y;
			fourCornersArray[3].x /= vector.x;
			fourCornersArray[3].y /= vector.y;
		}
		if (inset != 0f)
		{
			Vector3 vector2 = canvas.CTCorrectLossyScale();
			fourCornersArray[0].x += inset * vector2.x;
			fourCornersArray[0].y += inset * vector2.y;
			fourCornersArray[1].x += inset * vector2.x;
			fourCornersArray[1].y -= inset * vector2.y;
			fourCornersArray[2].x -= inset * vector2.x;
			fourCornersArray[2].y -= inset * vector2.y;
			fourCornersArray[3].x -= inset * vector2.x;
			fourCornersArray[3].y += inset * vector2.y;
		}
	}

	public static Vector3[] CTGetLocalCorners(this RectTransform transform, Canvas canvas, float inset = 0f, bool corrected = false)
	{
		Vector3[] array = new Vector3[4];
		transform.CTGetLocalCorners(array, canvas, inset, corrected);
		return array;
	}

	public static void CTGetScreenCorners(this RectTransform transform, Vector3[] fourCornersArray, Canvas canvas, float inset = 0f, bool corrected = false)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		if (fourCornersArray == null)
		{
			throw new ArgumentNullException("fourCornersArray");
		}
		if (canvas == null)
		{
			throw new ArgumentNullException("canvas");
		}
		transform.GetWorldCorners(fourCornersArray);
		if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
		{
			for (int i = 0; i < 4; i++)
			{
				fourCornersArray[i] = canvas.worldCamera.WorldToScreenPoint(fourCornersArray[i]);
				fourCornersArray[i].z = 0f;
			}
		}
		if (corrected)
		{
			Vector3 vector = canvas.CTCorrectLossyScale();
			fourCornersArray[0].x /= vector.x;
			fourCornersArray[0].y /= vector.y;
			fourCornersArray[1].x /= vector.x;
			fourCornersArray[1].y /= vector.y;
			fourCornersArray[2].x /= vector.x;
			fourCornersArray[2].y /= vector.y;
			fourCornersArray[3].x /= vector.x;
			fourCornersArray[3].y /= vector.y;
		}
		if (inset != 0f)
		{
			Vector3 vector2 = canvas.CTCorrectLossyScale();
			fourCornersArray[0].x += inset * vector2.x;
			fourCornersArray[0].y += inset * vector2.y;
			fourCornersArray[1].x += inset * vector2.x;
			fourCornersArray[1].y -= inset * vector2.y;
			fourCornersArray[2].x -= inset * vector2.x;
			fourCornersArray[2].y -= inset * vector2.y;
			fourCornersArray[3].x -= inset * vector2.x;
			fourCornersArray[3].y += inset * vector2.y;
		}
	}

	public static Vector3[] CTGetScreenCorners(this RectTransform transform, Canvas canvas, float inset = 0f, bool corrected = false)
	{
		Vector3[] array = new Vector3[4];
		transform.CTGetScreenCorners(array, canvas, inset, corrected);
		return array;
	}

	public static Bounds CTGetBounds(this RectTransform transform, float uiScaleFactor = 1f)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		Vector3 center = transform.anchoredPosition;
		Rect rect2;
		Rect rect = (rect2 = transform.rect);
		Bounds result = new Bounds(center, new Vector3(rect.width, rect2.height, 0f) * uiScaleFactor);
		if (transform.childCount > 0)
		{
			foreach (Bounds item in from RectTransform child in transform
				select new Bounds(child.anchoredPosition, new Vector3(child.rect.width, child.rect.height, 0f) * uiScaleFactor))
			{
				result.Encapsulate(item);
			}
		}
		return result;
	}

	public static void CTSetLeft(this RectTransform transform, float value)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		transform.offsetMin = new Vector2(value, transform.offsetMin.y);
	}

	public static void CTSetRight(this RectTransform transform, float value)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		transform.offsetMax = new Vector2(value, transform.offsetMax.y);
	}

	public static void CTSetTop(this RectTransform transform, float value)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		transform.offsetMax = new Vector2(transform.offsetMax.x, value);
	}

	public static void CTSetBottom(this RectTransform transform, float value)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		transform.offsetMin = new Vector2(transform.offsetMin.x, value);
	}

	public static float CTGetLeft(this RectTransform transform)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		return transform.offsetMin.x;
	}

	public static float CTGetRight(this RectTransform transform)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		return transform.offsetMax.x;
	}

	public static float CTGetTop(this RectTransform transform)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		return transform.offsetMax.y;
	}

	public static float CTGetBottom(this RectTransform transform)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		return transform.offsetMin.y;
	}

	public static Vector4 CTGetLRTB(this RectTransform transform)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		float x = transform.offsetMin.x;
		Vector2 offsetMax;
		Vector2 vector = (offsetMax = transform.offsetMax);
		return new Vector4(x, vector.x, offsetMax.y, transform.offsetMin.y);
	}

	public static void CTSetLRTB(this RectTransform transform, Vector4 lrtb)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		transform.offsetMin = new Vector2(lrtb.x, lrtb.w);
		transform.offsetMax = new Vector2(lrtb.y, lrtb.z);
	}

	public static List<GameObject> CTFindAll(this Component component, string name, int maxDepth = 0)
	{
		if (component == null)
		{
			throw new ArgumentNullException("component");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		List<GameObject> list = new List<GameObject>();
		foreach (Transform allChild in component.transform.getAllChildren(maxDepth))
		{
			list.Add(allChild.gameObject);
		}
		return list.Where((GameObject child) => child.name == name).ToList();
	}

	public static List<T> CTFindAll<T>(this Component component, string name) where T : Component
	{
		if (component == null)
		{
			throw new ArgumentNullException("component");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return (from child in component.GetComponentsInChildren<T>()
			where child.name == name
			select child).ToList();
	}

	public static GameObject CTFind(this MonoBehaviour mb, string name)
	{
		if (mb == null)
		{
			throw new ArgumentNullException("mb");
		}
		return mb.transform.CTFind(name).gameObject;
	}

	public static T CTFind<T>(this MonoBehaviour mb, string name)
	{
		if (mb == null)
		{
			throw new ArgumentNullException("mb");
		}
		return mb.transform.CTFind<T>(name);
	}

	public static GameObject CTFind(this GameObject go, string name)
	{
		if (go == null)
		{
			throw new ArgumentNullException("go");
		}
		return go.transform.CTFind(name).gameObject;
	}

	public static T CTFind<T>(this GameObject go, string name)
	{
		if (go == null)
		{
			throw new ArgumentNullException("go");
		}
		return go.transform.CTFind<T>(name);
	}

	public static Bounds CTGetBounds(this GameObject go)
	{
		if (go == null)
		{
			throw new ArgumentNullException("go");
		}
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
		if (componentsInChildren.Length == 0)
		{
			return new Bounds(go.transform.position, Vector3.zero);
		}
		Bounds bounds = componentsInChildren[0].bounds;
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			bounds.Encapsulate(renderer.bounds);
		}
		return bounds;
	}

	public static Transform CTFind(this Transform transform, string name)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return deepSearch(transform, name);
	}

	public static T CTFind<T>(this Transform transform, string name)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		Transform transform2 = transform.CTFind(name);
		if (!(transform2 != null))
		{
			return default(T);
		}
		return transform2.gameObject.GetComponent<T>();
	}

	public static byte[] CTToPNG(this Sprite sprite)
	{
		if (sprite == null)
		{
			throw new ArgumentNullException("sprite");
		}
		return sprite.texture.CTToPNG();
	}

	public static byte[] CTToJPG(this Sprite sprite)
	{
		if (sprite == null)
		{
			throw new ArgumentNullException("sprite");
		}
		return sprite.texture.CTToJPG();
	}

	public static byte[] CTToTGA(this Sprite sprite)
	{
		if (sprite == null)
		{
			throw new ArgumentNullException("sprite");
		}
		return sprite.texture.CTToTGA();
	}

	public static byte[] CTToEXR(this Sprite sprite)
	{
		if (sprite == null)
		{
			throw new ArgumentNullException("sprite");
		}
		return sprite.texture.CTToEXR();
	}

	public static byte[] CTToPNG(this Texture2D texture)
	{
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		return texture.EncodeToPNG();
	}

	public static byte[] CTToJPG(this Texture2D texture)
	{
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		return texture.EncodeToJPG();
	}

	public static byte[] CTToTGA(this Texture2D texture)
	{
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		return texture.EncodeToTGA();
	}

	public static byte[] CTToEXR(this Texture2D texture)
	{
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		return texture.EncodeToEXR();
	}

	public static Sprite CTToSprite(this Texture2D texture, float pixelsPerUnit = 100f)
	{
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
	}

	public static Texture2D CTRotate90(this Texture2D texture)
	{
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		Color32[] pixels = texture.GetPixels32(0);
		Color32[] array = new Color32[texture.width * texture.height];
		for (int i = 0; i < texture.height; i++)
		{
			for (int j = 0; j < texture.width; j++)
			{
				array[texture.width * texture.height - (texture.height * j + texture.height) + i] = pixels[texture.width * texture.height - (texture.width * i + texture.width) + j];
			}
		}
		Texture2D texture2D = new Texture2D(texture.height, texture.width, texture.format, mipChain: false);
		texture2D.SetPixels32(array, 0);
		texture2D.Apply();
		return texture2D;
	}

	public static Texture2D CTRotate180(this Texture2D texture)
	{
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		Color32[] pixels = texture.GetPixels32(0);
		Color32[] array = new Color32[texture.width * texture.height];
		for (int i = 0; i < pixels.Length; i++)
		{
			array[pixels.Length - i - 1] = pixels[i];
		}
		Texture2D texture2D = new Texture2D(texture.width, texture.height, texture.format, mipChain: false);
		texture2D.SetPixels32(array, 0);
		texture2D.Apply();
		return texture2D;
	}

	public static Texture2D CTRotate270(this Texture2D texture)
	{
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		Color32[] pixels = texture.GetPixels32(0);
		Color32[] array = new Color32[texture.width * texture.height];
		int num = 0;
		for (int i = 0; i < texture.height; i++)
		{
			for (int j = 0; j < texture.width; j++)
			{
				array[texture.width * texture.height - (texture.height * j + texture.height) + i] = pixels[num];
				num++;
			}
		}
		Texture2D texture2D = new Texture2D(texture.height, texture.width, texture.format, mipChain: false);
		texture2D.SetPixels32(array, 0);
		texture2D.Apply();
		return texture2D;
	}

	public static Texture2D CTToTexture2D(this Texture texture)
	{
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		return Texture2D.CreateExternalTexture(texture.width, texture.height, TextureFormat.RGB24, mipChain: false, linear: false, texture.GetNativeTexturePtr());
	}

	public static Texture2D CTFlipHorizontal(this Texture2D texture)
	{
		Texture2D texture2D = new Texture2D(texture.width, texture.height);
		int width = texture.width;
		int height = texture.height;
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				texture2D.SetPixel(width - i - 1, j, texture.GetPixel(i, j));
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	public static Texture2D CTFlipVertical(this Texture2D texture)
	{
		Texture2D texture2D = new Texture2D(texture.width, texture.height);
		int width = texture.width;
		int height = texture.height;
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				texture2D.SetPixel(i, height - j - 1, texture.GetPixel(i, j));
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	public static bool CTHasActiveClip(this AudioSource source)
	{
		if (source == null)
		{
			return false;
		}
		if (source.clip == null)
		{
			return false;
		}
		bool loop;
		if (!source.isPlaying && !(loop = source.loop))
		{
			int timeSamples;
			if (!loop && (timeSamples = source.timeSamples) > 0)
			{
				return timeSamples < source.clip.samples - 1024;
			}
			return false;
		}
		return true;
	}

	public static void CTAbort(this Thread thread, bool silent = true)
	{
		if (thread == null || !thread.IsAlive)
		{
			return;
		}
		try
		{
			thread.Abort();
		}
		catch (Exception message)
		{
			if (!silent)
			{
				Debug.LogWarning(message);
			}
		}
	}

	public static bool CTIsVisibleFrom(this Renderer renderer, Camera camera)
	{
		if (renderer == null)
		{
			throw new ArgumentNullException("renderer");
		}
		if (camera == null)
		{
			throw new ArgumentNullException("camera");
		}
		return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(camera), renderer.bounds);
	}

	private static Transform deepSearch(Transform parent, string name)
	{
		Transform transform = parent.Find(name);
		if (transform != null)
		{
			return transform;
		}
		foreach (Transform item in parent)
		{
			transform = deepSearch(item, name);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	private static List<Transform> getAllChildren(this Transform parent, int maxDepth = 0, List<Transform> transformList = null, int depth = 0)
	{
		if (transformList == null)
		{
			transformList = new List<Transform>();
		}
		if (maxDepth != 0)
		{
			depth++;
		}
		if (depth <= maxDepth)
		{
			foreach (Transform item in parent)
			{
				transformList.Add(item);
				item.getAllChildren(maxDepth, transformList, depth);
			}
		}
		return transformList;
	}

	private static float bytesToFloat(byte firstByte, byte secondByte)
	{
		return (float)(short)((secondByte << 8) | firstByte) / 32768f;
	}
}
