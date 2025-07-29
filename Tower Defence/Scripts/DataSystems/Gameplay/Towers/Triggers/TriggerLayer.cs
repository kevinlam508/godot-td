using Godot;
using System;
using System.Collections.Generic;
using DataSystems;

public class TriggerLayer : SparseGridLayer<(ITrigger, ITriggerable)>, IRegistrar
{
	public struct PendingTrigger
	{
		public double ActivateTime;
		public ITrigger Trigger;
	}
	
	private readonly HashSet<ITrigger> _triggers = new HashSet<ITrigger>();
	private readonly HashSet<ITriggerable> _triggerables = new HashSet<ITriggerable>();
	
	private readonly Dictionary<ITrigger, List<ITriggerable>> _triggerLinks = new Dictionary<ITrigger, List<ITriggerable>>();
	private readonly List<PendingTrigger> _pendingTriggers = new List<PendingTrigger>();
	private readonly Dictionary<ITriggerable, double> _triggerableNextActivation = new Dictionary<ITriggerable, double>();
	
	public Dictionary<ITrigger, List<ITriggerable>> Links => _triggerLinks;
	public List<PendingTrigger> PendingTriggers => _pendingTriggers;

	void IRegistrar.Register(object toRegister)
	{
		if (toRegister is ITrigger trigger)
		{
			_triggers.Add(trigger);
			foreach(Vector2I tile in trigger.Tiles)
			{
				(ITrigger existingTrigger, ITriggerable existingTriggerable) = GetData(tile);
				if (existingTrigger != null)
				{
					GD.PrintErr($"Overlapping triggers at {tile}");
				}
				else
				{
					SetData(tile, (trigger, existingTriggerable));
				}
			}
		}
		if (toRegister is ITriggerable triggerable)
		{
			_triggerables.Add(triggerable);
			foreach(Vector2I tile in triggerable.Tiles)
			{
				(ITrigger existingTrigger, ITriggerable existingTriggerable) = GetData(tile);
				if (existingTriggerable != null)
				{
					GD.PrintErr($"Overlapping triggerable at {tile}");
				}
				else
				{
					SetData(tile, (existingTrigger, triggerable));
				}
			}
		}
	}
	
	void IRegistrar.Unregister(object toUnregister)
	{
		if (toUnregister is ITrigger trigger 
			&& _triggers.Remove(trigger))
		{
			foreach(Vector2I tile in trigger.Tiles)
			{
				(ITrigger existingTrigger, ITriggerable existingTriggerable) = GetData(tile);
				if (existingTrigger != trigger)
				{
					GD.PrintErr($"Overlapping triggers at {tile}");
				}
				else
				{
					SetData(tile, (null, existingTriggerable));
				}
			}
			
			// Cleanup links
			_triggerLinks.Remove(trigger);
			for (int i = _pendingTriggers.Count - 1; i >= 0; i++)
			{
				if (_pendingTriggers[i].Trigger == trigger)
				{
					_pendingTriggers.RemoveAt(i);
				}
			}
			
			Dirty = true;
		}
		if (toUnregister is ITriggerable triggerable 
			&& _triggerables.Remove(triggerable))
		{
			foreach(Vector2I tile in triggerable.Tiles)
			{
				(ITrigger existingTrigger, ITriggerable existingTriggerable) = GetData(tile);
				if (existingTriggerable != triggerable)
				{
					GD.PrintErr($"Overlapping triggerable at {tile}");
				}
				else
				{
					SetData(tile, (existingTrigger, null));
				}
			}
			
			// Cleanup links
			foreach((_, List<ITriggerable> list) in _triggerLinks)
			{
				list.Remove(triggerable);
			}
			_triggerableNextActivation.Remove(triggerable);
			
			Dirty = true;
		}
	}
	
	public bool HasLink(ITrigger trigger, ITriggerable triggerable)
	{
		return _triggerLinks.TryGetValue(trigger, out var links)
			&& links.Contains(triggerable);
	}
	
	// Returns true if a link is added
	public bool AddLink(ITrigger trigger, ITriggerable triggerable)
	{
		// Make sure this is aware of the things being linked
		if (!_triggers.Contains(trigger))
		{
			GD.PrintErr("Adding link for unknown trigger");
			return false;
		}
		if (!_triggerables.Contains(triggerable))
		{
			GD.PrintErr("Adding link for unknown triggerable");
			return false;
		}
		
		if (!_triggerLinks.TryGetValue(trigger, out List<ITriggerable> list))
		{
			list = new List<ITriggerable>();
			_triggerLinks[trigger] = list;
		}
		
		// Already linked
		if (list.Contains(triggerable))
		{
			return false;
		}
		
		list.Add(triggerable);
		Dirty = true;
		return true;
	}
	
	// Returns true if the link is removed
	public bool RemoveLink(ITrigger trigger, ITriggerable triggerable)
	{
		// Make sure this is aware of the things being linked
		if (!_triggers.Contains(trigger))
		{
			GD.PrintErr("Adding link for unknown trigger");
			return false;
		}
		if (!_triggerables.Contains(triggerable))
		{
			GD.PrintErr("Adding link for unknown triggerable");
			return false;
		}
		
		// Link never existed
		if (!_triggerLinks.TryGetValue(trigger, out List<ITriggerable> list))
		{
			return false;
		}
		bool removed = list.Remove(triggerable);
		
		// Last link, clean up
		if (list.Count == 0)
		{
			_triggerLinks.Remove(trigger);
		}
		Dirty = true;
		return removed;
	}
	
	public ITrigger GetTrigger(Vector2I index)
	{
		return GetData(index).Item1;
	}
	
	public ITriggerable GetTriggerable(Vector2I index)
	{
		return GetData(index).Item2;
	}
	
	public void AddPendingTrigger(ITrigger t, double currentTime)
	{
		_pendingTriggers.Add(
				new PendingTrigger{
					Trigger = t,
					ActivateTime = currentTime + t.ActivationDelay
				}
			);
	}
	
	public double GetTriggerableNextActivationTime(ITriggerable triggerable)
	{
		return _triggerableNextActivation.TryGetValue(triggerable, out var time)
			? time : 0;
	}
	
	public void SetTriggerableNextActivationTime(ITriggerable triggerable, double currentTime)
	{
		_triggerableNextActivation[triggerable] = currentTime + triggerable.Cooldown;
	}
}
