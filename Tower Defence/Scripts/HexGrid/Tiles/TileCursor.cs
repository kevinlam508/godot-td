using Godot;
using System;

namespace HexGrid
{
	public partial class TileCursor : Node
	{
		[Export] protected Node2D _positionRoot;
			
		public Vector2 Position
		{
			get => _positionRoot.Position;
			set 
			{
				_positionRoot.Position = value;
			}
		}
	}
}
