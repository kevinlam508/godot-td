using Godot;
using System;
using System.Collections.Generic;
using HexGrid;

public partial class BasicTrigger : GridObject, ITrigger
{
	[Export] private NgonRenderer2D _renderer;
	
	[Export]
	public GridShape Shape
	{
		get => _shape;
		private set => _shape = value;
	}
	
	public override bool IsPermeable
	{
		get => true;
		protected set {} // Remove default setter, this is a constant
	}
	
	bool ITrigger.TriggerOnOverlap => true;
	float ITrigger.ActivationDelay => 0f;
	
	protected override void OnInitialize()
	{
		_renderer.Sides = 6;
		_renderer.Size = _tileSize / 2;
	}
	
	private void UpdatePosition()
	{
		Vector2 worldPosition = GridUtility.GetWorldPositionFromGridIndex
			(GridPosition, _originWorldPosition, _tileSize);
		_renderer.Position = worldPosition;
	}
	
	protected override void OnGridPositionChanged(Vector2I oldPosition) => UpdatePosition();
}
