using Godot;
using System;

[GlobalClass]
public partial class TowerList : Resource
{
	[Export] private TowerData[] _entries;
	
	public TowerData[] Entries => _entries;
}
