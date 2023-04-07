using System.Linq;
using Godot;

namespace MuseHead.debug;

public partial class Logger : Node
{
   FileAccess _file = default!;
   public override void _Ready()
   {
      _file = FileAccess.Open($"user://{Time.GetDatetimeStringFromSystem()}", FileAccess.ModeFlags.WriteRead);
   }

   public void LogValues(double[] buffer)
   {
      foreach (var d in buffer)
      {
         _file.StoreDouble(d);
      }
   }
}