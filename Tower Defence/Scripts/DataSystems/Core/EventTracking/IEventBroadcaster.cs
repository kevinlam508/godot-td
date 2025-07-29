using Godot;
using System;

namespace DataSystems.Events;

public interface IEventBroadcaster<TListener>
	where TListener : IEventListener
{
	void AddListener(TListener listener);
	void RemoveListener(TListener listener);
}
