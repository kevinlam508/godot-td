using Godot;
using System;

namespace HexGrid
{
	public abstract partial class HexTile : Node
	{
		// Worldspace of the position on the grid plane
		public abstract Vector2 Position { get; set; }

		public abstract void SetSize(float size);
	}
}
