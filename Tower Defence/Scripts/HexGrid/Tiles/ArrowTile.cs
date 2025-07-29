using Godot;
using System;

namespace HexGrid
{
	public abstract partial class ArrowTile : HexTile
	{
		[Export] protected GridDirection _arrowDirection;
		
		public void UpdateDirection(GridDirection newDirection)
		{
			if (_arrowDirection == newDirection)
			{
				return;
			}
			
			int change = (int)(newDirection - _arrowDirection);
			RotateArrow(-GridUtility.DegreesPerRotation * change);
			_arrowDirection = newDirection;
		}
		
		public abstract void ShowArrow(bool show);
		protected abstract void RotateArrow(float degrees);
	}
}
