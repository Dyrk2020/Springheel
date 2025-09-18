using System.Collections.Generic;
using Crosstales.BWF.Data;

namespace Crosstales.BWF.Provider;

public interface IProvider
{
	bool isReady { get; set; }

	void Load();

	void Save();

	List<string> Verify(Source source);
}
