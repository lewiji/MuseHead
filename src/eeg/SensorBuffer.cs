using System;
namespace MuseHead.eeg;

public class SensorBuffer
{
   public SensorBuffer(int size)
   {
      Id = _numBuffers;
      _numBuffers += 1;
      MaxSize = size;
      Current = new double[size];
      Last = new double[size];
   }
   
   static int _numBuffers;
   int _bufferCurrIndex = 0;
   
   public int Id { get; }
   public double[] Current { get; }
   public double[] Last { get; }
   public int MaxSize { get; }
   public int CurrIndex { get => _bufferCurrIndex; }

   public void Add(double value)
   {
      if (IsFull()) 
         throw new Exception("Buffer is full");

      Current[_bufferCurrIndex] = value;
      _bufferCurrIndex++;
   }

   public void Clear()
   {
      //Array.Copy(Current, Last, MaxSize);
      _bufferCurrIndex = 0;
   }

   public void CopyWindowToLast(double[] window)
   {
      Array.Copy(window, Last, window.Length);
   }

   public bool IsFull()
   {
      return _bufferCurrIndex >= MaxSize;
   }

}