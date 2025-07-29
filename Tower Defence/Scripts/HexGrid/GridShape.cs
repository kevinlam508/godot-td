using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

namespace HexGrid
{	
	[Tool]
	[GlobalClass]
	public partial class GridShape : Resource
	{
		public Array<Vector2I> _points = new Array<Vector2I>();
		[ExportGroup("Shape")]
		[Export] public Array<Vector2I> Points
		{
			get => _points;
			set 
			{
				_points = MakeUnique(value);
			}
		}
		
		public bool IsPointInShape(Vector2I point)
		{
			return _points.IndexOf(point) >= 0;
		}
		
		public Vector2I GetMaxPoint()
		{
			Vector2I result = new Vector2I();
			foreach(Vector2I point in _points)
			{
				result.X = Mathf.Max(point.X, result.X);
				result.Y = Mathf.Max(point.Y, result.Y);
			}
			return result;
		}
		
		private static Array<T> MakeUnique<[MustBeVariant] T>(Array<T> points)
		{
			// Find and move distinct elements to the front
			HashSet<T> uniquePoints = new HashSet<T>();
			int nextIndex = 0;
			int searchOffset = 0;
			while(nextIndex + searchOffset < points.Count)
			{
				int toCheck = nextIndex + searchOffset;
				if(uniquePoints.Add(points[toCheck]))
				{
					// Swap new distinct to front if there was
					// a previous non-distinct
					if (searchOffset > 0)
					{
						points[nextIndex] = points[toCheck];
					}
					nextIndex++;
				}
				else
				{
					searchOffset++;
				}
			}
			
			// Trim duplicates
			if (searchOffset > 0)
			{
				points.Resize(nextIndex);
			}
			
			return points;
		}
	}
}
