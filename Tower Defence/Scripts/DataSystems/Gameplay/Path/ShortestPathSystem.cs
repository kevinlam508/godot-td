using Godot;
using System;
using System.Collections.Generic;
using HexGrid;
using DataSystems;

public class ShortestPathSystem : DataLayerSystem
{
	private Vector2I _destination;
	
	public override Type[] ReadLayerTypes => 
		[
			typeof(HeightLayer), 
			typeof(GridObjectLayer)
		];
	public override Type[] WriteLayerTypes => 
		[
			typeof(ShortestPathLayer)
		];
	
	public ShortestPathSystem(Vector2I destination)
	{
		_destination = destination;
	}
	
	public override void Run (double deltaTime, double totalTime,
			Dictionary<Type, DataLayer> readLayers, 
			Dictionary<Type, DataLayer> writeLayers)
	{
		HeightLayer heightLayer = GetLayer<HeightLayer>(readLayers);
		GridObjectLayer gridObjectLayer = GetLayer<GridObjectLayer>(readLayers);
		if (!heightLayer.Dirty && !gridObjectLayer.Dirty)
		{
			return;
		}
		
		ShortestPathLayer pathLayer = GetLayer<ShortestPathLayer>(writeLayers);
		
		Vector2I gridSize = heightLayer.GridSize;
		int xDest = _destination.X;
		int yDest = _destination.Y;
		
		// Init data
		HashSet<(int x, int y)> visitable = new HashSet<(int, int)>();
		(int cost, int lastCost, int prevX, int prevY)[,] pathInfo =
			new (int, int, int, int)[gridSize.Y, gridSize.X];
		for (int y = 0; y < gridSize.Y; y++)
		{
			for (int x = 0; x < gridSize.X; x++)
			{
				pathInfo[y, x] = (int.MaxValue, int.MaxValue, -1, -1);
			}
		}
		pathInfo[yDest, xDest] = (0, int.MaxValue, -1, -1);
		visitable.Add((xDest, yDest));
		
		// Find paths to destination
		HashSet<(int x, int y)> toVisit = new HashSet<(int, int)>();
		for (int i = 0; i < gridSize.X * gridSize.Y; i++)
		{
			bool hasChanged = false;
			
			// Only check paths that involving tiles with a valid path already
			toVisit.Clear();
			toVisit.UnionWith(visitable);
			foreach ((int x, int y) in toVisit)
			{
				(int currentCost, int lastCost, int prevX, int prevY)
					= pathInfo[y, x];
				
				// Cost to get to this node hasn't changed
				// so cost of paths that include this don't need checking
				if (currentCost == lastCost)
				{
					continue;
				}
				pathInfo[y, x] = (currentCost, currentCost, prevX, prevY);
				
				int currentHeight = heightLayer[y, x];
				foreach ((int neighborX, int neighborY) in GridUtility.GetAdjacencies(x, y))
				{
					if (!heightLayer.IsIndexInBounds(neighborX, neighborY))
					{
						continue;
					}
					
					// Cannot visit solid tiles
					if (!gridObjectLayer.IsPermeable(neighborX, neighborY))
					{
						continue;
					}
					
					int neighborHeight = heightLayer[neighborY, neighborX];
					int neighborCost = pathInfo[neighborY, neighborX].cost;
					
					int heightDiff = neighborHeight - currentHeight;
					int heightCost = heightDiff >= 0 ? heightDiff : (-2 * heightDiff);
					int travelCost = 1 + heightCost;
					
					if (currentCost + travelCost < neighborCost)
					{
						pathInfo[neighborY, neighborX] = (currentCost + travelCost, 
							pathInfo[neighborY, neighborX].lastCost, x, y);
						
						visitable.Add((neighborX, neighborY));
						
						hasChanged = true;
					}
				}
			}
			
			// Went a whole pass without changes -> next pass will also not make changes
			if (!hasChanged)
			{
				break;
			}
		}
		
		// Set arrows
		for (int y = 0; y < gridSize.Y; y++)
		{
			for (int x = 0; x < gridSize.X; x++)
			{
				(int _, int __, int prevX, int prevY) = pathInfo[y, x];
				GridDirection? direction = GridUtility.IndexOffsetToDirection(
					prevX - x, prevY - y);
				
				pathLayer[y, x] = direction;
			}
		}
	}
}
