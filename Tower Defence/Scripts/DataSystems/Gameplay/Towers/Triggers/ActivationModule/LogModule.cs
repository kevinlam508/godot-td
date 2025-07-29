using Godot;
using System;
using DataSystems;

namespace Towers.ActivationModules;

public class LogModule : BaseActivationModule<ILogTriggerable>
{
	public override void Activate(ITriggerable t, DataLayer _)
	{
		if (Handles(t))
		{
			((ILogTriggerable)t).Log();
		}
	}
}
