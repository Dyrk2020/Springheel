using System;
using System.Reflection;

namespace BrainCloud.LitJson;

internal struct PropertyMetadata
{
	public MemberInfo Info;

	public bool IsField;

	public Type Type;
}
