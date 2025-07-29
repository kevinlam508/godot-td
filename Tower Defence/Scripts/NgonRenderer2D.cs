using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class NgonRenderer2D : Node2D
{
	private static Dictionary<int, Dictionary<float, Vector2[]>> ShapeCache
		= new Dictionary<int, Dictionary<float, Vector2[]>>();
	
	private Color _color = Colors.White;
	[Export] 
	public Color Color
	{
		get => _color;
		set
		{
			_color = value;
			QueueRedraw();
		}
	}
	
	// Distance from center to side
	private float _size = 5;
	[Export(PropertyHint.Range, "1,100,or_greater")] 
	public float Size
	{
		get => _size;
		set
		{
			_size = value;
			QueueRedraw();
		}
	}
	
	private int _sides = 6;
	[Export(PropertyHint.Range, "3,30,or_greater")] 
	public int Sides
	{
		get => _sides;
		set
		{
			_sides = value;
			QueueRedraw();
		}
	}
	
	private bool _showOutline = true;
	[Export]
	public bool ShowOutline
	{
		get  => _showOutline;
		set
		{
			_showOutline = value;
			QueueRedraw();
		}
	}
	
	public override void _Draw()
	{
		Vector2[] coords = GetShape(Sides, Size);
		DrawColoredPolygon(coords, _color);
		if (_showOutline)
		{
			DrawPolyline(coords, Colors.Black);
		}
	}
	
	private static Vector2[] GetShape(int sides, float size)
	{
		if (ShapeCache.TryGetValue(sides, out var sizeCache)
			&& sizeCache.TryGetValue(size, out var shape))
		{
			return shape;
		}
		
		if (sizeCache == null)
		{
			ShapeCache.Add(sides, new Dictionary<float, Vector2[]>());
		}
		
		float firstCornerRad = Mathf.Pi / 2;
		float radsBetweenCorners = Mathf.Pi * 2 / sides;
		float distanceToCorner = size / Mathf.Cos(radsBetweenCorners / 2);
		Vector2[] coords = new Vector2[sides];
		for(int i = 0; i < sides; i++)
		{
			coords[i] = new Vector2(
					Mathf.Cos(firstCornerRad + radsBetweenCorners * i),
					Mathf.Sin(firstCornerRad + radsBetweenCorners * i)
				) * distanceToCorner;
		}
		
		ShapeCache[sides][size] = coords;
		return coords;
	}
}
