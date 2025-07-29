using Godot;
using System;
using System.Collections.Generic;
using HexGrid;
using DataSystems;

public class MobLayer : SparseGridLayer<int>, IRegistrar
{
	private HashSet<BaseMob> _mobs = new HashSet<BaseMob>();
	
	public IEnumerable<BaseMob> Mobs => _mobs;
	public IEnumerable<Vector2I> OccupiedTiles => _grid.Keys;
	
	public bool IsTileOccupied(Vector2I index)
	{
		return GetData(index) > 0;
	}
	
	public void MoveTileOccupation(Vector2 startWorld, Vector2 endWorld)
	{
		Vector2I start = GetGridIndexFromWorldPosition(startWorld);
		Vector2I end = GetGridIndexFromWorldPosition(endWorld);
		
		// Didn't move, do nothing
		if (start == end)
		{
			return;
		}
		
		DecrementTileOccupation(start);
		IncrementTileOccupation(end);
	}
	
	private void DecrementTileOccupation(Vector2I index)
	{
		SetData(index, GetData(index) - 1);
	}
	
	private void IncrementTileOccupation(Vector2I index)
	{
		SetData(index, GetData(index) + 1);
	}
	
	private Vector2I GetGridPositionOfMob(BaseMob mob)
	{
		return GetGridIndexFromWorldPosition(mob.Position);
	}
	
	void IRegistrar.Register(object o)
	{
		if (o is BaseMob mob)
		{
			_mobs.Add(mob);
			
			Vector2I gridPosition = GetGridPositionOfMob(mob);
			IncrementTileOccupation(gridPosition);
		}
		Dirty = true;
	}
	
	void IRegistrar.Unregister(object o)
	{
		if (o is BaseMob mob)
		{
			_mobs.Remove(mob);
			
			Vector2I gridPosition = GetGridPositionOfMob(mob);
			DecrementTileOccupation(gridPosition);
		}
		Dirty = true;
	}
}
