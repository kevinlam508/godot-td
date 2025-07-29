using Godot;
using System;

[GlobalClass]
public partial class TowerData : Resource
{
	[Export] private string _name;
	[Export] private int _cost;
	[Export] private PackedScene _towerScene;
	
	public string Name => _name;
	public int Cost => _cost;
	public PackedScene TowerScene => _towerScene;
}
