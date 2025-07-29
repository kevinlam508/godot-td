using Godot;
using System;
using System.Collections.Generic;
using DataSystems;

public partial class MobSpawnSystem : DataLayerSystemNode
{
	[Export] private MobList _beastiary;
	[Export(PropertyHint.File, "*.txt")] private string _waveScriptPath;

	public override Type[] ReadLayerTypes =>
		[
			typeof(GridDataLayer)
		];
	
	public override Type[] WriteLayerTypes => 
		[
			typeof(ObjectRegistrationLayer)
		];
	
	public override void _Ready()
	{
		
	}
	
	public override void Run (double deltaTime, double totalTime,
			Dictionary<Type, DataLayer> readLayers, 
			Dictionary<Type, DataLayer> writeLayers)
	{
		GridDataLayer gridData = GetLayer<GridDataLayer>(readLayers);
		ObjectRegistrationLayer registration = GetLayer<ObjectRegistrationLayer>(writeLayers);
		
		RunTool(gridData, registration);
	}
	
	partial void RunTool(GridDataLayer gridData, ObjectRegistrationLayer registration);
	
	private void SpawnMob(ObjectRegistrationLayer registration,
		PackedScene mobScene, GridDataLayer gridData, Vector2 location)
	{
		Vector2I gridIndex = gridData.GetGridIndexFromWorldPosition(location);
		if (!gridData.IsIndexInBounds(gridIndex))
		{
			GD.PrintErr($"Ignoring mob spawn outside of bounds at {location}");
			return;
		}
		
		var mob = mobScene.Instantiate<BaseMob>();
		mob.Position = location;
		AddChild(mob);
		registration.RegisterObject(mob);
	}
}
