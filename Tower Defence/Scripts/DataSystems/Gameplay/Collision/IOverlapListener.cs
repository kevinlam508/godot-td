using Godot;
using System;
using DataSystems.Events;

namespace Collision;

public interface IOverlapListener<TMain, TOther> : IEventListener
{
	void OnOverlapBegin(TMain main, TOther other);
	void OnOverlapEnd(TMain main, TOther other);
}
