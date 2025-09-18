using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Dreamteck.Splines.Primitives;
using UnityEngine;

namespace Dreamteck.Splines.IO;

public class SVG : SplineParser
{
	public enum Axis
	{
		X,
		Y,
		Z
	}

	internal class PathSegment
	{
		internal enum Type
		{
			Cubic,
			CubicShort,
			Quadratic,
			QuadraticShort
		}

		internal Vector3 startTangent = Vector3.zero;

		internal Vector3 endTangent = Vector3.zero;

		internal Vector3 endPoint = Vector3.zero;

		internal PathSegment(Vector2 s, Vector2 e, Vector2 c)
		{
			startTangent = s;
			endTangent = e;
			endPoint = c;
		}

		internal PathSegment()
		{
		}
	}

	public enum Element
	{
		All,
		Path,
		Polygon,
		Ellipse,
		Rectangle,
		Line
	}

	private CultureInfo culture = new CultureInfo("en-US");

	private NumberStyles style = NumberStyles.Any;

	private List<SplineDefinition> paths = new List<SplineDefinition>();

	private List<SplineDefinition> polygons = new List<SplineDefinition>();

	private List<SplineDefinition> ellipses = new List<SplineDefinition>();

	private List<SplineDefinition> rectangles = new List<SplineDefinition>();

	private List<SplineDefinition> lines = new List<SplineDefinition>();

	private List<Transformation> transformBuffer = new List<Transformation>();

	public SVG(string filePath)
	{
		if (!File.Exists(filePath))
		{
			return;
		}
		string text = Path.GetExtension(filePath).ToLower();
		fileName = Path.GetFileNameWithoutExtension(filePath);
		if (text != ".svg" && text != ".xml")
		{
			Debug.LogError("SVG Parsing ERROR: Wrong format. Please use SVG or XML");
			return;
		}
		XmlDocument xmlDocument = new XmlDocument
		{
			XmlResolver = null
		};
		try
		{
			xmlDocument.Load(filePath);
		}
		catch (XmlException ex)
		{
			Debug.LogError(ex.Message);
			return;
		}
		Read(xmlDocument);
	}

	public SVG(List<SplineComputer> computers)
	{
		paths = new List<SplineDefinition>(computers.Count);
		for (int i = 0; i < computers.Count; i++)
		{
			if (!(computers[i] == null))
			{
				Spline spline = new Spline(computers[i].type, computers[i].sampleRate)
				{
					points = computers[i].GetPoints()
				};
				if (spline.type != Spline.Type.Bezier && spline.type != Spline.Type.Linear)
				{
					spline.CatToBezierTangents();
				}
				if (computers[i].isClosed)
				{
					spline.Close();
				}
				paths.Add(new SplineDefinition(computers[i].name, spline));
			}
		}
	}

	public void Write(string filePath, Axis ax = Axis.Z)
	{
		XmlDocument xmlDocument = new XmlDocument();
		XmlElement xmlElement = xmlDocument.CreateElement("svg");
		foreach (SplineDefinition path in paths)
		{
			string text = "path";
			string text2 = "d";
			if (path.type == Spline.Type.Linear)
			{
				text2 = "points";
				text = ((!path.closed) ? "polyline" : "polygon");
			}
			XmlElement xmlElement2 = xmlDocument.CreateElement(text);
			XmlAttribute xmlAttribute = xmlDocument.CreateAttribute("id");
			xmlAttribute.Value = path.name;
			xmlElement2.Attributes.Append(xmlAttribute);
			xmlAttribute = xmlDocument.CreateAttribute(text2);
			if (path.type == Spline.Type.Linear)
			{
				xmlAttribute.Value = EncodePolygon(path, ax);
			}
			else
			{
				xmlAttribute.Value = EncodePath(path, ax);
			}
			xmlElement2.Attributes.Append(xmlAttribute);
			xmlAttribute = xmlDocument.CreateAttribute("stroke");
			xmlAttribute.Value = "black";
			xmlElement2.Attributes.Append(xmlAttribute);
			xmlAttribute = xmlDocument.CreateAttribute("stroke-width");
			xmlAttribute.Value = "3";
			xmlElement2.Attributes.Append(xmlAttribute);
			xmlAttribute = xmlDocument.CreateAttribute("fill");
			xmlAttribute.Value = "none";
			xmlElement2.Attributes.Append(xmlAttribute);
			xmlElement.AppendChild(xmlElement2);
		}
		XmlAttribute xmlAttribute2 = xmlDocument.CreateAttribute("version");
		xmlAttribute2.Value = "1.1";
		xmlElement.Attributes.Append(xmlAttribute2);
		xmlAttribute2 = xmlDocument.CreateAttribute("xmlns");
		xmlAttribute2.Value = "http://www.w3.org/2000/svg";
		xmlElement.Attributes.Append(xmlAttribute2);
		xmlDocument.AppendChild(xmlElement);
		xmlDocument.Save(filePath);
	}

	private Vector2 MapPoint(Vector3 original, Axis ax)
	{
		return ax switch
		{
			Axis.X => new Vector2(original.z, 0f - original.y), 
			Axis.Y => new Vector2(original.x, 0f - original.z), 
			Axis.Z => new Vector2(original.x, 0f - original.y), 
			_ => original, 
		};
	}

	private void Read(XmlDocument doc)
	{
		transformBuffer.Clear();
		Traverse(doc.ChildNodes);
	}

	private void Traverse(XmlNodeList nodes)
	{
		foreach (XmlNode node in nodes)
		{
			int num = 0;
			switch (node.Name)
			{
			case "g":
				num = ParseTransformation(node);
				break;
			case "path":
				num = ReadPath(node);
				break;
			case "polygon":
				num = ReadPolygon(node, closed: true);
				break;
			case "polyline":
				num = ReadPolygon(node, closed: false);
				break;
			case "ellipse":
				num = ReadEllipse(node);
				break;
			case "circle":
				num = ReadEllipse(node);
				break;
			case "line":
				num = ReadLine(node);
				break;
			case "rect":
				num = ReadRectangle(node);
				break;
			}
			Traverse(node.ChildNodes);
			if (num > 0)
			{
				transformBuffer.RemoveRange(transformBuffer.Count - num, num);
			}
		}
	}

	public List<SplineComputer> CreateSplineComputers(Vector3 position, Quaternion rotation, Element elements = Element.All)
	{
		List<SplineComputer> list = new List<SplineComputer>();
		if (elements == Element.All || elements == Element.Path)
		{
			foreach (SplineDefinition path in paths)
			{
				list.Add(path.CreateSplineComputer(position, rotation));
			}
		}
		if (elements == Element.All || elements == Element.Polygon)
		{
			foreach (SplineDefinition polygon in polygons)
			{
				list.Add(polygon.CreateSplineComputer(position, rotation));
			}
		}
		if (elements == Element.All || elements == Element.Ellipse)
		{
			foreach (SplineDefinition ellipsis in ellipses)
			{
				list.Add(ellipsis.CreateSplineComputer(position, rotation));
			}
		}
		if (elements == Element.All || elements == Element.Rectangle)
		{
			foreach (SplineDefinition rectangle in rectangles)
			{
				list.Add(rectangle.CreateSplineComputer(position, rotation));
			}
		}
		if (elements == Element.All || elements == Element.Line)
		{
			foreach (SplineDefinition line in lines)
			{
				list.Add(line.CreateSplineComputer(position, rotation));
			}
		}
		return list;
	}

	public List<Spline> CreateSplines(Element elements = Element.All)
	{
		List<Spline> list = new List<Spline>();
		if (elements == Element.All || elements == Element.Path)
		{
			foreach (SplineDefinition path in paths)
			{
				list.Add(path.CreateSpline());
			}
		}
		if (elements == Element.All || elements == Element.Polygon)
		{
			foreach (SplineDefinition polygon in polygons)
			{
				list.Add(polygon.CreateSpline());
			}
		}
		if (elements == Element.All || elements == Element.Ellipse)
		{
			foreach (SplineDefinition ellipsis in ellipses)
			{
				list.Add(ellipsis.CreateSpline());
			}
		}
		if (elements == Element.All || elements == Element.Rectangle)
		{
			foreach (SplineDefinition rectangle in rectangles)
			{
				list.Add(rectangle.CreateSpline());
			}
		}
		if (elements == Element.All || elements == Element.Line)
		{
			foreach (SplineDefinition line in lines)
			{
				list.Add(line.CreateSpline());
			}
		}
		return list;
	}

	private int ReadRectangle(XmlNode rectNode)
	{
		float result = 0f;
		float result2 = 0f;
		float result3 = 0f;
		float result4 = 0f;
		float result5 = -1f;
		float result6 = -1f;
		string attributeContent = GetAttributeContent(rectNode, "x");
		if (attributeContent == "ERROR")
		{
			return 0;
		}
		float.TryParse(attributeContent, style, culture, out result);
		attributeContent = GetAttributeContent(rectNode, "y");
		if (attributeContent == "ERROR")
		{
			return 0;
		}
		float.TryParse(attributeContent, style, culture, out result2);
		attributeContent = GetAttributeContent(rectNode, "width");
		if (attributeContent == "ERROR")
		{
			return 0;
		}
		float.TryParse(attributeContent, style, culture, out result3);
		attributeContent = GetAttributeContent(rectNode, "height");
		if (attributeContent == "ERROR")
		{
			return 0;
		}
		float.TryParse(attributeContent, style, culture, out result4);
		attributeContent = GetAttributeContent(rectNode, "rx");
		if (attributeContent != "ERROR")
		{
			float.TryParse(attributeContent, style, culture, out result5);
		}
		attributeContent = GetAttributeContent(rectNode, "ry");
		if (attributeContent != "ERROR")
		{
			float.TryParse(attributeContent, style, culture, out result6);
		}
		else
		{
			result6 = result5;
		}
		string text = GetAttributeContent(rectNode, "id");
		if (result5 == -1f && result6 == -1f)
		{
			Rectangle rectangle = new Rectangle();
			rectangle.offset = new Vector2(result + result3 / 2f, 0f - result2 - result4 / 2f);
			rectangle.size = new Vector2(result3, result4);
			if (text == "ERROR")
			{
				text = fileName + "_rectangle" + (rectangles.Count + 1);
			}
			buffer = new SplineDefinition(text, rectangle.CreateSpline());
		}
		else
		{
			RoundedRectangle roundedRectangle = new RoundedRectangle();
			roundedRectangle.offset = new Vector2(result + result3 / 2f, 0f - result2 - result4 / 2f);
			roundedRectangle.size = new Vector2(result3, result4);
			roundedRectangle.xRadius = result5;
			roundedRectangle.yRadius = result6;
			if (text == "ERROR")
			{
				text = fileName + "_roundedRectangle" + (rectangles.Count + 1);
			}
			buffer = new SplineDefinition(text, roundedRectangle.CreateSpline());
		}
		int result7 = ParseTransformation(rectNode);
		WriteBufferTo(rectangles);
		return result7;
	}

	private int ReadLine(XmlNode lineNode)
	{
		float result = 0f;
		float result2 = 0f;
		float result3 = 0f;
		float result4 = 0f;
		string attributeContent = GetAttributeContent(lineNode, "x1");
		if (attributeContent == "ERROR")
		{
			return 0;
		}
		float.TryParse(attributeContent, style, culture, out result);
		attributeContent = GetAttributeContent(lineNode, "y1");
		if (attributeContent == "ERROR")
		{
			return 0;
		}
		float.TryParse(attributeContent, style, culture, out result2);
		attributeContent = GetAttributeContent(lineNode, "x2");
		if (attributeContent == "ERROR")
		{
			return 0;
		}
		float.TryParse(attributeContent, style, culture, out result3);
		attributeContent = GetAttributeContent(lineNode, "y2");
		if (attributeContent == "ERROR")
		{
			return 0;
		}
		float.TryParse(attributeContent, style, culture, out result4);
		string text = GetAttributeContent(lineNode, "id");
		if (text == "ERROR")
		{
			text = fileName + "_line" + (ellipses.Count + 1);
		}
		buffer = new SplineDefinition(text, Spline.Type.Linear);
		buffer.position = new Vector2(result, 0f - result2);
		buffer.CreateLinear();
		buffer.position = new Vector2(result3, 0f - result4);
		buffer.CreateLinear();
		int result5 = ParseTransformation(lineNode);
		WriteBufferTo(lines);
		return result5;
	}

	private int ReadEllipse(XmlNode ellipseNode)
	{
		float result = 0f;
		float result2 = 0f;
		float result3 = 0f;
		float result4 = 0f;
		string attributeContent = GetAttributeContent(ellipseNode, "cx");
		if (attributeContent == "ERROR")
		{
			return 0;
		}
		float.TryParse(attributeContent, style, culture, out result);
		attributeContent = GetAttributeContent(ellipseNode, "cy");
		if (attributeContent == "ERROR")
		{
			return 0;
		}
		float.TryParse(attributeContent, style, culture, out result2);
		attributeContent = GetAttributeContent(ellipseNode, "r");
		string text = "circle";
		if (attributeContent == "ERROR")
		{
			text = "ellipse";
			attributeContent = GetAttributeContent(ellipseNode, "rx");
			if (attributeContent == "ERROR")
			{
				return 0;
			}
			float.TryParse(attributeContent, style, culture, out result3);
			attributeContent = GetAttributeContent(ellipseNode, "ry");
			if (attributeContent == "ERROR")
			{
				return 0;
			}
		}
		else
		{
			float.TryParse(attributeContent, style, culture, out result3);
			result4 = result3;
		}
		float.TryParse(attributeContent, style, culture, out result4);
		Ellipse ellipse = new Ellipse();
		ellipse.offset = new Vector2(result, 0f - result2);
		ellipse.xRadius = result3;
		ellipse.yRadius = result4;
		string text2 = GetAttributeContent(ellipseNode, "id");
		if (text2 == "ERROR")
		{
			text2 = fileName + "_" + text + (ellipses.Count + 1);
		}
		buffer = new SplineDefinition(text2, ellipse.CreateSpline());
		int result5 = ParseTransformation(ellipseNode);
		WriteBufferTo(ellipses);
		return result5;
	}

	private int ReadPolygon(XmlNode polyNode, bool closed)
	{
		string attributeContent = GetAttributeContent(polyNode, "points");
		if (attributeContent == "ERROR")
		{
			return 0;
		}
		List<float> list = ParseFloatArray(attributeContent);
		if (list.Count % 2 != 0)
		{
			Debug.LogWarning("There is an error with one of the polygon shapes.");
			return 0;
		}
		string text = GetAttributeContent(polyNode, "id");
		if (text == "ERROR")
		{
			text = fileName + (closed ? "_polygon " : "_polyline") + (polygons.Count + 1);
		}
		buffer = new SplineDefinition(text, Spline.Type.Linear);
		int num = list.Count / 2;
		for (int i = 0; i < num; i++)
		{
			buffer.position = new Vector2(list[2 * i], 0f - list[1 + 2 * i]);
			buffer.CreateLinear();
		}
		if (closed)
		{
			buffer.CreateClosingPoint();
			buffer.closed = true;
		}
		int result = ParseTransformation(polyNode);
		WriteBufferTo(polygons);
		return result;
	}

	private int ParseTransformation(XmlNode node)
	{
		string attributeContent = GetAttributeContent(node, "transform");
		if (attributeContent == "ERROR")
		{
			return 0;
		}
		List<Transformation> list = ParseTransformations(attributeContent);
		transformBuffer.AddRange(list);
		return list.Count;
	}

	private List<Transformation> ParseTransformations(string transformContent)
	{
		List<Transformation> list = new List<Transformation>();
		foreach (Match item in Regex.Matches(transformContent.ToLower(), "(?<function>translate|rotate|scale|skewx|skewy|matrix)\\s*\\((\\s*(?<param>-?\\s*\\d+(\\.\\d+)?)\\s*\\,*\\s*)+\\)"))
		{
			if (!item.Groups["function"].Success)
			{
				continue;
			}
			CaptureCollection captures = item.Groups["param"].Captures;
			switch (item.Groups["function"].Value)
			{
			case "translate":
				if (captures.Count >= 2)
				{
					list.Add(new Translate(new Vector2(float.Parse(captures[0].Value), float.Parse(captures[1].Value))));
				}
				break;
			case "rotate":
				if (captures.Count >= 1)
				{
					list.Add(new Rotate(float.Parse(captures[0].Value)));
				}
				break;
			case "scale":
				if (captures.Count >= 2)
				{
					list.Add(new Scale(new Vector2(float.Parse(captures[0].Value), float.Parse(captures[1].Value))));
				}
				break;
			case "skewx":
				if (captures.Count >= 1)
				{
					list.Add(new SkewX(float.Parse(captures[0].Value)));
				}
				break;
			case "skewy":
				if (captures.Count >= 1)
				{
					list.Add(new SkewY(float.Parse(captures[0].Value)));
				}
				break;
			case "matrix":
				if (captures.Count >= 6)
				{
					list.Add(new MatrixTransform(float.Parse(captures[0].Value), float.Parse(captures[1].Value), float.Parse(captures[2].Value), float.Parse(captures[3].Value), float.Parse(captures[4].Value), float.Parse(captures[5].Value)));
				}
				break;
			}
		}
		return list;
	}

	private int ReadPath(XmlNode pathNode)
	{
		string attributeContent = GetAttributeContent(pathNode, "d");
		if (attributeContent == "ERROR")
		{
			return 0;
		}
		string text = GetAttributeContent(pathNode, "id");
		if (text == "ERROR")
		{
			text = fileName + "_path " + (paths.Count + 1);
		}
		IEnumerable<string> enumerable = from t in Regex.Split(attributeContent, "(?=[A-Za-z])")
			where !string.IsNullOrEmpty(t)
			select t;
		int num = 0;
		foreach (string item in enumerable)
		{
			switch (item.Substring(0, 1).Single())
			{
			case 'M':
				PathStart(text, item, relative: false);
				num++;
				break;
			case 'm':
				PathStart(text, item, relative: true);
				num++;
				break;
			case 'Z':
				PathClose();
				break;
			case 'z':
				PathClose();
				break;
			case 'L':
				PathLineTo(item, relative: false);
				break;
			case 'l':
				PathLineTo(item, relative: true);
				break;
			case 'H':
				PathHorizontalLineTo(item, relative: false);
				break;
			case 'h':
				PathHorizontalLineTo(item, relative: true);
				break;
			case 'V':
				PathVerticalLineTo(item, relative: false);
				break;
			case 'v':
				PathVerticalLineTo(item, relative: true);
				break;
			case 'C':
				PathCurveTo(item, PathSegment.Type.Cubic, relative: false);
				break;
			case 'c':
				PathCurveTo(item, PathSegment.Type.Cubic, relative: true);
				break;
			case 'S':
				PathCurveTo(item, PathSegment.Type.CubicShort, relative: false);
				break;
			case 's':
				PathCurveTo(item, PathSegment.Type.CubicShort, relative: true);
				break;
			case 'Q':
				PathCurveTo(item, PathSegment.Type.Quadratic, relative: false);
				break;
			case 'q':
				PathCurveTo(item, PathSegment.Type.Quadratic, relative: true);
				break;
			case 'T':
				PathCurveTo(item, PathSegment.Type.QuadraticShort, relative: false);
				break;
			case 't':
				PathCurveTo(item, PathSegment.Type.QuadraticShort, relative: true);
				break;
			case 'A':
				PathArcTo(item, relative: false);
				break;
			case 'a':
				PathArcTo(item, relative: true);
				break;
			}
		}
		if (buffer != null)
		{
			WriteBufferTo(paths);
		}
		int result = ParseTransformation(pathNode);
		for (int num2 = paths.Count - 1; num2 >= paths.Count - num; num2--)
		{
			paths[num2].Transform(transformBuffer);
		}
		return result;
	}

	private void PathStart(string name, string coords, bool relative)
	{
		if (buffer != null)
		{
			WriteBufferTo(paths);
		}
		buffer = new SplineDefinition(name, Spline.Type.Bezier);
		if (relative)
		{
			buffer.position = paths.Last().GetLastPoint().position;
		}
		Vector2[] array = ParseVector2(coords);
		foreach (Vector3 vector in array)
		{
			if (relative)
			{
				buffer.position += vector;
			}
			else
			{
				buffer.position = vector;
			}
			buffer.CreateLinear();
		}
	}

	private void PathClose()
	{
		buffer.CreateClosingPoint();
		buffer.closed = true;
	}

	private void PathLineTo(string coords, bool relative)
	{
		Vector2[] array = ParseVector2(coords);
		foreach (Vector3 vector in array)
		{
			if (relative)
			{
				buffer.position += vector;
			}
			else
			{
				buffer.position = vector;
			}
			buffer.CreateLinear();
		}
	}

	private void PathHorizontalLineTo(string coords, bool relative)
	{
		float[] array = ParseFloat(coords);
		foreach (float num in array)
		{
			if (relative)
			{
				buffer.position.x += num;
			}
			else
			{
				buffer.position.x = num;
			}
			buffer.CreateLinear();
		}
	}

	private void PathVerticalLineTo(string coords, bool relative)
	{
		float[] array = ParseFloat(coords);
		foreach (float num in array)
		{
			if (relative)
			{
				buffer.position.y -= num;
			}
			else
			{
				buffer.position.y = 0f - num;
			}
			buffer.CreateLinear();
		}
	}

	private void PathCurveTo(string coords, PathSegment.Type type, bool relative)
	{
		PathSegment[] array = ParsePathSegment(coords, type);
		for (int i = 0; i < array.Length; i++)
		{
			SplinePoint lastPoint = buffer.GetLastPoint();
			lastPoint.type = SplinePoint.Type.Broken;
			Vector3 position = lastPoint.position;
			Vector3 endPoint = array[i].endPoint;
			Vector3 vector = array[i].startTangent;
			Vector3 vector2 = array[i].endTangent;
			switch (type)
			{
			case PathSegment.Type.CubicShort:
				vector = position - lastPoint.tangent;
				break;
			case PathSegment.Type.Quadratic:
				buffer.tangent = array[i].startTangent;
				vector = position + 2f / 3f * (buffer.tangent - position);
				vector2 = endPoint + 2f / 3f * (buffer.tangent - endPoint);
				break;
			case PathSegment.Type.QuadraticShort:
			{
				Vector3 vector3 = position + (position - buffer.tangent);
				vector = position + 2f / 3f * (vector3 - position);
				vector2 = endPoint + 2f / 3f * (vector3 - endPoint);
				break;
			}
			}
			if (type == PathSegment.Type.CubicShort || type == PathSegment.Type.QuadraticShort)
			{
				lastPoint.type = SplinePoint.Type.SmoothMirrored;
			}
			else if (relative)
			{
				lastPoint.SetTangent2Position(position + vector);
			}
			else
			{
				lastPoint.SetTangent2Position(vector);
			}
			buffer.SetLastPoint(lastPoint);
			if (relative)
			{
				buffer.position += endPoint;
				buffer.tangent = position + vector2;
			}
			else
			{
				buffer.position = endPoint;
				buffer.tangent = vector2;
			}
			buffer.CreateBroken();
		}
	}

	private void PathArcTo(string coords, bool relative)
	{
		float[] array = ParseFloat(coords);
		float rx = array[0];
		float ry = array[1];
		float num = array[2] * (MathF.PI / 180f);
		bool fa = array[3] > 0.5f;
		bool fs = array[4] > 0.5f;
		float x = array[5];
		float y = array[6];
		SplinePoint lastPoint = buffer.GetLastPoint();
		lastPoint.type = SplinePoint.Type.Broken;
		Vector3 position = lastPoint.position;
		position.y *= -1f;
		Vector3 vector = new Vector3(x, y, 0f);
		if (relative)
		{
			vector += position;
		}
		CalculateEllipseParams(position, vector, num, rx, ry, fa, fs, out var c, out var theta, out var sweepTheta, out var adjustedRx, out var adjustedRy);
		c.y *= -1f;
		Spline spline = new Ellipse
		{
			offset = c,
			rotation = new Vector3(0f, 0f, -90f - num * 57.29578f),
			xRadius = adjustedRx,
			yRadius = adjustedRy
		}.CreateSpline();
		SplinePoint[] points = spline.points;
		SplinePoint splinePoint = points[1];
		points[1] = points[3];
		points[3] = splinePoint;
		for (int i = 0; i < points.Length; i++)
		{
			FlipTangents(ref points[i]);
		}
		float f = theta / (MathF.PI * 2f);
		f = ModP(f, 1f);
		float num2 = sweepTheta / (MathF.PI * 2f);
		float num3 = f + num2;
		double[] arcSegmentPercentages = GetArcSegmentPercentages(f, num3);
		for (int j = 1; j < arcSegmentPercentages.Length; j++)
		{
			double num4 = arcSegmentPercentages[j - 1];
			double num5 = arcSegmentPercentages[j];
			double num6 = num5 - num4;
			int num7 = Math.Sign(num6);
			num6 *= (double)num7;
			if (!(num6 < 0.0001))
			{
				double num8 = 0.75 / num6;
				num5 = ModP(num5, 1.0);
				num4 = ModP(num4, 1.0);
				Vector3 point = Vector3.zero;
				Vector3 tangent = Vector3.zero;
				Vector3 tangent2 = Vector3.zero;
				spline.EvaluatePosition(ref point, num5);
				spline.EvaluateTangent(ref tangent, num5);
				tangent *= (float)num7;
				tangent /= (float)num8;
				buffer.position = point;
				buffer.tangent = point - tangent;
				spline.EvaluateTangent(ref tangent2, num4);
				tangent2 *= (float)num7;
				tangent2 /= (float)num8;
				lastPoint = buffer.GetLastPoint();
				lastPoint.type = SplinePoint.Type.Broken;
				lastPoint.SetTangent2Position(lastPoint.position + tangent2);
				buffer.SetLastPoint(lastPoint);
				buffer.CreateBroken();
			}
		}
	}

	private void FlipTangents(ref SplinePoint point)
	{
		Vector3 tangent = point.tangent;
		point.tangent = point.tangent2;
		point.tangent2 = tangent;
	}

	private void CalculateEllipseParams(Vector2 p0, Vector2 p1, float phi, float rx, float ry, bool fa, bool fs, out Vector2 c, out float theta1, out float sweepTheta, out float adjustedRx, out float adjustedRy)
	{
		float num = Mathf.Sin(phi);
		float num2 = Mathf.Cos(phi);
		float num3 = num2 * (p0.x - p1.x) / 2f + num * (p0.y - p1.y) / 2f;
		float num4 = (0f - num) * (p0.x - p1.x) / 2f + num2 * (p0.y - p1.y) / 2f;
		float num5 = num3 * num3;
		float num6 = num4 * num4;
		float num7 = rx * rx;
		float num8 = ry * ry;
		rx = Mathf.Abs(rx);
		ry = Mathf.Abs(ry);
		float num9 = num5 / num7 + num6 / num8;
		if (num9 > 1f)
		{
			float num10 = Mathf.Sqrt(num9);
			rx = num10 * rx;
			ry = num10 * ry;
			num7 = rx * rx;
			num8 = ry * ry;
		}
		adjustedRx = rx;
		adjustedRy = ry;
		float num11 = ((fa != fs) ? 1 : (-1));
		float num12 = Mathf.Sqrt((num7 * num8 - num7 * num6 - num8 * num5) / (num7 * num6 + num8 * num5)) * num11;
		float num13 = num12 * (rx * num4) / ry;
		float num14 = num12 * ((0f - ry) * num3) / rx;
		c = new Vector2(num2 * num13 - num * num14 + (p0.x + p1.x) / 2f, num * num13 + num2 * num14 + (p0.y + p1.y) / 2f);
		theta1 = VectorAngle(new Vector2(1f, 0f), new Vector2((num3 - num13) / rx, (num4 - num14) / ry));
		sweepTheta = VectorAngle(new Vector2((num3 - num13) / rx, (num4 - num14) / ry), new Vector2((0f - num3 - num13) / rx, (0f - num4 - num14) / ry));
		sweepTheta *= 57.29578f;
		sweepTheta %= 360f;
		if (!fs && sweepTheta > 0f)
		{
			sweepTheta -= 360f;
		}
		if (fs && sweepTheta < 0f)
		{
			sweepTheta += 360f;
		}
		sweepTheta *= MathF.PI / 180f;
	}

	private double[] GetArcSegmentPercentages(double start, double end)
	{
		List<double> list = new List<double>();
		bool flag = start > end;
		if (flag)
		{
			double num = start;
			start = end;
			end = num;
		}
		list.Add(start);
		double num2 = Math.Ceiling(start * 4.0) * 0.25;
		if (num2 > end)
		{
			list.Add(end);
			return ReturnPercentage(flag, list);
		}
		if (start < num2)
		{
			list.Add(num2);
		}
		double num3;
		for (num3 = num2 + 0.25; num3 <= end; num3 += 0.25)
		{
			list.Add(num3);
		}
		num3 -= 0.25;
		if (num3 < end)
		{
			list.Add(end);
		}
		return ReturnPercentage(flag, list);
	}

	private double[] ReturnPercentage(bool swap, List<double> percentages)
	{
		double[] array = new double[percentages.Count];
		for (int i = 0; i < percentages.Count; i++)
		{
			int index = (swap ? (percentages.Count - 1 - i) : i);
			double num = percentages[index];
			array[i] = num;
		}
		return array;
	}

	private float VectorAngle(Vector2 u, Vector2 v)
	{
		float num = ((!(u.x * v.y - u.y * v.x < 0f)) ? 1 : (-1));
		float num2 = Mathf.Sqrt(u.x * u.x + u.y * u.y);
		float num3 = Mathf.Sqrt(v.x * v.x + v.y * v.y);
		float num4 = u.x * v.x + u.y * v.y;
		return num * Mathf.Acos(num4 / (num2 * num3));
	}

	private float ModP(float f, float div)
	{
		return (f % div + div) % div;
	}

	private double ModP(double d, double div)
	{
		return (d % div + div) % div;
	}

	private void WriteBufferTo(List<SplineDefinition> list)
	{
		buffer.Transform(transformBuffer);
		list.Add(buffer);
		buffer = null;
	}

	private PathSegment[] ParsePathSegment(string coord, PathSegment.Type type)
	{
		List<float> list = ParseFloatArray(coord.Substring(1));
		int num = 0;
		switch (type)
		{
		case PathSegment.Type.Cubic:
			num = list.Count / 6;
			break;
		case PathSegment.Type.Quadratic:
			num = list.Count / 4;
			break;
		case PathSegment.Type.CubicShort:
			num = list.Count / 4;
			break;
		case PathSegment.Type.QuadraticShort:
			num = list.Count / 2;
			break;
		}
		if (num == 0)
		{
			Debug.Log("Error in " + coord + " " + type);
			return new PathSegment[1]
			{
				new PathSegment()
			};
		}
		PathSegment[] array = new PathSegment[num];
		for (int i = 0; i < num; i++)
		{
			switch (type)
			{
			case PathSegment.Type.Cubic:
				array[i] = new PathSegment(new Vector2(list[6 * i], 0f - list[1 + 6 * i]), new Vector2(list[2 + 6 * i], 0f - list[3 + 6 * i]), new Vector2(list[4 + 6 * i], 0f - list[5 + 6 * i]));
				break;
			case PathSegment.Type.Quadratic:
				array[i] = new PathSegment(new Vector2(list[4 * i], 0f - list[1 + 4 * i]), Vector2.zero, new Vector2(list[2 + 4 * i], 0f - list[3 + 4 * i]));
				break;
			case PathSegment.Type.CubicShort:
				array[i] = new PathSegment(Vector2.zero, new Vector2(list[4 * i], 0f - list[1 + 4 * i]), new Vector2(list[2 + 4 * i], 0f - list[3 + 4 * i]));
				break;
			case PathSegment.Type.QuadraticShort:
				array[i] = new PathSegment(Vector2.zero, Vector2.zero, new Vector2(list[4 * i], 0f - list[1 + 4 * i]));
				break;
			}
		}
		return array;
	}

	private string EncodePath(SplineDefinition definition, Axis ax)
	{
		string text = "M";
		for (int i = 0; i < definition.pointCount; i++)
		{
			SplinePoint splinePoint = definition.points[i];
			Vector3 vector = MapPoint(splinePoint.tangent, ax);
			Vector3 vector2 = MapPoint(splinePoint.position, ax);
			if (i == 0)
			{
				text = text + vector2.x + "," + vector2.y;
				continue;
			}
			Vector3 vector3 = MapPoint(definition.points[i - 1].tangent2, ax);
			text = text + "C" + vector3.x + "," + vector3.y + "," + vector.x + "," + vector.y + "," + vector2.x + "," + vector2.y;
		}
		if (definition.closed)
		{
			text += "z";
		}
		return text;
	}

	private string EncodePolygon(SplineDefinition definition, Axis ax)
	{
		string text = "";
		for (int i = 0; i < definition.pointCount; i++)
		{
			Vector3 vector = MapPoint(definition.points[i].position, ax);
			if (text != "")
			{
				text += ",";
			}
			text = text + vector.x + "," + vector.y;
		}
		return text;
	}

	private string GetAttributeContent(XmlNode node, string attributeName)
	{
		for (int i = 0; i < node.Attributes.Count; i++)
		{
			if (node.Attributes[i].Name == attributeName)
			{
				return node.Attributes[i].InnerText;
			}
		}
		return "ERROR";
	}
}
