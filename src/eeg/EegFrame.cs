using System;
using System.Collections.Generic;
using System.Linq;

namespace MuseHead.eeg;

public readonly struct EegFrame
{
   public EegFrame(ICollection<double> data)
   {
      A = (0 < data.Count) ? data.ElementAt(0) : 0.0;
      B = (1 < data.Count) ? data.ElementAt(1) : 0.0;
      C = (2 < data.Count) ? data.ElementAt(2) : 0.0;
      D = (3 < data.Count) ? data.ElementAt(3) : 0.0;
   }
   public double A { get; }
   public double B { get; }
   public double C { get; }
   public double D { get; }

   public double this[int index] =>
      index switch
      {
         0 => A,
         1 => B,
         2 => C,
         3 => D,
         _ => throw new IndexOutOfRangeException()
      };
}