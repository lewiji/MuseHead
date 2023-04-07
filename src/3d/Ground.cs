using Godot;
using Godot.Collections;

namespace MuseHead._3d;

public partial class Ground : MeshInstance3D
{
	ShaderMaterial _material = default!;
	public override void _Ready()
	{
		_material = (ShaderMaterial)Mesh.SurfaceGetMaterial(0);
		GetNode<MuseConnector>("/root/MuseConnector").EegReceived += OnEegReceived;
	}

	void OnEegReceived(Dictionary<int, double[]> data)
	{
		_material.SetShaderParameter("height_scale", Mathf.Abs(data[-1][3]) * 10.0f);
		_material.SetShaderParameter("noise_period", new Vector2((float)data[-1][1] * 5.0f, (float)data[-1][2] * 5.0f).Abs());
	}
}