using Godot;
using Godot.Collections;

namespace MuseHead._3d;

public partial class Sky : WorldEnvironment
{
	PhysicalSkyMaterial _skyMaterial = default!;

	FastNoiseLite _skyNoise = default;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_skyMaterial = (PhysicalSkyMaterial)Environment.Sky.SkyMaterial;
		GetNode<MuseConnector>("/root/MuseConnector").EegReceived += OnEegReceived;
	}

	void OnEegReceived(Dictionary<int, double[]> data)
	{
		_skyMaterial.MieCoefficient = (float)data[-1][4];
	}
}