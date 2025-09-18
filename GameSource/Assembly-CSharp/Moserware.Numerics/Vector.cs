using System.Collections.Generic;

namespace Moserware.Numerics;

internal class Vector : Matrix
{
	public Vector(IList<double> vectorValues)
		: base(vectorValues.Count, 1, new IEnumerable<double>[1] { vectorValues })
	{
	}
}
