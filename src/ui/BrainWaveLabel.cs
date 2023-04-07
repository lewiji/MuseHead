using Godot;
using System;
using System.Linq;
using Godot.Collections;
using MuseHead;
using MuseHead.eeg;

public partial class BrainWaveLabel : Label
{
	int _currMaxIdx;
	public override void _Ready()
	{
		GetNode<MuseConnector>("/root/MuseConnector").EegReceived += OnEegReceived;
	}

	void OnEegReceived(Dictionary<int, double[]> data)
	{
		var max = 0.0;
		var maxIdx = 0;
		for (var i = 0; i < data[-1].Length; i++)
		{
			if ((data[-1][i] <= max)) continue;
			max = data[-1][i];
			maxIdx = i;
		}

		if (maxIdx == _currMaxIdx) return;
		_currMaxIdx = maxIdx;
		Text = ((BrainWave)maxIdx).ToString();
	}
}
