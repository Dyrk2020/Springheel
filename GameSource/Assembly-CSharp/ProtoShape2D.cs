using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProtoShape2D : MonoBehaviour
{
	public Mesh meshAsset;

	public PS2DType type;

	public List<PS2DPoint> points = new List<PS2DPoint>(200);

	public List<Vector3> pointsFinal = new List<Vector3>(500);

	public List<PS2DColliderPoint> cpoints = new List<PS2DColliderPoint>(500);

	public Vector2[] cpointsFinal;

	public int triangleCount;

	public PS2DFillType fillType;

	public float textureScale = 1f;

	public float textureRotation;

	public Vector2 textureOffset;

	public Color color1 = Color.red;

	public Color color2 = Color.red;

	public float gradientScale = 1f;

	public float gradientRotation;

	public float gradientOffset;

	public float outlineWidth;

	public Color outlineColor = Color.red;

	public List<Vector2> outlineVertices = new List<Vector2>(500);

	public bool outlineLoop = true;

	public int outlineConnect;

	public bool HDRColors;

	public Material defaultMaterial;

	public Material spriteMaterial;

	public Material customMaterial;

	public Texture2D texture;

	public string uniqueName = "";

	public int curveIterations = 10;

	public bool antialias;

	public float aaridge = 0.002f;

	public List<Vector2> aaridgeVertices = new List<Vector2>(500);

	public int sortingLayer;

	private int _sortingLayer;

	public int orderInLayer;

	private int _orderInLayer;

	public PS2DSnapType snapType;

	public float gridSize = 1f;

	public PS2DPivotPositions PivotPosition;

	public PS2DColliderType colliderType;

	public float colliderTopAngle = 90f;

	public float colliderOffsetTop;

	public bool showNormals;

	public Mesh cMesh;

	public float cMeshDepth = 3f;

	private float edgeSum;

	public bool clockwise = true;

	private MeshRenderer mr;

	private MeshFilter mf;

	private List<Vector3> vertices = new List<Vector3>(200);

	private List<Color> colors = new List<Color>(200);

	private List<Vector2> uvs = new List<Vector2>(200);

	private int[] tris;

	private int[] trisOutline;

	private Vector2 lastPos;

	public Vector2 minPoint;

	public Vector2 maxPoint;

	public bool showFillSettings = true;

	public bool showOutlineSettings = true;

	public bool showMeshSetting = true;

	public bool showSnapSetting = true;

	public bool showColliderSettings = true;

	public bool showTools = true;

	private void Awake()
	{
		mr = GetComponent<MeshRenderer>();
		mf = GetComponent<MeshFilter>();
		vertices = new List<Vector3>();
		colors = new List<Color>();
		uvs = new List<Vector2>();
		if (uniqueName == "")
		{
			uniqueName = Random.Range(1000, 999999).ToString();
			mr.shadowCastingMode = ShadowCastingMode.Off;
			mr.receiveShadows = false;
			mr.lightProbeUsage = LightProbeUsage.Off;
			mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
			SetSpriteMaterial();
		}
		else
		{
			ProtoShape2D[] array = Object.FindObjectsOfType<ProtoShape2D>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].uniqueName == uniqueName)
				{
					uniqueName = Random.Range(1000, 999999).ToString();
					if (fillType == PS2DFillType.Color || fillType == PS2DFillType.None)
					{
						SetSpriteMaterial();
						continue;
					}
					if (fillType == PS2DFillType.CustomMaterial)
					{
						SetCustomMaterial();
						continue;
					}
					defaultMaterial = null;
					mr.sharedMaterial = null;
					SetDefaultMaterial();
				}
			}
		}
		setupSharedMesh();
		lastPos = base.transform.position;
		UpdateMaterialSettings();
		UpdateMesh();
	}

	public void setupSharedMesh()
	{
		Mesh mesh;
		if (meshAsset == null)
		{
			mesh = new Mesh();
			mesh.name = uniqueName;
		}
		else
		{
			mesh = meshAsset;
		}
		mf.sharedMesh = mesh;
	}

	private void OnDestroy()
	{
		if (fillType != PS2DFillType.Color && fillType != PS2DFillType.None && fillType != PS2DFillType.CustomMaterial)
		{
			if (Application.isEditor)
			{
				Object.DestroyImmediate(defaultMaterial);
			}
			else
			{
				Object.Destroy(defaultMaterial);
			}
		}
	}

	private void Update()
	{
		if (mr.sharedMaterial != null && fillType != PS2DFillType.CustomMaterial && fillType != PS2DFillType.Color && fillType != PS2DFillType.None && !lastPos.Equals(base.transform.position))
		{
			lastPos = base.transform.position;
			mr.sharedMaterial.SetVector("_WPos", base.transform.position);
			mr.sharedMaterial.SetVector("_MinWPos", mr.bounds.min);
			mr.sharedMaterial.SetVector("_MaxWPos", mr.bounds.max);
		}
		if (sortingLayer != _sortingLayer || orderInLayer != _orderInLayer)
		{
			mr.sortingLayerID = sortingLayer;
			mr.sortingOrder = orderInLayer;
			_sortingLayer = sortingLayer;
			_orderInLayer = orderInLayer;
		}
	}

	public void SetSpriteMaterial(Material mat)
	{
		spriteMaterial = mat;
		SetSpriteMaterial();
	}

	public void SetSpriteMaterial()
	{
		if (spriteMaterial == null)
		{
			spriteMaterial = (Material)Resources.Load("PS2DSpritesDefault");
		}
		if (mr.sharedMaterial == null || !mr.sharedMaterial.Equals(spriteMaterial))
		{
			spriteMaterial.name = "PS2DSpritesDefault";
			mr.sharedMaterial = spriteMaterial;
		}
	}

	public void SetDefaultMaterial()
	{
		if (defaultMaterial == null)
		{
			defaultMaterial = new Material(Shader.Find("ProtoShape2D/TextureAndColors"));
		}
		if (mr.sharedMaterial == null || !mr.sharedMaterial.Equals(defaultMaterial))
		{
			defaultMaterial.name = "PS2DTextureAndColors";
			mr.sharedMaterial = defaultMaterial;
		}
	}

	public void SetCustomMaterial()
	{
		if (customMaterial == null)
		{
			customMaterial = new Material(Shader.Find("ProtoShape2D/TextureAndColors"));
		}
		SetCustomMaterial(customMaterial);
	}

	public void SetCustomMaterial(Material mat)
	{
		if (mr.sharedMaterial == null || !mr.sharedMaterial.Equals(mat))
		{
			customMaterial = mat;
			mr.sharedMaterial = customMaterial;
		}
	}

	public void UpdateMaterialSettings()
	{
		if (!(mf != null) || !(mr != null) || !(mr.sharedMaterial != null) || fillType == PS2DFillType.CustomMaterial || fillType == PS2DFillType.Color || fillType == PS2DFillType.None)
		{
			return;
		}
		if (fillType == PS2DFillType.Texture)
		{
			mr.sharedMaterial.SetVector("_Color1", Color.white);
			mr.sharedMaterial.SetVector("_Color2", Color.white);
		}
		if (fillType == PS2DFillType.Color || fillType == PS2DFillType.TextureWithColor)
		{
			mr.sharedMaterial.SetVector("_Color1", color1);
			mr.sharedMaterial.SetVector("_Color2", color1);
		}
		if (fillType == PS2DFillType.Gradient || fillType == PS2DFillType.TextureWithGradient)
		{
			mr.sharedMaterial.SetVector("_Color1", color1);
			mr.sharedMaterial.SetVector("_Color2", color2);
		}
		mr.sharedMaterial.SetFloat("_GradientAngle", gradientRotation);
		mr.sharedMaterial.SetFloat("_GradientScale", gradientScale);
		mr.sharedMaterial.SetFloat("_GradientOffset", gradientOffset);
		if (fillType == PS2DFillType.Texture || fillType == PS2DFillType.TextureWithColor || fillType == PS2DFillType.TextureWithGradient)
		{
			mr.sharedMaterial.SetTexture("_Texture", texture);
			if (texture != null)
			{
				mr.sharedMaterial.SetTextureScale("_Texture", new Vector2(texture.width, texture.height) / 100f * textureScale);
				mr.sharedMaterial.SetFloat("_TextureAngle", textureRotation);
				mr.sharedMaterial.SetVector("_TextureOffset", textureOffset);
			}
		}
		else
		{
			mr.sharedMaterial.SetTexture("_Texture", null);
		}
	}

	public void UpdateMesh()
	{
		if (mf != null && mr != null && mr.sharedMaterial != null)
		{
			edgeSum = 0f;
			for (int i = 0; i < points.Count; i++)
			{
				edgeSum += (points[i].position.x - points.Loop(i + 1).position.x) * (points[i].position.y + points.Loop(i + 1).position.y);
			}
			clockwise = edgeSum <= 0f;
			if (type == PS2DType.Simple)
			{
				GenerateHandles();
			}
			vertices.Clear();
			colors.Clear();
			uvs.Clear();
			minPoint = Vector2.one * 9999f;
			maxPoint = -Vector2.one * 9999f;
			outlineConnect = 0;
			Color item = ((fillType == PS2DFillType.Color) ? color1 : Color.white);
			for (int j = 0; j < points.Count; j++)
			{
				if (points[j].curve > 0f || points.Loop(j + 1).curve > 0f)
				{
					for (int k = 0; k < curveIterations; k++)
					{
						vertices.Add((Vector2)CalculateBezierPoint((float)k / (float)curveIterations, points[j].position, points[j].handleN, points.Loop(j + 1).handleP, points.Loop(j + 1).position));
						colors.Add(color1);
						if (vertices[vertices.Count - 1].x < minPoint.x)
						{
							minPoint.x = vertices[vertices.Count - 1].x;
						}
						if (vertices[vertices.Count - 1].y < minPoint.y)
						{
							minPoint.y = vertices[vertices.Count - 1].y;
						}
						if (vertices[vertices.Count - 1].x > maxPoint.x)
						{
							maxPoint.x = vertices[vertices.Count - 1].x;
						}
						if (vertices[vertices.Count - 1].y > maxPoint.y)
						{
							maxPoint.y = vertices[vertices.Count - 1].y;
						}
					}
					if (outlineLoop || j < points.Count - 1)
					{
						outlineConnect += curveIterations;
					}
					if (!outlineLoop && j == points.Count - 1)
					{
						outlineConnect++;
					}
				}
				else
				{
					vertices.Add(points[j].position);
					colors.Add(item);
					if (points[j].position.x < minPoint.x)
					{
						minPoint.x = points[j].position.x;
					}
					if (points[j].position.y < minPoint.y)
					{
						minPoint.y = points[j].position.y;
					}
					if (points[j].position.x > maxPoint.x)
					{
						maxPoint.x = points[j].position.x;
					}
					if (points[j].position.y > maxPoint.y)
					{
						maxPoint.y = points[j].position.y;
					}
					outlineConnect++;
				}
			}
			for (int l = 0; l < vertices.Count; l++)
			{
				uvs.Add(new Vector2(Mathf.Min(0.99f, Mathf.InverseLerp(minPoint.x, maxPoint.x, vertices[l].x)), Mathf.Min(0.99f, Mathf.InverseLerp(minPoint.y, maxPoint.y, vertices[l].y))));
			}
			pointsFinal = new List<Vector3>(vertices);
			if (fillType == PS2DFillType.None || points.Count < 3)
			{
				tris = new int[0];
			}
			else
			{
				PS2DTriangulator pS2DTriangulator = new PS2DTriangulator(vertices);
				tris = pS2DTriangulator.Triangulate();
			}
			outlineVertices.Clear();
			if (outlineWidth > 0f && outlineConnect > 0)
			{
				int count = vertices.Count;
				for (int m = 0; m < outlineConnect; m++)
				{
					Vector2 vector = (vertices.Loop(m - 1) - vertices[m]).normalized;
					vector = new Vector2(vector.y, 0f - vector.x) * (outlineWidth / 2f);
					Vector2 vector2 = (vertices[m] - vertices.Loop(m + 1)).normalized;
					vector2 = new Vector2(vector2.y, 0f - vector2.x) * (outlineWidth / 2f);
					if (!outlineLoop && (m == 0 || m == outlineConnect - 1))
					{
						if (m == 0)
						{
							outlineVertices.Add((Vector2)vertices[m] - vector2);
							outlineVertices.Add((Vector2)vertices[m] + vector2);
						}
						if (m == outlineConnect - 1)
						{
							outlineVertices.Add((Vector2)vertices[m] - vector);
							outlineVertices.Add((Vector2)vertices[m] + vector);
						}
					}
					else
					{
						outlineVertices.Add(LineIntersectionPoint((Vector2)vertices.Loop(m - 1) - vector, (Vector2)vertices[m] - vector, (Vector2)vertices[m] - vector2, (Vector2)vertices.Loop(m + 1) - vector2));
						outlineVertices.Add(LineIntersectionPoint((Vector2)vertices.Loop(m - 1) + vector, (Vector2)vertices[m] + vector, (Vector2)vertices[m] + vector2, (Vector2)vertices.Loop(m + 1) + vector2));
					}
				}
				for (int n = 0; n < outlineVertices.Count; n++)
				{
					vertices.Add(outlineVertices[n]);
					colors.Add(outlineColor);
					uvs.Add(Vector2.one);
				}
				trisOutline = new int[tris.Length + outlineVertices.Count * 3];
				for (int num = 0; num < tris.Length; num++)
				{
					trisOutline[num] = tris[num];
				}
				for (int num2 = 0; num2 < outlineConnect - ((!outlineLoop) ? 1 : 0); num2++)
				{
					trisOutline[tris.Length + num2 * 6] = count + num2 * 2;
					trisOutline[tris.Length + num2 * 6 + 1] = count + num2 * 2 + 1;
					trisOutline[tris.Length + num2 * 6 + 2] = ((count + num2 * 2 + 3 < vertices.Count) ? (count + num2 * 2 + 3) : (count + 1));
					trisOutline[tris.Length + num2 * 6 + 3] = count + num2 * 2;
					trisOutline[tris.Length + num2 * 6 + 4] = ((count + num2 * 2 + 3 < vertices.Count) ? (count + num2 * 2 + 3) : (count + 1));
					trisOutline[tris.Length + num2 * 6 + 5] = ((count + num2 * 2 + 3 < vertices.Count) ? (count + num2 * 2 + 2) : count);
				}
				tris = trisOutline;
			}
			aaridgeVertices.Clear();
			if (antialias)
			{
				int count2 = vertices.Count;
				for (int num3 = 0; num3 < vertices.Count; num3++)
				{
					Vector2 vector3 = (vertices[num3] - vertices.Loop(num3 + 1)).normalized;
					vector3 = new Vector2(vector3.y, 0f - vector3.x) * aaridge;
					Vector2 vector4 = (vertices.Loop(num3 + 1) - vertices.Loop(num3 + 2)).normalized;
					vector4 = new Vector2(vector4.y, 0f - vector4.x) * aaridge;
					if (!clockwise)
					{
						vector3 *= -1f;
						vector4 *= -1f;
					}
					aaridgeVertices.Add(LineIntersectionPoint((Vector2)vertices[num3] + vector3, (Vector2)vertices.Loop(num3 + 1) + vector3, (Vector2)vertices.Loop(num3 + 1) + vector4, (Vector2)vertices.Loop(num3 + 2) + vector4));
				}
				Color item2 = new Color(item.r, item.g, item.b, 0f);
				for (int num4 = 0; num4 < aaridgeVertices.Count; num4++)
				{
					vertices.Add(aaridgeVertices[num4]);
					colors.Add(item2);
					uvs.Add(Vector2.zero);
				}
				trisOutline = new int[tris.Length + aaridgeVertices.Count * 2 * 3];
				for (int num5 = 0; num5 < tris.Length; num5++)
				{
					trisOutline[num5] = tris[num5];
				}
				for (int num6 = 0; num6 < aaridgeVertices.Count; num6++)
				{
					trisOutline[tris.Length + num6 * 6] = num6;
					trisOutline[tris.Length + num6 * 6 + 1] = ((count2 + num6 - 1 < count2) ? (count2 + aaridgeVertices.Count - 1) : (count2 + num6 - 1));
					trisOutline[tris.Length + num6 * 6 + 2] = count2 + num6;
					trisOutline[tris.Length + num6 * 6 + 3] = num6;
					trisOutline[tris.Length + num6 * 6 + 4] = count2 + num6;
					trisOutline[tris.Length + num6 * 6 + 5] = ((num6 + 1 <= count2 - 1) ? (num6 + 1) : 0);
				}
				tris = trisOutline;
			}
			if (mf.sharedMesh == null)
			{
				mf.sharedMesh = new Mesh();
			}
			mf.sharedMesh.Clear();
			mf.sharedMesh.SetVertices(vertices);
			mf.sharedMesh.SetColors(colors);
			mf.sharedMesh.SetUVs(0, uvs);
			mf.sharedMesh.RecalculateBounds();
			mf.sharedMesh.SetTriangles(tris, 0);
			triangleCount = mf.sharedMesh.triangles.Length / 3;
			mr.sharedMaterial.SetVector("_WPos", base.transform.position);
			mr.sharedMaterial.SetVector("_MinWPos", mr.bounds.min);
			mr.sharedMaterial.SetVector("_MaxWPos", mr.bounds.max);
		}
		UpdateCollider();
	}

	private void UpdateCollider()
	{
		Collider2D component = GetComponent<Collider2D>();
		MeshCollider component2 = GetComponent<MeshCollider>();
		if (!(component != null) && !(component2 != null))
		{
			return;
		}
		cpoints.Clear();
		for (int i = 0; i < points.Count; i++)
		{
			if (points[i].curve > 0f || points.Loop(i + 1).curve > 0f)
			{
				for (int j = 0; j < curveIterations; j++)
				{
					cpoints.Add(new PS2DColliderPoint(CalculateBezierPoint((float)j / (float)curveIterations, points[i].position, points[i].handleN, points.Loop(i + 1).handleP, points.Loop(i + 1).position)));
					cpoints[cpoints.Count - 1].wPosition = base.transform.TransformPoint(cpoints[cpoints.Count - 1].position);
				}
			}
			else
			{
				cpoints.Add(new PS2DColliderPoint(points[i].position));
				cpoints[cpoints.Count - 1].wPosition = base.transform.TransformPoint(cpoints[cpoints.Count - 1].position);
			}
		}
		for (int k = 0; k < cpoints.Count; k++)
		{
			cpoints[k].normal = cpoints[k].wPosition - cpoints.Loop(k + 1).wPosition;
			cpoints[k].normal = new Vector2(cpoints[k].normal.y, 0f - cpoints[k].normal.x).normalized;
			if (!clockwise)
			{
				cpoints[k].normal *= -1f;
			}
			cpoints[k].signedAngle = Vector2Extension.SignedAngle(Vector2.up, cpoints[k].normal);
			if (Mathf.Abs(cpoints[k].signedAngle) <= colliderTopAngle / 2f)
			{
				cpoints[k].direction = PS2DDirection.Up;
			}
			else if (cpoints[k].signedAngle > colliderTopAngle / 2f && cpoints[k].signedAngle < 135f)
			{
				cpoints[k].direction = PS2DDirection.Left;
			}
			else if (cpoints[k].signedAngle < 0f - colliderTopAngle / 2f && cpoints[k].signedAngle > -135f)
			{
				cpoints[k].direction = PS2DDirection.Right;
			}
			else if (Mathf.Abs(cpoints[k].signedAngle) >= 135f)
			{
				cpoints[k].direction = PS2DDirection.Down;
			}
		}
		if (component != null)
		{
			if (component.GetType() == typeof(PolygonCollider2D))
			{
				cpointsFinal = new Vector2[cpoints.Count];
				for (int l = 0; l < cpoints.Count; l++)
				{
					cpointsFinal[l] = cpoints[l].position;
					if (cpoints[l].direction == PS2DDirection.Up || cpoints.Loop(l - 1).direction == PS2DDirection.Up)
					{
						cpointsFinal[l] += Vector2.up * colliderOffsetTop;
					}
				}
				GetComponent<PolygonCollider2D>().points = cpointsFinal;
			}
			else if (component.GetType() == typeof(EdgeCollider2D) && colliderType == PS2DColliderType.Edge)
			{
				cpointsFinal = new Vector2[cpoints.Count];
				for (int m = 0; m < cpoints.Count; m++)
				{
					cpointsFinal[m] = cpoints[m].position;
					if (cpoints[m].direction == PS2DDirection.Up || cpoints.Loop(m - 1).direction == PS2DDirection.Up)
					{
						cpointsFinal[m] += Vector2.up * colliderOffsetTop;
					}
				}
				GetComponent<EdgeCollider2D>().points = cpointsFinal;
			}
			else if (component.GetType() == typeof(EdgeCollider2D) && colliderType == PS2DColliderType.TopEdge)
			{
				int num = 0;
				for (int n = 0; n < cpoints.Count; n++)
				{
					if (n == 0 || cpoints[n].wPosition.y < cpoints[num].wPosition.y)
					{
						num = n;
					}
				}
				int num2 = -1;
				for (int num3 = num; num3 < num + cpoints.Count; num3++)
				{
					if (cpoints.Loop(num3).direction == PS2DDirection.Up)
					{
						num2 = cpoints.LoopID(num3);
						break;
					}
				}
				int num4 = -1;
				for (int num5 = num; num5 > num - cpoints.Count; num5--)
				{
					if (cpoints.Loop(num5).direction == PS2DDirection.Up)
					{
						num4 = cpoints.LoopID(num5 + 1);
						break;
					}
				}
				if (num2 >= 0 && num4 >= 0)
				{
					int num6 = 1;
					for (int num7 = num2; num7 != num4; num7 = cpoints.LoopID(num7 + 1))
					{
						num6++;
					}
					if (num6 > 1)
					{
						cpointsFinal = new Vector2[num6];
						for (int num8 = 0; num8 < num6; num8++)
						{
							cpointsFinal[num8] = cpoints.Loop(num2 + num8).position + Vector2.up * colliderOffsetTop;
						}
						GetComponent<EdgeCollider2D>().enabled = true;
						GetComponent<EdgeCollider2D>().points = cpointsFinal;
					}
					else
					{
						GetComponent<EdgeCollider2D>().enabled = false;
					}
				}
				else
				{
					GetComponent<EdgeCollider2D>().enabled = false;
				}
			}
		}
		if (component2 != null)
		{
			Vector3[] array = new Vector3[mf.sharedMesh.vertices.Length * 2];
			for (int num9 = 0; num9 < mf.sharedMesh.vertices.Length; num9++)
			{
				array[num9] = mf.sharedMesh.vertices[num9];
				array[num9].z -= cMeshDepth / 2f;
			}
			for (int num10 = mf.sharedMesh.vertices.Length; num10 < mf.sharedMesh.vertices.Length * 2; num10++)
			{
				array[num10] = mf.sharedMesh.vertices[num10 - mf.sharedMesh.vertices.Length];
				array[num10].z += cMeshDepth / 2f;
			}
			int[] array2 = new int[mf.sharedMesh.triangles.Length * 2 + mf.sharedMesh.vertices.Length * 2 * 3];
			for (int num11 = 0; num11 < mf.sharedMesh.triangles.Length; num11++)
			{
				array2[num11] = mf.sharedMesh.triangles[num11];
			}
			for (int num12 = mf.sharedMesh.triangles.Length * 2 - 1; num12 >= mf.sharedMesh.triangles.Length; num12--)
			{
				array2[mf.sharedMesh.triangles.Length * 2 + mf.sharedMesh.triangles.Length - 1 - num12] = mf.sharedMesh.triangles[num12 - mf.sharedMesh.triangles.Length] + mf.sharedMesh.vertices.Length;
			}
			for (int num13 = 0; num13 < mf.sharedMesh.vertices.Length; num13++)
			{
				array2[mf.sharedMesh.triangles.Length * 2 + num13 * 6] = num13;
				array2[mf.sharedMesh.triangles.Length * 2 + num13 * 6 + 1] = mf.sharedMesh.vertices.Length + num13;
				array2[mf.sharedMesh.triangles.Length * 2 + num13 * 6 + 2] = mf.sharedMesh.vertices.Length + num13 + 1 - ((num13 == mf.sharedMesh.vertices.Length - 1) ? mf.sharedMesh.vertices.Length : 0);
				array2[mf.sharedMesh.triangles.Length * 2 + num13 * 6 + 3] = num13;
				array2[mf.sharedMesh.triangles.Length * 2 + num13 * 6 + 4] = mf.sharedMesh.vertices.Length + num13 + 1 - ((num13 == mf.sharedMesh.vertices.Length - 1) ? mf.sharedMesh.vertices.Length : 0);
				array2[mf.sharedMesh.triangles.Length * 2 + num13 * 6 + 5] = num13 + 1 - ((num13 == mf.sharedMesh.vertices.Length - 1) ? mf.sharedMesh.vertices.Length : 0);
			}
			cMesh = new Mesh();
			cMesh.SetVertices(new List<Vector3>(array));
			cMesh.SetTriangles(array2, 0);
			cMesh.name = base.transform.name;
			component2.sharedMesh = null;
			component2.sharedMesh = cMesh;
		}
	}

	public Mesh GetMesh()
	{
		return mf.sharedMesh;
	}

	private void GenerateHandles()
	{
		for (int i = 0; i < points.Count; i++)
		{
			GenerateHandles(i);
		}
	}

	public void GenerateHandles(int i)
	{
		float num = Vector2Extension.SignedAngle((points.Loop(i + 1).position - points[i].position).normalized, (points.Loop(i - 1).position - points[i].position).normalized);
		if (num > 0f)
		{
			num = 0f - (360f - num);
		}
		Vector2 vector = (points.Loop(i + 1).position - points[i].position).normalized.Rotate(num / 2f);
		if (!clockwise)
		{
			vector *= -1f;
		}
		if (points[i].clockwise == clockwise && Mathf.Abs(Vector2Extension.SignedAngle(points[i].median, vector)) > 135f)
		{
			vector *= -1f;
		}
		points[i].handleP = vector.Rotate(90 * ((!clockwise) ? 1 : (-1))) + points[i].position;
		points[i].handleN = vector.Rotate(90 * (clockwise ? 1 : (-1))) + points[i].position;
		points[i].handleP = (points[i].handleP - points[i].position) * (Vector2.Distance(points.Loop(i - 1).position, points[i].position) * points[i].curve) + points[i].position;
		points[i].handleN = (points[i].handleN - points[i].position) * (Vector2.Distance(points.Loop(i + 1).position, points[i].position) * points[i].curve) + points[i].position;
		points[i].median = vector;
		points[i].clockwise = clockwise;
	}

	public void StraightenHandles(int i)
	{
		Vector2 normalized = ((points[i].handleP - points[i].position).normalized + (points[i].handleN - points[i].position).normalized).normalized;
		if (normalized != Vector2.zero)
		{
			Vector2 vector = normalized.Rotate(-90f);
			Vector2 vector2 = normalized.Rotate(90f);
			if (Vector2.Distance(points[i].handleP, vector + points[i].position) < Vector2.Distance(points[i].handleP, vector2 + points[i].position))
			{
				points[i].handleP = vector * (points[i].handleP - points[i].position).magnitude + points[i].position;
				points[i].handleN = vector2 * (points[i].handleN - points[i].position).magnitude + points[i].position;
			}
			else
			{
				points[i].handleP = vector2 * (points[i].handleP - points[i].position).magnitude + points[i].position;
				points[i].handleN = vector * (points[i].handleN - points[i].position).magnitude + points[i].position;
			}
		}
	}

	private Vector2 LineIntersectionPoint(Vector2 l1s, Vector2 l1e, Vector2 l2s, Vector2 l2e)
	{
		float num = l1e.y - l1s.y;
		float num2 = l1s.x - l1e.x;
		float num3 = num * l1s.x + num2 * l1s.y;
		float num4 = l2e.y - l2s.y;
		float num5 = l2s.x - l2e.x;
		float num6 = num4 * l2s.x + num5 * l2s.y;
		float num7 = num * num5 - num4 * num2;
		if (num7 < 0.01f && num7 > -0.01f && l1e == l2s)
		{
			return l1e;
		}
		return new Vector2((num5 * num3 - num2 * num6) / num7, (num * num6 - num4 * num3) / num7);
	}

	private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float num = 1f - t;
		float num2 = t * t;
		float num3 = num * num;
		float num4 = num3 * num;
		float num5 = num2 * t;
		return num4 * p0 + 3f * num3 * t * p1 + 3f * num * num2 * p2 + num5 * p3;
	}
}
