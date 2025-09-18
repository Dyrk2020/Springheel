using System.Collections.Generic;

namespace BrainCloud.ModernHttpClient;

public class TLSConfig
{
	public List<Pin> Pins { get; set; }

	public ClientCertificate ClientCertificate { get; set; }

	public bool DangerousAcceptAnyServerCertificateValidator { get; set; }

	public bool DangerousAllowInsecureHTTPLoads { get; set; }
}
