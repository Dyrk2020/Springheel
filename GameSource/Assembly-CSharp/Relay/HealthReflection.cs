using System;
using Google.Protobuf.Reflection;

namespace Relay;

public static class HealthReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static HealthReflection()
	{
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CgxoZWFsdGgucHJvdG8SBVJlbGF5Ig0KC1BpbmdSZXF1ZXN0Ig4KDFBpbmdS" + "ZXNwb25zZSIOCgxHZXRJcFJlcXVlc3QiGwoNR2V0SXBSZXNwb25zZRIKCgJp" + "cBgBIAEoCWIGcHJvdG8z"), new FileDescriptor[0], new GeneratedClrTypeInfo(null, new GeneratedClrTypeInfo[4]
		{
			new GeneratedClrTypeInfo(typeof(PingRequest), PingRequest.Parser, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PingResponse), PingResponse.Parser, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetIpRequest), GetIpRequest.Parser, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetIpResponse), GetIpResponse.Parser, new string[1] { "Ip" }, null, null, null)
		}));
	}
}
