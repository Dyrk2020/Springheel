using System;
using System.Runtime.InteropServices;
using System.Text;

namespace nn.friends;

public struct InAppScreenName
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
	private byte[] name;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
	private char[] language;

	public string Name
	{
		get
		{
			if (name == null)
			{
				return string.Empty;
			}
			return Encoding.UTF8.GetString(name);
		}
		set
		{
			if (name == null)
			{
				name = new byte[64];
			}
			string s = value[..Math.Min(value.Length, 20)];
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			bytes.CopyTo(name, 0);
			name[bytes.Length] = 0;
		}
	}

	public string Language
	{
		get
		{
			if (language == null)
			{
				return string.Empty;
			}
			int i;
			for (i = 0; i < 7 && language[i] != 0; i++)
			{
			}
			return new string(language, 0, i);
		}
		set
		{
			if (language == null)
			{
				language = new char[8];
			}
			int num = Math.Min(value.Length, 7);
			value.CopyTo(0, language, 0, num);
			language[num] = '\0';
		}
	}

	public InAppScreenName(string name, string language = "")
	{
		this.name = new byte[64];
		this.language = new char[8];
		Name = name;
		Language = language;
	}

	public override string ToString()
	{
		return $"name:{Name} language:{Language}";
	}
}
