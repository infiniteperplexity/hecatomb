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
        public List<ListenerHandledEntityPointer<Creature>> Minions;
        [JsonIgnore] public Type[] Tasks;

        public TaskHandler() : base()
        {
            Minions = new List<ListenerHandledEntityPointer<Creature>>();
            Tasks = new[] {
                typeof(DigTask),
                typeof(UndesignateTask)
            };
            AddListener<DestroyEvent>(OnDestroy);
            AddListener<DespawnEvent>(OnDespawn);
        }

        public GameEvent OnDestroy(GameEvent ge)
        {
            DestroyEvent dse = (DestroyEvent)ge;
            // I'm not sure if lists of TypedEntityFields can use Contains...
            foreach (var m in Minions)
            {
                //if (m.Unbox() == dse.Entity)
                //{
                //    if (dse.Cause == "Decay")
                //    {
                //        Game.InfoPanel.PushMessage("{orange}Your " + (dse.Entity as Creature).Describe(article: false) + " has rotted away, leaving naught but bones.");
                //    }
                //    else
                //    {
                //        Game.InfoPanel.PushMessage("{orange}Your " + (dse.Entity as Creature).Describe(article: false) + " has perished!");
                //    }
                //}
            }
            return ge;
        }


        public void AddMinion(Creature cr)
        {
            Minions.Add(cr.GetPointer<Creature>(OnDespawn));
        }
        public GameEvent OnDespawn(GameEvent ge)
        {
            DespawnEvent dse = (DespawnEvent)ge;
            Minions = Minions.Where((ListenerHandledEntityPointer<Creature> c) => c.UnboxBriefly() != dse.Entity).ToList();
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
    }
}
