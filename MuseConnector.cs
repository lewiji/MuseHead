using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FftSharp;
using FftSharp.Windows;
using Godot;
using MuseHead.eeg;
using SacaDev.Muse;
using Window = FftSharp.Window;

namespace MuseHead;

public partial class MuseConnector : Node
{
   [Signal] public delegate void ConnectedEventHandler();
   [Signal] public delegate void DisconnectedEventHandler();
   [Signal] public delegate void EegReceivedEventHandler(double[] data);

   const int NumSensors = 4;
   const int SampleRate = 256;
   const int BufferSize = SampleRate;

   readonly MuseManager _museManager = new();
   readonly Welch _window = new();
   readonly SensorBuffer[] _sensorBuffers;
   bool _connected;

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

   void MusePacketReceived(object? sender, MusePacket e)
   {
      if (!_connected)
      {
         _connected = true;
         EmitSignal(SignalName.Connected);
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

         if (!buffer.IsFull()) continue;
         
         ProcessBuffer(buffer);
         buffer.Clear();
      }
   }

   void ProcessBuffer(SensorBuffer buffer)
   {
      var currentWindow = _window.Apply(Filter.BandPass(buffer.Current, 256.0, 0.5, 40.0));
      var combinedBuffer = new double[BufferSize];
      Array.Copy(buffer.Last, BufferSize / 2, combinedBuffer, 0, BufferSize / 2);
      Array.Copy(currentWindow, 0, combinedBuffer, BufferSize / 2, BufferSize / 2);
      buffer.CopyWindowToLast(currentWindow);
      
      var pow = Transform.FFTmagnitude(combinedBuffer);
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

      EmitSignal(SignalName.EegReceived, bandPowerNormalized);
   }

   double GetSummedBandPower(double[] mag, double[] freq, BrainWave band)
   {
      var minFreqIndex = Array.IndexOf(freq, freq.First(f => f > BrainWaves.FreqBands[band].Min));
      var maxFreqIndex = Array.IndexOf(freq, freq.Last(f => f <= BrainWaves.FreqBands[band].Max));
      return mag[minFreqIndex..(maxFreqIndex)].Sum();
   }
}