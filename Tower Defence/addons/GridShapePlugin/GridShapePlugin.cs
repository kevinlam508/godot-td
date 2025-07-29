#if TOOLS
using Godot;
using System;

namespace HexGrid.Editor;

[Tool]
public partial class GridShapePlugin : EditorPlugin
{
	private GridShapeInspectorPlugin _plugin;
	
	public override void _EnterTree()
	{
		// Initialization of the plugin goes here.
		_plugin = new GridShapeInspectorPlugin();
		AddInspectorPlugin(_plugin);
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveInspectorPlugin(_plugin);
	}
}
#endif
