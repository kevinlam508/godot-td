using Godot;
using System;
using System.Collections.Generic;
using DataSystems;

namespace Movement;

public enum ModType
{
	Additive,
	Multiplicitive,
	Static
}

public class MovementData
{
	public MovementStats Stats;
	public MovementState State;
	public MovementChange PrimaryChange;
	public int NextOverrideId = 0;
	public Dictionary<int, MovementOverride> Overrides 
		= new  Dictionary<int, MovementOverride>();
}

public struct MovementStats
{
	public float MaxSpeed;
	public float Acceleration;
	public int MaxStepHeight;
}

public struct MovementState
{
	private Vector2 _position;
	public Vector2 Position
	{
		get => _position;
		set
		{
			PreviousPosition = _position;
			_position = value;
		}
	}
	public Vector2 PreviousPosition;
	public Vector2 Forward; // Separate forward since velocity could be 0
	public Vector2 Velocity;
	public bool HasHitWall;
}

public struct MovementChange
{
	public Vector2 Direction;
}

public struct MovementOverride
{
	public ModType Type;
	public Vector2 Velocity;
}

public class GenericMovementLayer : DataLayer, IRegistrar, IDisposable
{
	public const int InvalidOverrideId = -1;
	
	public Dictionary<IGenericMover, MovementData> _data 
		= new Dictionary<IGenericMover, MovementData>();
	
	public IEnumerable<KeyValuePair<IGenericMover, MovementData>> Data => _data;

	public MovementState GetState(IGenericMover m)
	{
		return _data.TryGetValue(m, out var data) ? data.State : default;
	}
	
	public void SetDirection(IGenericMover m, Vector2 direction)
	{
		if (_data.TryGetValue(m, out var data))
		{
			data.PrimaryChange.Direction = direction;
		}
		else
		{
			GD.PrintErr("Modifying unknown mover");
		}
	}
	
	// Returns -1 if override not applied
	public int AddOverride(IGenericMover m, MovementOverride o)
	{
		if (_data.TryGetValue(m, out var data))
		{
			int id = data.NextOverrideId;
			data.NextOverrideId++;
			data.Overrides[id] = o;
			return id;
		}
		else
		{
			GD.PrintErr("Adding override to unknown mover");
			return -1;
		}
	}
	
	public MovementOverride GetOverride(IGenericMover m, int overrideId)
	{
		if (_data.TryGetValue(m, out var data)
			&& data.Overrides.TryGetValue(overrideId, out var overrideData))
		{
			return overrideData;
		}
		else
		{
			GD.PrintErr("Getting unknown override");
			return default;
		}
	}
	
	public void RemoveOverride(IGenericMover m, int overrideId)
	{
		if (_data.TryGetValue(m, out var data))
		{
			data.Overrides.Remove(overrideId);
		}
		else
		{
			GD.PrintErr("Remove override from unknown mover");
		}
	}
	
	void IRegistrar.Register(object o)
	{
		if (!(o is IGenericMover m))
		{
			return;
		}
		
		_data.Add(m, 
			new MovementData
			{
				Stats = new MovementStats
				{
					MaxSpeed = m.MaxSpeed,
					Acceleration = m.Acceleration,
					MaxStepHeight = m.MaxStepHeight
				},
				State = new MovementState
				{
					Position = m.Position,
					PreviousPosition = m.Position,
					Forward = m.Forward
				},
				PrimaryChange = new MovementChange
				{
					Direction = m.Forward
				}
			}
		);
		
	}
	
	void IRegistrar.Unregister(object o)
	{
		if (!(o is IGenericMover m))
		{
			return;
		}
		_data.Remove(m);
	}
	
	void IDisposable.Dispose()
	{
		_data.Clear();
	}
}
