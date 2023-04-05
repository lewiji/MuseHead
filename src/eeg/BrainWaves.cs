using System;
namespace MuseHead.eeg;

public static class BrainWaves
{
   /*
      Delta(δ) 1-4Hz
      Theta(θ) 4-8Hz
      Alpha(α) 7.5-13Hz
      Beta(β) 13-30Hz
      Gamma(γ) 30-44Hz
   */
   public static Range Delta = new Range(1, 4);
   public static Range Theta = new Range(5, 8);
   public static Range Alpha = new Range(9, 13);
   public static Range Beta = new Range(13, 30);
   public static Range Gamma = new Range(30, 50);
}