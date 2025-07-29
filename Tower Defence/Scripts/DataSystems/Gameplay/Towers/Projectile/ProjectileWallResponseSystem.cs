using Godot;
using System;
using System.Collections.Generic;
using DataSystems;
using Movement;

public class ProjectileWallResponseSystem : DataLayerSystem
{
	public override Type[] ReadLayerTypes => 
		[
			typeof(ProjectileLayer),
			typeof(GenericMovementLayer)
		];
	public override Type[] WriteLayerTypes => 
		[
			typeof(ObjectRegistrationLayer)
		];
		
	public override void Run (double deltaTime, double totalTime,
			Dictionary<Type, DataLayer> readLayers, 
			Dictionary<Type, DataLayer> writeLayers)
	{
		ProjectileLayer projectileLayer = GetLayer<ProjectileLayer>(readLayers);
		GenericMovementLayer moveLayer = GetLayer<GenericMovementLayer>(readLayers);
		ObjectRegistrationLayer objectLayer = GetLayer<ObjectRegistrationLayer>(writeLayers);
	
		foreach(IProjectile p in projectileLayer.Projectiles)
		{
			MovementState state = moveLayer.GetState(p);
			if (!state.HasHitWall)
			{
				continue;
			}
			
			switch(p.WallResponse)
			{
				case WallCollisionResponse.Destroy:
					objectLayer.UnregisterObject(p);
					break;
			}
		}
	}
}
