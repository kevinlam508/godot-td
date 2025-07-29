using Godot;
using System;
using InputEvents;

namespace Debug.Tools
{
	public interface IDebugTool
	{
		string Name { get; }
		
		void PerformPrimaryAction(InputEventData inputData);
		void PerformSecondaryAction(InputEventData inputData);
	}
}
