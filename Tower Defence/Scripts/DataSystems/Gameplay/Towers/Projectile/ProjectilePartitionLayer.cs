using Godot;
using System;
using System.Collections.Generic;
using DataSystems;

public class ProjectilePartitionLayer : DenseGridLayer<HashSet<IProjectile>>
{
	public readonly Dictionary<BaseMob, int> ActiveEffectIds
		= new Dictionary<BaseMob, int>();
	public static readonly CircleShape2D TileShape = new CircleShape2D();
	
	protected override void InitializeData()
	{
		base.InitializeData();
		
		// Approx tile shape as a circle with radius to the corner
		float colliderRadius = 2 / Mathf.Sqrt(3) * TileSize;
		TileShape.Radius = colliderRadius;
	}
	
	public void AddProjectileToTile(Vector2I index, IProjectile p)
	{
		HashSet<IProjectile> projectiles = GetData(index);
		if (projectiles == null)
		{
			projectiles = new HashSet<IProjectile>();
			SetData(index, projectiles);
		}
		projectiles.Add(p);
	}
	
	public void ClearProjectiles()
	{
		foreach (var set in _grid)
		{
			set?.Clear();
		}
	}
	
	public HashSet<IProjectile> GetProjectilesInTile(Vector2I index)
		=> GetData(index);
	
	public bool HasAnyPush(Vector2I index)
	{
		return (GetData(index)?.Count ?? 0) > 0;
	}
}
