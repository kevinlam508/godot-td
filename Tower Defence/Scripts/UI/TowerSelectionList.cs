using Godot;
using System;

public partial class TowerSelectionList : Control
{
	[Export] private Node _listContainer;
	[Export] private PackedScene _towerListing;
	
	[Export] private TowerList _towers;
	
	private ITowerDataSelectionHandler _handler;
	
	public void Init(ITowerDataSelectionHandler handler)
	{
		_handler = handler;
		
		foreach (TowerData data in _towers.Entries)
		{
			TowerSelectionButton newButton = _towerListing.Instantiate<TowerSelectionButton>();
			newButton.Init(data, _handler);
			_listContainer.AddChild(newButton);
		}
	}
}
