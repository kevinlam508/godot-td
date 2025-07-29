using Godot;
using System;
using System.Collections.Generic;
using DataSystems.Util;

namespace DataSystems
{
	// System type to allow systems to directly interact with the world
	// Also allows serialization of values
	[GlobalClass]
	public abstract partial class DataLayerSystemNode : Node, IDataLayerSystem
	{
		public bool _enabled = true;
		public bool Enabled 
		{ 
			get => _enabled;
			set
			{
				_enabled = value;
				if (_enabled)
				{
					OnEnable();
				}
				else
				{
					OnDisable();
				}
			}
		}
		
		public virtual Type[] ReadLayerTypes => null;
		public virtual Type[] WriteLayerTypes => null;
		
		public abstract void Run(double deltaTime, double totalTime,
			Dictionary<Type, DataLayer> readLayers, 
			Dictionary<Type, DataLayer> writeLayers);
			
		protected DataLayer GetLayer(Type t, Dictionary<Type, DataLayer> layers)
			=> DataLayerSystemUtil.GetLayer(t, layers);
		protected T GetLayer<T>(Dictionary<Type, DataLayer> layers)
			where T : DataLayer
			=> (T) DataLayerSystemUtil.GetLayer(typeof(T), layers);
		
		protected virtual void OnEnable(){}
		protected virtual void OnDisable(){}
	}
}
