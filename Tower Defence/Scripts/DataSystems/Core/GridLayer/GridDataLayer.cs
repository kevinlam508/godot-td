using Godot;
using System;

namespace DataSystems
{
	// Simple layer to get grid data without additional data
	public class GridDataLayer : DataLayer, IGridDataInitialize
	{
		protected GridData _gridData;
		
		public Vector2I GridSize => _gridData.GridSize;
		public float TileSize => _gridData.TileSize;
		public Vector2 OriginWorldPosition => _gridData.OriginWorldPosition;
		
		public virtual void InitializeGridData(GridData data)
		{
			_gridData = data;
		}
		
		public bool IsIndexInBounds(int x, int y)
			=> _gridData.IsIndexInBounds(x, y);
		public bool IsIndexInBounds(Vector2I index) 
			=> _gridData.IsIndexInBounds(index);
		public Vector2I GetGridIndexFromWorldPosition(Vector2 worldPosition)
			=> _gridData.GetGridIndexFromWorldPosition(worldPosition);
		public Vector2 GetWorldPositionFromGridIndex(Vector2I index)
			=> _gridData.GetWorldPositionFromGridIndex(index);
	}
}
