using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

// this is getting to be a bit of a God Object
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
        static ControlContext? ParentControls;
        public static DefaultControls? DefaultControls;
        public static CameraControls? CameraControls;
        public static Camera? Camera;
        public static Colors? Colors;
        public static HecatombCommands? Commands;
        public static HashSet<Coord> PlayerVisible = new HashSet<Coord>();
        public static ListArray3D<Particle>? Particles;
        public static List<ParticleEmitter> Emitters = new List<ParticleEmitter>();
        public static HashSet<Coord> OldDirtyTiles = new HashSet<Coord>();
        public static HashSet<Coord> NextDirtyTiles = new HashSet<Coord>();
        public static bool MovingCamera = false;
        public static Coord? Cursor;
        public static string CursorColor = "cyan";
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
            DirtifyMainPanel();
            DirtifyTextPanels();
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

        public static void DirtifyTile(int x, int y, int z)
        {
            NextDirtyTiles.Add(new Coord(x, y, z));
        }
        public static void DirtifyTile(Coord c)
        {
            NextDirtyTiles.Add(c);
        }

        public static void DirtifyMainPanel()
        {
            if (InterfaceState.MainPanel != null)
            {
                InterfaceState.MainPanel.Dirty = true;
            }
        }

        public static void DirtifyTextPanels()
        {
            if (InterfaceState.InfoPanel != null)
            {
                InterfaceState.InfoPanel.Dirty = true;
            }
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

        public static void RewindControls()
        {
            var old = Controls;
            controls = ParentControls;
            if (GameState.World!= null)
            {
                //Game.World.Events.Publish(new ContextChangeEvent() { Note = "Back", OldContext = old, NewContext = Game.Controls });
                //Game.World.Events.Publish(new TutorialEvent() { Action = "Cancel" });
            }
            // I have no idea why typecasting is needed here
            ParentControls = (MovingCamera) ? (ControlContext)CameraControls! : DefaultControls;
            Controls.RefreshContent();
            DirtifyMainPanel();
            DirtifyTextPanels();
            //Game.ForegroundPanel.Active = false;
        }

        public static void ResetControls()
        {
            //var old = Controls;
            //Game.Controls.CleanUp();
            //if (Game.ReconstructMode)
            //{
            //    Game.Controls = Game.ReconstructControls;
            //}
            //else if (LogMode)
            //{
            //    Game.Commands.ShowLog();
            //}
            //else 
            
            if (MovingCamera)
            {
                controls = CameraControls!;
            }
            else
            {
                controls = DefaultControls!;
            }
            //if (Game.World != null)
            //{
            //    Game.World.Events.Publish(new ContextChangeEvent() { Note = "Reset", OldContext = old, NewContext = Game.Controls });
            //    Game.World.Events.Publish(new TutorialEvent() { Action = "Cancel" });
            //}
            ParentControls = Controls;
            Controls.RefreshContent();
            DirtifyMainPanel();
            DirtifyTextPanels();       
            //Game.SplashPanel.Active = false;
            //Game.ForegroundPanel.Active = false;
            //Game.Time.Frozen = false;
        }

        public static InterfacePanel? GetPanel(int x, int y)
        {
            //if SplashPanel is active, return that

            InterfacePanel[] panels = new InterfacePanel[]
            {
                MainPanel,
                InfoPanel
            };
            foreach (var panel in panels)
            {
                if (panel.X0 <= x && panel.Y0 <= y && panel.X0 + panel.PixelWidth > x && panel.Y0 + panel.PixelHeight > y)
                {
                    return panel;
                }
            }
            return null;
        }

        public static void CenterCursor()
        {
            Cursor = new Coord(Camera!.XOffset + Camera.Width / 2, Camera.YOffset + Camera.Height / 2, Camera.Z);
        }

        public static void HideCursor()
        {
            Cursor = null;
        }
    }
}
