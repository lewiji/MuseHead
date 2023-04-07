using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FftSharp;
using FftSharp.Windows;
using Godot;
using MuseHead.eeg;
using SacaDev.Muse;
using Range = System.Range;

namespace MuseHead;

public partial class MuseConnector : Node
{
   [Signal] public delegate void ConnectedEventHandler();
   [Signal] public delegate void DisconnectedEventHandler();
   [Signal] public delegate void EegReceivedEventHandler(Godot.Collections.Dictionary<int, double[]> data);

   const int NumSensors = 4;
   const int SampleRate = 256;
   const int SlidingSamples = SampleRate / 16;
   const int BufferSize = SampleRate;

   readonly MuseManager _museManager = new();
   readonly Welch _window = new();
   readonly SensorBuffer[] _sensorBuffers;
   bool _connected;
   Godot.Collections.Dictionary<int, double[]> _signalData = new();
   bool _locked;

   public MuseConnector()
   {
      _sensorBuffers = new SensorBuffer[NumSensors];
      for (var i = 0; i < NumSensors; i++)
      {
         _sensorBuffers[i] = new SensorBuffer(BufferSize);
      }
   }

   public void Connect(string deviceName, int port)
   {
      GD.Print($"Attempting connection to muse '{deviceName}#{port}''");
      _museManager.Connect(deviceName, port);
      _museManager.SetSubscriptions(SignalAddress.Eeg);
      _museManager.MusePacketReceived += MusePacketReceived;
   }

   async void MusePacketReceived(object? sender, MusePacket e)
   {
      if (!_connected)
      {
         _connected = true;
         EmitSignal(SignalName.Connected);
      }

      if (_locked)
      {
         await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
      }

      switch (e.Address)
      {
         case SignalAddress.Eeg:
            AddFrame(e.Values);
            break;
         case >= SignalAddress.Alpha_Abs and <= SignalAddress.Gamma_Rel:
            GD.Print(e.Address);
            GD.Print(e.Values);
            break;
         case SignalAddress.Drlref:
            break;
         default:
            GD.Print($"Unknown: {e.Address}");
            break;
      }

   }

   void AddFrame(ICollection<double> frame)
   {
      var values = frame.ToArray();
      for (var i = 0; i < Math.Min(NumSensors, values.Length); i++)
      {
         var buffer = _sensorBuffers[i];
         buffer.Add(values[i]);
      }
   }

   double[] ProcessBuffer(double[] values)
   {
      var currentWindow = _window.Apply(Filter.BandPass(values.ToArray(), 256.0, 0.5, 40.0));
      var pow = Transform.FFTmagnitude(currentWindow);
      var freq = Transform.FFTfreq(SampleRate, pow.Length);

      var aDeltaPower = GetSummedBandPower(pow, freq, BrainWave.Delta);
      var aThetaPower = GetSummedBandPower(pow, freq, BrainWave.Theta);
      var aAlphaPower = GetSummedBandPower(pow, freq, BrainWave.Alpha);
      var aBetaPower = GetSummedBandPower(pow, freq, BrainWave.Beta);
      var aGammaPower = GetSummedBandPower(pow, freq, BrainWave.Gamma);
         
      var totalPower = pow.Sum();

      var bandPowerNormalized = new[] {
         aDeltaPower / totalPower,
         aThetaPower / totalPower,
         aAlphaPower / totalPower,
         aBetaPower / totalPower,
         aGammaPower / totalPower
      };

      return bandPowerNormalized;
   }

   double GetSummedBandPower(double[] mag, double[] freq, BrainWave band)
   {
      var minFreqIndex = Array.IndexOf(freq, freq.First(f => f > BrainWaves.FreqBands[band].Min));
      var maxFreqIndex = Array.IndexOf(freq, freq.Last(f => f <= BrainWaves.FreqBands[band].Max));
      return mag[minFreqIndex..(maxFreqIndex)].Sum();
   }

   public override void _PhysicsProcess(double delta)
   {
      _locked = true;
      _signalData.Clear();
      foreach (var sensorBuffer in _sensorBuffers)
      {
         if (sensorBuffer.Values.Count < BufferSize) continue;
         
         _signalData[sensorBuffer.Id] = ProcessBuffer(sensorBuffer.Values.TakeLast(BufferSize).ToArray());

         foreach (var i in Enumerable.Range(0, SlidingSamples))
         {
            sensorBuffer.Values.TryDequeue(out _);
         }
      }

      _locked = false;

      if (_signalData.Count > 0)
      {
         double[] avg = {0.0, 0.0, 0.0, 0.0, 0.0};
         foreach (var (key, value) in _signalData)
         {
            for (var index = 0; index < value.Length; index++)
            {
               var t = value[index];
               avg[index] += t;
            }
         }

         for (var i = 0; i < avg.Length; i++)
         {
            avg[i] /= 4.0;
         }
         _signalData[-1] = avg;
         
         EmitSignal(SignalName.EegReceived, _signalData);
      }
   }
}