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
using System.Text.RegularExpressions;

namespace Hecatomb8
{
    using static HecatombAliases;

    class SaveGameFile : IMenuListable, IDisplayInfo
    {
        public string? Name;

        public SaveGameFile()
        {

        }
        public SaveGameFile(string name)
        {
            Name = name;
        }

        public ColoredText ListOnMenu()
        {
            return Name!;
        }

        public void ChooseFromMenu()
        {
            if (GameState.World != null)
            {
                InterfaceState.SetControls(new ConfirmationControls("Really quit the current game?", CheckBuildDate));
            }
            else
            {
                RestoreGame();
            }
        }

        public void CheckBuildDate()
        {
            RestoreGame();
        }

        public void RestoreGame()
        {
            GameManager.GameName = Name!;
            
            InterfaceState.Splash(new List<ColoredText>() {
                    $"Restoring {Name}...this should take less than a minute."
                },
                fullScreen: false
            );
            //InterfaceState.DirtifyTextPanels();
            InterfaceState.InfoPanel.Prepare();
            HecatombGame.DeferUntilAfterDraw(GameManager.RestoreGameProcess);
        }

        public void BuildInfoDisplay(InfoDisplayControls menu)
        {
            var path = (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location));
            menu.Header = "Choose a saved game:";
            menu.Choices = new List<IMenuListable>();
            System.IO.Directory.CreateDirectory(path + @"\saves");
            string[] filePaths = Directory.GetFiles(path + @"\saves", "*.zip");
            foreach (string paths in filePaths)
            {
                string[] split = paths.Split('\\');
                string fname = split[split.Length - 1];
                split = fname.Split('.');
                fname = split[0];
                menu.Choices.Add(new SaveGameFile(fname));
            }
        }

        public void FinishInfoDisplay(InfoDisplayControls menu)
        {
            menu.KeyMap[Microsoft.Xna.Framework.Input.Keys.Escape] = InterfaceState.RewindControls;
        }
    }
}
