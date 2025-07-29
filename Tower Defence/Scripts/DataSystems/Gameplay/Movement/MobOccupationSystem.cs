using Godot;
using System;
using System.Collections.Generic;
using DataSystems;
using Movement;

public class MobOccupationSystem : DataLayerSystem
{
	public override Type[] ReadLayerTypes => 
		[
			typeof(GenericMovementLayer)
		];
	
	public override Type[] WriteLayerTypes => 
		[
			typeof(MobLayer)
		];
	
	public override void Run (double deltaTime, double totalTime,
			Dictionary<Type, DataLayer> readLayers, 
			Dictionary<Type, DataLayer> writeLayers)
	{
		GenericMovementLayer moveLayer = GetLayer<GenericMovementLayer>(readLayers);
		MobLayer mobLayer = GetLayer<MobLayer>(writeLayers);
		foreach (BaseMob mob in mobLayer.Mobs)
		{
			MovementState state = moveLayer.GetState(mob);
			mobLayer.MoveTileOccupation(state.PreviousPosition, 
				state.Position);
		}
	}
}
