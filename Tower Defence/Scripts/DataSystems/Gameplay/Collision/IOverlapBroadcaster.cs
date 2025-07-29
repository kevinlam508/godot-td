using Godot;
using System;
using DataSystems.Events;

namespace Collision;

public interface IOverlapBroadcaster<TMain, TOther>
	: IEventBroadcaster<IOverlapListener<TMain, TOther>>
{

}
