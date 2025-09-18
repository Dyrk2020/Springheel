namespace Moserware.Skills;

public class Player<T> : ISupportPartialPlay, ISupportPartialUpdate
{
	private const double DefaultPartialPlayPercentage = 1.0;

	private const double DefaultPartialUpdatePercentage = 1.0;

	private readonly T _Id;

	private readonly double _PartialPlayPercentage;

	private readonly double _PartialUpdatePercentage;

	public T Id => _Id;

	public double PartialPlayPercentage => _PartialPlayPercentage;

	public double PartialUpdatePercentage => _PartialUpdatePercentage;

	public Player(T id)
		: this(id, 1.0, 1.0)
	{
	}

	public Player(T id, double partialPlayPercentage)
		: this(id, partialPlayPercentage, 1.0)
	{
	}

	public Player(T id, double partialPlayPercentage, double partialUpdatePercentage)
	{
		Guard.ArgumentInRangeInclusive(partialPlayPercentage, 0.0, 1.0, "partialPlayPercentage");
		Guard.ArgumentInRangeInclusive(partialUpdatePercentage, 0.0, 1.0, "partialUpdatePercentage");
		_Id = id;
		_PartialPlayPercentage = partialPlayPercentage;
		_PartialUpdatePercentage = partialUpdatePercentage;
	}

	public override string ToString()
	{
		if (Id != null)
		{
			return Id.ToString();
		}
		return base.ToString();
	}
}
public class Player : Player<object>
{
	public Player(object id)
		: base(id)
	{
	}

	public Player(object id, double partialPlayPercentage)
		: base(id, partialPlayPercentage)
	{
	}

	public Player(object id, double partialPlayPercentage, double partialUpdatePercentage)
		: base(id, partialPlayPercentage, partialUpdatePercentage)
	{
	}
}
