using System;
using Google.Protobuf.Reflection;

namespace Relay;

public static class ServiceErrorReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static ServiceErrorReflection()
	{
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChNzZXJ2aWNlLWVycm9yLnByb3RvEgVSZWxheSIvCgxTZXJ2aWNlRXJyb3IS" + "EQoJZXJyb3JDb2RlGAEgASgFEgwKBHBhdGgYAiABKAliBnByb3RvMw=="), new FileDescriptor[0], new GeneratedClrTypeInfo(null, new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ServiceError), ServiceError.Parser, new string[2] { "ErrorCode", "Path" }, null, null, null)
		}));
	}
}
