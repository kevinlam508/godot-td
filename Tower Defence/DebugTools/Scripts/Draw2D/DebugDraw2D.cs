using Godot;
using System;
using System.Collections.Generic;

#if DEBUG
namespace Debug
{
	public partial class DebugDraw2D : Node2D
	{
		private abstract class DrawCommand
		{
			protected Color _color;
			public double EndTime { get; private set; }
			
			public DrawCommand(Color color, double duration)
			{
				_color = color;
				EndTime = (ulong)(duration * 1000) + TimeProcessed;
			}
			
			public abstract void Draw(CanvasItem node);
		}
		
		private static DebugDraw2D Instance;
		private readonly static List<DrawCommand> Commands = new List<DrawCommand>();
		private static double TimeProcessed = 0;
		
		public override void _Ready()
		{
			if (Instance != null)
			{
				GD.Print($"Multiple {nameof(DebugDraw2D)} instances");
				return;
			}
			
			Instance = this;
		}
		
		public override void _ExitTree()
		{
			if (Instance == this)
			{
				Instance = null;
			}
		}
		
		public override void _Draw()
		{
			foreach (DrawCommand command in Commands)
			{
				command.Draw(this);
			}
		}
		
		public override void _Process(double delta)
		{
			bool hasRemoved = false;
			for (int i = Commands.Count - 1; i >= 0; i--)
			{
				if (Commands[i].EndTime < TimeProcessed)
				{
					Commands.RemoveAt(i);
					hasRemoved = true;
				}
			}
			
			if (hasRemoved)
			{
				Instance.QueueRedraw();	
			}
			
			TimeProcessed += delta;
		}
		
		private static void AddCommand(DrawCommand command)
		{
			Commands.Add(command);
			Instance.QueueRedraw();
		}
	}
}
#endif
