#if TOOLS
using Godot;
using System;
using System.Reflection;

public static class EditorPropertyUtil
{
	private static PropertyUsageFlags[] GroupHierarchy = 
	{
		PropertyUsageFlags.Category,
		PropertyUsageFlags.Group,
		PropertyUsageFlags.Subgroup
	};
	
	public static bool IsPropertyDeclaredInClass<T>(GodotObject o, string name)
	{
		bool isDerivedProperty = false;
		
		BindingFlags searchFlags = BindingFlags.NonPublic 
			| BindingFlags.Public 
			| BindingFlags.Instance
			| BindingFlags.DeclaredOnly;
		Type classType = typeof(T);
		if (classType.GetProperty(name, searchFlags) != null)
		{
			isDerivedProperty = true;
		}
		else if (classType.GetField(name, searchFlags) != null)
		{
			isDerivedProperty = true;
		}
		
		return isDerivedProperty;
	}
	
	public static bool IsPropertyInCategory(GodotObject o, string name, 
		string categoryName)
	{
		return IsPropertyInGrouping(o, name, categoryName,
			PropertyUsageFlags.Category);
	}
	
	public static bool IsPropertyInGroup(GodotObject o, string name, 
		string groupName)
	{
		return IsPropertyInGrouping(o, name, groupName,
			PropertyUsageFlags.Group);
	}
	
	private static bool IsPropertyInGrouping(GodotObject o, string name, 
		string groupName, PropertyUsageFlags usageGroup)
	{
		int groupIndex = Array.IndexOf(GroupHierarchy, usageGroup);
		
		string[] lastGroup = new string[3];
		foreach(var property in o.GetPropertyList())
		{
			for(int i = 0; i < GroupHierarchy.Length; i++)
			{
				if (property["usage"].As<PropertyUsageFlags>().HasFlag(GroupHierarchy[i]))
				{
					lastGroup[i] = property["name"].As<string>();
					for (int j = i + 1; j < GroupHierarchy.Length; j++)
					{
						lastGroup[j] = null;
					}
					break;
				}
			}
			
			if (property["usage"].As<PropertyUsageFlags>().HasFlag(PropertyUsageFlags.ScriptVariable)
				&& property["name"].As<string>() == name)
			{
				return lastGroup[groupIndex] == groupName;
			}
		}
		
		return false;
	}
}
#endif
