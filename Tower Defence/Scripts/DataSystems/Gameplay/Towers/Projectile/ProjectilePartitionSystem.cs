using Godot;
using System;
using System.Collections.Generic;
using DataSystems;
using Movement;
using HexGrid;

public class ProjectilePartitionSystem : DataLayerSystem
{
	public override Type[] ReadLayerTypes => 
		[
			typeof(ProjectileLayer)
		];
	public override Type[] WriteLayerTypes => 
		[
			typeof(ProjectilePartitionLayer)
		];
		
	public override void Run (double deltaTime, double totalTime,
			Dictionary<Type, DataLayer> readLayers, 
			Dictionary<Type, DataLayer> writeLayers)
	{
		ProjectileLayer projectileLayer = GetLayer<ProjectileLayer>(readLayers);
		ProjectilePartitionLayer partitionLayer = GetLayer<ProjectilePartitionLayer>(writeLayers);
		
		partitionLayer.ClearProjectiles();		
		foreach (var projectile in projectileLayer.Projectiles)
		{
			AddProjectileToTiles(projectile, partitionLayer);
		}
	}
	
	private void AddProjectileToTiles(IProjectile projectile, 
		ProjectilePartitionLayer partitionLayer)
	{
		// Guarenteed to be in the tile the center is in
		Vector2I projectileGridIndex = partitionLayer.GetGridIndexFromWorldPosition(
			projectile.Position
		);
		partitionLayer.AddProjectileToTile(projectileGridIndex, projectile);
		
		// Look outwards from starting tile to check tile containment
		HashSet<Vector2I> visited = new HashSet<Vector2I>();
		Queue<Vector2I> toVisit = new Queue<Vector2I>();
		visited.Add(projectileGridIndex);
		EnqueueAdjacencies(projectileGridIndex);
		
		Transform2D projectileTransform = new Transform2D(
			projectile.Rotation, 
			projectile.Position);
		while (toVisit.Count > 0)
		{
			Vector2I current  = toVisit.Dequeue();
			visited.Add(current);
			Vector2 tilePosition = partitionLayer.GetWorldPositionFromGridIndex(current);
			Transform2D tileTransform = new Transform2D(0, tilePosition);
			
			// Not in this tile, stop searching this direction
			bool inTile = ProjectilePartitionLayer.TileShape.Collide(
					tileTransform,
					projectile.Collider,
					projectileTransform
				);
			if (!inTile)
			{
				continue;
			}
			
			// In this tile, check the adjacenct tiles
			partitionLayer.AddProjectileToTile(current, projectile);
			EnqueueAdjacencies(current);
		}
		
		void EnqueueAdjacencies(Vector2I index)
		{
			foreach (Vector2I adjacent in GridUtility.GetAdjacencies(index))
			{
				if (!partitionLayer.IsIndexInBounds(adjacent)
					|| visited.Contains(adjacent))
				{
					continue;
				}
				toVisit.Enqueue(adjacent);
			}
		}
	}
}
