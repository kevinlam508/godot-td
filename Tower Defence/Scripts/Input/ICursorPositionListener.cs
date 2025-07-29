using Godot;
using System;

namespace InputEvents
{	
	public interface ICursorPositionListener
	{
		void OnCursorMoved(Vector2 screenPosition, Vector2 worldPosition);
	}
}
