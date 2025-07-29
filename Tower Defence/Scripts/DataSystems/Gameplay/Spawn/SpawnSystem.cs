using Godot;
using System;
using System.Collections.Generic;

namespace DataSystems.Spawn;

public abstract partial class SpawnSystem<TRequestType> : DataLayerSystemNode
	where TRequestType : struct
{
	public override Type[] WriteLayerTypes => 
		[
			typeof(ObjectRegistrationLayer),
			typeof(SpawnLayer<TRequestType>)
		];
	
	public override void Run (double deltaTime, double totalTime,
			Dictionary<Type, DataLayer> readLayers, 
			Dictionary<Type, DataLayer> writeLayers)
	{
		ObjectRegistrationLayer registration = GetLayer<ObjectRegistrationLayer>(writeLayers);
		SpawnLayer<TRequestType> spawnLayer = GetLayer<SpawnLayer<TRequestType>>(writeLayers);
		
		foreach (TRequestType request in spawnLayer.Requests)
		{
			object result = Spawn(request);
			if (result == null)
			{
				continue;
			}
			registration.RegisterObject(result);
		}
		spawnLayer.Requests.Clear();
	}
	
	protected abstract object Spawn(TRequestType request);
}
