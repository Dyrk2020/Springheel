using System;
using Org.BouncyCastle.Crypto.Tls;

namespace GameSparks.RT;

public static class GSTlsVerifyCertificate
{
	public static Func<Certificate, string> OnVerifyCertificate { get; set; }
}
