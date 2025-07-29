using Godot;
using System;

public static class GodotUtils
{
	public static T GetParent<T>(this Node node)
		where T : Node
	{
		while (node.GetParent() != null)
		{
			node = node.GetParent();
			if (node is T result)
			{
				return result;
			}
		}
		return null;
	}
	
	public static bool IsInside(this Shape2D shape, Transform2D transform, Vector2 point)
	{
		switch (shape)
		{
			case CircleShape2D circle:
				float distanceSquared = transform.Origin.DistanceSquaredTo(point);
				return distanceSquared <= circle.Radius * circle.Radius;
			default:
				GD.PrintErr("Unimplemented point to shape check");
				return false;
		}
	}
}
