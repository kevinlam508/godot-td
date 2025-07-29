using Godot;
using System;
using System.Collections.Generic;
using DataSystems;

public class ProjectileLayer : DataLayer, IRegistrar
{
	public readonly HashSet<IProjectile> Projectiles = new HashSet<IProjectile>();
	
	void IRegistrar.Register(object o)
	{
		if (!(o is IProjectile projectile))
		{
			return;
		}
		
		Projectiles.Add(projectile);
	}
	
	void IRegistrar.Unregister(object o)
	{
		if (!(o is IProjectile projectile))
		{
			return;
		}
		
		Projectiles.Remove(projectile);
	}
}
