using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace Hecatomb8
{
    using static HecatombAliases;
    public static class GameManager
    {
        public static string GameName = "NewGame";
        public static DateTime BuildDate = GetBuildDate();
        public static void StartGameWithConfirmation()
        {
            InterfaceState.SetControls(new ConfirmationControls("Really quit the current game?", StartGame));
        }


        public static void StartGame()
        {
            InterfaceState.Splash(new List<ColoredText>() {
                "{yellow}Welcome to Hecatomb!",
                " ",
                "You are a necromancer: A despised sorcerer who reanimates the dead to do your bidding.  Cast out from society, you flee to the wild hills to plot your revenge and purse the forbidden secrets of immortality.",
                " ",
                "{lime green}Cast spells, raise zombies from their graves, and command them to harvest resources and build you a fortress.  But beware: The forces of good will not long stand for your vile ways.",
                " ",
                "{cyan}Once the game begins, follow the in-game tutorial instructions on the right hand panel, or press \"/\" to turn off the messages.",
                " ",
                "(Building world...please wait.)",
                },
                fullScreen: true
            );
            HecatombGame.DeferUntilAfterDraw(StartGameProcess);
        }

        public static void StartGameProcess()
        {
            var world = new World(256, 256, 64);
            GameState.World = world;
            var ws = new BuildWorldStrategy();
            bool succeeded = false;
            while (!succeeded)
            {
                succeeded = ws.Generate();
            }
            InterfaceState.Splash(new List<ColoredText>() {
                "{yellow}Welcome to Hecatomb!",
                " ",
                "You are a necromancer: A despised sorcerer who reanimates the dead to do your bidding.  Cast out from society, you flee to the wild hills to plot your revenge and purse the forbidden secrets of immortality.",
                " ",
                "{lime green}Cast spells, raise zombies from their graves, and command them to harvest resources and build you a fortress.  But beware: The forces of good will not long stand for your vile ways.",
                " ",
                "{cyan}Once the game begins, follow the in-game tutorial instructions on the right hand panel, or press \"/\" to turn off the messages.",
                " ",
                "{orange}(Press Space Bar to continue.)",
                },
                fullScreen: true,
                callback: InterfaceState.ResetControls
            );
            //InterfaceState.SetControls(InterfaceState.DefaultControls!);
            InterfaceState.PlayerIsReady();
        }

        public static void SaveGameCheckFileName()
        {
            var path = (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location));
            System.IO.Directory.CreateDirectory(path + @"\saves");
            if (File.Exists(path + @"\saves\" + GameName + ".json"))
            {
                InterfaceState.SetControls(new ConfirmationControls("Really overwrite " + GameName + ".json?", SaveGame));
            }
            else
            {
                SaveGame();
            }
        }
        public static void SaveGame()
        {
            //Game.SplashPanel.Splash(new List<ColoredText>()
            //{
            //    $"Saving {Game.GameName}..."
            //}, frozen: true);
            Debug.WriteLine("saving the game");
            HecatombGame.DeferUntilAfterDraw(SaveGameProcess);
            //Thread thread = new Thread(SaveGameProcess);
            //thread.Start();
        }

        public static void SaveGameProcess()
        {
            try
            {
                GameState.World!.Stringify();
                InterfaceState.ResetControls();
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        public static void RestoreGameWithConfirmation()
        {
            InterfaceState.SetControls(new ConfirmationControls("Really quit the current game?", RestoreGame));
        }

        public static void RestoreGameProcess()
        {
            try
            {
                //string json = System.IO.File.ReadAllText(@"..\" + Game.GameName + ".json");
                if (GameState.World == null)
                {
                    GameState.World = new World(256, 256, 64);
                }
                var path = (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location));
                System.IO.Directory.CreateDirectory(path + @"\saves");
                // we need some kind of failure handling...
                GameState.World!.Parse(path + @"\saves\" + GameName + ".json");
                InterfaceState.ResetControls();
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        public static void RestoreGame()
        {
            //throw new Exception("test exception");
            // I guess maybe a Save File should have an object representation?
            InterfaceState.SetControls(new MenuChoiceControls(new SaveGameFile()));
        }

        public static void SaveGameAs()
        {
            Action<string> saveGameAs = (string name) =>
            {
                //could check legality of name here?
                GameName = name;
                SaveGameCheckFileName();
            };
            Debug.WriteLine("flag 0");
            InterfaceState.SetControls(new TextEntryControls("Type a name for your saved game.", saveGameAs));
            (Controls as TextEntryControls)!.CurrentText = GameName;
        }

        public static void BackToTitleWithConfirmation()
        {
            InterfaceState.SetControls(new ConfirmationControls("Really quit the current game?", BackToTitle));
        }

        public static void BackToTitle()
        {
            //SplashPanel.Active = false;
            GameState.World = null;
            InterfaceState.MainPanel.ClearGlyphs();
            SetUpTitle();
        }

        public static void SetUpTitle()
        {
            var commands = new List<(Keys, ColoredText, Action)>() {
                (Keys.N, "New game.", GameManager.StartGame),
                (Keys.R, "Restore game.", GameManager.RestoreGame),
                (Keys.Q, "Quit.", HecatombGame.QuitHook!)
            };
            InterfaceState.SetControls(new StaticMenuControls("{yellow}Welcome to Hecatomb!", commands));
        }

        public static void HandleException(Exception e)
        {

        }

        public static DateTime GetLinkerTimestampUtc(Assembly assembly)
        {
            var location = assembly.Location;
            return GetLinkerTimestampUtc(location);
        }

        public static DateTime GetLinkerTimestampUtc(string filePath)
        {
            const int peHeaderOffset = 60;
            const int linkerTimestampOffset = 8;
            var bytes = new byte[2048];

            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                file.Read(bytes, 0, bytes.Length);
            }

            var headerPos = BitConverter.ToInt32(bytes, peHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(bytes, headerPos + linkerTimestampOffset);
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return dt.AddSeconds(secondsSince1970);
        }

        public static DateTime GetBuildDate()
        {
            return GetLinkerTimestampUtc(Assembly.GetExecutingAssembly());
        }
    }
}
