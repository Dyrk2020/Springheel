using System;
using UnityEngine;

namespace Dreamteck.Splines;

[ExecuteInEditMode]
[AddComponentMenu("Dreamteck/Splines/Node Connector")]
public class Node : MonoBehaviour
{
	[Serializable]
	public class Connection
	{
		public bool invertTangents;

		[SerializeField]
		private int _pointIndex;

		[SerializeField]
		private SplineComputer _computer;

		[HideInInspector]
		[SerializeField]
		internal SplinePoint point;

		public SplineComputer spline => _computer;

		public int pointIndex => _pointIndex;

		internal bool isValid
		{
			get
			{
				if (_computer == null)
				{
					return false;
				}
				if (_pointIndex >= _computer.pointCount)
				{
					return false;
				}
				return true;
			}
		}

		internal Connection(SplineComputer comp, int index, SplinePoint inputPoint)
		{
			_pointIndex = index;
			_computer = comp;
			point = inputPoint;
		}
	}

	public enum Type
	{
		Smooth,
		Free
	}

	[HideInInspector]
	public Type type;

	[SerializeField]
	[HideInInspector]
	protected Connection[] connections = new Connection[0];

	[SerializeField]
	[HideInInspector]
	private bool _transformSize = true;

	[HideInInspector]
	[SerializeField]
	private bool _transformNormals = true;

	[HideInInspector]
	[SerializeField]
	private bool _transformTangents = true;

	private Vector3 lastPosition;

	private Vector3 lastScale;

	private Quaternion lastRotation;

	private Transform trs;

	public bool transformNormals
	{
		get
		{
			return _transformNormals;
		}
		set
		{
			if (value != _transformNormals)
			{
				_transformNormals = value;
				UpdatePoints();
			}
		}
	}

	public bool transformSize
	{
		get
		{
			return _transformSize;
		}
		set
		{
			if (value != _transformSize)
			{
				_transformSize = value;
				UpdatePoints();
			}
		}
	}

	public bool transformTangents
	{
		get
		{
			return _transformTangents;
		}
		set
		{
			if (value != _transformTangents)
			{
				_transformTangents = value;
				UpdatePoints();
			}
		}
	}

	private void Awake()
	{
		trs = base.transform;
		SampleTransform();
	}

	private void LateUpdate()
	{
		Run();
	}

	private void Update()
	{
		Run();
	}

	private bool TransformChanged()
	{
		if (!(lastPosition != trs.position) && !(lastRotation != trs.rotation))
		{
			return lastScale != trs.lossyScale;
		}
		return true;
	}

	private void SampleTransform()
	{
		lastPosition = trs.position;
		lastScale = trs.lossyScale;
		lastRotation = trs.rotation;
	}

	private void Run()
	{
		if (TransformChanged())
		{
			UpdateConnectedComputers();
			SampleTransform();
		}
	}

	public SplinePoint GetPoint(int connectionIndex, bool swapTangents)
	{
		SplinePoint result = PointToWorld(connections[connectionIndex].point);
		if (connections[connectionIndex].invertTangents && swapTangents)
		{
			Vector3 tangent = result.tangent;
			result.tangent = result.tangent2;
			result.tangent2 = tangent;
		}
		return result;
	}

	public void SetPoint(int connectionIndex, SplinePoint worldPoint, bool swappedTangents)
	{
		Connection connection = connections[connectionIndex];
		connection.point = PointToLocal(worldPoint);
		if (connection.invertTangents && swappedTangents)
		{
			Vector3 tangent = connection.point.tangent;
			connection.point.tangent = connection.point.tangent2;
			connection.point.tangent2 = tangent;
		}
		if (type != Type.Smooth)
		{
			return;
		}
		if (connection.point.type == SplinePoint.Type.SmoothFree)
		{
			for (int i = 0; i < connections.Length; i++)
			{
				if (i != connectionIndex)
				{
					Vector3 vector = (connection.point.tangent - connection.point.position).normalized;
					if (vector == Vector3.zero)
					{
						vector = -(connection.point.tangent2 - connection.point.position).normalized;
					}
					float magnitude = (connections[i].point.tangent - connections[i].point.position).magnitude;
					float magnitude2 = (connections[i].point.tangent2 - connections[i].point.position).magnitude;
					connections[i].point = connection.point;
					connections[i].point.tangent = connections[i].point.position + vector * magnitude;
					connections[i].point.tangent2 = connections[i].point.position - vector * magnitude2;
				}
			}
			return;
		}
		for (int j = 0; j < connections.Length; j++)
		{
			if (j != connectionIndex)
			{
				connections[j].point = connection.point;
			}
		}
	}

	private void OnDestroy()
	{
		ClearConnections();
	}

	public void ClearConnections()
	{
		for (int num = connections.Length - 1; num >= 0; num--)
		{
			if (connections[num].spline != null)
			{
				connections[num].spline.DisconnectNode(connections[num].pointIndex);
			}
		}
		connections = new Connection[0];
	}

	public void UpdateConnectedComputers(SplineComputer excludeComputer = null)
	{
		for (int num = connections.Length - 1; num >= 0; num--)
		{
			if (!connections[num].isValid)
			{
				RemoveConnection(num);
			}
			else if (!(connections[num].spline == excludeComputer))
			{
				if (type == Type.Smooth && num != 0)
				{
					SetPoint(num, GetPoint(0, swapTangents: false), swappedTangents: false);
				}
				SplinePoint point = GetPoint(num, swapTangents: true);
				if (!transformNormals)
				{
					point.normal = connections[num].spline.GetPointNormal(connections[num].pointIndex);
				}
				if (!transformTangents)
				{
					point.tangent = connections[num].spline.GetPointTangent(connections[num].pointIndex);
					point.tangent2 = connections[num].spline.GetPointTangent2(connections[num].pointIndex);
				}
				if (!transformSize)
				{
					point.size = connections[num].spline.GetPointSize(connections[num].pointIndex);
				}
				connections[num].spline.SetPoint(connections[num].pointIndex, point);
			}
		}
	}

	public void UpdatePoint(SplineComputer computer, int pointIndex, SplinePoint point, bool updatePosition = true)
	{
		trs.position = point.position;
		for (int i = 0; i < connections.Length; i++)
		{
			if (connections[i].spline == computer && connections[i].pointIndex == pointIndex)
			{
				SetPoint(i, point, swappedTangents: true);
			}
		}
	}

	public void UpdatePoints()
	{
		for (int num = connections.Length - 1; num >= 0; num--)
		{
			if (!connections[num].isValid)
			{
				RemoveConnection(num);
			}
			else
			{
				SplinePoint point = connections[num].spline.GetPoint(connections[num].pointIndex);
				point.SetPosition(base.transform.position);
				SetPoint(num, point, swappedTangents: true);
			}
		}
	}

	protected void RemoveInvalidConnections()
	{
		for (int num = connections.Length - 1; num >= 0; num--)
		{
			if (connections[num] == null || !connections[num].isValid)
			{
				RemoveConnection(num);
			}
		}
	}

	public virtual void AddConnection(SplineComputer computer, int pointIndex)
	{
		RemoveInvalidConnections();
		Node node = computer.GetNode(pointIndex);
		if (node != null)
		{
			Debug.LogError(computer.name + " is already connected to node " + node.name + " at point " + pointIndex);
			return;
		}
		SplinePoint point = computer.GetPoint(pointIndex);
		point.SetPosition(base.transform.position);
		ArrayUtility.Add(ref connections, new Connection(computer, pointIndex, PointToLocal(point)));
		if (connections.Length == 1)
		{
			SetPoint(connections.Length - 1, point, swappedTangents: true);
		}
		UpdateConnectedComputers();
	}

	protected SplinePoint PointToLocal(SplinePoint worldPoint)
	{
		worldPoint.position = Vector3.zero;
		worldPoint.tangent = base.transform.InverseTransformPoint(worldPoint.tangent);
		worldPoint.tangent2 = base.transform.InverseTransformPoint(worldPoint.tangent2);
		worldPoint.normal = base.transform.InverseTransformDirection(worldPoint.normal);
		worldPoint.size /= (base.transform.localScale.x + base.transform.localScale.y + base.transform.localScale.z) / 3f;
		return worldPoint;
	}

	protected SplinePoint PointToWorld(SplinePoint localPoint)
	{
		localPoint.position = base.transform.position;
		localPoint.tangent = base.transform.TransformPoint(localPoint.tangent);
		localPoint.tangent2 = base.transform.TransformPoint(localPoint.tangent2);
		localPoint.normal = base.transform.TransformDirection(localPoint.normal);
		localPoint.size *= (base.transform.localScale.x + base.transform.localScale.y + base.transform.localScale.z) / 3f;
		return localPoint;
	}

	public virtual void RemoveConnection(SplineComputer computer, int pointIndex)
	{
		int num = -1;
		for (int i = 0; i < connections.Length; i++)
		{
			if (connections[i].pointIndex == pointIndex && connections[i].spline == computer)
			{
				num = i;
				break;
			}
		}
		if (num >= 0)
		{
			RemoveConnection(num);
		}
	}

	private void RemoveConnection(int index)
	{
		Connection[] array = new Connection[connections.Length - 1];
		_ = connections[index].spline;
		_ = connections[index].pointIndex;
		for (int i = 0; i < connections.Length; i++)
		{
			if (i < index)
			{
				array[i] = connections[i];
			}
			else if (i != index)
			{
				array[i - 1] = connections[i];
			}
		}
		connections = array;
	}

	public virtual bool HasConnection(SplineComputer computer, int pointIndex)
	{
		for (int num = connections.Length - 1; num >= 0; num--)
		{
			if (!connections[num].isValid)
			{
				RemoveConnection(num);
			}
			else if (connections[num].spline == computer && connections[num].pointIndex == pointIndex)
			{
				return true;
			}
		}
		return false;
	}

	public Connection[] GetConnections()
	{
		return connections;
	}
}
