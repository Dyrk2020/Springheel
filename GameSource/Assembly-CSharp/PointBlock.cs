public class PointBlock
{
	public enum pointBlockType
	{
		win,
		winDead,
		soloWin,
		first,
		trap,
		suicide,
		comeback,
		coin,
		second,
		third,
		fourth
	}

	public pointBlockType type;

	public int playerNumber;

	public string SFXEventName;

	public int suicideValue;

	public Placeable causedBy;

	public bool AlwaysAward => GameSettings.GetInstance().AlwaysAwardPointType(type);

	public int pointValue => GameSettings.GetInstance().PointTypeValue(type);

	public PointBlock(pointBlockType _type, int _playerNumber)
	{
		type = _type;
		playerNumber = _playerNumber;
	}
}
