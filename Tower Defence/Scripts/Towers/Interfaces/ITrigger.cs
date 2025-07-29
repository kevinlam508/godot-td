using Godot;
using System;
using System.Collections.Generic;

public interface ITrigger
{
	bool TriggerOnOverlap { get; }
	float ActivationDelay { get; }
	
	IEnumerable<Vector2I> Tiles { get; }
}
