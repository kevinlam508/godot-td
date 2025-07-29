using Godot;
using System;
using System.Collections.Generic;

namespace DataSystems.Events;

/*
 * To use:
 * 1) Define IEventListener
 * 2) Define IEventBroadcaster<>
 * 3) Define the EventLayer
 *    - Must implement the created IEventListener
 *    - Override Listener as self
*/
public abstract class EventLayer<TListener, TBroadcaster> 
	: DataLayer, IRegistrar, IDisposable
	where TListener : IEventListener
	where TBroadcaster : IEventBroadcaster<TListener>
{
	private readonly HashSet<TBroadcaster> _subscriptions 
		= new HashSet<TBroadcaster>();
	protected abstract TListener Listener { get; }
	
	void IRegistrar.Register(object o)
	{
		if (o is TBroadcaster b)
		{
			b.AddListener(Listener);
			_subscriptions.Add(b);
		}
	}
	
	void IRegistrar.Unregister(object o)
	{
		if (o is TBroadcaster b)
		{
			b.RemoveListener(Listener);
			_subscriptions.Remove(b);
		}
	}
	
	void IDisposable.Dispose()
	{
		foreach(TBroadcaster b in _subscriptions)
		{
			b.RemoveListener(Listener);
		}
	}
}
