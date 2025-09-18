using System;
using UnityEngine.Events;

namespace Crosstales.BWF;

[Serializable]
public class OnContainsCompleted : UnityEvent<string, bool>
{
}
