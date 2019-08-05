using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// This thing tracks inconsistent world states so we don't have to crash
namespace Hecatomb
{
    static class TheFixer
    {
        static List<string> Log;

        static TheFixer()
        {
            Log = new List<string>();
        }

        public static void PushMessage(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
            Log.Add(s);
        }

        public static void Purge()
        {
            Log = new List<string>();
        }
        public static void Dump()
        {
            if (Log.Count == 0)
            {
                return;
            }
            string timestamp = DateTime.Now.ToString("yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss.fffffffK");
            System.IO.File.WriteAllLines(@"..\"+Game.GameName+timestamp + ".txt", Log);
        }

        public static void CheckStates()
        {
            foreach (Entity e in Game.World.Entities.Values.Where((Entity ent) => ent is Minion))
            {
                Minion minion = (Minion)e;
                var task = minion.Task;
                if (task != null)
                {
                    if (task.Unbox().Worker.Unbox() != minion.Entity.Unbox())
                    {
                        PushMessage($"Unassigned desynched task and minion; {task.Unbox().Describe()}");
                       
                        task.Unbox().Unassign();
                        minion.Task = null;
                    }
                }
            }
            foreach (Task t in Game.World.Tasks)
            {
                if (!t.ValidTile(new Coord(t.X, t.Y, t.Z)))
                {
                    if (t is DigTask)
                    {
                        // known to happen for good reasons
                    }
                    else
                    {
                        PushMessage($"Canceling an invalid task; {t.Describe()}");
                        t.Cancel();
                    }
                }
                else if (t is HaulTask)
                {
                    if (t.Claims.Count == 0)
                    {
                        PushMessage($"Canceling a haul task with no claims; {t.Describe()}");
                        t.Cancel();
                    }
                }
            }
        }
    }

    
}
