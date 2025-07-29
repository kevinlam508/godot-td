using Godot;
using System;
using DataSystems;
using DataSystems.Spawn;

namespace Towers.ActivationModules;

public class ProjectileModule : BaseActivationModule<IProjectileTriggerable, SpawnLayer<ProjectileSpawnRequest>>
{
	protected override void ActivateInternal(IProjectileTriggerable t,
		SpawnLayer<ProjectileSpawnRequest> layer)
	{
		layer.Requests.Add(
			new ProjectileSpawnRequest
			{
				Template = t.Template,
				Location = t.Position,
				Rotation = t.AimDirection
			}
		);
	}
}
