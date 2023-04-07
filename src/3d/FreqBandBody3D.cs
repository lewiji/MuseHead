using Godot;

namespace MuseHead._3d;

public partial class FreqBandBody3D : CharacterBody3D
{
	int _index;
	double _value;
	static readonly Vector3 Gravity = new (0, -0.618f, 0);
	Label3D _label = default!;
	
	public override void _Ready()
	{
		_index = GetIndex();
		_label = GetNode<Label3D>("Label3D");
		GetNode<MuseConnector>("/root/MuseConnector").EegReceived += OnEegReceived;
	}

	void OnEegReceived(double[] data)
	{
		if (data.Length >= _index)
		{
			_value += data[_index] * 1.618;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		var col = MoveAndCollide((float)delta * new Vector3(0, (float)_value, 0));
		
		_value += Gravity.Y;
		if (_value < Gravity.Y * 2)
		{
			_value = Gravity.Y * 2;
		}
		_label.Text = _value.ToString("0.##");
		
		if (col?.GetCollisionCount() > 0)
		{
			_value = 0;
		}
	}
}