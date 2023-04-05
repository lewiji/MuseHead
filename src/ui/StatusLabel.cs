using Godot;
using System;
using MuseHead;

public partial class StatusLabel : Label
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<MuseConnector>("/root/MuseConnector").Connected += OnConnected;
		GetNode<MuseConnector>("/root/MuseConnector").Disconnected += OnDisconnected;
	}

	void OnDisconnected()
	{
		Text = "DISCONNECTED";
	}

	void OnConnected()
	{
		Text = "CONNECTED";
	}
}
