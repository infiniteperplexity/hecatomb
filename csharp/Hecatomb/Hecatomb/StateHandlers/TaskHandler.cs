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
        public string[] Tasks;

        public TaskHandler() : base()
        {
            Minions = new List<TypedEntityField<Creature>>();
            Tasks = new[] { "DigTask", "BuildTask", "ConstructTask", "FurnishTask", "UndesignateTask" };
        }


        [JsonIgnore]
        public string MenuHeader
        {
            get
            {
                return "Choose a task:";
            }
            set { }
        }
        [JsonIgnore]
        public List<IMenuListable> MenuChoices
        {
            get
            {
                List<IMenuListable> tasks = new List<IMenuListable>();
                foreach (string t in Tasks)
                {
                    tasks.Add(GetTask(t));
                }
                return tasks;
            }
            set { }
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
