#if DEBUG
using Godot;
using System;
using System.Collections.Generic;
using DataSystems;
using Debug.Tools;
using InputEvents;

public partial class MobSpawnSystem : IDebugTool, IDisposable
{
	private struct SpawnRequest
	{
		public Vector2 Location;
		public PackedScene MobTemplate;
	}
	
	private int _selectionIndex = 0;
	private Queue<SpawnRequest> _toolSpawnQueue = new Queue<SpawnRequest>();
	
	public MobSpawnSystem()
	{
		DebugToolManager.RegisterTool(this);
	}
	
	void IDisposable.Dispose()
	{
		DebugToolManager.UnregisterTool(this);
	}
	
	string IDebugTool.Name => "Mob Spawn";
	
	void IDebugTool.PerformPrimaryAction(InputEventData inputData)
	{
		if (inputData.Type != InputEventType.Press)
		{
			return;
		}
		
		_toolSpawnQueue.Enqueue(
			new SpawnRequest
			{
				Location = InputManager.CursorWorldPosition,
				MobTemplate = _beastiary.MobInfo[_selectionIndex].Template
			}
		);
	}
	
	void IDebugTool.PerformSecondaryAction(InputEventData inputData)
	{
		if (inputData.Type != InputEventType.Press)
		{
			return;
		}
		
		_selectionIndex = (_selectionIndex + 1) % _beastiary.MobInfo.Length;
	}
	
	partial void RunTool(GridDataLayer gridData, ObjectRegistrationLayer registration)
	{
		while (_toolSpawnQueue.Count > 0)
		{
			SpawnRequest request = _toolSpawnQueue.Dequeue();
			SpawnMob(registration,
				request.MobTemplate,
				gridData,
				request.Location);
		}
	}
}
#endif
