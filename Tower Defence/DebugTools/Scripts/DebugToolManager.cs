#if DEBUG
using Godot;
using System;
using System.Collections.Generic;
using InputEvents;

namespace Debug.Tools
{
	public partial class DebugToolManager : Node
	{
		private class BlankTool : IDebugTool
		{
			string IDebugTool.Name => "";
			
			void IDebugTool.PerformPrimaryAction(InputEventData _) {}
			void IDebugTool.PerformSecondaryAction(InputEventData _) {}
		}
		
		private static List<IDebugTool> _debugTools = new List<IDebugTool>();
		private int _toolIndex;
		
		[Export] private RichTextLabel _toolNameText;
		
		public override void _Ready()
		{
			// Always add to the front so this isn't order of operations
			// dependent
			_debugTools.Insert(0, new BlankTool());
			
			InputManager.RegisterInputEvent("debug_primary_action", 
				PerformPrimaryAction,
				InputEventType.All);
			InputManager.RegisterInputEvent("debug_secondary_action", 
				PerformSecondaryAction,
				InputEventType.All);
			InputManager.RegisterInputEvent("debug_next_tool", 
				NextTool,
				InputEventType.Press);
			InputManager.RegisterInputEvent("debug_previous_tool", 
				PreviousTool,
				InputEventType.Press);
			
			SetTool(0);
		}
		
		public override void _ExitTree()
		{
			InputManager.UnregisterInputEvent("debug_primary_action", 
				PerformPrimaryAction,
				InputEventType.All);
			InputManager.UnregisterInputEvent("debug_secondary_action", 
				PerformSecondaryAction,
				InputEventType.All);
			InputManager.UnregisterInputEvent("debug_next_tool", 
				NextTool,
				InputEventType.Press);
			InputManager.UnregisterInputEvent("debug_previous_tool", 
				PreviousTool,
				InputEventType.Press);
		}
		
		private void NextTool(InputEventData _) => SetTool(_toolIndex + 1);
		private void PreviousTool(InputEventData _) => SetTool(_toolIndex - 1);
		private void SetTool(int index)
		{
			_toolIndex = index;
			if (_toolIndex < 0)
			{
				_toolIndex += _debugTools.Count;
			}
			else if (_toolIndex >= _debugTools.Count)
			{
				_toolIndex = 0;
			}
			
			_toolNameText.Text = _debugTools[_toolIndex].Name;
		}
		
		private void PerformPrimaryAction(InputEventData inputData) 
			=> _debugTools[_toolIndex].PerformPrimaryAction(inputData);
			
		private void PerformSecondaryAction(InputEventData inputData) 
			=> _debugTools[_toolIndex].PerformSecondaryAction(inputData);
			
		public static void RegisterTool(IDebugTool tool) => _debugTools.Add(tool);
		public static void UnregisterTool(IDebugTool tool) => _debugTools.Remove(tool);
	}
}
#endif
