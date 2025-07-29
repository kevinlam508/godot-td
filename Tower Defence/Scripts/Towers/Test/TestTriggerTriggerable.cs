using Godot;
using System;
using HexGrid;

public partial class TestTriggerTriggerable : GridObject, 
	ITrigger, ITriggerable
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
		get => false;
		protected set {} // Remove default setter, this is a constant
	}
	
	bool ITrigger.TriggerOnOverlap => false;
	float ITrigger.ActivationDelay => 1f;
	double ITriggerable.Cooldown => 3;
	
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
