using System;
using Google.Protobuf.Reflection;

namespace Relay;

public static class GameServerReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static GameServerReflection()
	{
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChFnYW1lLXNlcnZlci5wcm90bxIFUmVsYXkiPwobUmVtb3ZlR2FtZUZyb21T" + "ZXJ2ZXJSZXF1ZXN0Eg4KBmdhbWVJZBgBIAEoCRIQCghzZXJ2ZXJJZBgCIAEo" + "CSIeChxSZW1vdmVHYW1lRnJvbVNlcnZlclJlc3BvbnNlIjcKE1JlZ2lzdGVy" + "R2FtZVJlcXVlc3QSDgoGZ2FtZUlkGAEgASgJEhAKCHNlcnZlcklkGAIgASgJ" + "IhYKFFJlZ2lzdGVyR2FtZVJlc3BvbnNlYgZwcm90bzM="), new FileDescriptor[0], new GeneratedClrTypeInfo(null, new GeneratedClrTypeInfo[4]
		{
			new GeneratedClrTypeInfo(typeof(RemoveGameFromServerRequest), RemoveGameFromServerRequest.Parser, new string[2] { "GameId", "ServerId" }, null, null, null),
			new GeneratedClrTypeInfo(typeof(RemoveGameFromServerResponse), RemoveGameFromServerResponse.Parser, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(RegisterGameRequest), RegisterGameRequest.Parser, new string[2] { "GameId", "ServerId" }, null, null, null),
			new GeneratedClrTypeInfo(typeof(RegisterGameResponse), RegisterGameResponse.Parser, null, null, null, null)
		}));
	}
}
