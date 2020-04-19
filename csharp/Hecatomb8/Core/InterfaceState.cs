using System;
using System.Diagnostics;

namespace Hecatomb8
{
    // a locator class for global interface stuff
    // what if we modified this only with a method called Update?
    static class InterfaceState
    {
        static GamePanel? mainPanel;
        public static GamePanel MainPanel { get => mainPanel!; set => mainPanel = value; }
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
    }
}
