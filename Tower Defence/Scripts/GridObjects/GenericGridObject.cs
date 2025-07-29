using Godot;
using System;
using System.Collections.Generic;
using HexGrid;

public partial class GenericGridObject : GridObject
{
	[Export] private PackedScene _tileRenderer;
	
	[Export] public GridShape Shape
	{
		get => _shape;
		set => _shape = value;
	}
	
	[Export] public override bool IsPermeable
	{
		get => base.IsPermeable;
		protected set => base.IsPermeable = value;
	}
	
	private List<NgonRenderer2D> _renderers = new List<NgonRenderer2D>();
	
	protected override void OnInitialize()
	{
		foreach(Vector2I tile in Tiles)
		{
			NgonRenderer2D renderer = _tileRenderer.Instantiate<NgonRenderer2D>();
			AddChild(renderer);
			renderer.Sides = 6;
			renderer.Size = _tileSize / 2;
			
			_renderers.Add(renderer);
		}
		
		PlaceTiles();
	}
	
	private void PlaceTiles()
	{
		if (_renderers.Count == 0)
		{
			return;
		}
		
		int rendererIndex = 0;
		foreach(Vector2I tile in Tiles)
		{
			Vector2 worldPosition = _originWorldPosition
				+ (tile.X * _tileSize * Vector2.Right)
				+ (tile.Y * _tileSize * GridUtility.YIndexToWorld);
			_renderers[rendererIndex].Position = worldPosition;
			
			rendererIndex++;
		}
	}
	
	protected override void OnGridPositionChanged(Vector2I oldPosition) => PlaceTiles();
	protected override void OnRotationChanged(GridRotation oldRotation) => PlaceTiles();
}
