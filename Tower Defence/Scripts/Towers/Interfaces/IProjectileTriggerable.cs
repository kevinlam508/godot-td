using Godot;
using System;
using HexGrid;

public interface IProjectileTriggerable : ITriggerable
{
	PackedScene Template { get; }
	Vector2 Position { get; }
	GridRotation AimDirection { get; }
}
