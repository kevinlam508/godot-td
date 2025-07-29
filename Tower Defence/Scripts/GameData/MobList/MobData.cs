using Godot;
using System;

[GlobalClass]
public partial class MobData : Resource
{
	[Export] private string _name;
	[Export] private PackedScene _template;
	
	public string Name => _name;
	public PackedScene Template => _template;
}
