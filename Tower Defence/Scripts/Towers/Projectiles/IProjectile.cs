using Godot;
using System;
using Movement;

public enum WallCollisionResponse
{
	None,
	Destroy
}

public interface IProjectile : IGenericMover
{
	float IGenericMover.Acceleration => GenericMovementSystem.InstantAcceleration;
	int IGenericMover.MaxStepHeight => int.MaxValue;
	
	Shape2D Collider { get; }
	WallCollisionResponse WallResponse { get; }
}
