using System;
using System.Collections.Generic;
using System.Text;
using Crosstales.BWF.Data;
using Crosstales.Common.Util;

namespace Crosstales.BWF.Model;

[Serializable]
public class Domains
{
	public Source Source;

	public List<string> DomainList = new List<string>();

	public Domains(Source source, IEnumerable<string> domainList)
	{
		Source = source;
		foreach (string domain in domainList)
		{
			DomainList.Add(domain.Split('#')[0]);
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
		stringBuilder.Append("DomainList='");
		stringBuilder.Append(DomainList.Count);
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
