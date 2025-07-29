using Godot;
using System;
using InputEvents;
using DataSystems;

namespace HexGrid
{
	public abstract partial class HexGrid : Node, ICursorPositionListener
	{
		protected static readonly Vector2I NoSelection = Vector2I.One * -1;
		
		[Export] private PackedScene _tile;
		[Export] private PackedScene _arrowTile;
		[Export] private PackedScene _tileCursorScene;
		
		protected int _width;
		protected int _height;
		protected float _tileSize;
		
		protected HexTile[,] _tiles;
		protected ArrowTile[,] _arrowTiles;
		
		protected Vector2 _originWorldPosition;
		
		protected Vector2I _selectedTile = new Vector2I(-1, -1);
		private TileCursor _tileCursor;
		
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			_tileCursor = _tileCursorScene.Instantiate<TileCursor>();
			InputManager.RegisterCursorPositionListener(this);
		}
		
		public override void _ExitTree()
		{
			_tiles = null;
			_arrowTiles = null;
			
			if (_tileCursor.GetParent() == null)
			{
				_tileCursor.QueueFree();
			}
			InputManager.UnregisterCursorPositionListener(this);
		}
		
		public void Init(Vector2I gridSize, float tileSize, Vector2 originWorldPosition)
		{
			_width = gridSize.X;
			_height = gridSize.Y;
			_tileSize = tileSize;
			_originWorldPosition = originWorldPosition;
			
			CreateGrid();
		}
		
		public void RegisterRenderSystems(DataManager manager)
		{
			manager.AddSystem(new HeightRenderSystem(_tiles));
			manager.AddSystem(new PathRenderSystem(_arrowTiles));
		}
		
		protected virtual void CreateGrid()
		{
			Vector2 xOffset = Vector2.Right;
			Vector2 yOffset = GridUtility.YIndexToWorld;
			
			_tiles = new HexTile[_height, _width];
			_arrowTiles = new ArrowTile[_height, _width];
			for (int y = 0; y < _height; y++)
			{
				for (int x = 0; x < _width; x++)
				{
					HexTile newTile = _tile.Instantiate<HexTile>();
					newTile.Position = _originWorldPosition
									 + (xOffset * x * _tileSize)
									 + (yOffset * y * _tileSize);
					AddChild(newTile);
					_tiles[y, x] = newTile;
					
					ArrowTile newArrow = _arrowTile.Instantiate<ArrowTile>();
					newArrow.Position = newTile.Position;
					AddChild(newArrow);
					_arrowTiles[y, x] = newArrow;
				}
			}
		}
		
		protected bool IsIndexInBounds(int x, int y)
		{
			return 0 <= x && x < _width 
				&& 0 <= y && y < _height;
		}
		
		void ICursorPositionListener.OnCursorMoved(Vector2 screenPosition, Vector2 worldPosition)
		{
			Vector2I index = GridUtility.GetGridIndexFromWorldPosition(
					worldPosition,
					_originWorldPosition,
					_tileSize
				);
			
			if(IsIndexInBounds(index.X, index.Y))
			{
				if (index != _selectedTile)
				{
					if (_tileCursor.GetParent() == null)
					{
						AddChild(_tileCursor);
					}
					_tileCursor.Position = _tiles[index.Y, index.X].Position;
					
					_selectedTile = index;
				}
			}
			else if (_selectedTile != NoSelection)
			{
				_selectedTile = NoSelection;
				RemoveChild(_tileCursor);
			}
		}
	}
}
