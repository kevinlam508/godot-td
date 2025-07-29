using Godot;
using System;
using Collision;

public interface IPushProjectile 
	: IProjectile
{
	Vector2 PushVelocity { get; }
}
