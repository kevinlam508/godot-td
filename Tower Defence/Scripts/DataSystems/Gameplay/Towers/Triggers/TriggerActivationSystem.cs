using Godot;
using System;
using System.Collections.Generic;
using DataSystems;
using Towers.ActivationModules;

public class TriggerActivationSystem : DataLayerSystem
{
	private static readonly IActivationModule[] ActivationModules;
	private static readonly Type[] BaseWriteTypes = 
	[
		typeof(TriggerLayer)
	];
	private static readonly Type[] WriteTypes;
	static TriggerActivationSystem()
	{
		ActivationModules = 
		[
			new LogModule(),
			new ProjectileModule(),
		];
		HashSet<Type> activationLayers = new HashSet<Type>();
		foreach(var module in ActivationModules)
		{
			if (module.WriteLayer == null)
			{
				continue;
			}
			activationLayers.Add(module.WriteLayer);
		}
		
		WriteTypes = new Type[BaseWriteTypes.Length + activationLayers.Count];
		int i = 0;
		foreach (Type t in BaseWriteTypes)
		{
			WriteTypes[i] = t;
			i++;
		}
		foreach (Type t in activationLayers)
		{
			WriteTypes[i] = t;
			i++;
		}
	}
	
	private readonly HashSet<ITrigger> _chainTriggers = new HashSet<ITrigger>();
	
	public override Type[] ReadLayerTypes =>
		[
			typeof(MobLayer)
		];
	
	public override Type[] WriteLayerTypes => WriteTypes;
		
	public override void Run(double deltaTime, double totalTime,
			Dictionary<Type, DataLayer> readLayers, 
			Dictionary<Type, DataLayer> writeLayers)
	{
		MobLayer mobs = GetLayer<MobLayer>(readLayers);
		TriggerLayer triggers = GetLayer<TriggerLayer>(writeLayers);
		
		ActivateTriggerables(totalTime, triggers, writeLayers);
		ActivateTriggers(totalTime, triggers, mobs);
	}
	
	// Enqueue pending triggers based on mob position
	private void ActivateTriggers(double time, TriggerLayer triggers, MobLayer mobs)
	{
		foreach (Vector2I tile in mobs.OccupiedTiles)
		{
			// Didn't step on a trigger
			ITrigger t = triggers.GetTrigger(tile);
			if (t == null || !t.TriggerOnOverlap)
			{
				continue;
			}
			
			triggers.AddPendingTrigger(t, time);
		}
	}
	
	// Activate triggerables on pending triggers
	private void ActivateTriggerables(double time, TriggerLayer triggers, 
			Dictionary<Type, DataLayer> writeLayers)
	{
		for (int i = triggers.PendingTriggers.Count - 1; i >= 0 ; i--)
		{
			// Not passed activation delay
			TriggerLayer.PendingTrigger pending = triggers.PendingTriggers[i];
			if (pending.ActivateTime > time)
			{
				continue;
			}
			
			// No links for this trigger
			if (!triggers.Links.TryGetValue(pending.Trigger, out var links))
			{
				continue;
			}
			foreach (ITriggerable t in links)
			{
				if (triggers.GetTriggerableNextActivationTime(t) > time)
				{
					continue;
				}
				ActivateTriggerable(t, writeLayers);
				triggers.SetTriggerableNextActivationTime(t, time);
				
				if (t is ITrigger trigger)
				{
					_chainTriggers.Add(trigger);
				}
			}
			
			triggers.PendingTriggers.RemoveAt(i);
		}
		
		foreach (ITrigger t in _chainTriggers)
		{
			triggers.AddPendingTrigger(t, time);
		}
		_chainTriggers.Clear();
	}
	
	private void ActivateTriggerable(ITriggerable t, 
		Dictionary<Type, DataLayer> writeLayers)
	{
		foreach (IActivationModule module in ActivationModules)
		{
			if (module.Handles(t))
			{
				DataLayer layer = module.WriteLayer != null 
					? GetLayer(module.WriteLayer, writeLayers)
					: null;
				module.Activate(t, layer);
			}
		}
	}
}
