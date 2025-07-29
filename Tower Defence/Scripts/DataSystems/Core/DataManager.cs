//#define PERFORMANCE_LOG

using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace DataSystems
{
	public class DataManager
	{
		private readonly GridData _globalData;
		private readonly Dictionary<Type, DataLayer> _layers = new Dictionary<Type, DataLayer>();
		private readonly List<IDataLayerSystem> _systems = new List<IDataLayerSystem>();
		
		private readonly ObjectRegistrationLayer _registrationChanges = new ObjectRegistrationLayer();
		private readonly HashSet<IRegistrar> _registrars = new HashSet<IRegistrar>();
		
		private readonly Dictionary<Type, DataLayer> _runReadLayers = new Dictionary<Type, DataLayer>();
		private readonly Dictionary<Type, DataLayer> _runWriteLayers = new Dictionary<Type, DataLayer>();
		private double _totalTimePassed;
		
		public DataManager(GridData globalData)
		{
			_globalData = globalData;
			
			_layers.Add(typeof(ObjectRegistrationLayer), _registrationChanges);
		} 
		
		~DataManager()
		{
			foreach ((_, DataLayer layer) in _layers)
			{
				if (layer is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
		}
		
		public T GetSystem<T>()
			where T : IDataLayerSystem
		{
			foreach(IDataLayerSystem system in _systems)
			{
				if (system is T result)
				{
					return result;
				}
			}
			return default(T);
		}
		
		public void ToggleSystem<T>()
			where T : IDataLayerSystem
		{
			T system = GetSystem<T>();
			if (system.Equals(default(T)))
			{
				GD.PrintErr($"Trying to toggle {typeof(T).Name} but it is not registered");
				return;
			}
			system.Enabled = !system.Enabled;
		}
		
		public T GetLayer<T>()
			where T : DataLayer
		{
			_layers.TryGetValue(typeof(T), out var layer);
			return (T)layer;
		}
		
		public void AddSystem(IDataLayerSystem system)
		{
			AddLayers(system.ReadLayerTypes);
			AddLayers(system.WriteLayerTypes);
			_systems.Add(system);
			
			void AddLayers(Type[] types)
			{
				if (types == null)
				{
					return;
				}
				
				foreach (Type t in types)
				{
					if (_layers.ContainsKey(t))
					{
						continue;
					}
					
					DataLayer newLayer = (DataLayer)Activator.CreateInstance(t);
					_layers[t] = newLayer;
					
					if (newLayer is IGridDataInitialize gridLayer)
					{
						gridLayer.InitializeGridData(_globalData);
					}
					
					if (newLayer is IRegistrar registrar)
					{
						_registrars.Add(registrar);
					}
				}
			}
		}
		
		public void RunSystems(double deltaTime)
		{
			#if PERFORMANCE_LOG
			using var __ = new Debug.TimeLogger.TimeScope("Data Update");
			#endif
			
			UpdateObjectRegistrations();
			
			foreach (IDataLayerSystem system in _systems)
			{
				RunSystem(system, deltaTime);
			}
			
			_totalTimePassed += deltaTime;
			foreach ((_, DataLayer layer) in _layers)
			{
				layer.Dirty = false;
			}
		}
		
		private void RunSystem(IDataLayerSystem system, double deltaTime)
		{
			if (!system.Enabled)
			{
				return;
			}
			
			#if PERFORMANCE_LOG
			using var _ = new Debug.TimeLogger.TimeScope(system.GetType().Name);
			#endif
			
			GetLayers(system.ReadLayerTypes, _runReadLayers);
			GetLayers(system.WriteLayerTypes, _runWriteLayers);
			
			system.Run(deltaTime, _totalTimePassed, _runReadLayers, _runWriteLayers);
			
			_runReadLayers.Clear();
			_runWriteLayers.Clear();
			
			void GetLayers(Type[] layers, Dictionary<Type, DataLayer> result)
			{
				if (layers == null)
				{
					return;
				}
				
				foreach(Type t in layers)
				{
					result[t] = _layers[t];
				}
			}
		}
		
		private void UpdateObjectRegistrations()
		{
			#if PERFORMANCE_LOG
			using var _ = new Debug.TimeLogger.TimeScope("Object Registrations");
			#endif
			
			foreach (object o in _registrationChanges.PendingRegistrations)
			{
				foreach (IRegistrar registrar in _registrars)
				{
					registrar.Register(o);
				}
			}
			
			foreach (object o in _registrationChanges.PendingUnregistrations)
			{
				foreach (IRegistrar registrar in _registrars)
				{
					registrar.Unregister(o);
				}
				if (o is Node n)
				{
					n.QueueFree();
				}
			}
			
			_registrationChanges.Clear();
		}
	}
}
