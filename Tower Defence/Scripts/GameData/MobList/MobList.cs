using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class MobList : Resource
{
	[Export] private MobData[] _mobs;
	
	public MobData[] MobInfo => _mobs;
}
