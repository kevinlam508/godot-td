using Godot;
using System;

namespace DataSystems
{
	// Any DataLayer with info per tile
	public abstract class GridLayer<T> : GridDataLayer
	{
		public override void InitializeGridData(GridData globalData)
		{
			base.InitializeGridData(globalData);
			InitializeData();
		}
		
		protected abstract void InitializeData();
		
		protected abstract T GetData(int x, int y);
		protected abstract T GetData(Vector2I index);
		
		protected abstract void SetData(int x, int y, T value);
		protected abstract void SetData(Vector2I index, T value);
		
		protected abstract void Clear();
	}
}
