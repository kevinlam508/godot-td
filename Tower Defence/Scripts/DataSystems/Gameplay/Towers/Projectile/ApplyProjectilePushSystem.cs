using Godot;
using System;
using System.Collections.Generic;
using DataSystems;
using Movement;

public class ApplyProjectilePushSystem : DataLayerSystem
{
	public override Type[] ReadLayerTypes => 
		[
			typeof(MobLayer)
		];
	public override Type[] WriteLayerTypes => 
		[
			typeof(ProjectilePartitionLayer),
			typeof(GenericMovementLayer)
		];
		
	public override void Run (double deltaTime, double totalTime,
			Dictionary<Type, DataLayer> readLayers, 
			Dictionary<Type, DataLayer> writeLayers)
	{
		MobLayer mobLayer = GetLayer<MobLayer>(readLayers);
		ProjectilePartitionLayer pushLayer = GetLayer<ProjectilePartitionLayer>(writeLayers);
		GenericMovementLayer moveLayer = GetLayer<GenericMovementLayer>(writeLayers);

		foreach (BaseMob mob in mobLayer.Mobs)
		{
			// Find new override
			Vector2I gridIndex = mobLayer.GetGridIndexFromWorldPosition(mob.Position);
			bool hasPush = TryGetPush(gridIndex, pushLayer, mob.Position,
				out Vector2 newPush);
			
			// Determine existing override
			int existingOverride = GenericMovementLayer.InvalidOverrideId;
			if(!pushLayer.ActiveEffectIds.TryGetValue(mob, out existingOverride))
			{
				 existingOverride = GenericMovementLayer.InvalidOverrideId;
			}
			
			// Pushing this frame
			if (hasPush)
			{
				// Pushed last frame
				if (existingOverride != GenericMovementLayer.InvalidOverrideId)
				{
					Vector2 existingPush = moveLayer.GetOverride(mob, existingOverride).Velocity;
					// No change, skip
					if (newPush == existingPush)
					{
						continue;
					}
					
					// Different push, remove old
					// Effect id replaced laters
					moveLayer.RemoveOverride(mob, existingOverride);
				}
				
				int id = moveLayer.AddOverride(mob, 
					new MovementOverride
					{
						Velocity = newPush
					}
				);
				pushLayer.ActiveEffectIds[mob] = id;
			}
			// Not pushing this frame, but pushed last frame
			else if (existingOverride != GenericMovementLayer.InvalidOverrideId)
			{
				// Remove the push effect
				moveLayer.RemoveOverride(mob, existingOverride);
				pushLayer.ActiveEffectIds.Remove(mob);
			}
		}
	}
	
	private bool TryGetPush(Vector2I gridIndex, ProjectilePartitionLayer pushLayer,
		Vector2 mobPosition, out Vector2 pushValue)
	{
		// Nothing happening in the tile, cannot have a push
		pushValue = Vector2.Zero;
		if (!pushLayer.HasAnyPush(gridIndex))
		{
			return false;
		}

		bool gotPushed = false;
		HashSet<IProjectile> projectiles = pushLayer.GetProjectilesInTile(gridIndex);
		foreach (IProjectile projectile in projectiles)
		{
			if(!(projectile is IPushProjectile p))
			{
				continue;
			}
			
			Transform2D projectileTransform = new Transform2D(p.Rotation, p.Position);
			if (p.Collider.IsInside(projectileTransform, mobPosition))
			{
				pushValue += p.PushVelocity;
				gotPushed = true;
			}
		}
		return gotPushed;
	}
}
