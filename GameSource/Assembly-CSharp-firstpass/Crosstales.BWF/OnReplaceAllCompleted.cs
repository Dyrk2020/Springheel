using System;
using UnityEngine.Events;

namespace Crosstales.BWF;

[Serializable]
public class OnReplaceAllCompleted : UnityEvent<string, string>
{
}
