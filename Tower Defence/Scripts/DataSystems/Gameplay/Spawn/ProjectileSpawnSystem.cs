using Godot;
using System;
using HexGrid;
using DataSystems;
using DataSystems.Spawn;

public struct ProjectileSpawnRequest
{
	public PackedScene Template;
	public Vector2 Location;
	public GridRotation Rotation;
}

public partial class ProjectileSpawnSystem : SpawnSystem<ProjectileSpawnRequest>
{
	protected override object Spawn(ProjectileSpawnRequest request)
	{
		Node newInstance = request.Template.Instantiate();
		if (!(newInstance is IProjectile projectile))
		{
			GD.PrintErr($"Tried to spawn non-projectile {newInstance.Name}");
			newInstance.QueueFree();
			return null;
		}
		
		projectile.Position = request.Location;
		projectile.Forward = Vector2.Left.Rotated(Mathf.DegToRad(
			(int)request.Rotation 
			* GridUtility.DegreesPerRotation));
		AddChild(newInstance);
		return newInstance;
	}
}
