using Godot;
using System;
using System.Collections.Generic;

namespace DataSystems
{
	public class ObjectRegistrationLayer : DataLayer, IDisposable
	{
		private HashSet<object> _toRegister = new HashSet<object>();
		private HashSet<object> _toUnregister = new HashSet<object>();
		
		public IEnumerable<object> PendingRegistrations => _toRegister;
		public IEnumerable<object> PendingUnregistrations => _toUnregister;
		
		public void RegisterObject(object o)
		{
			_toUnregister.Remove(o);
			_toRegister.Add(o);
			Dirty = true;
		}
		
		public void UnregisterObject(object o)
		{
			_toRegister.Remove(o);
			_toUnregister.Add(o);
			Dirty = true;
		}
		
		public void Clear()
		{
			_toRegister.Clear();
			_toUnregister.Clear();
		}
		
		void IDisposable.Dispose() => Clear();
	}
}
