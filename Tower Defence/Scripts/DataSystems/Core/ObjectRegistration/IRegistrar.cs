using Godot;
using System;

namespace DataSystems
{
	public interface IRegistrar
	{
		void Register(object toRegister);
		void Unregister(object toUnregister);
	}
}
