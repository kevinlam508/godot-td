using Godot;
using System;
using DataSystems;

namespace Towers.ActivationModules;

public interface IActivationModule
{	
	Type WriteLayer { get; }
	bool Handles(ITriggerable t);
	void Activate(ITriggerable t, DataLayer layer);
}
