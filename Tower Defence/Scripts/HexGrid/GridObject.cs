using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

namespace HexGrid
{
	[GlobalClass]
	public abstract partial class GridObject : Node
	{
		private struct PointEnumerator : IEnumerator<Vector2I>, IEnumerable<Vector2I>
		{
			private GridObject _o;
			private int _index;
			
			public PointEnumerator(GridObject o)
			{
				_o = o;
				_index = -1;
			}
			
			Vector2I IEnumerator<Vector2I>.Current => 
				_o._gridPosition 
				+ GridUtility.RotatePointAroundOrigin(_o._shape.Points[_index], _o._rotation);
		
			object IEnumerator.Current => (this as IEnumerator<Vector2I>).Current;
		
			bool IEnumerator.MoveNext()
			{
				_index++;
				return _index < _o._shape.Points.Count;
			}
			
			void IEnumerator.Reset()
			{
				_index = -1;
			}
			
			void IDisposable.Dispose() {}
			
			public IEnumerator<Vector2I> GetEnumerator() => this;
			
			IEnumerator IEnumerable.GetEnumerator() => this;
		}
		
		protected Vector2 _originWorldPosition;
		protected float _tileSize;
		
		protected GridShape _shape;
		private Vector2I _gridPosition;
		private GridRotation _rotation;
		
		public Vector2I GridPosition
		{
			get => _gridPosition;
			set
			{
				Vector2I oldValue = _gridPosition;
				_gridPosition = value;
				
				ComputeCenter();
				OnGridPositionChanged(oldValue);
			}
		}
		
		public GridRotation Rotation
		{
			get => _rotation;
			set
			{
				GridRotation oldValue = _rotation;
				_rotation = value;
				
				ComputeCenter();
				OnRotationChanged(oldValue);
			}
		}
		
		private Vector2 _center;
		public Vector2 Center => _center;
		
		public virtual bool IsPermeable { get; protected set; }
		
		public IEnumerable<Vector2I> Tiles => new PointEnumerator(this);
			
		public void Initialize(Vector2 originWorldPosition, float tileSize)
		{
			_originWorldPosition = originWorldPosition;
			_tileSize = tileSize;
			ComputeCenter();
			
			OnInitialize();
		}
		
		private void ComputeCenter()
		{
			int count = 0;
			Vector2 center = Vector2.Zero;
			foreach(Vector2I tile in Tiles)
			{
				center += GridUtility.GetWorldPositionFromGridIndex
					(
						tile,
						_originWorldPosition,
						_tileSize
					);
				count++;
			}
			center /= count;
			_center = center;
		}
		
		protected virtual void OnInitialize() {}
		
		protected virtual void OnGridPositionChanged(Vector2I oldPosition) {}
		
		protected virtual void OnRotationChanged(GridRotation oldRotation) {}
	}
}
