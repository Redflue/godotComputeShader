using Godot;
using System;

namespace SettingsMenu 
{
	public partial class SettingsMenu : MarginContainer
	{
		
		CheckBox settingsEnable;
		VBoxContainer settings;
		SpinBox speed;
		SpinBox scanDist;

		[Export]
		node_2d Controller;

		public override void _Ready()
		{
			settingsEnable = GetNode<CheckBox>("%SettingsEnable");
			settings = GetNode<VBoxContainer>("%Settings");

			speed = settings.GetNode<SpinBox>("Speed");
			scanDist = settings.GetNode<SpinBox>("ScanDist");
		}

		public void OnSettingsEnabled(bool enabled) {
			settings.Visible = enabled;
		}

		public void OnApplyPressed() {
			int sc = Mathf.RoundToInt(scanDist.Value);
			float sp = (float)speed.Value;
			Controller.applySettings(sc,sp);
		}
	}
}