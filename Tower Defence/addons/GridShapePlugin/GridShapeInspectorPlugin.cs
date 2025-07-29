#if TOOLS
using Godot;
using System;
using System.Reflection;

namespace HexGrid.Editor;

public partial class GridShapeInspectorPlugin : EditorInspectorPlugin
{
	public override bool _CanHandle(GodotObject o)
	{
		return o is GridShape;
	}
	
	public override bool _ParseProperty(GodotObject o, Variant.Type type,
		string name, PropertyHint hintType, string hintString,
		PropertyUsageFlags usageFlags, bool wide)
	{
		if (name == nameof(GridShape.Points))
		{
			AddPropertyEditor(name, new GridShapePropertyInspector());
		}
		return EditorPropertyUtil.IsPropertyInGroup(o, name, "Shape");
	}
	
	public override void _ParseGroup(GodotObject o, string name)
	{
		if (name != "Shape")
		{
			return;
		}
	}
}
#endif
