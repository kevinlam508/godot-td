using Godot;
using System;
using HexGrid;

public partial class TerrainHex : HexTile2D, IHeightTile
{
	[Export] private Color _bottomColor = Colors.SaddleBrown;
	[Export] private Color _topColor = Colors.Green;

	private int _minHeight;
	private int _maxHeight;
	private int _height;

	int IHeightTile.Height
	{
		get => _height;
		set
		{
			int newValue = Mathf.Clamp(value, _minHeight, _maxHeight);
			if (newValue == _height)
			{
				return;
			}
			
			_height = newValue;
			UpdateRender();
		}
	}
	
	public override void _Ready()
	{
		UpdateRender();
	}

	void IHeightTile.SetBounds(int min, int max)
	{
		if (min == _minHeight && max == _maxHeight)
		{
			return;
		}
		
		_minHeight = min;
		_maxHeight = max;
		_height = Mathf.Clamp(_height, _minHeight, _maxHeight);
		
		UpdateRender();
	}

	public void UpdateRender()
	{
		_renderer.Color = _bottomColor.Lerp(_topColor, 1.0f * _height / (_maxHeight - _minHeight));
	}
}
