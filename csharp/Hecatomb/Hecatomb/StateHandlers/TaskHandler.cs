using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;

    class TaskHandler : StateHandler, IChoiceMenu
    {
        public List<TypedEntityField<Creature>> Minions;
        [JsonIgnore]public string[] Tasks;

        public TaskHandler() : base()
        {
            Minions = new List<TypedEntityField<Creature>>();
            Tasks = new[] { "DigTask", "BuildTask", "ConstructTask", "FurnishTask", "MurderTask", "PatrolTask", "ButcherTask", "UndesignateTask" };
            AddListener<DespawnEvent>(OnDespawn);
        }

        public GameEvent OnDespawn(GameEvent ge)
        {   
            // I could potentially make these lists a special thing that always listens?
            DespawnEvent dse = (DespawnEvent)ge;
            Minions = Minions.Where((TypedEntityField<Creature> c) => c.Entity != dse.Entity).ToList();
            return ge;
        }


        public void BuildMenu(MenuChoiceControls menu)
        {
            menu.Header = "Choose a task:";
            List<IMenuListable> tasks = new List<IMenuListable>();
            var structures = Structure.ListAsStrings();
            foreach (string t in Tasks)
            {
                bool valid = true;
                Task task = GetTask(t);
                foreach (string s in task.PrereqStructures)
                {
                    if (!structures.Contains(s))
                    {
                        valid = false;
                    }
                }
                if (valid || Options.NoIngredients)
                {
                    tasks.Add(GetTask(t));
                }
            }
            menu.Choices = tasks;
        }
        public void FinishMenu(MenuChoiceControls menu)
        {

        }
        
        public Task GetTask(Type t)
        {
            return (Task)Activator.CreateInstance(t);
        }

        public Task GetTask(String s)
        {
            Type t = Type.GetType("Hecatomb." + s);
            return (Task)Activator.CreateInstance(t);
        }
    }
}
