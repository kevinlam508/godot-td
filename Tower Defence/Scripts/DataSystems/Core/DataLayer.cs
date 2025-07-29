using Godot;
using System;

namespace DataSystems
{
	public abstract class DataLayer
	{
		public bool Dirty { get; set; } = true;
	}
}
