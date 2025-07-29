using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

namespace HexGrid
{	
	public enum GridDirection
	{
		Left,
		UpLeft,
		UpRight,
		Right,
		DownRight,
		DownLeft,
	}
	
	public enum GridRotation
	{
		None,
		OneTick,
		TwoTick,
		ThreeTick,
		FourTick,
		FiveTick,
		FullCircle
	}
	
	/*
	 * Utils for traversing a 2d array as a hex grid
	 * 
	 *     ^ O O O O O O
	 *    / O O O O O O 
	 *   y O O O O O O 
	 *     x - >
	 */
	public static class GridUtility
	{
		public const int DegreesPerRotation = 60;
		
		public static readonly Vector2I[] DirectionIndexOffset = 
		{
			new Vector2I(-1,  0), // Left
			new Vector2I(-1,  1), // Up Left
			new Vector2I( 0,  1), // Up Right
			new Vector2I( 1,  0), // Right
			new Vector2I( 1, -1), // Down Right
			new Vector2I( 0, -1), // Down Left
		};
		
		public static readonly Vector2 YIndexToWorld = new Vector2(
				Mathf.Cos(Mathf.Pi / 3),
				Mathf.Sin(Mathf.Pi / 3)
			);

		public static readonly Vector2[] DirectionUnitVectors = 
			Array.ConvertAll(
				DirectionIndexOffset,
				(Vector2I offset) => 
				{
					return GridToUnitWorldSpace(offset).Normalized();
				}
			);
		
		// Convert coordinates using (1, 0) and (0, 1) as basis vectors
		// to coordinates using (1, 0) and YIndexToWorld as basis vectors
		public static Vector2 UnitWorldToGridSpace(Vector2 worldPosition)
		{
			float y = worldPosition.Y / YIndexToWorld.Y;
			float x = worldPosition.X - (y * YIndexToWorld.X);
			return new Vector2(x, y);
		}
		
		// Convert coordinates as using (1, 0) and YIndexToWorld as basis vectors
		public static Vector2 GridToUnitWorldSpace(Vector2 gridIndex)
		{
			return new Vector2(gridIndex.X, 0) 
				+ (gridIndex.Y * YIndexToWorld); 
		}
		
		// Special rounded from https://www.redblobgames.com/grids/hexagons/#rounding
		// Essentially hex coords can be expressed as x + y + z = 0 
		// (z is just dropped here), but rounding can break that. So 
		// re-align the value with the largest change
		public static Vector2I RoundToHexIndex(Vector2 gridPosition)
		{
			float z = -gridPosition.Y - gridPosition.X;
			
			int roundedX = (int)Mathf.Round(gridPosition.X);
			int roundedY = (int)Mathf.Round(gridPosition.Y);
			int roundedZ = (int)Mathf.Round(z);
			
			float xDiff = Mathf.Abs(gridPosition.X - roundedX);
			float yDiff = Mathf.Abs(gridPosition.Y - roundedY);
			float zDiff = Mathf.Abs(z - roundedZ);
			
			if(xDiff > yDiff && xDiff > zDiff)
			{
				roundedX = -roundedY - roundedZ;
			}
			else if(yDiff > zDiff)
			{
				roundedY = -roundedX - roundedZ;
			}
			
			return new Vector2I(roundedX, roundedY);
		}
		
		public static GridDirection? IndexOffsetToDirection(Vector2I offset)
			=> IndexOffsetToDirection(offset.X, offset.Y);
		
		public static GridDirection? IndexOffsetToDirection(int xOffset, int yOffset)
		{
			for (int i = 0; i < DirectionIndexOffset.Length; i++)
			{
				(int x, int y) = DirectionIndexOffset[i];
				
				// Along only x or y -> nonzero offset must have same sign
				if (x == 0 && xOffset == 0 && yOffset / y > 0)
				{
					return (GridDirection)i;
				}
				else if (y == 0 && yOffset == 0 && xOffset / x > 0)
				{
					return (GridDirection)i;
				}
				// Both non-zero -> ratio must be positive and the same for both
				else if (x != 0 && y != 0)
				{
					int xRatio = xOffset / x;
					int yRatio = yOffset / y;
					if (xRatio > 0 && xRatio == yRatio)
					{
						return (GridDirection)i;
					}
				}
			}
			return null;
		}
		
		public static Vector2I RotatePointAroundOrigin(Vector2I point, GridRotation rotations)
		{
			// Clamp rotations
			rotations = (GridRotation)((int)rotations % (int)GridRotation.FullCircle);
			if (rotations < 0)
			{
				rotations += (int)GridRotation.FullCircle;
			}
			
			// Coords follow x + y + z = 0, so
			// 1 rotation of (x, y, z) => (-y, -z, -x)
			// Apply iteratively for more rotations. But for
			// performanc, iterations are flattened
			int z = -point.X - point.Y;
			return rotations switch {
				GridRotation.OneTick => new Vector2I(-point.Y, -z),
				GridRotation.TwoTick => new Vector2I(z, point.X),
				GridRotation.ThreeTick => new Vector2I(-point.X, -point.Y),
				GridRotation.FourTick => new Vector2I(point.Y, z),
				GridRotation.FiveTick => new Vector2I(-z, -point.X),
				_ => point
			};
		}
		
		public static Vector2I GetGridIndexFromWorldPosition(Vector2 worldPosition, Vector2 originWorldPosition, float tileSize)
		{
			Vector2 normalizedPosition = (worldPosition - originWorldPosition) / tileSize;
			Vector2 indexPosition = GridUtility.UnitWorldToGridSpace(normalizedPosition);
			
			Vector2I index = GridUtility.RoundToHexIndex(indexPosition);
			return index;
		}
		
		public static Vector2 GetWorldPositionFromGridIndex(Vector2I gridIndex, Vector2 originWorldPosition, float tileSize)
		{
			Vector2 unitWorldPosition = GridToUnitWorldSpace(gridIndex);
			return originWorldPosition + unitWorldPosition * tileSize;
		}
		
		// Returns direction for the side interested with, null otherwise
		// returns float of scale to multiple direction with to get intersect point
		public static (GridDirection?, float) GetTileExitPoint(Vector2 tileCenter, float tileSize, 
			Vector2 start, Vector2 direction)
		{
			float intersectTime = float.MaxValue;
			GridDirection? intersectDirection = null;
			for (int i = 0; i < DirectionUnitVectors.Length; i++)
			{
				// Not going towards that side, can't exit that way
				Vector2 vectorToSide = DirectionUnitVectors[i];
				if (direction.Dot(vectorToSide) <= 0)
				{
					continue;
				}
				
				// Find intersection point
				Vector2 sideCenter = tileCenter + vectorToSide * (tileSize / 2);
				Vector2 sideRay = vectorToSide.Orthogonal();
				Transform2D matrix = new Transform2D
					(
						direction,
						sideRay,
						Vector2.Zero
					);
				Vector2 intersectionScalar = matrix.AffineInverse() 
					* (sideCenter - start);
				
				// Determine if intersect was actually within tile
				float halfSideLength = (tileSize / 2) / Mathf.Sqrt(3);
				if (Mathf.Abs(intersectionScalar.Y) > halfSideLength)
				{
					continue;
				}
				
				// Determine earliest intersection
				float time = intersectionScalar.X;
				if (time < intersectTime)
				{
					intersectTime = time;
					intersectDirection = (GridDirection)i;
				}
			}
			
			return (intersectDirection, intersectTime);
		}
		
		#region Adjacency
		public struct AdjacencyEnumerator : IEnumerator<Vector2I>
		{
			private Vector2I _center;
			private int _offsetIndex = -1;
			
			public AdjacencyEnumerator(Vector2I center)
			{
				_center = center;
			}
			
			public Vector2I Current 
			{
				get
				{
					Vector2I offset = DirectionIndexOffset[_offsetIndex];
					return _center + offset;
				}
			}
			
			object IEnumerator.Current => Current;
			
			public bool MoveNext()
			{
				_offsetIndex++;
				return _offsetIndex < DirectionIndexOffset.Length;
			}
			
			public void Reset()
			{
				_offsetIndex = -1;
			}
			
			public void Dispose(){}
			
			public IEnumerator<Vector2I> GetEnumerator() => this;
		}
		
		public static AdjacencyEnumerator GetAdjacencies(Vector2I center) 
			=> new AdjacencyEnumerator(center);
			
		public static AdjacencyEnumerator GetAdjacencies(int x, int y) 
			=> new AdjacencyEnumerator(new Vector2I(x, y));
		#endregion
		
		#region Line
		public struct LineEnumerator : IEnumerator<Vector2I>
		{
			private Vector2I _start;
			private Vector2I _offset;
			private int _offsetMultiplier = 0;
			private int _maxSteps;
			
			public LineEnumerator(int x, int y, GridDirection direction, int maxSteps)
			{
				_start = new Vector2I(x, y);
				_offset = DirectionIndexOffset[(int)direction];
				_maxSteps = maxSteps;
			}
			
			public Vector2I Current 
			{
				get
				{
					return _start + (_offset * _offsetMultiplier);
				}
			}
			
			object IEnumerator.Current => Current;
			
			public bool MoveNext()
			{
				_offsetMultiplier++;
				return _maxSteps == 0 || _offsetMultiplier < _maxSteps;
			}
			
			public void Reset()
			{
				_offsetMultiplier = 0;
			}
			
			public void Dispose(){}
			
			public IEnumerator<Vector2I> GetEnumerator() => this;
		}
		
		public static LineEnumerator GetLineEnumerator(int x, int y, GridDirection direction, int maxSteps = 0) 
			=> new LineEnumerator(x, y, direction, maxSteps);
		#endregion
	}
}
