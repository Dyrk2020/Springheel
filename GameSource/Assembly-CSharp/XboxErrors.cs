using System.Collections.Generic;
using System.Reflection;

public static class XboxErrors
{
	public const uint SUCCESS = 0u;

	public const uint GENERIC_ERROR = 1u;

	public const uint E_FAIL = 2147500037u;

	public const uint E_INVALIDARG = 2147942487u;

	public const uint E_PROP_ID_UNSUPPORTED = 2147943568u;

	public const uint ERROR_INTERNET_TIMEOUT = 2147954402u;

	public const uint INET_E_RESOURCE_NOT_FOUND = 2148270085u;

	public const uint HTTP_E_STATUS_BAD_REQUEST = 2149122448u;

	public const uint HTTP_E_STATUS_FORBIDDEN = 2149122451u;

	public const uint HTTP_E_STATUS_PRECOND_FAILED = 2149122460u;

	public const uint InvalidContainerName = 2156068865u;

	public const uint NoAccess = 2156068866u;

	public const uint OutOfLocalStorage = 2156068867u;

	public const uint UserCanceled = 2156068868u;

	public const uint UpdateTooBig = 2156068869u;

	public const uint QuotaExceeded = 2156068870u;

	public const uint ProvidedBufferTooSmall = 2156068871u;

	public const uint BlobNotFound = 2156068872u;

	public const uint NoXboxLiveInfo = 2156068873u;

	public const uint ContainerNotInSync = 2156068874u;

	public const uint ContainerSyncFailed = 2156068875u;

	public const uint WEB_E_INVALID_JSON_STRING = 2205483015u;

	public const uint PARTY_InternalError = 2278293505u;

	public const uint PARTY_BadToken = 2278293506u;

	public const uint PARTY_InvalidSecureDeviceAddress = 2278293509u;

	public const uint PARTY_EmptyParty = 2278293511u;

	public const uint PARTY_PartyActionRestricted = 2278293512u;

	public const uint PARTY_PartyAlreadyInTitle = 2278293513u;

	public const uint PARTY_QualityOfServiceFailed = 2278293514u;

	public const uint PARTY_PartyJoinFailure = 2278293515u;

	public const uint AM_E_XSTS_TIMEOUT = 2279407641u;

	private static bool reflected;

	private static Dictionary<uint, string> errors;

	public static string GetErrorFromNumber(uint hresult)
	{
		if (!reflected)
		{
			errors = new Dictionary<uint, string>();
			FieldInfo[] fields = typeof(XboxErrors).GetFields(BindingFlags.Static | BindingFlags.Public);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.IsLiteral && !fieldInfo.IsInitOnly && fieldInfo.FieldType == typeof(uint))
				{
					uint key = (uint)fieldInfo.GetRawConstantValue();
					errors.Add(key, fieldInfo.Name);
				}
			}
			reflected = true;
		}
		if (errors.ContainsKey(hresult))
		{
			return errors[hresult];
		}
		return "UNKNOWN";
	}
}
