using System;
using System.Runtime.InteropServices;
using System.Text;

namespace nn.friends;

public struct FriendInvitationGameModeDescription
{
	private enum Language
	{
		EnUs,
		EnGb,
		Ja,
		Fr,
		De,
		Es419,
		Es,
		It,
		Nl,
		FrCa,
		Pt,
		Ru,
		ZhHans,
		ZhHant,
		Ko,
		ptBr,
		Length
	}

	public const int TextSize = 192;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3072)]
	private byte[] data;

	public string enUs
	{
		get
		{
			return Get(Language.EnUs);
		}
		set
		{
			Set(value, Language.EnUs);
		}
	}

	public string enGb
	{
		get
		{
			return Get(Language.EnGb);
		}
		set
		{
			Set(value, Language.EnGb);
		}
	}

	public string ja
	{
		get
		{
			return Get(Language.Ja);
		}
		set
		{
			Set(value, Language.Ja);
		}
	}

	public string fr
	{
		get
		{
			return Get(Language.Fr);
		}
		set
		{
			Set(value, Language.Fr);
		}
	}

	public string de
	{
		get
		{
			return Get(Language.De);
		}
		set
		{
			Set(value, Language.De);
		}
	}

	public string es419
	{
		get
		{
			return Get(Language.Es419);
		}
		set
		{
			Set(value, Language.Es419);
		}
	}

	public string es
	{
		get
		{
			return Get(Language.Es);
		}
		set
		{
			Set(value, Language.Es);
		}
	}

	public string it
	{
		get
		{
			return Get(Language.It);
		}
		set
		{
			Set(value, Language.It);
		}
	}

	public string nl
	{
		get
		{
			return Get(Language.Nl);
		}
		set
		{
			Set(value, Language.Nl);
		}
	}

	public string frCa
	{
		get
		{
			return Get(Language.FrCa);
		}
		set
		{
			Set(value, Language.FrCa);
		}
	}

	public string pt
	{
		get
		{
			return Get(Language.Pt);
		}
		set
		{
			Set(value, Language.Pt);
		}
	}

	public string ru
	{
		get
		{
			return Get(Language.Ru);
		}
		set
		{
			Set(value, Language.Ru);
		}
	}

	public string zhHans
	{
		get
		{
			return Get(Language.ZhHans);
		}
		set
		{
			Set(value, Language.ZhHans);
		}
	}

	public string zhHant
	{
		get
		{
			return Get(Language.ZhHant);
		}
		set
		{
			Set(value, Language.ZhHant);
		}
	}

	public string ko
	{
		get
		{
			return Get(Language.Ko);
		}
		set
		{
			Set(value, Language.Ko);
		}
	}

	public string ptBr
	{
		get
		{
			return Get(Language.ptBr);
		}
		set
		{
			Set(value, Language.ptBr);
		}
	}

	private string Get(Language language)
	{
		if (data == null)
		{
			return string.Empty;
		}
		return Encoding.UTF8.GetString(data, (int)language * 192, 192);
	}

	private void Set(string value, Language language)
	{
		if (data == null)
		{
			data = new byte[3072];
		}
		byte[] bytes = Encoding.UTF8.GetBytes(value);
		int num = Math.Min(bytes.Length, 191);
		Array.Copy(bytes, 0L, data, (int)language * 192, num);
		data[(int)language * 192 + bytes.Length] = 0;
	}
}
