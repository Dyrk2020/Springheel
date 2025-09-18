using System;
using Google.Protobuf.Reflection;

namespace Relay;

public static class RelayReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static RelayReflection()
	{
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CgtyZWxheS5wcm90bxIFUmVsYXkiJgoUR2V0R2FtZVNlcnZlclJlcXVlc3QS" + "DgoGZ2FtZUlkGAEgASgJIj0KFUdldEdhbWVTZXJ2ZXJSZXNwb25zZRIQCghz" + "ZXJ2ZXJJcBgBIAEoCRISCgpzZXJ2ZXJQb3J0GAIgASgFIh8KHUdldE5leHRB" + "dmFpbGFibGVTZXJ2ZXJSZXF1ZXN0IkYKHkdldE5leHRBdmFpbGFibGVTZXJ2" + "ZXJSZXNwb25zZRIQCghzZXJ2ZXJJcBgBIAEoCRISCgpzZXJ2ZXJQb3J0GAIg" + "ASgFYgZwcm90bzM="), new FileDescriptor[0], new GeneratedClrTypeInfo(null, new GeneratedClrTypeInfo[4]
		{
			new GeneratedClrTypeInfo(typeof(GetGameServerRequest), GetGameServerRequest.Parser, new string[1] { "GameId" }, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetGameServerResponse), GetGameServerResponse.Parser, new string[2] { "ServerIp", "ServerPort" }, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetNextAvailableServerRequest), GetNextAvailableServerRequest.Parser, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetNextAvailableServerResponse), GetNextAvailableServerResponse.Parser, new string[2] { "ServerIp", "ServerPort" }, null, null, null)
		}));
	}
}
