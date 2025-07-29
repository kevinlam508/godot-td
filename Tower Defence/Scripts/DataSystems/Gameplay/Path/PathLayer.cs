using Godot;
using System;
using HexGrid;
using DataSystems;

public abstract class PathLayer : DenseGridLayer<GridDirection?>
{
	public GridDirection? this[int y, int x]
	{
		get => GetData(x, y);
		set 
		{
			SetData(x, y, value);
			Dirty = true;
		}
	}
	
	public GridDirection? this[Vector2I index]
	{
		get => this[index.Y, index.X];
		set => this[index.Y, index.X] = value;
	}
}
