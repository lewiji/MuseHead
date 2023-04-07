using Godot;
using Godot.Collections;

namespace MuseHead._3d;

public partial class FreqBandBody3D : RigidBody3D
{
	int _index;
	double _value;
	Label3D _label = default!;
	[Export] public float VelocityScale { get; set; } = 20.0f;
	
	public override void _Ready()
	{
		_index = GetIndex();
		_label = GetNode<Label3D>("Label3D");
		var mat = (StandardMaterial3D)GetNode<MeshInstance3D>("MeshInstance3D").Mesh.SurfaceGetMaterial(0);
		mat.AlbedoColor = Color.FromHsv(_index / 5.0f, 0.618f, 0.828f);
		GetNode<MuseConnector>("/root/MuseConnector").EegReceived += OnEegReceived;
	}

	void OnEegReceived(Dictionary<int, double[]> data)
	{
		_value = data[-1][_index] * VelocityScale;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		//var col = MoveAndCollide((float)delta * new Vector3(0, (float)_value, 0));
		ApplyCentralForce(new Vector3(0, (float)_value, 0));

		_value = 0;
	}
}