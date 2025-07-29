using Godot;
using System;
using Godot.Collections;

namespace InputEvents.Data
{
	[GlobalClass]  // Adds to editor's create lists. Required to edit nested usages
	public partial class InputGroup : Resource
	{
		[Export] public string Name { get; private set; }
		[Export] public Array<string> InputActions { get; private set; }
	}
}
