using Godot;
using System;
using System.Collections.Generic;
using HexGrid;
using DataSystems;
using InputEvents;

using Debug;

public class TriggerLinkSystem : DataLayerSystem, IDisposable
{
	private InputEventType _lastInput;
	private Vector2I? _currentSelection;
	private InputCallback _dragCallback;
		
	public override Type[] WriteLayerTypes => 
		[
			typeof(TriggerLayer)
		];
		
	public TriggerLinkSystem()
	{
		_dragCallback = ConnectionDrag;
		Enabled = false;
	}
	
	public override void Run(double deltaTime, double totalTime,
			Dictionary<Type, DataLayer> readLayers, 
			Dictionary<Type, DataLayer> writeLayers)
	{
		TriggerLayer triggerLayer = GetLayer<TriggerLayer>(writeLayers);
		
		InputEventType input = _lastInput;
		_lastInput = InputEventType.None;
		switch(input)
		{
			case InputEventType.Press:
				BeginConnection(triggerLayer);
				break;
			case InputEventType.Release:
				EndConnection(triggerLayer);
				break;
		}
		
		if (_currentSelection != null)
		{
			UpdateConnectionLinePreview
				(
					triggerLayer.GetWorldPositionFromGridIndex(_currentSelection.Value),
					InputManager.CursorWorldPosition
				);
		}
	}
	
	private void BeginConnection(TriggerLayer triggerLayer)
	{
		Vector2I gridIndex = triggerLayer.GetGridIndexFromWorldPosition
			(
				InputManager.CursorWorldPosition
			);
		if (!triggerLayer.IsIndexInBounds(gridIndex))
		{
			return;
		}
		if (triggerLayer.GetTrigger(gridIndex) != null)
		{
			_currentSelection = gridIndex;
		}
	}
	
	private void EndConnection(TriggerLayer triggerLayer)
	{
		// Didn't start drag on a valid index
		if (_currentSelection == null)
		{
			return;
		}
		
		Vector2I gridIndex = triggerLayer.GetGridIndexFromWorldPosition
			(
				InputManager.CursorWorldPosition
			);
		if (triggerLayer.IsIndexInBounds(gridIndex))
		{
			ITrigger trigger = triggerLayer.GetTrigger(_currentSelection.Value);
			ITriggerable triggerable = triggerLayer.GetTriggerable(gridIndex);
			if (triggerable != null && triggerable != trigger)
			{
				if (triggerLayer.HasLink(trigger, triggerable))
				{
					triggerLayer.RemoveLink(trigger, triggerable);
				}
				else
				{
					triggerLayer.AddLink(trigger, triggerable);
				}
			}
		}
		_currentSelection = null;
	}
	
	private void UpdateConnectionLinePreview(Vector2 start, Vector2 end)
	{
		DebugDraw2D.DrawLine(
				start,
				end,
				Colors.White,
				0
			);
	}
	
	protected override void OnEnable()
	{
		InputManager.RegisterInputEvent("level_select_action", 
				_dragCallback,
				InputEventType.PressOrRelease);
	}
	
	protected override void OnDisable()
	{
		InputManager.UnregisterInputEvent("level_select_action", 
				_dragCallback,
				InputEventType.PressOrRelease);
	}
	
	private void ConnectionDrag(InputEventData inputData)
	{
		_lastInput = inputData.Type;
	}
	
	void IDisposable.Dispose()
	{
		OnDisable();
	}
}
