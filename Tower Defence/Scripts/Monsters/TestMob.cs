using Godot;
using System;

public partial class TestMob : BaseMob
{
	[Export] private float _maxSpeed;
	[Export] private float _acceleration;
	[Export] private Node2D _positionRoot;
	
	public override float MaxSpeed => _maxSpeed;
	public override float Acceleration => _acceleration;
	public override Vector2 Position
	{
		get => _positionRoot.Position;
		set 
		{
			_positionRoot.Position = value;
		}
	}
}
