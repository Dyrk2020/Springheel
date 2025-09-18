using System;
using UnityEngine.Events;

namespace Crosstales.BWF;

[Serializable]
public class OnGetAllCompleted : UnityEvent<string, string>
{
}
