using Godot;

namespace MuseHead.ui;

public partial class ConnectButton : Button
{
	
	public override void _Ready()
	{
		Pressed += OnPressed;
	}

	void OnPressed()
	{
		var device = GetNode<ConnectOption>("%DeviceName").GetValue().AsStringName();
		var port = GetNode<ConnectOption>("%Port").GetValue().AsInt32();
		GetNode<MuseConnector>("/root/MuseConnector").Connect(device, port);
		Disabled = true;
	}
}