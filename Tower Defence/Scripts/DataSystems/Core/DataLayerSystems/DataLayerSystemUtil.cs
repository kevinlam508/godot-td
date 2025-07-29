using Godot;
using System;
using System.Collections.Generic;

namespace DataSystems.Util
{
	public static class DataLayerSystemUtil
	{
		public static DataLayer GetLayer(Type t, Dictionary<Type, DataLayer> layers)
		{
			#if TOOLS
			if (!layers.TryGetValue(t, out var layer))	
			{
				GD.Print($"Layer {t.Name} not found");
			}
			return layer;
			#else
			return layers[t];
			#endif
		}
	}
}
