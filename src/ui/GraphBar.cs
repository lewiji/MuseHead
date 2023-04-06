using System.Globalization;
using Godot;

namespace MuseHead.ui;

public partial class GraphBar : MarginContainer
{
	int _index;
	ProgressBar _progressBar = default!;
	Label _label = default!;
	public override void _Ready()
	{
		_index = GetIndex();
		_progressBar = GetNode<ProgressBar>("%ProgressBar");
		_label = GetNode<Label>("%Label");
		_label.Text = Name;
		GetNode<MuseConnector>("/root/MuseConnector").EegReceived += OnEegReceived;
	}

	void OnEegReceived(double[] data)
	{
		/*if (min < _progressBar.MinValue) _progressBar.MinValue = min;
		if (max > _progressBar.MaxValue) _progressBar.MaxValue = max;*/
		
		if (data.Length >= _index)
		{
			_progressBar.Value = data[_index];
			_label.Text = data[_index].ToString(CultureInfo.InvariantCulture);
		}
	}

	public override void _Process(double delta) { base._Process(delta); }
}