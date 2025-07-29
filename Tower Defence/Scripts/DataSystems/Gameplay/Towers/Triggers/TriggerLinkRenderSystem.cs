using Godot;
using System;
using System.Collections.Generic;
using DataSystems;

public partial class TriggerLinkRenderSystem : DataLayerSystemNode, IDisposable
{
	[Export] private Node2D _arrowsRoot;
	[Export] private PackedScene _arrowTemplate;
	
	private readonly Dictionary<(ITrigger, ITriggerable), TriggerLinkArrow> _linkRenderers
		= new Dictionary<(ITrigger, ITriggerable), TriggerLinkArrow>();
	
	public override Type[] ReadLayerTypes => 
		[
			typeof(TriggerLayer)
		];
	
	public override void _Ready()
	{
		Enabled = false;
	}
	
	public override void Run(double deltaTime, double totalTime,
			Dictionary<Type, DataLayer> readLayers, 
			Dictionary<Type, DataLayer> writeLayers)
	{
		TriggerLayer triggerLayer = GetLayer<TriggerLayer>(readLayers);
		if (!triggerLayer.Dirty)
		{
			return;
		}
		
		UpdateLinkRenders(triggerLayer);
	}
	
	private void UpdateLinkRenders(TriggerLayer triggerLayer)
	{
		// Delete removed arrows
		HashSet<(ITrigger, ITriggerable)> toRemove = new HashSet<(ITrigger, ITriggerable)>();
		foreach (((ITrigger, ITriggerable) pair, _) in _linkRenderers)
		{
			if (!triggerLayer.HasLink(pair.Item1, pair.Item2))
			{
				toRemove.Add(pair);
			}
		}
		foreach ((ITrigger, ITriggerable) pair in toRemove)
		{
			Node arrow = _linkRenderers[pair];
			_linkRenderers.Remove(pair);
			arrow.QueueFree();
		}
		toRemove.Clear();
		
		// Add new arrows
		foreach ((ITrigger trigger, var linkList) in triggerLayer.Links)
		{
			foreach (ITriggerable triggerable in linkList)
			{
				if (!_linkRenderers.ContainsKey((trigger, triggerable)))
				{
					CreateArrow(triggerLayer, trigger, triggerable);
				}
			}
		}
	}
	
	private void CreateArrow(GridDataLayer gridData,
		ITrigger trigger, ITriggerable triggerable)
	{
		// TEMP: point to the first tile in their list
		// Maybe point to center of mass instead?
		IEnumerator<Vector2I> triggerTiles = trigger.Tiles.GetEnumerator();
		triggerTiles.MoveNext();
		Vector2 start = gridData.GetWorldPositionFromGridIndex(
			triggerTiles.Current
		);
		IEnumerator<Vector2I> triggerableTiles = triggerable.Tiles.GetEnumerator();
		triggerableTiles.MoveNext();
		Vector2 end = gridData.GetWorldPositionFromGridIndex(
			triggerableTiles.Current
		);
		
		TriggerLinkArrow newArrow = _arrowTemplate.Instantiate<TriggerLinkArrow>();
		_arrowsRoot.AddChild(newArrow);
		newArrow.SetPosition(start, end);
		
		_linkRenderers.Add((trigger, triggerable), newArrow);
	}
	
	protected override void OnEnable()
	{
		AddChild(_arrowsRoot);
	}
	
	protected override void OnDisable()
	{
		RemoveChild(_arrowsRoot);
	}
	
	void IDisposable.Dispose()
	{
		_arrowsRoot.QueueFree();
	}
}
