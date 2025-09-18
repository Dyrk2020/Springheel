using UnityEngine;

public class KeyItem : ActiveBlock
{
	public LockItem[] Locks;

	private void OnTriggerEnter2D(Collider2D c)
	{
		string[] array = c.gameObject.tag.Split('_');
		bool flag = false;
		for (int i = 0; i != array.Length; i++)
		{
			if (array[i].Equals("Player"))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			LockItem[] locks = Locks;
			for (int j = 0; j < locks.Length; j++)
			{
				locks[j].Disable();
			}
			Disable();
		}
	}

	public override void Reset()
	{
		base.Reset();
		Enable();
	}
}
