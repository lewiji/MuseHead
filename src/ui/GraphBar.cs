using System.Globalization;
using Godot;

namespace MuseHead.ui;

public partial class GraphBar : MarginContainer
{
	int _index;
	double _value;
	ProgressBar _progressBar = default!;
	static readonly NodePath ProgressBarValueProperty = new (Range.PropertyName.Value);
	Label _label = default!;
	Tween? _tween;
	StyleBoxTexture _fillTexture = default!;
	public override void _Ready()
	{
		_index = GetIndex();
		_progressBar = GetNode<ProgressBar>("%ProgressBar");
		var fill = _progressBar.GetThemeStylebox("fill");
		_fillTexture = (StyleBoxTexture)fill;
		_label = GetNode<Label>("%Label");
		_label.Text = Name;
		GetNode<MuseConnector>("/root/MuseConnector").EegReceived += OnEegReceived;
	}

	void OnEegReceived(double[] data)
	{
		if (data.Length >= _index)
		{
			_tween?.Kill();
			_tween = CreateTween();
			_tween
				.TweenProperty(_progressBar, ProgressBarValueProperty, data[_index], 0.512f)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.Out);
			//_value = data[_index];
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		_fillTexture.RegionRect = new Rect2(
			new Vector2((float)_progressBar.Value / (float)_progressBar.MaxValue * 100f, 0),
			_fillTexture.RegionRect.Size);
	}
}