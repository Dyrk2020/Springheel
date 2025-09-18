using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkTester : NetworkBehaviour, InputReceiver
{
	[SyncVar]
	public bool A;

	[SyncVar]
	public bool B;

	[SyncVar]
	public bool X;

	[SyncVar]
	public bool Y;

	[SyncVar]
	public float SyncFloat;

	private Material mat;

	public bool NetworkA
	{
		get
		{
			return A;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref A, 1u);
		}
	}

	public bool NetworkB
	{
		get
		{
			return B;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref B, 2u);
		}
	}

	public bool NetworkX
	{
		get
		{
			return X;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref X, 4u);
		}
	}

	public bool NetworkY
	{
		get
		{
			return Y;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref Y, 8u);
		}
	}

	public float NetworkSyncFloat
	{
		get
		{
			return SyncFloat;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref SyncFloat, 16u);
		}
	}

	public void ReceiveEvent(InputEvent e)
	{
		if (!base.hasAuthority)
		{
			return;
		}
		if (e.Changed)
		{
			if (e.Key == InputEvent.InputKey.Accept)
			{
				NetworkA = e.Valueb;
			}
			if (e.Key == InputEvent.InputKey.Back)
			{
				NetworkB = e.Valueb;
			}
			if (e.Key == InputEvent.InputKey.Sprint)
			{
				NetworkX = e.Valueb;
			}
			if (e.Key == InputEvent.InputKey.Inventory)
			{
				NetworkY = e.Valueb;
			}
		}
		NetworkSyncFloat = e.Sender.GetVector().x;
	}

	private void Start()
	{
		mat = GetComponent<MeshRenderer>().material;
		Controller.AddGlobalReceiver(this);
	}

	private void Update()
	{
		Color a = Color.grey;
		if (A)
		{
			a = Color.green;
		}
		else if (B)
		{
			a = Color.red;
		}
		else if (X)
		{
			a = Color.blue;
		}
		else if (Y)
		{
			a = Color.yellow;
		}
		if (SyncFloat < 0f)
		{
			mat.color = Color.Lerp(a, Color.black, 0f - SyncFloat);
		}
		else
		{
			mat.color = Color.Lerp(a, Color.white, SyncFloat);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(A);
			writer.Write(B);
			writer.Write(X);
			writer.Write(Y);
			writer.Write(SyncFloat);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(A);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(B);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(X);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(Y);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(SyncFloat);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			A = reader.ReadBoolean();
			B = reader.ReadBoolean();
			X = reader.ReadBoolean();
			Y = reader.ReadBoolean();
			SyncFloat = reader.ReadSingle();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			A = reader.ReadBoolean();
		}
		if ((num & 2) != 0)
		{
			B = reader.ReadBoolean();
		}
		if ((num & 4) != 0)
		{
			X = reader.ReadBoolean();
		}
		if ((num & 8) != 0)
		{
			Y = reader.ReadBoolean();
		}
		if ((num & 0x10) != 0)
		{
			SyncFloat = reader.ReadSingle();
		}
	}

	public override void PreStartClient()
	{
	}
}
