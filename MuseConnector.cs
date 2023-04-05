using System.Collections.Generic;
using System.Linq;
using Godot;
using SacaDev.Muse;
namespace MuseHead;

public partial class MuseConnector : Node
{
   [Signal] public delegate void ConnectedEventHandler();
   [Signal] public delegate void DisconnectedEventHandler();
   [Signal] public delegate void EegReceivedEventHandler(double[] data, double lowest, double highest);
   
   MuseManager _museManager = new ();
   string _museAlias { get; set; } = "Muse";
   int _musePort { get; set; } = 5000;
   bool _connected;
   double _lowestEegValue = 0.0;
   double _highestEegValue = 0.0;
   Queue<ICollection<double>> _eegQueue = new();

   public void Connect(string deviceName, int port)
   {
      _museAlias = deviceName;
      _musePort = port;
      GD.Print($"Attempting connection to muse '{_museAlias}#{_musePort}''");
      
      _museManager.Connect(_museAlias, _musePort);
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

      if (e.Address == SignalAddress.Eeg && e.Values is { })
      {
         var lowest = e.Values.Min();
         var highest = e.Values.Max();

         if (lowest < _lowestEegValue)
         {
            _lowestEegValue = lowest;
            GD.Print($"Lowest: {_lowestEegValue}");
         }

         if (highest > _highestEegValue)
         {
            _highestEegValue = highest;
            GD.Print($"Highest: {_highestEegValue}");
         }
         
         _eegQueue.Enqueue(e.Values);
      } 
      else if (e.Address is >= SignalAddress.Alpha_Abs and <= SignalAddress.Gamma_Rel)
      {
         GD.Print(e.Address);
         GD.Print(e.Values);
      } else if (e.Address == SignalAddress.Drlref)
      {
         
      }
      else
      {
         GD.Print($"Unknown: {e.Address}");
      }
   }

   public override void _Process(double delta)
   {
      if (_eegQueue.Count < 22) return;
      
      var buffer = new double[4][];
      buffer[0] = new double[_eegQueue.Count];
      buffer[1] = new double[_eegQueue.Count];
      buffer[2] = new double[_eegQueue.Count];
      buffer[3] = new double[_eegQueue.Count];
      
      var queueIndex = 0;
      while (_eegQueue.Count > 0)
      {
         var eeg = _eegQueue.Dequeue();
         var channel = 0;
         foreach (var d in eeg)
         {
            buffer[channel][queueIndex] = d;
            channel += 1;
         }
         queueIndex += 1;
      }

      var window = new FftSharp.Windows.Hanning();
      window.ApplyInPlace(buffer[0]);
      window.ApplyInPlace(buffer[1]);
      window.ApplyInPlace(buffer[2]);
      window.ApplyInPlace(buffer[3]);

      var channel0 = FftSharp.Transform.FFTfreq(220.0, buffer[0]);
      var channel1 = FftSharp.Transform.FFTfreq(220.0, buffer[1]);
      var channel2 = FftSharp.Transform.FFTfreq(220.0, buffer[2]);
      var channel3 = FftSharp.Transform.FFTfreq(220.0, buffer[3]);

      var averaged = new double[] {Mathf.LinearToDb(buffer[0].Average()), Mathf.LinearToDb(buffer[1].Average()), Mathf.LinearToDb(buffer[2]
      .Average()), Mathf.LinearToDb(buffer[3].Average())};

      EmitSignal(SignalName.EegReceived, averaged, _lowestEegValue, _highestEegValue);
   }
}