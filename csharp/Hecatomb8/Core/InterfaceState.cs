using System;
using System.Diagnostics;

namespace Hecatomb8
{
    // a locator class for global interface stuff
    // what if we modified this only with a method called Update?
    static class InterfaceState
    {
        static MainPanel? mainPanel;
        public static MainPanel MainPanel { get => mainPanel!; set => mainPanel = value; }
        static InformationPanel? infoPanel;
        public static InformationPanel InfoPanel { get => infoPanel!; set => infoPanel = value; }
        static ControlContext? controls;
        public static ControlContext Controls { get => controls!; set => controls = value; }
        public static Camera? Camera;
        public static Colors? Colors;
        public static HecatombCommands? Commands;
        public static bool ReadyForInput;

        public static void HandleInput()
        {
            controls!.HandleInput();
        }
        public static void PrepareSprites()
        {
            mainPanel!.PrepareGlyphs();
        }
        public static void DrawInterfacePanels()
        {
            mainPanel!.Draw();
        }
        public static void PlayerIsReady()
        {
            var p = GameState.World!.Player!;
            InterfaceState.Camera!.Center((int)p.X!, (int)p.Y!, (int)p.Z!);
            // handle visibility
            InterfaceState.ReadyForInput = true;
        }
    }
}
