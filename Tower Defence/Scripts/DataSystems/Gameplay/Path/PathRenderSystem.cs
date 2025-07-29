using Godot;
using System;
using System.Collections.Generic;
using HexGrid;
using DataSystems;

public class PathRenderSystem : DataLayerSystem, IDisposable
{
	private enum Mode
	{
		None,
		ShortestPath,
	}
	
	private static readonly Dictionary<Mode, Type[]> ModeLayers = new Dictionary<Mode, Type[]>
	{
		{ Mode.None, null},
		{ Mode.ShortestPath, new Type[]{typeof(ShortestPathLayer)}},
	};
	
	private Mode _currentMode = Mode.ShortestPath;
	private ArrowTile[,] _arrows;
	
	public override Type[] ReadLayerTypes => ModeLayers[_currentMode];
	
	public PathRenderSystem(ArrowTile[,] arrows)
	{
		_arrows = arrows;
	}
	
	public override void Run(double deltaTime, double totalTime,
			Dictionary<Type, DataLayer> readLayers, 
			Dictionary<Type, DataLayer> writeLayers)
	{	
		// No layers to render
		Type[] layerType = ModeLayers[_currentMode];
		if (layerType == null)
		{
			return;
		}
		
		PathLayer pathLayer = (PathLayer)GetLayer(layerType[0], readLayers);
		UpdateArrows(pathLayer);
	}
	
	private void UpdateArrows(PathLayer pathLayer)
	{
		if (!pathLayer.Dirty)
		{
			return;
		}
		
		for (int y = 0; y < pathLayer.GridSize.Y; y++)
		{
			for (int x = 0; x < pathLayer.GridSize.X; x++ )
			{
				GridDirection? direction = pathLayer[y, x];
				ArrowTile hex = _arrows[y, x];
				
				hex.ShowArrow(direction.HasValue);
				if (direction.HasValue)
				{
					hex.UpdateDirection(direction.Value);
				}
			}
		}
	}
	
	void IDisposable.Dispose()
	{
		_arrows = null;
	}
}
