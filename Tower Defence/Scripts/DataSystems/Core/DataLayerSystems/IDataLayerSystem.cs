using Godot;
using System;
using System.Collections.Generic;

namespace DataSystems
{
	public interface IDataLayerSystem
	{
		bool Enabled { get; set; }
	
		Type[] ReadLayerTypes { get; }
		Type[] WriteLayerTypes { get; }
		void Run(double deltaTime, double totalTime,
			Dictionary<Type, DataLayer> readLayers, 
			Dictionary<Type, DataLayer> writeLayers);
	}
}
