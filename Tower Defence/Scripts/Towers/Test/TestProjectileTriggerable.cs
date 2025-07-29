using Godot;
using System;
using HexGrid;

public partial class TestProjectileTriggerable : TestTriggerTriggerable, IProjectileTriggerable
{
	[Export] private PackedScene _projectile;
	
	PackedScene IProjectileTriggerable.Template => _projectile;
	Vector2 IProjectileTriggerable.Position => Center;
	GridRotation IProjectileTriggerable.AimDirection => Rotation;
}
