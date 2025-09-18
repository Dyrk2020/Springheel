namespace nn.hid;

public struct AnalogStickState
{
	public const int Max = 32767;

	public int x;

	public int y;

	public float fx => (float)x / 32767f;

	public float fy => (float)y / 32767f;

	public void Clear()
	{
		x = 0;
		y = 0;
	}

	public override string ToString()
	{
		return $"({x,6} {y,6})";
	}
}
