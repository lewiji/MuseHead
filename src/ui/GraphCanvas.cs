using System;
using System.Linq;
using Godot;
using GodotSharpSome.Drawing2D;

namespace MuseHead.ui;

public partial class GraphCanvas : ColorRect
{
	float[] _sinSamplePointsX = Enumerable.Range(0, 500).Select(i => 1.0f * i).ToArray();
	Vector2 _origin = new Vector2(250, 250);
	
	float _time;
	int _pointIdx;

	public GraphCanvas()
	{
		_pointIdx = _sinSamplePointsX.Length;
	}

	public override void _Process(double delta)
	{
		_time += (float) delta;
		_pointIdx = _pointIdx <= _sinSamplePointsX.Length ? _pointIdx + 1 : 0;
		QueueRedraw();
	}

	public override void _Draw()
	{
		var points = _sinSamplePointsX.Select(x => _origin + new Vector2(x, 100 * Mathf.Sin(_time + 0.05f * x)));
		this.DrawDots(points.ToArray(), Colors.Black);
	}
}