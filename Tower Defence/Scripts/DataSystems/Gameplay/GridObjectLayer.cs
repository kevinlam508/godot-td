using Godot;
using System;
using System.Collections.Generic;
using HexGrid;
using DataSystems;

public class GridObjectLayer : SparseGridLayer<GridObject>, IRegistrar
{
	private HashSet<GridObject> _objects = new HashSet<GridObject>();
	
	public bool IsOccupied(Vector2I index) => IsOccupied(index.X, index.Y);
	public bool IsOccupied(int x, int y)
	{
		return IsIndexInBounds(x, y) && GetData(x, y) != null;
	}
	
	// Can walk through space if there's no object or object is permeable
	public bool IsPermeable(Vector2I index) => IsPermeable(index.X, index.Y);
	public bool IsPermeable(int x, int y)
	{
		return IsIndexInBounds(x, y)
			 && (GetData(x, y)?.IsPermeable ?? true);
	}
	
	public bool CanPlaceGridObject(GridObject gridObject)
	{
		foreach(Vector2I index in gridObject.Tiles)
		{
			if (!IsIndexInBounds(index))
			{
				return false;
			}
					
			if (GetData(index) != null)
			{
				return false;
			}
		}
		
		return true;
	}
	
	public bool TryAddGridObject(GridObject gridObject)
	{
		if (!CanPlaceGridObject(gridObject))
		{
			return false;
		}
		
		_objects.Add(gridObject);
		foreach(Vector2I index in gridObject.Tiles)
		{
			SetData(index, gridObject);
		}
		
		Dirty = true;
		return true;
	}
	
	public bool TryRemoveGridObject(GridObject gridObject)
	{
		if (!_objects.Remove(gridObject))
		{
			return false;
		}
		
		foreach(Vector2I index in gridObject.Tiles)
		{
			SetData(index, null);
		}
		
		Dirty = true;
		return true;
	}
	
	void IRegistrar.Register(object o)
	{
		if(o is GridObject go)
		{
			TryAddGridObject(go);
		}
	}
	
	void IRegistrar.Unregister(object o)
	{
		if(o is GridObject go)
		{
			TryRemoveGridObject(go);
		}
	}
}
