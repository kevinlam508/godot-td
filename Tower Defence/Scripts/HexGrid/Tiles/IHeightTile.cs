using Godot;
using System;

namespace HexGrid
{
	public interface IHeightTile
	{
		int Height { get; set; }
		void SetBounds(int min, int max);
	}
}
