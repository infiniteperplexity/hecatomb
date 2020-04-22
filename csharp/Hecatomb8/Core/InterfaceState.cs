using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

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
        public static ControlContext Controls { get => controls!;}
        public static DefaultControls? DefaultControls;
        public static Camera? Camera;
        public static Colors? Colors;
        public static HecatombCommands? Commands;
        public static HashSet<Coord> PlayerVisible = new HashSet<Coord>();
        //public static bool ReadyForInput;

        public static void HandleInput()
        {
            controls!.HandleInput();
        }
        public static void PreparePanels()
        {
            mainPanel!.Prepare();
            infoPanel!.Prepare();
        }
        public static void DrawInterfacePanels()
        {
            mainPanel!.Draw();
            infoPanel!.Draw();
        }

        public static void SetControls(ControlContext c)
        {
            controls = c;
        }
        public static void PlayerIsReady()
        {
            DirtifyMainPanel();
            DirtifyTextPanels();
            var p = GameState.World!.Player!;
            InterfaceState.Camera!.Center((int)p.X!, (int)p.Y!, (int)p.Z!);
            HandlePlayerVisibility();
            // may want to refactor this out
            //InterfaceState.ReadyForInput = true;
        }

        public static void DirtifyMainPanel()
        {
            InterfaceState.MainPanel.Dirty = true;
        }

        public static void DirtifyTextPanels()
        {
            InterfaceState.InfoPanel.Dirty = true;
        }
        public static void HandlePlayerVisibility()
        {
            //if (!Game.ForegroundPanel.Active)
            //{
            //    InterfacePanel.DirtifyUsualPanels();
            //    var m = Mouse.GetState();
            //    Controls?.HandleHover(m.X, m.Y);
            //}
            //TheFixer.CheckStates();
            //Game.World.ValidateLighting();
            //if (!(Controls is CameraControls))
            //{
            //    if (ControlContext.Selection is Creature)
            //    {
            //        Creature c = (Creature)ControlContext.Selection;
            //        Game.Camera.Center(c.X, c.Y, c.Z);
            //    }
            //    else
            //    {
            //        Game.Camera.Center(Player.X, Player.Y, Player.Z);
            //    }
            //}
            PlayerVisible = GameState.World!.Player!.GetComponent<Senses>().GetFOV();
            //foreach (Creature c in GetState<TaskHandler>().Minions)
            //{
            //    Senses s = c.GetComponent<Senses>();
            //    Game.Visible.UnionWith(s.GetFOV());
            //}
            foreach (var t in PlayerVisible)
            {
                GameState.World!.Explored.Add(t);
            }
        }
    }
}
