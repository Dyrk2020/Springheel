using System;
using System.Collections.Generic;
using System.Text;
using Crosstales.BWF.Data;
using Crosstales.Common.Util;

namespace Crosstales.BWF.Model;

[Serializable]
public class BadWords
{
	public Source Source;

	public List<string> BadWordList = new List<string>();

	public BadWords(Source source, IEnumerable<string> badWordList)
	{
		Source = source;
		foreach (string badWord in badWordList)
		{
			BadWordList.Add(badWord.Split('#')[0]);
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(GetType().Name);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_START);
		stringBuilder.Append("Source='");
		stringBuilder.Append(Source);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("BadWordList='");
		stringBuilder.Append(BadWordList.Count);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER_END);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_END);
		return stringBuilder.ToString();
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		BadWords badWords = (BadWords)obj;
		if (!(Source == null))
		{
			return Source.Equals(badWords.Source);
		}
		return true;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
