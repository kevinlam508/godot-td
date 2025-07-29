//#define PERFORMANCE_LOG

using Godot;
using System;
using System.Collections.Generic;
using HexGrid;
using DataSystems;

namespace Movement;

public class GenericMovementSystem : DataLayerSystem
{
	public static readonly float InstantAcceleration = float.MaxValue;
	private static readonly float WallCollisionBuffer = .1f;
	
	public override Type[] ReadLayerTypes => 
		[
			typeof(GridObjectLayer),
			typeof(HeightLayer)
		];
		
	public override Type[] WriteLayerTypes => 
		[
			typeof(GenericMovementLayer)
		];
	
	public override void Run(double deltaTime, double totalTime,
			Dictionary<Type, DataLayer> readLayers, 
			Dictionary<Type, DataLayer> writeLayers)
	{
		GridObjectLayer objectLayer = GetLayer<GridObjectLayer>(readLayers);
		HeightLayer heightLayer = GetLayer<HeightLayer>(readLayers);
		
		GenericMovementLayer moveLayer = GetLayer<GenericMovementLayer>(writeLayers);
	
		ComputeMotion(moveLayer, deltaTime, objectLayer, heightLayer);
		ApplyMotion(moveLayer);
	}
	
	private void ComputeMotion(GenericMovementLayer moveLayer,
		double deltaTime, GridObjectLayer objectLayer, 
		HeightLayer heightLayer)
	{
		#if PERFORMANCE_LOG
		using var __ = new Debug.TimeLogger.TimeScope("Compute Motion");
		#endif
		
		foreach((_, MovementData data) in moveLayer.Data)
		{
			ComputeMotion(data, deltaTime, objectLayer, heightLayer);
		}
	}
	
	private void ComputeMotion(MovementData data, double deltaTime,
		GridObjectLayer objectLayer, HeightLayer heightLayer)
	{
		data.State.HasHitWall = false;
		Vector2 destination = data.Overrides.Count > 0
			? ComputeOverrideChange(data, deltaTime)
			: ComputeNormalChange(data, deltaTime);
			
		if (AdjustForWallCollision(data.State.Position, destination, 
			objectLayer, heightLayer, data.Stats,
			out var adjusted))
		{
			destination = adjusted;
			data.State.HasHitWall = true;
			data.State.Velocity = Vector2.Zero;
		}
		
		data.State.Position = destination;
	}
	
	private Vector2 ComputeOverrideChange(MovementData data, double deltaTime)
	{
		Vector2 compoundVelocity = Vector2.Zero;
		foreach ((_, MovementOverride o) in data.Overrides)
		{
			switch (o.Type)
			{
				case ModType.Additive:
					compoundVelocity += o.Velocity;
					break;
				default:
					GD.PrintErr($"Unsupported override type {o.Type}");
					break;
			}
		}
		
		return data.State.Position 
			+ (compoundVelocity * (float)deltaTime);
	}
	
	private Vector2 ComputeNormalChange(MovementData data, double deltaTime)
	{
		Vector2 deltaPosition = Vector2.Zero;
		// Jump straight to max speed
		if (data.Stats.Acceleration == InstantAcceleration)
		{
			deltaPosition = data.Stats.MaxSpeed 
				* data.PrimaryChange.Direction;
			data.State.Velocity = deltaPosition;
		}
		// Verlet integration of position with velocity storage
		else
		{
			Vector2 acceleration = data.Stats.Acceleration 
				* data.PrimaryChange.Direction;
			data.State.Velocity += acceleration * (float)deltaTime;
			
			// Apply velocity, clamping by max speed
			deltaPosition = data.State.Velocity 
				+ .5f * acceleration * (float)deltaTime;
			if (deltaPosition.LengthSquared() > data.Stats.MaxSpeed * data.Stats.MaxSpeed)
			{
				deltaPosition = deltaPosition.Normalized() * data.Stats.MaxSpeed;
				data.State.Velocity = deltaPosition;
			}
		}
		
		data.State.Forward = data.State.Velocity.Normalized();
		return data.State.Position + (deltaPosition * (float)deltaTime);
	}
	
	private bool AdjustForWallCollision(Vector2 start, Vector2 end,
		GridObjectLayer objectLayer, HeightLayer heightLayer,
		MovementStats stats,
		out Vector2 adjustedPosition)
	{
		adjustedPosition = end;
		
		Vector2 direction = end - start;
		float tileSize = heightLayer.TileSize;
		
		Vector2I currentIndex = heightLayer.GetGridIndexFromWorldPosition(start);
		Vector2I endIndex = heightLayer.GetGridIndexFromWorldPosition(end);
		
		// Travel tile by tile to check if route is possible
		while (currentIndex != endIndex)
		{
			Vector2 tileCenter = heightLayer.GetWorldPositionFromGridIndex(currentIndex);
			(GridDirection? gridDirection, float scale) = GridUtility
				.GetTileExitPoint(tileCenter, tileSize, start, direction);
			if (!gridDirection.HasValue)
			{
				break;
			}
			
			Vector2I nextIndex = currentIndex + GridUtility.DirectionIndexOffset[(int)gridDirection.Value];
			if (!CanPassThroughTile(currentIndex, nextIndex))
			{
				// Stop movement just before the tile boundry
				// so movement isn't caught permanently
				adjustedPosition = start 
					+ (direction * scale)
					- (direction.Normalized() * WallCollisionBuffer);
				break;
			}
			currentIndex = nextIndex;
		}
		
		return adjustedPosition != end;
		
		bool CanPassThroughTile(Vector2I from, Vector2I to)
		{
			// Don't fall off the map
			if (!heightLayer.IsIndexInBounds(to))
			{
				return false;
			}
			
			// Can't walk through objects
			// But allow exiting objects if already in an object
			if (objectLayer.IsPermeable(from)
				&& !objectLayer.IsPermeable(to))
			{
				return false;
			}
			
			// Can't step up too much at once
			int fromHeight = heightLayer[from];
			int toHeight = heightLayer[to];
			if (toHeight - fromHeight > stats.MaxStepHeight)
			{
				return false;
			}
			
			return true;
		}
	}
	
	private void ApplyMotion(GenericMovementLayer moveLayer)
	{
		#if PERFORMANCE_LOG
		using var _ = new Debug.TimeLogger.TimeScope("Apply Motion");
		#endif
		
		foreach ((IGenericMover m, MovementData data) in moveLayer.Data)
		{
			m.Position = data.State.Position;
			m.Forward = data.State.Forward;
		}
	}
}
