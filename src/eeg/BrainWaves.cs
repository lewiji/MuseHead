using System;
using System.Collections.Generic;
namespace MuseHead.eeg;

public static class BrainWaves
{
   public static readonly Dictionary<BrainWave, BrainFreqBand> FreqBands = new () {
      {BrainWave.Delta, new BrainFreqBand(0.5, 4.0)},
      {BrainWave.Theta, new BrainFreqBand(4.0, 8.0)},
      {BrainWave.Alpha, new BrainFreqBand(8.0, 12.5)},
      {BrainWave.Beta, new BrainFreqBand(12.5, 30.0)},
      {BrainWave.Gamma, new BrainFreqBand(30.0, 40.0)},
   };
}