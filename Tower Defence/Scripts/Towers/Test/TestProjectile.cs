using Godot;
using System;
using System.Collections.Generic;
using Movement;
using Collision;

[Tool]
public partial class TestProjectile : Node2D, 
	IProjectile, IPushProjectile
{
	[Export] private float _speed;
	
	private Shape2D _collider;
	[Export]
	private Shape2D Collider
	{
		get => _collider;
		set
		{
			if (_collider == value)
			{
				return;
			}
			
			if (_collider != null)
			{
				_collider.Changed -= QueueRedraw;
			}
			_collider = value;
			if (_collider != null)
			{
				_collider.Changed += QueueRedraw;
			}
			
			QueueRedraw();
		}
	}
	WallCollisionResponse IProjectile.WallResponse => WallCollisionResponse.Destroy;
	float IGenericMover.MaxSpeed => _speed;
	public Vector2 Forward { get; set; }
	
	Shape2D IProjectile.Collider => _collider;
	Vector2 IPushProjectile.PushVelocity => Forward * _speed;
	
	#if TOOLS
	public override void _Draw()
	{
		if (Engine.IsEditorHint() && _collider != null)
		{
			_collider.Draw(GetCanvasItem(), 
				ProjectSettings.GetSetting("debug/shapes/collision/shape_color").AsColor());
		}
	}
	#endif
}
