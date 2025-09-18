using UnityEngine;

public class RaftBlock : ActiveBlock
{
	[SerializeField]
	private Transform _initialParent;

	private Transform _transform;

	protected override void Awake()
	{
		_transform = base.transform;
		base.Awake();
		pms = new PhysicsModifier[1];
		pms[0] = new PhysicsModifier(PhysicsModifier.ModType.BaseMotion, 0f, Vector2.zero, base.gameObject);
	}

	protected override void Start()
	{
		base.Start();
	}

	public override PhysicsModifier[] GetPhysicsModifiers()
	{
		return pms;
	}

	public override PhysicsModifier[] GetPhysicsModifier()
	{
		return pms;
	}

	private void Update()
	{
		if (!(_transform.parent == _initialParent))
		{
			_transform.parent = _initialParent;
		}
	}
}
