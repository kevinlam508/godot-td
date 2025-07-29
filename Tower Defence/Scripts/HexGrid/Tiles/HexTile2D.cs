using Godot;
using System;

namespace HexGrid
{
	public abstract partial class HexTile2D : HexTile
	{
		[Export] protected Node2D _positionRoot;
		
		[Export] protected NgonRenderer2D _renderer;
		[Export] protected Color _highlightColor;
		
		public override Vector2 Position
		{
			get => _positionRoot.Position;
			set 
			{
				_positionRoot.Position = value;
			}
		}
		
		public override void SetSize(float size)
		{
			_renderer.Size = size / 2;
		}
	}
}
