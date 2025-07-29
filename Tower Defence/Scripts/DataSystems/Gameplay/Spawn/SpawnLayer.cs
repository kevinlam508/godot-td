using Godot;
using System;
using System.Collections.Generic;

namespace DataSystems.Spawn;

public class SpawnLayer<TRequestType> : DataLayer
	where TRequestType : struct
{
	public readonly List<TRequestType> Requests = new List<TRequestType>();
}
