using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb8
{
    using static HecatombAliases;

    class TaskHandler : StateHandler, IChoiceMenu
    {
        public List<int> Minions;
        [JsonIgnore] public Type[] Tasks;

        public TaskHandler() : base()
        {
            Minions = new List<int>();
            Tasks = new[] {
                typeof(DigTask),
                typeof(BuildTask),
                typeof(UndesignateTask)
            };
            AddListener<DestroyEvent>(OnDestroy);
            AddListener<DespawnEvent>(OnDespawn);
        }

        public GameEvent OnDestroy(GameEvent ge)
        {
            DestroyEvent dse = (DestroyEvent)ge;
            if (dse.Entity!.EID != null && Minions.Contains((int)dse.Entity.EID))
            {
                Minions.Remove((int)dse.Entity!.EID);
                if (dse.Cause == "Decay")
                {
                    PushMessage("{orange}Your " + (dse.Entity as Creature)!.Describe(article: false) + " has rotted away, leaving naught but bones.");
                }
                else
                {
                    PushMessage("{orange}Your " + (dse.Entity as Creature)!.Describe(article: false) + " has perished!");
                }
            }
            return ge;
        }


        public void AddMinion(Creature cr)
        {
            if (!cr.HasComponent<Minion>())
            {
                cr.AddComponent(Entity.Spawn<Minion>());
            }
            if (!cr.HasComponent<Inventory>())
            {
                cr.AddComponent(Entity.Spawn<Inventory>());
            }
            if (cr.EID != null)
            {
                Minions.Add((int)cr.EID);
            }
        }
        public GameEvent OnDespawn(GameEvent ge)
        {
            DespawnEvent dse = (DespawnEvent)ge;
            if (dse.Entity!.EID is null)
            {
                return ge;
            }
            Minions = Minions.Where((int eid) => dse.Entity.EID != eid).ToList();
            return ge;
        }

        // this causes some issues because we want to cache only until the interface changes
        //protected List<IMenuListable>? cachedChoices;

        //public void PurgeCache()
        //{
        //    cachedChoices = null;
        //}
        public void BuildMenu(MenuChoiceControls menu)
        {
            //if (cachedChoices != null)
            //{
            //    menu.Choices = cachedChoices;
            //    return;
            //}
            menu.Header = "Choose a task:";
            List<IMenuListable> tasks = new List<IMenuListable>();
            foreach (var t in Tasks)
            {
                tasks.Add((Task)Activator.CreateInstance(t)!);
            }
            //var structures = Structure.ListAsStrings();
            //foreach (string t in Tasks)
            //{
            //    bool valid = true;
            //    Task task = GetTask(t);
            //    foreach (string s in task.PrereqStructures)
            //    {
            //        if (!structures.Contains(s))
            //        {
            //            valid = false;
            //        }
            //    }
            //    if (valid || Options.NoIngredients)
            //    {
            //        tasks.Add(GetTask(t));
            //    }
            //}
            //cachedChoices = tasks;
            menu.Choices = tasks;
        }

        public T? GetTask<T>() where T: Task
        {
            foreach (Type t in Tasks)
            {
                if (t == typeof(T))
                {
                    return (T)Activator.CreateInstance(t)!;
                }
            }
            return null;
        }
        public void FinishMenu(MenuChoiceControls menu)
        {

        }

        public List<Creature> GetMinions()
        {
            var minions = new List<Creature>();
            foreach (int eid in Minions)
            {
                Creature? cr = GetEntity<Creature>(eid);
                if (cr != null)
                {
                    minions.Add(cr);
                }
            }
            return minions;
        }
    }
}
