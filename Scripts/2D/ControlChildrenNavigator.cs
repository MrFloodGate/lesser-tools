using Godot;
using System;

public partial class ControlChildrenNavigator : Control
{
	int currentSelected = -1;

	[Signal]
	public delegate void NavigationPageChangedEventHandler(int i);

	[Export]
	public int CurrentSelected
	{
		get => currentSelected;
		set
		{
			bool changed = false;
			if (currentSelected < 0 || currentSelected >= GetChildCount())
			{
				if (currentSelected > -1) changed = true;
				currentSelected = -1;
			}
			else
			{
				changed = currentSelected != value;
				currentSelected = value;
			}



			if (changed)
			{
				UpdateChildren();
				EmitSignal(nameof(NavigationPageChanged), currentSelected);
			}
		}
	}

	public void UpdateChildren()
	{
		foreach (CanvasItem child in GetChildren())
		{

			if (child.GetIndex() == currentSelected) child.Visible = true;
			else child.Visible = false;
		}
	}

	public override void _Ready()
	{
		UpdateChildren();
	}
}
