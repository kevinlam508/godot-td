using Godot;
using System;
using System.Collections.Generic;

public interface ITriggerable
{
	double Cooldown { get; }
	
	IEnumerable<Vector2I> Tiles { get; }
}
