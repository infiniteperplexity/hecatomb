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

        public static void QuitGame() { }

    }
}
