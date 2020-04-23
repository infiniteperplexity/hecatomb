using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;

namespace Hecatomb8
{
    public static class GameManager
    {
        public static string GameName = "NewGame";
        public static void StartGameWithConfirmation()
        {
            ControlContext.Set(new ConfirmationControls("Really quit the current game?", StartGame));
        }


        public static void StartGame() {
            var world = new World(256, 256, 64);
            GameState.World = world;
            var ws = new BuildWorldStrategy();
            Debug.WriteLine("flag 0");
            ws.Generate();
            Debug.WriteLine("flag 1");
            InterfaceState.SetControls(InterfaceState.DefaultControls!);
            InterfaceState.PlayerIsReady();
        }

        public static void RestoreGame() { }

        public static void SaveGameCheckFileName()
        {
            var path = (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            System.IO.Directory.CreateDirectory(path + @"\saves");
            if (File.Exists(path + @"\saves\" + Game.GameName + ".json"))
            {
                ControlContext.Set(new ConfirmationControls("Really overwrite " + Game.GameName + ".json?", SaveGameCommand));
            }
            else
            {
                SaveGameCommand();
            }
        }
        public static void SaveGame()
        {
            Game.SplashPanel.Splash(new List<ColoredText>()
            {
                $"Saving {Game.GameName}..."
            }, frozen: true);
            Debug.WriteLine("saving the game");
            Thread thread = new Thread(SaveGameProcess);
            thread.Start();
        }

        public static void SaveGameProcess()
        {
            try
            {
                Game.World.Stringify();
                ControlContext.Reset();
            }
            catch (Exception e)
            {
                Game.HandleException(e);
            }
        }

        public static void RestoreGameWithConfirmation()
        {
            ControlContext.Set(new ConfirmationControls("Really quit the current game?", RestoreGame));
        }

        public static void RestoreGameProcess()
        {
            try
            {
                //string json = System.IO.File.ReadAllText(@"..\" + Game.GameName + ".json");
                if (Game.World == null)
                {
                    Game.World = new World(256, 256, 64);
                }
                //Game.World.Parse(json);
                var path = (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
                System.IO.Directory.CreateDirectory(path + @"\saves");
                Game.World.Parse(path + @"\saves\" + Game.GameName + ".json");
                // we need some kind of failure handling...
                Game.MainPanel.IntroState = false;
                ControlContext.Reset();
            }
            catch (Exception e)
            {
                Game.HandleException(e);
            }
        }

        public static void RestoreGame()
        {
            //throw new Exception("test exception");
            // I guess maybe a Save File should have an object representation?
            ControlContext.Set(new MenuChoiceControls(new SaveGameChooser()));
        }

        public static void SaveGameAs()
        {
            Action<string> saveGameAs = (string name) =>
            {
                //could check legality of name here?
                Game.GameName = name;
                SaveGameCommandCheckFileName();
            };
            ControlContext.Set(new TextEntryControls("Type a name for your saved game.", saveGameAs));
            (Controls as TextEntryControls).CurrentText = Game.GameName;
        }

        public static void BackToTitleWithConfirmation()
        {
            ControlContext.Set(new ConfirmationControls("Really quit the current game?", BackToTitle));
        }

        public static void BackToTitle()
        {
            SplashPanel.Active = false;
            SetUpTitle();
        }

    }
}
