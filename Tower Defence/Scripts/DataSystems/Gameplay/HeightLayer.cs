using Godot;
using System;
using DataSystems;

public partial class HeightLayer : DenseGridLayer<int>
{
	public int MinHeight;
	public int MaxHeight;
	
	public int this[int y, int x]
	{
		get => GetData(x, y);
		set 
		{
			SetData(x, y, value);
			Dirty = true;
		}
	}
	
	public int this[Vector2I index]
	{
		get => this[index.Y, index.X];
		set => this[index.Y, index.X] = value;
	}
	
	protected override void InitializeData()
	{
		base.InitializeData();
		ToolsInit();
	}
		
	partial void ToolsInit();
}
