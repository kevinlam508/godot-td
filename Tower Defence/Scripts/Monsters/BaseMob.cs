using Godot;
using System;
using Movement;

public abstract partial class BaseMob : Node, IGenericMover
{
	public abstract Vector2 Position { get; set; }
	public Vector2 Forward { get; set; }
	
	public abstract float MaxSpeed { get; }
	public abstract float Acceleration { get; }
	int IGenericMover.MaxStepHeight => 3;
}
