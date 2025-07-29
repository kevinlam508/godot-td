using Godot;
using System;
using System.Collections.Generic;

namespace DataSystems
{
	public abstract class SparseGridLayer<T> : GridLayer<T>, IDisposable
	{
		protected Dictionary<Vector2I, T> _grid = new Dictionary<Vector2I, T>();

		protected virtual T DefaultValue => default(T);

		protected override void InitializeData() {}

		protected override T GetData(int x, int y) 
		{
			return GetData(new Vector2I(x, y));
		}
		
		protected override T GetData(Vector2I index)
		{
			if (!IsIndexInBounds(index))
			{
				throw new IndexOutOfRangeException();	
			}
			
			if (_grid.TryGetValue(index, out T value))
			{	
				return value;
			}
			return DefaultValue;
		}
		
		protected override void SetData(int x, int y, T value)
		{
			Vector2I index = new Vector2I(x, y);
			SetData(index, value);
		}
		
		protected override void SetData(Vector2I index, T value)
		{
			if (!IsIndexInBounds(index))
			{
				throw new IndexOutOfRangeException();	
			}
			
			if (value.Equals(DefaultValue))
			{
				_grid.Remove(index);
			}
			else
			{
				_grid[index] = value;
			}
		}
		
		protected override void Clear()
		{
			_grid.Clear();
		}
		
		// Precaution to release any Godot ref counted
		void IDisposable.Dispose()
		{
			_grid = null;
		}
	}
}
