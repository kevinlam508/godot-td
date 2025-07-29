using Godot;
using System;
using System.Collections.Generic;
using HexGrid;
using DataSystems;

namespace Movement;

public class MobMovementSystem : DataLayerSystem
{
	public override Type[] ReadLayerTypes => 
		[
			typeof(ShortestPathLayer),
			typeof(MobLayer)
		];
		
	public override Type[] WriteLayerTypes => 
		[
			typeof(GenericMovementLayer)
		];
	
	public override void Run (double deltaTime, double totalTime,
			Dictionary<Type, DataLayer> readLayers, 
			Dictionary<Type, DataLayer> writeLayers)
	{
		PathLayer path = GetLayer<ShortestPathLayer>(readLayers);
		MobLayer mobLayer = GetLayer<MobLayer>(readLayers);
		
		GenericMovementLayer moveLayer = GetLayer<GenericMovementLayer>(writeLayers);
		
		foreach(BaseMob mob in mobLayer.Mobs)
		{
			// Accelerate towards the desired path
			Vector2 desiredDirection = GetLagrangeDirection(
				mob.Position,
				path
			);
			moveLayer.SetDirection(mob, desiredDirection);
		}
	}
	
	private Vector2 GetLagrangeDirection(Vector2 position, PathLayer path)
	{
		Vector2I startingGridIndex = path.GetGridIndexFromWorldPosition(position);
		
		// Panic, mob fell off grid
		if (!path.IsIndexInBounds(startingGridIndex))
		{
			return Vector2.Zero;
		}
		
		// Dead end, stop moving
		GridDirection? startingDirection = path[startingGridIndex];
		if (!startingDirection.HasValue)
		{
			return Vector2.Zero;
		}
		
		Vector2I secondGridIndex = startingGridIndex
			+ GridUtility.DirectionIndexOffset[(int)startingDirection.Value];
		Vector2 secondWorldPosition = path.GetWorldPositionFromGridIndex(secondGridIndex);
			
		// 2nd tile is dead end or moving straight, just go straight towards 2nd tile
		GridDirection? secondDirection = path[secondGridIndex];
		if (!secondDirection.HasValue || startingDirection == secondDirection)
		{
			return (secondWorldPosition - position).Normalized();
		}
		
		Vector2I thirdGridIndex = secondGridIndex
			+ GridUtility.DirectionIndexOffset[(int)secondDirection.Value];
		Vector2 thirdWorldPosition = path.GetWorldPositionFromGridIndex(thirdGridIndex);
			
		// 3 valid tiles to path between
		// Use lagrange interpolate x and y seprately
		//   t  | (x, y)
		//   0  | position
		//   t1 | secondWorldPosition
		//   t2 | thirdWorldPosition
		//  - All even spacings of t result in same curve
		//  - Uneven spacing creates bulge in path where spacing is greatest
		// Use derivative of interpolation to determine direction
		
		Vector2 derivatives = new Vector2
			(
				DerivativeAtZero(position.X, secondWorldPosition.X, thirdWorldPosition.X),
				DerivativeAtZero(position.Y, secondWorldPosition.Y, thirdWorldPosition.Y)
			);
		return derivatives.Normalized();
		
		float DerivativeAtZero(float a, float b, float c)
		{
			const float t1 = 1f;
			const float t2 = 10f;
			return (a * -(t1 + t2) / -t1 / -t2)
				 + (b * -(t2)	   /  t1 / (t1 - t2))
				 + (c * -(t1)      /  t2 / (t2 - t1));
		}
	}
}
