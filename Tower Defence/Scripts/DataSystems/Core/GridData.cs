using Godot;
using System;
using HexGrid;

namespace DataSystems
{
	public class GridData
	{
		public Vector2I GridSize { get; init; }
		
		public float TileSize { get; init; }
		public Vector2 OriginWorldPosition { get; init; }
		
		public bool IsIndexInBounds(int x, int y)
		{
			return 0 <= x && x < GridSize.X 
				&& 0 <= y && y < GridSize.Y;
		}
		
		public bool IsIndexInBounds(Vector2I index) => IsIndexInBounds(index.X, index.Y);
	
		public Vector2I GetGridIndexFromWorldPosition(Vector2 worldPosition)
		{
			return GridUtility.GetGridIndexFromWorldPosition
				(
					worldPosition,
					OriginWorldPosition,
					TileSize
				);
		}
		
		public Vector2 GetWorldPositionFromGridIndex(Vector2I index)
		{
			return GridUtility.GetWorldPositionFromGridIndex
				(
					index,
					OriginWorldPosition,
					TileSize
				);
		}
	}
}
