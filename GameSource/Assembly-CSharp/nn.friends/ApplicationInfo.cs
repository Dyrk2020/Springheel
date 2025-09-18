namespace nn.friends;

public struct ApplicationInfo
{
	public ulong appId;

	public ulong presenceGroupId;

	public override string ToString()
	{
		return $"{appId} {presenceGroupId}";
	}
}
