using Godot;
using System;

public partial class TowerSelectionButton : Control
{
	[Export] private Button _button;
	
	private TowerData _data;
	private ITowerDataSelectionHandler _handler;
	
	public override void _ExitTree()
	{
		_button.Pressed -= OnButtonPressed;
	}
	
	public void Init(TowerData data, ITowerDataSelectionHandler handler)
	{
		_data = data;
		_handler = handler;
		
		_button.Text = $"{data.Cost}: {data.Name}";
		_button.Pressed += OnButtonPressed;
	}
	
	public void OnButtonPressed()
	{
		_handler?.OnTowerDataSelected(_data);
	}
}
