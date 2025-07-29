using Godot;
using System;
using HexGrid;
using DataSystems;
using Movement;

public partial class LevelManager : Node
{
	public struct LevelData
	{
		public Vector2I GridSize;
		public float TileSize;
		public Vector2 OriginWorldPosition;
		
		public int MinHeight;
		public int MaxHeight;
	
		public Vector2I PathDestination;
	}
	
	[Export] private PackedScene _testGridObject;
	[Export] private PackedScene _testMob;
	
	[Export] private TerrainGrid _gridRender;
	[Export] private LevelTowerPanel _towerPanel;
	[Export] private DataLayerSystemNode[] _nodeSystems;
	
	private LevelData _levelData;
	private DataManager _dataManager;
	
	private Vector2 OriginWorldPosition => _levelData.OriginWorldPosition;
	
	private float TileSize => _levelData.TileSize;
	private Vector2I GridSize => _levelData.GridSize;
	
	// TODO: Testing temp
	public override void _Ready()
	{
		Init(new LevelData{
			GridSize = new Vector2I(44, 32),
			TileSize = 20,
			OriginWorldPosition = Vector2.One * 20 * 1.5f,
			MinHeight = 0,
			MaxHeight = 10,
			PathDestination = new Vector2I(22, 16)
		});
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (Engine.IsEditorHint())
		{
			return;
		}
		_dataManager.RunSystems(delta);
	}
	
	public void Init(LevelData data)
	{
		_levelData = data;
		_gridRender.Init(GridSize, TileSize, OriginWorldPosition);
		
		_dataManager = new DataManager(
			new GridData
			{
				GridSize = _levelData.GridSize,
				TileSize = _levelData.TileSize,
				
				OriginWorldPosition = _levelData.OriginWorldPosition
			}
		);
		
		// TODO: Need a solution for registration order affecting process 
		// order. Systems that act based on layer dirty have to go after
		// systems that do the dirtying. This makes generic registration
		// potentially lead to issues
		_dataManager.AddSystem(new TriggerLinkSystem());
		_dataManager.AddSystem(new TowerPlacementSystem(this));
		_dataManager.AddSystem(new MobMovementSystem());
		_dataManager.AddSystem(new GenericMovementSystem());
		_dataManager.AddSystem(new ProjectilePartitionSystem());
		_dataManager.AddSystem(new MobOccupationSystem());
		_dataManager.AddSystem(new ApplyProjectilePushSystem());
		_dataManager.AddSystem(new ProjectileWallResponseSystem());
		_dataManager.AddSystem(new ShortestPathSystem(_levelData.PathDestination));
		_dataManager.AddSystem(new TriggerActivationSystem());
		foreach (IDataLayerSystem system in _nodeSystems)
		{
			_dataManager.AddSystem(system);
		}
		_gridRender.RegisterRenderSystems(_dataManager);
		
		HeightLayer heightLayer = _dataManager.GetLayer<HeightLayer>();
		heightLayer.MinHeight = _levelData.MinHeight;
		heightLayer.MaxHeight = _levelData.MaxHeight;
		
		InitTowerPanel();
		InitTestData();
	}
	
	private void InitTowerPanel()
	{
		_towerPanel.Init(_dataManager.GetSystem<TowerPlacementSystem>());
		_towerPanel.LinkModePressed += ToggleLinkMode;
	}
	
	private void InitTestData()
	{
		ObjectRegistrationLayer registrationLayer = _dataManager.GetLayer<ObjectRegistrationLayer>();
		
		var gridObject = _testGridObject.Instantiate<GenericGridObject>();
		AddChild(gridObject);
		gridObject.Initialize(OriginWorldPosition, TileSize);
		gridObject.GridPosition = new Vector2I(0, 0);
		registrationLayer.RegisterObject(gridObject);
		
		RandomNumberGenerator rng = new RandomNumberGenerator();
		for (int i = 0; i < 3000; i++)
		{
			var mob = _testMob.Instantiate<TestMob>();
			
			Vector2I randomIndex = new Vector2I
			(
				rng.RandiRange(0, GridSize.X - 1),
				rng.RandiRange(0, GridSize.Y - 1)
			);
			mob.Position = GridUtility.GetWorldPositionFromGridIndex
				(randomIndex, OriginWorldPosition, TileSize);
			mob.Position += Vector2.Up.Rotated(rng.RandfRange(0, Mathf.Pi))
				* rng.RandfRange(0, TileSize / 2);
			
			AddChild(mob);
			registrationLayer.RegisterObject(mob);
		}	
	}
	
	public void ToggleLinkMode()
	{
		_dataManager.ToggleSystem<TriggerLinkSystem>();
		_dataManager.ToggleSystem<TriggerLinkRenderSystem>();
	}
}
