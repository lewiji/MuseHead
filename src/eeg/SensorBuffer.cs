using System;
using System.Collections.Generic;
namespace MuseHead.eeg;

public class SensorBuffer
{
   public SensorBuffer(int size)
   {
      Id = _numBuffers;
      _numBuffers += 1;
      MaxSize = size;
      Values = new Queue<double>();
   }
   
   static int _numBuffers;
   public int Id { get; }
   public Queue<double> Values { get; }
   public int MaxSize { get; }

   public void Add(double value)
   {
      Values.Enqueue(value);
   }
}