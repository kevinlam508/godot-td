using Godot;
using System;

namespace Movement;

public interface IGenericMover
{
	// Current state
	Vector2 Position { get; set; }
	float Rotation => Vector2.Left.AngleTo(Forward);
	Vector2 Forward { get; set; }
	
	// Control stats
	float MaxSpeed { get; }
	float Acceleration { get; }
	int MaxStepHeight { get; }
}
