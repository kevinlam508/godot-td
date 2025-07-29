#if TOOLS
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

namespace HexGrid.Editor;

using ToogledEventHandler = BaseButton.ToggledEventHandler;

public partial class GridShapePropertyInspector : EditorProperty
{
	private class ToggleRow : IDisposable
	{
		private HBoxContainer _container;
		private List<CheckBox> _toggles = new List<CheckBox>();
		private System.Collections.Generic.Dictionary<CheckBox, ToogledEventHandler> _toggleCallbacks = new System.Collections.Generic.Dictionary<CheckBox, ToogledEventHandler>();
		
		private int _num;
		private Action<Vector2I> _toggleCallback;
		
		public ToggleRow(Control parent, int num, Action<Vector2I> toggleCallback)
		{
			var margin = new MarginContainer();
			margin.AddThemeConstantOverride("margin_left", num * IndentPerRow);
			parent.AddChild(margin);
			
			_container = new HBoxContainer();
			_container.AddThemeConstantOverride("separation", GridSeparation);
			margin.AddChild(_container);
			
			_num = num;
			_toggleCallback = toggleCallback;
		}
		
		public void Resize(int size)
		{
			// Expand
			for(int i = _toggles.Count; i < size; i++)
			{
				var toggle = new CheckBox();
				_container.AddChild(toggle);
				_toggles.Add(toggle);
				
				Vector2I point = new Vector2I(i, _num);
				ToogledEventHandler callback = (bool _) => _toggleCallback.Invoke(point);
				toggle.Toggled += callback;
				
				_toggleCallbacks.Add(toggle, callback);
			}
			
			// Shrink or show previous
			for(int i = 0; i < _toggles.Count; i++)
			{
				_toggles[i].Visible = i < size;
			}
		}
		
		public void Show(bool show)
		{
			_container.Visible = show;
		}
		
		public void ToggleOn(int i)
		{
			_toggles[i].SetPressedNoSignal(true);
		}
		
		public void ToggleAllOff()
		{
			foreach(var toggle in _toggles)
			{
				toggle.SetPressedNoSignal(false);
			}
		}
		
		void IDisposable.Dispose()
		{
			foreach ((CheckBox toggle, ToogledEventHandler callback) in _toggleCallbacks)
			{
				toggle.Toggled -= callback;
			}
			_toggleCallback = null;
		}
	}
	
	private class ToggleHexGrid : IDisposable
	{
		private VBoxContainer _container;
		private List<ToggleRow> _rows = new List<ToggleRow>();
		
		private Action<Vector2I> _toggleCallback;
		
		public ToggleHexGrid(Control parent, Action<Vector2I> toggleCallback)
		{
			_container = new VBoxContainer();
			_container.AddThemeConstantOverride("separation", GridSeparation);
			parent.AddChild(_container);
			
			_toggleCallback = toggleCallback;
		}
		
		public void Resize(Vector2 newSize)
		{
			// Expand
			for(int i = _rows.Count; i < newSize.Y; i++)
			{
				var row = new ToggleRow(_container, i, _toggleCallback);
				row.Resize((int)newSize.X);
				_rows.Add(row);
			}
			
			// Shrink or show previous
			for(int i = 0; i < _rows.Count; i++)
			{
				bool show = i < newSize.Y;
				_rows[i].Show(show);
				
				if (show)
				{
					_rows[i].Resize((int)newSize.X);
				}
			}
		}
		
		public void ToggleOn(Vector2 point)
		{
			_rows[(int)point.Y].ToggleOn((int)point.X);
		}
		
		public void ToggleAllOff()
		{
			foreach(var row in _rows)
			{
				row.ToggleAllOff();
			}
		}
		
		void IDisposable.Dispose()
		{
			foreach(var row in _rows)
			{
				(row as IDisposable).Dispose();
			}
			_toggleCallback = null;
		}
	}
	
	private const int MinShapeGridSize = 5;
	private const int GridSeparation = 0;
	private const int IndentPerRow = 13;
	private static readonly Vector2 PropertyHeight = new Vector2(0, 120);
	
	private ScrollContainer _shapeContainer = new ScrollContainer();
	
	private ToggleHexGrid _toggleGrid;
	
	private GridShape _currentShape;
	private Array<Vector2I> _currentPoints = new Array<Vector2I>();
	
	private GridShape Shape => GetEditedObject() as GridShape;
	
	public GridShapePropertyInspector()
	{
		// Set value box to custom height
		_shapeContainer.CustomMinimumSize = PropertyHeight;
		
		AddChild(_shapeContainer);
		_toggleGrid = new ToggleHexGrid(_shapeContainer, ToggleShapePoint);
	}
	
	public override void _ExitTree()
	{
		base._ExitTree();
		((IDisposable)_toggleGrid).Dispose();
	}
	
	public override void _UpdateProperty()
	{
		if(_currentShape != Shape)
		{
			_currentShape = Shape;
			_currentPoints.Clear();
			_currentPoints.AddRange(Shape.Points);
		}

		// Allow grid up to +1 of largest point
		Vector2I shapeExtrema = Shape.GetMaxPoint();
		Vector2I newSize = new Vector2I(
			Mathf.Max(shapeExtrema.X + 2, MinShapeGridSize),
			Mathf.Max(shapeExtrema.Y + 2, MinShapeGridSize)
		);
		
		_toggleGrid.ToggleAllOff();
		_toggleGrid.Resize(newSize);
		foreach(Vector2I point in _currentPoints)
		{
			_toggleGrid.ToggleOn(point);
		}
	}

	private void ToggleShapePoint(Vector2I point)
	{	
		int index = _currentPoints.IndexOf(point);
		// New point, add
		if (index < 0)
		{
			_currentPoints.Add(point);
		}
		// Existing point, remove
		else
		{
			_currentPoints[index] = _currentPoints[_currentPoints.Count - 1];
			_currentPoints.RemoveAt(_currentPoints.Count - 1);
		}
		
		// Modifies the property and sets up undo/redo
		// Also calls for redraw
		EmitChanged(nameof(GridShape.Points), _currentPoints);
	}
}
#endif
