using Godot;
using System;
using InputEvents;

namespace Camera
{
	public enum ZoomDirection
	{
		In,
		Out
	}
	
	public partial class CameraController : Camera2D
	{
		[ExportGroup("Zoom")]
		[Export] private float _maxZoom = .5f;
		[Export] private float _minZoom = 1f;
		[Export] private float _relativeZoomSpeed = .1f;
		[Export] private float _holdZoomSpeedMultiplier = 1f;

		private ulong _zoomPressFrame = 0;

		public override void _Ready()
		{
			InputManager.RegisterInputEvent("camera_zoom_in", 
				OnZoomIn, 
				InputEventType.All);
			InputManager.RegisterInputEvent("camera_zoom_out", 
				OnZoomOut, 
				InputEventType.All);
			InputManager.RegisterInputEvent("camera_pan_drag", 
				OnPanDrag,
				InputEventType.Hold);
		}
		
		public override void _ExitTree()
		{
			InputManager.UnregisterInputEvent("camera_zoom_in", 
				OnZoomIn, 
				InputEventType.All);
			InputManager.UnregisterInputEvent("camera_zoom_out", 
				OnZoomOut, 
				InputEventType.All);
			InputManager.UnregisterInputEvent("camera_pan_drag", 
				OnPanDrag, 
				InputEventType.Hold);
		}
		
		private void OnZoomIn(InputEventData data) 
			=> OnZoom(ZoomDirection.In, data);
		private void OnZoomOut(InputEventData data) 
			=> OnZoom(ZoomDirection.Out, data);

		private void OnZoom(ZoomDirection dir, InputEventData inputData)
		{
			// Process zoom input as:
			// - A held button (most cases)
			// - Rapid repeated presses (mouse wheel). 
			//    Detectable as press and release on the same frame
			ulong frame = Engine.GetProcessFrames();
			if (inputData.Type == InputEventType.Press)
			{
				_zoomPressFrame = frame;
				return;
			}
			else if (inputData.Type == InputEventType.Release 
				&& frame != _zoomPressFrame)
			{
				return;
			}
			
			float delta = (ZoomDirection)dir switch
			{
				ZoomDirection.In => -1 * _relativeZoomSpeed,
				ZoomDirection.Out => _relativeZoomSpeed,
				_ => 0	
			};
			float timeDelta = (float)inputData.HoldTimeDelta;
			if (timeDelta > 0)
			{
				delta *= timeDelta * _holdZoomSpeedMultiplier;
			}
			
			float currentPercent = Mathf.InverseLerp(_minZoom, _maxZoom, Zoom.X);
			float newZoom = Mathf.Lerp(_minZoom, _maxZoom, currentPercent + delta);
			Zoom = Vector2.One * Mathf.Clamp(newZoom, _maxZoom, _minZoom);
		}
		
		private void OnPanDrag(InputEventData _) => OnPan(InputManager.CursorScreenDelta);
		
		private void OnPan(Vector2 delta)
		{
			Position -= delta / Zoom.X;
			
			Vector2 viewSize = GetViewport().GetVisibleRect().Size / Zoom.X;
			Position = Position.Clamp(
					new Vector2(LimitLeft, LimitTop) + viewSize / 2,
					new Vector2(LimitRight, LimitBottom) - viewSize / 2
				);
		}
	}
}
