namespace Dreamteck;

public static class ArrayUtility
{
	public static void Add<T>(ref T[] array, T item)
	{
		T[] array2 = new T[array.Length + 1];
		array.CopyTo(array2, 0);
		array2[^1] = item;
		array = array2;
	}

	public static bool Contains<T>(T[] array, T item)
	{
		for (int i = 0; i < array.Length; i++)
		{
			ref readonly T reference = ref array[i];
			object obj = item;
			if (reference.Equals(obj))
			{
				return true;
			}
		}
		return false;
	}

	public static int IndexOf<T>(T[] array, T value)
	{
		for (int i = 0; i < array.Length; i++)
		{
			ref readonly T reference = ref array[i];
			object obj = value;
			if (reference.Equals(obj))
			{
				return i;
			}
		}
		return 0;
	}

	public static void Insert<T>(ref T[] array, int index, T item)
	{
		T[] array2 = new T[array.Length + 1];
		for (int i = 0; i < array2.Length; i++)
		{
			if (i < index)
			{
				array2[i] = array[i];
			}
			else if (i > index)
			{
				array2[i] = array[i - 1];
			}
			else
			{
				array2[i] = item;
			}
		}
		array = array2;
	}

	public static void RemoveAt<T>(ref T[] array, int index)
	{
		if (array.Length == 0)
		{
			return;
		}
		T[] array2 = new T[array.Length - 1];
		for (int i = 0; i < array.Length; i++)
		{
			if (i < index)
			{
				array2[i] = array[i];
			}
			else if (i > index)
			{
				array2[i - 1] = array[i];
			}
		}
		array = array2;
	}
}
