using Godot;
using System;
using System.Collections.Generic;
using DataSystems;
using DataSystems.Events;

namespace Collision;

public struct OverlapEvent<TMain, TOther>
{
	public CollisionEventType Type;
	public TMain MainObject;
	public TOther OtherObject;
}

public class OverlapLayer<TMain, TOther> 
	: EventLayer<IOverlapListener<TMain, TOther>, IOverlapBroadcaster<TMain, TOther>>
	, IOverlapListener<TMain, TOther>
{
	public readonly List<OverlapEvent<TMain, TOther>> Events
		= new List<OverlapEvent<TMain, TOther>>();
	
	protected override IOverlapListener<TMain, TOther> Listener => this;
	
	void IOverlapListener<TMain, TOther>.OnOverlapBegin(TMain main, TOther other)
	{
		Events.Add(new OverlapEvent<TMain, TOther>
		{
			Type = CollisionEventType.OverlapBegin,
			MainObject = main,
			OtherObject = other
		});
	}
	
	void IOverlapListener<TMain, TOther>.OnOverlapEnd(TMain main, TOther other)
	{
		Events.Add(new OverlapEvent<TMain, TOther>
		{
			Type = CollisionEventType.OverlapEnd,
			MainObject = main,
			OtherObject = other
		});
	}
}
