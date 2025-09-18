namespace Moserware.Skills.FactorGraphs;

public class Message<T>
{
	private readonly string _NameFormat;

	private readonly object[] _NameFormatArgs;

	public T Value { get; set; }

	public Message()
		: this(default(T), (string)null, (object[])null)
	{
	}

	public Message(T value, string nameFormat, params object[] args)
	{
		_NameFormat = nameFormat;
		_NameFormatArgs = args;
		Value = value;
	}

	public override string ToString()
	{
		if (_NameFormat != null)
		{
			return string.Format(_NameFormat, _NameFormatArgs);
		}
		return base.ToString();
	}
}
