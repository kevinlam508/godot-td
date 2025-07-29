using Godot;
using System;

public partial class TriggerLinkArrow : Node2D
{
	[Export] private float _defaultBodyLength;
	[Export] private Node2D _arrowBody;
	[Export] private Node2D _arrowHead;
	
	public void SetPosition(Vector2 start, Vector2 end)
	{
		Position = start;
		Vector2 direction = end - start;
		Rotation = Vector2.Right.AngleTo(direction);
		
		// Position head
		float distance = direction.Length();
		_arrowHead.Position = Vector2.Right * distance;
		
		// Stretch body
		Vector2 scale = _arrowBody.Scale;
		scale.X = distance / _defaultBodyLength;
		_arrowBody.Scale = scale;
	}
}
