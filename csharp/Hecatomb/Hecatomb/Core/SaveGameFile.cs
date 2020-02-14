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
            ControlContext.Set(new FrozenControls());
            Thread thread = new Thread(Commands.RestoreGameProcess);
            thread.Start();
        }
    }

    class SaveGameChooser : IChoiceMenu
    {
        public void BuildMenu(MenuChoiceControls menu)
        {
            var path = (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            menu.Header = "Choose a saved game:";
            menu.Choices = new List<IMenuListable>();
            System.IO.Directory.CreateDirectory(path + @"\saves");
            string[] filePaths = Directory.GetFiles(path + @"\saves", "*.json");
            foreach (string paths in filePaths)
            {
                string[] split = paths.Split('\\');
                string fname = split[split.Length-1];
                split = fname.Split('.');
                fname = split[0];
                menu.Choices.Add(new SaveGameFile(fname));
            }
        }

        public void FinishMenu(MenuChoiceControls menu)
        {
            menu.KeyMap[Microsoft.Xna.Framework.Input.Keys.Escape] = Controls.Back;
        }
    }
}
