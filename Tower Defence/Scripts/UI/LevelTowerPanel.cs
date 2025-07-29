using Godot;
using System;

public partial class LevelTowerPanel : TowerSelectionList
{
	public event Action LinkModePressed;
	
	public void OnLinkModePressed()
	{
		LinkModePressed?.Invoke();
	}
}
