//#define PERFORMANCE_LOG

using Godot;
using System;
using System.Collections.Generic;
using Camera;
using InputEvents.Data;

namespace InputEvents
{
	[Flags]
	public enum InputEventType
	{
		None 	= 0,
		Press	= 1 << 1,
		Release	= 1 << 2,
		Hold	= 1 << 3,
		PressOrRelease = Press | Release,
		PressOrHold = Press | Hold,
		ReleaseOrHold = Release | Hold,
		All = Press | Release | Hold
	}
	
	public struct InputEventData
	{
		public InputEventType Type;
		
		public double HoldTimeDelta;
	}
	
	public delegate void InputCallback(InputEventData data);
	
	/*
	 *  Owns if and when an input action happens
	 *   - Does the checks when the keys are pressed
	 *   - Triggers events for input actions
	 *   - Handles changes in input mode
	 */
	public partial class InputManager : Node
	{
		// Basic Events
		private static Dictionary<string, InputCallback> _pressEvents = new Dictionary<string, InputCallback>();
		private static Dictionary<string, InputCallback> _releaseEvents = new Dictionary<string, InputCallback>();
		private static Dictionary<string, InputCallback> _holdEvents = new Dictionary<string, InputCallback>();
		private static HashSet<string> _heldEvents = new HashSet<string>();
		
		// Cursor Move Event
		private static HashSet<ICursorPositionListener> _cursorMoveListeners = new HashSet<ICursorPositionListener>();
		
		// Input Enable/Disable
		private static HashSet<string> _disabledInputGroups = new HashSet<string>();
		
		// Current Position
		public static Vector2 CursorWorldPosition { get; private set; }
		public static Vector2 CursorScreenPosition { get; private set; }
		
		// Delta since last frame
		public static Vector2 CursorWorldDelta 
		{ 
			get => _cursorDeltaFrame == Engine.GetProcessFrames() 
				? _cursorWorldDelta : Vector2.Zero;
			private set => _cursorWorldDelta = value;
		}
		public static Vector2 CursorScreenDelta 
		{ 
			get => _cursorDeltaFrame == Engine.GetProcessFrames() 
				? _cursorScreenDelta : Vector2.Zero;
			private set => _cursorScreenDelta = value;
		}
		private static Vector2 _cursorWorldDelta;
		private static Vector2 _cursorScreenDelta;
		private static ulong _cursorDeltaFrame;
		
		[Export] private InputGroups _groups;
		
		public override void _Ready()
		{
			_groups.InitGroupLookup();
		}
		
		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			HandleHoldEvents(delta);
		}
		
		// Hooked into unhandled input so this only catches unused input
		// EX: only mouse events that aren't caught by UI
		public override void _UnhandledInput(InputEvent e)
		{
			#if PERFORMANCE_LOG
			using var _ = new Debug.TimeLogger.TimeScope("InputLoop");
			#endif

			if (e is InputEventMouseMotion mouseEvent)
			{
				UpdateCursorPosition(mouseEvent);
				return;
			}
			
			if (e.IsPressed())
			{
				HandlePressEvents(e);
			}
			else if (e.IsReleased())
			{
				HandleReleaseEvents(e);
			}
			UpdateHoldEvent(e);
		}
		
		private void HandlePressEvents(InputEvent e)
		{
			#if PERFORMANCE_LOG
			using var _ = new Debug.TimeLogger.TimeScope("Press");
			#endif
			
			foreach((string actionName, InputCallback callback) in _pressEvents)
			{
				if (!IsActionEnabled(actionName) || !e.IsActionPressed(actionName))
				{
					continue;
				}
				
				#if PERFORMANCE_LOG
				using var __ = new Debug.TimeLogger.TimeScope(actionName);
				#endif
				
				callback?.Invoke(new InputEventData{
					Type = InputEventType.Press
				});
			}
		}
		
		private void HandleReleaseEvents(InputEvent e)
		{
			#if PERFORMANCE_LOG
			using var _ = new Debug.TimeLogger.TimeScope("Release");
			#endif
			
			foreach((string actionName, InputCallback callback) in _releaseEvents)
			{
				if (!IsActionEnabled(actionName) || !e.IsActionReleased(actionName))
				{
					continue;
				}
				
				#if PERFORMANCE_LOG
				using var __ = new Debug.TimeLogger.TimeScope(actionName);
				#endif

				callback?.Invoke(new InputEventData{
					Type = InputEventType.Release
				});
			}
		}
		
		private void UpdateHoldEvent(InputEvent e)
		{
			#if PERFORMANCE_LOG
			using var _ = new Debug.TimeLogger.TimeScope("Update Hold");
			#endif
			
			foreach((string actionName, InputCallback action) in _holdEvents)
			{
				if (!IsActionEnabled(actionName) || e.IsActionReleased(actionName))
				{
					_heldEvents.Remove(actionName);
				}
				else if (e.IsActionPressed(actionName))
				{
					_heldEvents.Add(actionName);
				}
			}
		}
		
		private void UpdateCursorPosition(InputEventMouseMotion mouseEvent)
		{
			#if PERFORMANCE_LOG
			using var _ = new Debug.TimeLogger.TimeScope("Cursor Position");
			#endif
			
			CursorScreenPosition = mouseEvent.GlobalPosition;
			CursorWorldPosition = GetViewport().CanvasTransform.AffineInverse()
				* mouseEvent.GlobalPosition;
			
			CursorScreenDelta = mouseEvent.Relative;
			CursorWorldDelta = GetViewport().CanvasTransform.AffineInverse()
				* mouseEvent.Relative;
			_cursorDeltaFrame = Engine.GetProcessFrames();
			
			foreach (var listener in _cursorMoveListeners)
			{
				listener.OnCursorMoved(CursorScreenPosition, CursorWorldPosition);
			}
		}
		
		private void HandleHoldEvents(double timeDelta)
		{
			if (_heldEvents.Count == 0)
			{
				return;
			}
			
			#if PERFORMANCE_LOG
			using var _ = new Debug.TimeLogger.TimeScope("Perform Hold");
			#endif
				
			foreach (string actionName in _heldEvents)
			{
				#if PERFORMANCE_LOG
				using var __ = new Debug.TimeLogger.TimeScope(actionName);
				#endif
				
				_holdEvents[actionName]?.Invoke(
					new InputEventData{
						Type = InputEventType.Hold,
						HoldTimeDelta = timeDelta
					});
			}
		}
		
		private bool IsActionEnabled(string actionName)
		{
			string groupName = _groups.GetInputGroup(actionName);
			return string.IsNullOrEmpty(groupName)
				|| !_disabledInputGroups.Contains(groupName);
		}
		
		public static void RegisterInputEvent(string actionName, InputCallback action, InputEventType types)
		{
			if ((types & InputEventType.Press) == InputEventType.Press)
			{
				RegisterWithEvent(actionName, action, _pressEvents);
			}
			if ((types & InputEventType.Release) == InputEventType.Release)
			{
				RegisterWithEvent(actionName, action, _releaseEvents);
			}
			if ((types & InputEventType.Hold) == InputEventType.Hold)
			{
				RegisterWithEvent(actionName, action, _holdEvents);
			}
		}
		
		private static void RegisterWithEvent(string actionName, InputCallback action, Dictionary<string, InputCallback> events)
		{
			events.TryGetValue(actionName, out var existing);
			events[actionName] = existing + action;
		}
		
		public static void UnregisterInputEvent(string actionName, InputCallback action, InputEventType types)
		{
			if ((types & InputEventType.Press) == InputEventType.Press)
			{
				UnregisterWithEvent(actionName, action, _pressEvents);
			}
			if ((types & InputEventType.Release) == InputEventType.Release)
			{
				UnregisterWithEvent(actionName, action, _releaseEvents);
			}
			if ((types & InputEventType.Hold) == InputEventType.Hold)
			{
				UnregisterWithEvent(actionName, action, _holdEvents);
			}
		}
		
		private static void UnregisterWithEvent(string actionName, InputCallback action, Dictionary<string, InputCallback> events)
		{
			if(!events.TryGetValue(actionName, out var existing))
			{
				return;
			}
			
			existing -= action;
			if (existing == null)
			{
				events.Remove(actionName);
			}
			else
			{
				events[actionName] = existing;
			}
		}
		
		public static void RegisterCursorPositionListener(ICursorPositionListener l)
			=> _cursorMoveListeners.Add(l);
		public static void UnregisterCursorPositionListener(ICursorPositionListener l)
			=> _cursorMoveListeners.Remove(l);
			
		public static void EnableGroup(string groupName, bool enable)
		{
			GD.Print($"[Input] {(enable ? "Enabling" : "Disabling")} {groupName}");
			if (enable)
			{
				_disabledInputGroups.Remove(groupName);
			}
			else
			{
				_disabledInputGroups.Add(groupName);
			}
		}
		
		public static bool IsGroupEnabled(string groupName)
			=> !_disabledInputGroups.Contains(groupName);
	}
}
