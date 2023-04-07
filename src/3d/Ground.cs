using Godot;

namespace MuseHead._3d;

public partial class Ground : MeshInstance3D
{
	ShaderMaterial _material = default!;
	public override void _Ready()
	{
		_material = (ShaderMaterial)Mesh.SurfaceGetMaterial(0);
		GetNode<MuseConnector>("/root/MuseConnector").EegReceived += OnEegReceived;
	}

	void OnEegReceived(double[] data)
	{
		_material.SetShaderParameter("height_scale", data[3]);
		_material.SetShaderParameter("noise_period", new Vector2((float)data[1], (float)data[2]));
	}
}