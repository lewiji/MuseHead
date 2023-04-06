using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MuseHead.eeg;
using SacaDev.Muse;
using Range = System.Range;

namespace MuseHead;

public partial class MuseConnector : Node
{
   [Signal] public delegate void ConnectedEventHandler();
   [Signal] public delegate void DisconnectedEventHandler();
   [Signal] public delegate void EegReceivedEventHandler(double[] data);

   readonly MuseManager _museManager = new ();
   const int SampleRate = 256;
   const int TotalBufferLength = SampleRate / 4;
   const double FreqResolution = (double)SampleRate / TotalBufferLength;
   const int PrimaryBufferLength = (TotalBufferLength); // 16ms = 60fps
   const int OverlapLength = TotalBufferLength - PrimaryBufferLength;
   bool _connected;
   readonly EegFrame[] _primaryEegBuffer = new EegFrame[PrimaryBufferLength];
   readonly EegFrame[] _windowOverlapBuffer = new EegFrame[OverlapLength];
   int _bufferCount;
   int _overlapCount;
   
   public bool IsConnected => _connected;

   public void Connect(string deviceName, int port)
   {
      GD.Print($"Attempting connection to muse '{deviceName}#{port}''");
      
      _museManager.Connect(deviceName, port);
      _museManager.SetSubscriptions(SignalAddress.All);
      _museManager.RemoveSubscriptions(SignalAddress.Gyro | SignalAddress.Acceleration);
      _museManager.MusePacketReceived += MusePacketReceived;
   }
   
   void MusePacketReceived(object sender, MusePacket e)
   {
      if (!_connected)
      {
         _connected = true;
         EmitSignal(SignalName.Connected);
      }

      switch (e.Address)
      {
         case SignalAddress.Eeg when e.Values is { }:
            AddFrame(e.Values.ToArray());
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

   void AddFrame(double[] values)
   {
      _primaryEegBuffer[_bufferCount++] = new EegFrame(values);
      
      if (_bufferCount < PrimaryBufferLength) return;
      
      if (_overlapCount >= OverlapLength)
      {
         ProcessBuffer();
      }

      CopyOverlap();
      _bufferCount = 0;
   }

   void CopyOverlap()
   {
      Array.Copy(_primaryEegBuffer, PrimaryBufferLength - OverlapLength - 1, _windowOverlapBuffer, 0, OverlapLength);
      _overlapCount = OverlapLength;
   }

   public void ProcessBuffer()
   {
      if (_bufferCount + _overlapCount != TotalBufferLength)
      {
         throw new Exception(
            $"Primary + overlap buffer length was {_bufferCount + _overlapCount}, expected {TotalBufferLength}");
      }
      
      var aFrames = new double[TotalBufferLength];
      var bFrames = new double[TotalBufferLength];
      var cFrames = new double[TotalBufferLength];
      var dFrames = new double[TotalBufferLength];

      for (var olaIdx = 0; olaIdx < _overlapCount; olaIdx++)
      {
         aFrames[olaIdx] = _windowOverlapBuffer[olaIdx].A;
         bFrames[olaIdx] = _windowOverlapBuffer[olaIdx].B;
         cFrames[olaIdx] = _windowOverlapBuffer[olaIdx].C;
         dFrames[olaIdx] = _windowOverlapBuffer[olaIdx].D;
      }
      
      for (var i = 0; i < _bufferCount; i++)
      {
         aFrames[i] = _primaryEegBuffer[i].A;
         bFrames[i] = _primaryEegBuffer[i].B;
         cFrames[i] = _primaryEegBuffer[i].C;
         dFrames[i] = _primaryEegBuffer[i].D;
      }

      var window = new FftSharp.Windows.Hanning();
      window.ApplyInPlace(aFrames);
      window.ApplyInPlace(bFrames);
      window.ApplyInPlace(cFrames);
      window.ApplyInPlace(dFrames);

      var aFft = FftSharp.Transform.FFT(aFrames);
      var bFft = FftSharp.Transform.FFT(bFrames);
      var cFft = FftSharp.Transform.FFT(cFrames);
      var dFft = FftSharp.Transform.FFT(dFrames);
      
      var aFreq = FftSharp.Transform.FFTfreq(SampleRate, aFrames);
      var bFreq = FftSharp.Transform.FFTfreq(SampleRate, bFrames);
      var cFreq = FftSharp.Transform.FFTfreq(SampleRate, cFrames);
      var dFreq = FftSharp.Transform.FFTfreq(SampleRate, dFrames);

      var aDeltaIndices = new Range(Array.IndexOf(aFreq, aFreq.First(f => f >= BrainWaves.Delta.Start.Value)), Array.IndexOf(aFreq, aFreq.Last(f => f <= BrainWaves.Delta.End.Value)));
      var aThetaIndices = new Range(Array.IndexOf(aFreq, aFreq.First(f => f >= BrainWaves.Theta.Start.Value)), Array.IndexOf(aFreq, aFreq.Last(f => f <= BrainWaves.Theta.End.Value)));
      var aAlphaIndices = new Range(Array.IndexOf(aFreq, aFreq.First(f => f >= BrainWaves.Alpha.Start.Value)), Array.IndexOf(aFreq, aFreq.Last(f => f <= BrainWaves.Alpha.End.Value)));
      var aBetaIndices = new Range(Array.IndexOf(aFreq, aFreq.First(f => f >= BrainWaves.Beta.Start.Value)), Array.IndexOf(aFreq, aFreq.Last(f => f <= BrainWaves.Beta.End.Value)));
      var aGammaIndices = new Range(Array.IndexOf(aFreq, aFreq.First(f => f >= BrainWaves.Gamma.Start.Value)), Array.IndexOf(aFreq, aFreq.Last(f => f <= BrainWaves.Gamma.End.Value)));

      var aDeltaPower = (aFft[aDeltaIndices.Start.Value..(aDeltaIndices.End.Value + 1)].Sum(f => f.MagnitudeSquared));
      var aThetaPower = (aFft[aThetaIndices.Start.Value..(aThetaIndices.End.Value + 1)].Sum(f => f.MagnitudeSquared));
      var aAlphaPower = (aFft[aAlphaIndices.Start.Value..(aAlphaIndices.End.Value + 1)].Sum(f => f.MagnitudeSquared));
      var aBetaPower = (aFft[aBetaIndices.Start.Value..(aBetaIndices.End.Value + 1)].Sum(f => f.MagnitudeSquared));
      var aGammaPower = (aFft[aGammaIndices.Start.Value..(aGammaIndices.End.Value + 1)].Sum(f => f.MagnitudeSquared));

      var totalPower = aFft.Sum(f => f.MagnitudeSquared);
      //var totalPower = aDeltaPower + aThetaPower + aAlphaPower + aBetaPower + aGammaPower;

      var bandPowerNormalized = new double[5]
      {
         aDeltaPower / totalPower,
         aThetaPower / totalPower,
         aAlphaPower / totalPower,
         aBetaPower / totalPower,
         aGammaPower / totalPower
      };

      EmitSignal(SignalName.EegReceived, bandPowerNormalized);
   }
}