using System.Text;

public static class Util_String
{
	public static bool NullOrEmpty(this string str)
	{
		if (str != null)
		{
			return str.Length == 0;
		}
		return true;
	}

	public static string CompactWhitespaces(this string s)
	{
		StringBuilder stringBuilder = new StringBuilder(s);
		CompactWhitespaces(stringBuilder);
		return stringBuilder.ToString();
	}

	private static void CompactWhitespaces(StringBuilder sb)
	{
		if (sb.Length == 0)
		{
			return;
		}
		int i;
		for (i = 0; i < sb.Length && char.IsWhiteSpace(sb[i]); i++)
		{
		}
		if (i == sb.Length)
		{
			sb.Length = 0;
			return;
		}
		int num = sb.Length - 1;
		while (num >= 0 && char.IsWhiteSpace(sb[num]))
		{
			num--;
		}
		int num2 = 0;
		bool flag = false;
		for (int j = i; j <= num; j++)
		{
			if (char.IsWhiteSpace(sb[j]))
			{
				if (!flag)
				{
					flag = true;
					sb[num2] = ' ';
					num2++;
				}
			}
			else
			{
				flag = false;
				sb[num2] = sb[j];
				num2++;
			}
		}
		sb.Length = num2;
	}
}
