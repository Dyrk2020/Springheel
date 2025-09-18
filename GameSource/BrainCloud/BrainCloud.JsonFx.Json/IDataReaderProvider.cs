namespace BrainCloud.JsonFx.Json;

public interface IDataReaderProvider
{
	IDataReader Find(string contentTypeHeader);
}
