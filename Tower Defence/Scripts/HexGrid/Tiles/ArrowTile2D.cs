using Godot;
using System;

namespace HexGrid
{
	public partial class ArrowTile2D : ArrowTile
	{
		[Export] protected Node2D _positionRoot;
		
		[Export] private Node2D _arrowTransform;
		
		public override Vector2 Position
		{
			get => _positionRoot.Position;
			set 
			{
				_positionRoot.Position = value;
			}
		}
		
		public override void SetSize(float size) {}
		
		public override void ShowArrow(bool show)
		{
			_arrowTransform.Visible = show;
		}
		
		protected override void RotateArrow(float degrees)
		{
			_arrowTransform.RotationDegrees += degrees;
		}
	}
}
