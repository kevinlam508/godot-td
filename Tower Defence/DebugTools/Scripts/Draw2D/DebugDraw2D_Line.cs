using Godot;
using System;
using System.Diagnostics;

namespace Debug
{
	public partial class DebugDraw2D : Node2D
	{
		[Conditional("DEBUG")]
		public static partial void DrawLine(Vector2 start, Vector2 end, Color color,
			double duration);
	}
	
	#if DEBUG
	public partial class DebugDraw2D : Node2D
	{
		private class LineCommand : DrawCommand
		{
			private Vector2 _start;
			private Vector2 _end;
			
			public LineCommand(Vector2 start, Vector2 end, Color color,
				double duration) : base(color, duration)
			{
				_start = start;
				_end = end;
			}
			
			public override void Draw(CanvasItem node)
			{
				node.DrawLine(_start, _end, _color);
			}
		}
		
		public static partial void DrawLine(Vector2 start, Vector2 end, Color color,
			double duration)
		{
			AddCommand(new LineCommand(start, end, color, duration));
		}
	}
	#else
	public partial class DebugDraw2D : Node2D
	{
		public static partial void DrawLine(Vector2 start, Vector2 end, Color color,
			double duration) {}
	}
	#endif
}
