using Godot;
using System;
using System.Collections.Generic;
using HexGrid;
using DataSystems;
using InputEvents;

public class TowerPlacementSystem : DataLayerSystem, 
	ITowerDataSelectionHandler, IDisposable
{
	// Selection
	private TowerData _currentSelection;
	
	// Placement
	private Node _towerParent;
	private GridObject _placementPreview;
	private GridRotation _previewRotation;
	private bool _tryPlace;
	
	// Callback caching
	private InputCallback _rotateInputCallback;
	private InputCallback _placeInputCallback;
	
	public override Type[] ReadLayerTypes => 
		[
			typeof(GridObjectLayer),
			typeof(MobLayer)
		];
	public override Type[] WriteLayerTypes => 
		[
			typeof(ObjectRegistrationLayer)
		];
	
	public TowerPlacementSystem(Node towerParent)
	{
		_towerParent = towerParent;
		
		_rotateInputCallback = RotateTower;
		_placeInputCallback = ReqestPlacement;
	}
	
	void IDisposable.Dispose()
	{
		ClearPreview();
	}
	
	public override void Run (double deltaTime, double totalTime,
			Dictionary<Type, DataLayer> readLayers, 
			Dictionary<Type, DataLayer> writeLayers)
	{
		GridObjectLayer goLayer = GetLayer<GridObjectLayer>(readLayers);
		MobLayer mobLayer = GetLayer<MobLayer>(readLayers);
		ObjectRegistrationLayer registerLayer = GetLayer<ObjectRegistrationLayer>(writeLayers);
		
		Vector2 cursorPosition = InputManager.CursorWorldPosition;
		Vector2I cursorGridPosition = goLayer.GetGridIndexFromWorldPosition(cursorPosition);
		bool cursorInBounds = goLayer.IsIndexInBounds(cursorGridPosition);
		
		// Tower selected, but preivew not made yet. Do that now
		if (_currentSelection != null && _placementPreview == null)
		{
			CreatePreview(
				goLayer.OriginWorldPosition,
				goLayer.TileSize);
		}
		UpdatePreview(goLayer, cursorGridPosition, cursorInBounds);
		PlaceTower(goLayer, mobLayer, registerLayer, cursorGridPosition, cursorInBounds);
	}
	
	private void UpdatePreview(GridObjectLayer goLayer, Vector2I cursorGridPosition,
		bool cursorInBounds)
	{
		// Nothing to place, do nothing
		if (_placementPreview == null)
		{
			return;
		}
		
		// Not in bounds, cannot place
		if (!cursorInBounds)
		{
			if(_placementPreview.GetParent() != null)
			{
				_towerParent.RemoveChild(_placementPreview);
			}
			return;
		}
		
		// Place preview
		if (_placementPreview.GetParent() == null)
		{
			_towerParent.AddChild(_placementPreview);
		}
		_placementPreview.GridPosition = cursorGridPosition;
	}
	
	private void PlaceTower(GridObjectLayer goLayer, MobLayer mobLayer, 
		ObjectRegistrationLayer registerLayer, Vector2I cursorGridPosition, bool cursorInBounds)
	{
		// Consume the input
		if (!_tryPlace)
		{
			return;
		}
		_tryPlace = false;
		
		// Nothing to place, do nothing
		if (_placementPreview == null)
		{
			return;
		}
		
		// Not placing a preview
		if (!cursorInBounds)
		{
			return;
		}
		
		// Placement checks
		if (!CanPlace())
		{
			ClearPreview();
			return;
		}
		
		// Register to all relavent layers and treat as full object
		registerLayer.RegisterObject(_placementPreview);
		_placementPreview = null;
		_currentSelection = null;
		
		bool CanPlace()
		{
			if (!goLayer.CanPlaceGridObject(_placementPreview))
			{
				return false;
			}
			
			// If monsters can't walk on the object, don't place
			// ontop of monsters
			if (!_placementPreview.IsPermeable)
			{
				foreach (Vector2I tile in _placementPreview.Tiles)
				{
					if (mobLayer.IsTileOccupied(tile))
					{
						return false;
					}
				}
			}
			
			return true;
		}
	}
	
	void ITowerDataSelectionHandler.OnTowerDataSelected(TowerData data)
	{
		ClearPreview();
		_currentSelection = data;
	}
	
	private void CreatePreview(Vector2 originWorldPosition, float tileSize)
	{
		_placementPreview = _currentSelection.TowerScene.Instantiate<GridObject>();
		_placementPreview.Initialize(originWorldPosition, tileSize);
		_placementPreview.Rotation = _previewRotation;
		
		RegisterInput();
	}
	
	private void ClearPreview()
	{
		_currentSelection = null;
		if (_placementPreview != null)
		{
			_placementPreview.QueueFree();
			_placementPreview = null;
		}
		
		UnregisterInput();
	}
	
	private void RegisterInput()
	{
		InputManager.RegisterInputEvent("tower_placement_rotate", 
				_rotateInputCallback,
				InputEventType.Press);
		InputManager.RegisterInputEvent("level_select_action", 
				_placeInputCallback,
				InputEventType.Press);
	}
	
	private void UnregisterInput()
	{
		InputManager.UnregisterInputEvent("tower_placement_rotate", 
				_rotateInputCallback,
				InputEventType.Press);
		InputManager.UnregisterInputEvent("level_select_action", 
				_placeInputCallback,
				InputEventType.Press);
	}
	
	private void RotateTower(InputEventData inputData)
	{
		_previewRotation++;
		_previewRotation = (GridRotation)((int)_previewRotation % (int)GridRotation.FullCircle);
		
		_placementPreview.Rotation = _previewRotation;
	}
	
	private void ReqestPlacement(InputEventData inputData)
	{
		_tryPlace = true;
	}
}
