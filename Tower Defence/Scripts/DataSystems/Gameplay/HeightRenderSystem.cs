using Godot;
using System;
using System.Collections.Generic;
using HexGrid;
using DataSystems;

// TODO: Replace with TileMapLayer instead of manually spawned tiles
// Possible rendering gains from better batching, but definitely
// would make the art side better
public class HeightRenderSystem : DataLayerSystem, IDisposable
{
	private HexTile[,] _hexes;
	
	public override Type[] ReadLayerTypes => 
		[
			typeof(HeightLayer)
		];
	
	public HeightRenderSystem(HexTile[,] hexes)
	{
		_hexes = hexes;
	}
	
	public override void Run(double deltaTime, double totalTime,
			Dictionary<Type, DataLayer> readLayers, 
			Dictionary<Type, DataLayer> writeLayers)
	{
		HeightLayer heightLayer = GetLayer<HeightLayer>(readLayers);
		if (!heightLayer.Dirty)
		{
			return;
		}
		
		for (int y = 0; y < heightLayer.GridSize.Y; y++)
		{
			for (int x = 0; x < heightLayer.GridSize.X; x++ )
			{
				if (!(_hexes[y, x] is IHeightTile heightTile))
				{
					continue;
				}
				
				heightTile.SetBounds(heightLayer.MinHeight, heightLayer.MaxHeight);
				heightTile.Height = heightLayer[y, x];
			}
		}
	}
	
	void IDisposable.Dispose()
	{
		_hexes = null;
	}
}
