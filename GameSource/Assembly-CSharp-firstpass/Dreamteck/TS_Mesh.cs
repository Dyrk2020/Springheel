using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck;

public class TS_Mesh
{
	public Vector3[] vertices = new Vector3[0];

	public Vector3[] normals = new Vector3[0];

	public Vector4[] tangents = new Vector4[0];

	public Color[] colors = new Color[0];

	public Vector2[] uv = new Vector2[0];

	public Vector2[] uv2 = new Vector2[0];

	public Vector2[] uv3 = new Vector2[0];

	public Vector2[] uv4 = new Vector2[0];

	public int[] triangles = new int[0];

	public List<int[]> subMeshes = new List<int[]>();

	public TS_Bounds bounds = new TS_Bounds(Vector3.zero, Vector3.zero);

	public volatile bool hasUpdate;

	public int vertexCount
	{
		get
		{
			return vertices.Length;
		}
		set
		{
		}
	}

	public TS_Mesh()
	{
	}

	public TS_Mesh(Mesh mesh)
	{
		CreateFromMesh(mesh);
	}

	public void Clear()
	{
		vertices = new Vector3[0];
		normals = new Vector3[0];
		tangents = new Vector4[0];
		colors = new Color[0];
		uv = new Vector2[0];
		uv2 = new Vector2[0];
		uv3 = new Vector2[0];
		uv4 = new Vector2[0];
		triangles = new int[0];
		subMeshes = new List<int[]>();
		bounds = new TS_Bounds(Vector3.zero, Vector3.zero);
	}

	public void CreateFromMesh(Mesh mesh)
	{
		vertices = mesh.vertices;
		normals = mesh.normals;
		tangents = mesh.tangents;
		colors = mesh.colors;
		uv = mesh.uv;
		uv2 = mesh.uv2;
		uv3 = mesh.uv3;
		uv4 = mesh.uv4;
		triangles = mesh.triangles;
		bounds = new TS_Bounds(mesh.bounds);
		for (int i = 0; i < mesh.subMeshCount; i++)
		{
			subMeshes.Add(mesh.GetTriangles(i));
		}
	}

	public void Combine(List<TS_Mesh> newMeshes, bool overwrite = false)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < newMeshes.Count; i++)
		{
			num += newMeshes[i].vertexCount;
			num2 += newMeshes[i].triangles.Length;
			if (newMeshes[i].subMeshes.Count > num3)
			{
				num3 = newMeshes[i].subMeshes.Count;
			}
		}
		int[] array = new int[num3];
		int[] array2 = new int[num3];
		for (int j = 0; j < newMeshes.Count; j++)
		{
			for (int k = 0; k < newMeshes[j].subMeshes.Count; k++)
			{
				array[k] += newMeshes[j].subMeshes[k].Length;
			}
		}
		if (overwrite)
		{
			int num4 = 0;
			int num5 = 0;
			if (vertices.Length != num)
			{
				vertices = new Vector3[num];
			}
			if (normals.Length != num)
			{
				normals = new Vector3[num];
			}
			if (uv.Length != num)
			{
				uv = new Vector2[num];
			}
			if (uv2.Length != num)
			{
				uv2 = new Vector2[num];
			}
			if (uv3.Length != num)
			{
				uv3 = new Vector2[num];
			}
			if (uv4.Length != num)
			{
				uv4 = new Vector2[num];
			}
			if (colors.Length != num)
			{
				colors = new Color[num];
			}
			if (tangents.Length != num)
			{
				tangents = new Vector4[num];
			}
			if (triangles.Length != num2)
			{
				triangles = new int[num2];
			}
			if (subMeshes.Count != num3)
			{
				subMeshes.Clear();
			}
			for (int l = 0; l < newMeshes.Count; l++)
			{
				newMeshes[l].vertices.CopyTo(vertices, num4);
				newMeshes[l].normals.CopyTo(normals, num4);
				newMeshes[l].uv.CopyTo(uv, num4);
				newMeshes[l].uv2.CopyTo(uv2, num4);
				newMeshes[l].uv3.CopyTo(uv3, num4);
				newMeshes[l].uv4.CopyTo(uv4, num4);
				newMeshes[l].colors.CopyTo(colors, num4);
				newMeshes[l].tangents.CopyTo(tangents, num4);
				for (int m = num5; m < num5 + newMeshes[l].triangles.Length; m++)
				{
					triangles[m] = newMeshes[l].triangles[m - num2] + num4;
				}
				num5 += newMeshes[l].triangles.Length;
				for (int n = 0; n < newMeshes[l].subMeshes.Count; n++)
				{
					if (n >= subMeshes.Count)
					{
						subMeshes.Add(new int[array[n]]);
					}
					else if (subMeshes[n].Length != array[n])
					{
						subMeshes[n] = new int[array[n]];
					}
					for (int num6 = array2[n]; num6 < array2[n] + newMeshes[l].subMeshes[n].Length; num6++)
					{
						subMeshes[n][num6] = newMeshes[l].subMeshes[n][num6 - array2[n]] + num4;
					}
					array2[n] += newMeshes[l].subMeshes[n].Length;
				}
				num4 += newMeshes[l].vertexCount;
			}
			return;
		}
		Vector3[] array3 = new Vector3[vertices.Length + num];
		Vector3[] array4 = new Vector3[vertices.Length + num];
		Vector2[] array5 = new Vector2[vertices.Length + num];
		Vector2[] array6 = new Vector2[vertices.Length + num];
		Vector2[] array7 = new Vector2[vertices.Length + num];
		Vector2[] array8 = new Vector2[vertices.Length + num];
		Color[] array9 = new Color[vertices.Length + num];
		Vector4[] array10 = new Vector4[tangents.Length + num];
		int[] array11 = new int[triangles.Length + num2];
		List<int[]> list = new List<int[]>();
		for (int num7 = 0; num7 < array.Length; num7++)
		{
			list.Add(new int[array[num7]]);
			if (num7 < subMeshes.Count)
			{
				array[num7] = subMeshes[num7].Length;
			}
			else
			{
				array[num7] = 0;
			}
		}
		num = vertexCount;
		num2 = triangles.Length;
		vertices.CopyTo(array3, 0);
		normals.CopyTo(array4, 0);
		uv.CopyTo(array5, 0);
		uv2.CopyTo(array6, 0);
		uv3.CopyTo(array7, 0);
		uv4.CopyTo(array8, 0);
		colors.CopyTo(array9, 0);
		tangents.CopyTo(array10, 0);
		triangles.CopyTo(array11, 0);
		for (int num8 = 0; num8 < newMeshes.Count; num8++)
		{
			newMeshes[num8].vertices.CopyTo(array3, num);
			newMeshes[num8].normals.CopyTo(array4, num);
			newMeshes[num8].uv.CopyTo(array5, num);
			newMeshes[num8].uv2.CopyTo(array6, num);
			newMeshes[num8].uv3.CopyTo(array7, num);
			newMeshes[num8].uv4.CopyTo(array8, num);
			newMeshes[num8].colors.CopyTo(array9, num);
			newMeshes[num8].tangents.CopyTo(array10, num);
			for (int num9 = num2; num9 < num2 + newMeshes[num8].triangles.Length; num9++)
			{
				array11[num9] = newMeshes[num8].triangles[num9 - num2] + num;
			}
			for (int num10 = 0; num10 < newMeshes[num8].subMeshes.Count; num10++)
			{
				for (int num11 = array[num10]; num11 < array[num10] + newMeshes[num8].subMeshes[num10].Length; num11++)
				{
					list[num10][num11] = newMeshes[num8].subMeshes[num10][num11 - array[num10]] + num;
				}
				array[num10] += newMeshes[num8].subMeshes[num10].Length;
			}
			num2 += newMeshes[num8].triangles.Length;
			num += newMeshes[num8].vertexCount;
		}
		vertices = array3;
		normals = array4;
		uv = array5;
		uv2 = array6;
		uv3 = array7;
		uv4 = array8;
		colors = array9;
		tangents = array10;
		triangles = array11;
		subMeshes = list;
	}

	public void Combine(TS_Mesh newMesh)
	{
		Vector3[] array = new Vector3[vertices.Length + newMesh.vertices.Length];
		Vector3[] array2 = new Vector3[normals.Length + newMesh.normals.Length];
		Vector2[] array3 = new Vector2[uv.Length + newMesh.uv.Length];
		Vector2[] array4 = new Vector2[uv.Length + newMesh.uv2.Length];
		Vector2[] array5 = new Vector2[uv.Length + newMesh.uv3.Length];
		Vector2[] array6 = new Vector2[uv.Length + newMesh.uv4.Length];
		Color[] array7 = new Color[colors.Length + newMesh.colors.Length];
		Vector4[] array8 = new Vector4[tangents.Length + newMesh.tangents.Length];
		int[] array9 = new int[triangles.Length + newMesh.triangles.Length];
		vertices.CopyTo(array, 0);
		newMesh.vertices.CopyTo(array, vertices.Length);
		normals.CopyTo(array2, 0);
		newMesh.normals.CopyTo(array2, normals.Length);
		uv.CopyTo(array3, 0);
		newMesh.uv.CopyTo(array3, uv.Length);
		uv2.CopyTo(array4, 0);
		newMesh.uv2.CopyTo(array4, uv2.Length);
		uv3.CopyTo(array5, 0);
		newMesh.uv3.CopyTo(array5, uv3.Length);
		uv4.CopyTo(array6, 0);
		newMesh.uv4.CopyTo(array6, uv4.Length);
		colors.CopyTo(array7, 0);
		newMesh.colors.CopyTo(array7, colors.Length);
		tangents.CopyTo(array8, 0);
		newMesh.tangents.CopyTo(array8, tangents.Length);
		for (int i = 0; i < array9.Length; i++)
		{
			if (i < triangles.Length)
			{
				array9[i] = triangles[i];
			}
			else
			{
				array9[i] = newMesh.triangles[i - triangles.Length] + vertices.Length;
			}
		}
		for (int j = 0; j < newMesh.subMeshes.Count; j++)
		{
			if (j >= subMeshes.Count)
			{
				subMeshes.Add(newMesh.subMeshes[j]);
				continue;
			}
			int[] array10 = new int[subMeshes[j].Length + newMesh.subMeshes[j].Length];
			subMeshes[j].CopyTo(array10, 0);
			for (int k = 0; k < newMesh.subMeshes[j].Length; k++)
			{
				array10[subMeshes[j].Length + k] = newMesh.subMeshes[j][k] + vertices.Length;
			}
			subMeshes[j] = array10;
		}
		vertices = array;
		normals = array2;
		uv = array3;
		uv2 = array4;
		uv3 = array5;
		uv4 = array6;
		colors = array7;
		tangents = array8;
		triangles = array9;
	}

	public static TS_Mesh Copy(TS_Mesh input)
	{
		TS_Mesh tS_Mesh = new TS_Mesh();
		tS_Mesh.vertices = new Vector3[input.vertices.Length];
		input.vertices.CopyTo(tS_Mesh.vertices, 0);
		tS_Mesh.normals = new Vector3[input.normals.Length];
		input.normals.CopyTo(tS_Mesh.normals, 0);
		tS_Mesh.uv = new Vector2[input.uv.Length];
		input.uv.CopyTo(tS_Mesh.uv, 0);
		tS_Mesh.uv2 = new Vector2[input.uv2.Length];
		input.uv2.CopyTo(tS_Mesh.uv2, 0);
		tS_Mesh.uv3 = new Vector2[input.uv3.Length];
		input.uv3.CopyTo(tS_Mesh.uv3, 0);
		tS_Mesh.uv4 = new Vector2[input.uv4.Length];
		input.uv4.CopyTo(tS_Mesh.uv4, 0);
		tS_Mesh.colors = new Color[input.colors.Length];
		input.colors.CopyTo(tS_Mesh.colors, 0);
		tS_Mesh.tangents = new Vector4[input.tangents.Length];
		input.tangents.CopyTo(tS_Mesh.tangents, 0);
		tS_Mesh.triangles = new int[input.triangles.Length];
		input.triangles.CopyTo(tS_Mesh.triangles, 0);
		tS_Mesh.subMeshes = new List<int[]>();
		for (int i = 0; i < input.subMeshes.Count; i++)
		{
			tS_Mesh.subMeshes.Add(new int[input.subMeshes[i].Length]);
			input.subMeshes[i].CopyTo(tS_Mesh.subMeshes[i], 0);
		}
		tS_Mesh.bounds = new TS_Bounds(input.bounds.center, input.bounds.size);
		return tS_Mesh;
	}

	public void Absorb(TS_Mesh input)
	{
		if (vertices.Length != input.vertexCount)
		{
			vertices = new Vector3[input.vertexCount];
		}
		if (normals.Length != input.normals.Length)
		{
			normals = new Vector3[input.normals.Length];
		}
		if (colors.Length != input.colors.Length)
		{
			colors = new Color[input.colors.Length];
		}
		if (uv.Length != input.uv.Length)
		{
			uv = new Vector2[input.uv.Length];
		}
		if (uv2.Length != input.uv2.Length)
		{
			uv2 = new Vector2[input.uv2.Length];
		}
		if (uv3.Length != input.uv3.Length)
		{
			uv3 = new Vector2[input.uv3.Length];
		}
		if (uv4.Length != input.uv4.Length)
		{
			uv4 = new Vector2[input.uv4.Length];
		}
		if (tangents.Length != input.tangents.Length)
		{
			tangents = new Vector4[input.tangents.Length];
		}
		if (triangles.Length != input.triangles.Length)
		{
			triangles = new int[input.triangles.Length];
		}
		input.vertices.CopyTo(vertices, 0);
		input.normals.CopyTo(normals, 0);
		input.colors.CopyTo(colors, 0);
		input.uv.CopyTo(uv, 0);
		input.uv2.CopyTo(uv2, 0);
		input.uv3.CopyTo(uv3, 0);
		input.uv4.CopyTo(uv4, 0);
		input.tangents.CopyTo(tangents, 0);
		input.triangles.CopyTo(triangles, 0);
		if (subMeshes.Count == input.subMeshes.Count)
		{
			for (int i = 0; i < subMeshes.Count; i++)
			{
				if (input.subMeshes[i].Length != subMeshes[i].Length)
				{
					subMeshes[i] = new int[input.subMeshes[i].Length];
				}
				input.subMeshes[i].CopyTo(subMeshes[i], 0);
			}
		}
		else
		{
			subMeshes = new List<int[]>();
			for (int j = 0; j < input.subMeshes.Count; j++)
			{
				subMeshes.Add(new int[input.subMeshes[j].Length]);
				input.subMeshes[j].CopyTo(subMeshes[j], 0);
			}
		}
		bounds = new TS_Bounds(input.bounds.center, input.bounds.size);
	}

	public void WriteMesh(ref Mesh input)
	{
		if (input == null)
		{
			input = new Mesh();
		}
		if (vertices != null && vertices.Length > 65000)
		{
			return;
		}
		input.Clear();
		input.vertices = vertices;
		input.normals = normals;
		if (tangents.Length == vertices.Length)
		{
			input.tangents = tangents;
		}
		if (colors.Length == vertices.Length)
		{
			input.colors = colors;
		}
		if (uv.Length == vertices.Length)
		{
			input.uv = uv;
		}
		if (uv2.Length == vertices.Length)
		{
			input.uv2 = uv2;
		}
		if (uv3.Length == vertices.Length)
		{
			input.uv3 = uv3;
		}
		if (uv4.Length == vertices.Length)
		{
			input.uv4 = uv4;
		}
		input.triangles = triangles;
		if (subMeshes.Count > 0)
		{
			input.subMeshCount = subMeshes.Count;
			for (int i = 0; i < subMeshes.Count; i++)
			{
				input.SetTriangles(subMeshes[i], i);
			}
		}
		input.RecalculateBounds();
		hasUpdate = false;
	}
}
