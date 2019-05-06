using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Hecatomb
{
    using static HecatombAliases;

    class SaveGameFile : IMenuListable
    {
        public string Name;

        public SaveGameFile(string name)
        {
            Name = name;
        }

        public ColoredText ListOnMenu()
        {
            return Name;
        }

        public void ChooseFromMenu()
        {
            Game.GameName = Name;
            Game.SplashPanel.Splash(new List<ColoredText>()
            {
                $"Restoring {Name}..."
            }, frozen: true);
            Debug.WriteLine("restoring the game");
            Controls.Set(new FrozenControls());
            Thread thread = new Thread(Commands.RestoreGameProcess);
            thread.Start();
        }
    }

    class SaveGameChooser : IChoiceMenu
    {
        public void BuildMenu(MenuChoiceControls menu)
        {
            menu.Header = "Choose a saved game:";
            menu.Choices = new List<IMenuListable>();
            string[] filePaths = Directory.GetFiles(@"..\", "*.json");
            foreach (string path in filePaths)
            {
                string[] split = path.Split('\\');
                string fname = split[split.Length-1];
                split = fname.Split('.');
                fname = split[0];
                menu.Choices.Add(new SaveGameFile(fname));
            }
        }

        public void FinishMenu(MenuChoiceControls menu)
        {

        }
    }
}
