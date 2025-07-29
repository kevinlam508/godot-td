#if DEBUG
using Godot;
using System;
using InputEvents;
using Debug.Tools;
using HexGrid;

public partial class HeightLayer : IDebugTool, IDisposable
{
	private static readonly Vector2I NoSelection = new Vector2I(-1, -1);
	private Vector2I _lastPaintedTile;

	partial void ToolsInit()
	{
		DebugToolManager.RegisterTool(this);
	}
	
	void IDisposable.Dispose()
	{
		DebugToolManager.UnregisterTool(this);
	}
	
	string IDebugTool.Name => "Terrain Height";
	
	void IDebugTool.PerformPrimaryAction(InputEventData inputData)
	{
		if (inputData.Type != InputEventType.Hold)
		{
			return;
		}
		
		ChangeHeight(true);
	}
	
	void IDebugTool.PerformSecondaryAction(InputEventData inputData)
	{
		if (inputData.Type != InputEventType.Hold)
		{
			return;
		}
		
		ChangeHeight(false);
	}
	
	private void ChangeHeight(bool isIncrease)
	{
		// Off grid or just painted, do nothing
		Vector2I selectedTile = GetSelectedTile();
		if (selectedTile == NoSelection || selectedTile == _lastPaintedTile)
		{
			return;
		}
		
		int height = this[selectedTile.Y, selectedTile.X];
		if (isIncrease)
		{
			height++;
		}
		else
		{
			height--;
		}
		height = Mathf.Clamp(height, MinHeight, MaxHeight);
		this[selectedTile.Y, selectedTile.X] = height;
		
		_lastPaintedTile = selectedTile;
		
		Dirty = true;
	}
	
	private Vector2I GetSelectedTile()
	{
		Vector2I index = _gridData.GetGridIndexFromWorldPosition(
			InputManager.CursorWorldPosition
		);
		
		if (IsIndexInBounds(index.X, index.Y))
		{
			return index;
		}
		else
		{
			return NoSelection;
		}
	}
}
#endif
