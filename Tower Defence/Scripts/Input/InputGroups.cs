using Godot;
using System;
using Godot.Collections;
using System.Collections;

namespace InputEvents.Data
{
	[GlobalClass] // Adds to editor's create lists
	public partial class InputGroups : Resource
	{
		[Export] public Array<InputGroup> Groups { get; private set; }
		
		private Dictionary<string, string> _groupLookup;
		
		public void InitGroupLookup()
		{
			if (_groupLookup != null)
			{
				return;
			}
			
			_groupLookup = new Dictionary<string, string>();
			foreach(InputGroup g in Groups)
			{
				string groupName = g.Name;
				foreach(string inputAction in g.InputActions)
				{
					if (_groupLookup.TryGetValue(inputAction, out string existingGroup))
					{
						GD.PrintErr($"[Input] {inputAction} is assigned to {existingGroup}. Cannot assign to {groupName}.");
						continue;
					}
					
					_groupLookup[inputAction] = groupName;
				}
			}
		}
		
		public string GetInputGroup(string inputAction)
			=> _groupLookup.TryGetValue(inputAction, out string groupName)
				? groupName : string.Empty; 
	}
}
