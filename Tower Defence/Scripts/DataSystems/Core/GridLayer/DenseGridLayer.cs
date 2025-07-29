using Godot;
using System;

namespace DataSystems
{
	public abstract class DenseGridLayer<T> : GridLayer<T>, IDisposable
	{
		protected T[,] _grid;
		
		protected override void InitializeData()
		{
			_grid = new T[GridSize.Y, GridSize.X];
		}
		
		protected override T GetData(int x, int y) => _grid[y, x];
		protected override T GetData(Vector2I index) => GetData(index.X, index.Y);
		
		protected override void SetData(int x, int y, T value) => _grid[y, x] = value;
		protected override void SetData(Vector2I index, T value) => SetData(index.X, index.Y, value);
	
		protected override void Clear()
		{
			for (int y = 0; y < GridSize.Y; y++)
			{
				for (int x = 0; x < GridSize.X; x++)
				{
					_grid[y, x] = default(T);
				}
			}
		}
	
		// Precaution to release any Godot ref counted
		void IDisposable.Dispose()
		{
			_grid = null;
		}
	}
}
