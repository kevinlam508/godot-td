using Godot;
using System;
using DataSystems;

namespace Towers.ActivationModules;

public abstract class BaseActivationModule<TActivationType>
	: IActivationModule
	where TActivationType : ITriggerable
{
	public virtual Type WriteLayer => null;
	public bool Handles(ITriggerable t) => t is TActivationType;
	public abstract void Activate(ITriggerable t, DataLayer layer);
}

public abstract class BaseActivationModule<TActivationType, TWriteLayer>
	: BaseActivationModule<TActivationType>
	where TActivationType : ITriggerable
	where TWriteLayer : DataLayer
{
	private static readonly Type LayerType = typeof(TWriteLayer);
	
	public override Type WriteLayer => LayerType;
	public override void Activate(ITriggerable t, DataLayer layer)
	{
		#if TOOLS
		if (!(this as IActivationModule).Handles(t) || !(layer is TWriteLayer))
		{
			GD.PrintErr($"Invalid types {t.GetType()}/{layer.GetType()} passed to {this.GetType()}");
			return;
		}
		#endif
		
		ActivateInternal((TActivationType)t, (TWriteLayer)layer);
	}
	
	protected abstract void ActivateInternal(TActivationType t, TWriteLayer layer);
}
