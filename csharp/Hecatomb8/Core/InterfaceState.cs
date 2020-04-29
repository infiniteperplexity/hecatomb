using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

// this is getting to be a bit of a God Object
namespace Hecatomb8
{
    // a locator class for global interface stuff
    // what if we modified this only with a method called Update?
    using static HecatombAliases;
    static class InterfaceState
    {
        static MainPanel? mainPanel;
        public static MainPanel MainPanel { get => mainPanel!; set => mainPanel = value; }
        static InformationPanel? infoPanel;
        public static InformationPanel InfoPanel { get => infoPanel!; set => infoPanel = value; }
        static MenuPanel? menuPanel;
        public static MenuPanel MenuPanel { get => menuPanel!; set => menuPanel = value; }
        static FullScreenPanel? foregroundPanel;
        public static FullScreenPanel ForegroundPanel { get => foregroundPanel!; set => foregroundPanel = value; }
        static PopupPanel? popupPanel;
        public static PopupPanel PopupPanel { get => popupPanel!; set => popupPanel = value; }
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
            if (foregroundPanel!.Active)
            {
                foregroundPanel!.Prepare();
            }
            else if (popupPanel!.Active)
            {
                popupPanel!.Prepare();
            }
            else
            {
                mainPanel!.Prepare();
                infoPanel!.Prepare();
                menuPanel!.Prepare();
            }
        }

        public static void DrawInterfacePanels()
        {
            if (foregroundPanel!.Active)
            {
                foregroundPanel!.Draw();
            }
            else if (popupPanel!.Active)
            {
                popupPanel!.Draw();
            }
            else
            {
                mainPanel!.Draw();
                infoPanel!.Draw();
                menuPanel!.Draw();
            }          
        }

        public static void SetControls(ControlContext c)
        {
            if (Controls != null)
            {
                controls!.CleanUp();
            }
            ParentControls = controls;
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
            if (InterfaceState.MenuPanel != null)
            {
                InterfaceState.MenuPanel.Dirty = true;
            }
        }
        public static void HandlePlayerVisibility()
        {
            if (GameState.World is null || GameState.World.Player is null || !GameState.World.Player.Placed)
            {
                return;
            }
            //if (!Game.ForegroundPanel.Active)
            //{
            //    InterfacePanel.DirtifyUsualPanels();
            //    var m = Mouse.GetState();
            //    Controls?.HandleHover(m.X, m.Y);
            //}
            //TheFixer.CheckStates();
            //Game.World.ValidateLighting();
            if (!(Controls is CameraControls))
            {
                if (Controls.SelectedEntity is Creature && Controls.SelectedEntity.Placed)
                {
                    Creature c = (Creature)Controls.SelectedEntity;
                    Camera!.Center((int)c.X!, (int)c.Y!, (int)c.Z!);
                }
                else
                {
                    var (x, y, z) = GameState.World!.Player!;
                    Camera!.Center((int)x!, (int)y!, (int)z!);
                }
            }
            PlayerVisible = GameState.World!.Player!.GetComponent<Senses>().GetFOV();
            foreach (Creature cr in GetState<TaskHandler>().GetMinions())
            {
                Senses s = cr.GetComponent<Senses>();
                PlayerVisible.UnionWith(s.GetFOV());
            }
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
                Publish(new ContextChangeEvent() { Note = "Back", OldContext = old, NewContext = Controls });
                Publish(new TutorialEvent() { Action = "Cancel" });
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
            var old = Controls;
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
            foregroundPanel!.Active = false;
            popupPanel!.Active = false;
            if (MovingCamera)
            {
                controls = CameraControls!;
            }
            else
            {
                controls = DefaultControls!;
            }
            if (GameState.World != null)
            {
                Publish(new ContextChangeEvent() { Note = "Reset", OldContext = old, NewContext = Controls });
                Publish(new TutorialEvent() { Action = "Cancel" });
            }
            ParentControls = Controls;
            Controls.RefreshContent();
            DirtifyMainPanel();
            DirtifyTextPanels();
            MainPanel.PrepareGlyphs();
            InfoPanel.Prepare();
            //Game.Time.Frozen = false;
        }

        public static InterfacePanel? GetPanel(int x, int y)
        {
            //if SplashPanel is active, return that

            InterfacePanel[] panels = new InterfacePanel[]
            {
                MainPanel,
                InfoPanel,
                MenuPanel
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

        public static void Splash(List<ColoredText> lines, Action? callback = null, bool fullScreen = false, ColoredText? logText = null)
        {
            var splash = new SplashControls();
            splash.SplashText = lines;
            splash.MyCallback = callback;
            splash.IsFullScreen = fullScreen;
            // push a message to the log
            SetControls(splash);
            if (fullScreen)
            {
                InterfaceState.ForegroundPanel.Active = true;
                InterfaceState.ForegroundPanel.Dirty = true;
            }
            else
            {
                InterfaceState.PopupPanel.Active = true;
                InterfaceState.PopupPanel.Dirty = true;
            }
        }

        public static void CenterCameraOnSelection()
        {
            if (Controls.SelectedEntity != null && Controls.SelectedEntity.Placed)
            {
                var (x, y, z) = Controls.SelectedEntity;
                Camera!.Center((int)x!, (int)y!, (int)z!);
                Cursor = new Coord((int)x!, (int)y!, (int)z!);
            }
        }

        public static List<ColoredText> GetTileDetails(Coord c)
        {
            int x = c.X;
            int y = c.Y;
            int z = c.Z;
            int za = z + 1;
            int zb = z - 1;
            Coord above = new Coord(x, y, za);
            Coord below = new Coord(x, y, zb);
            string main = "light cyan";
            string other = "gainsboro";
            int change = 0;
            List<ColoredText> text = new List<ColoredText>() {
                "Tile: " + x + "," + y + ", " + z,
                " "
            };
            TileEntity? t;
            if (Explored.Contains(c) || HecatombOptions.Explored)
            {
                text.Add("Terrain: " + Terrains.GetWithBoundsChecked(x, y, z).Name);
                if (Covers.GetWithBoundsChecked(x, y, z) != Cover.NoCover)
                {
                    text.Add("Cover: " + Covers.GetWithBoundsChecked(x, y, z).Name);
                }
                //text.Add("Lighting: " + GetLighting(x, y, z));
                t = Creatures.GetWithBoundsChecked(x, y, z);
                if (t != null)
                {
                    text.Add("Creature: " + t.Describe());
                }
                t = Features.GetWithBoundsChecked(x, y, z);
                if (t != null)
                {
                    text.Add("Feature: " + t.Describe());
                }
                t = Items.GetWithBoundsChecked(x, y, z);
                if (t != null)
                {
                    text.Add("Item: " + t.Describe());
                }
                t = Tasks.GetWithBoundsChecked(x, y, z);
                if (t != null)
                {
                    //text.Add("Task: " + t.Describe(article: false));
                    text.Add("Task: " + t.Describe() + " " + Resource.Format((t as Task)!.Ingredients));
                }
                text.Add(" ");
            }
            else if (Tasks.GetWithBoundsChecked(x, y, z) != null)
            {
                t = Tasks.GetWithBoundsChecked(x, y, z);
                if (t != null)
                {
                    text.Add("Task: " + t.Describe() + " " + Resource.Format((t as Task)!.Ingredients)); ;
                }
            }
            change = text.Count;
            if (Explored.Contains(above) || HecatombOptions.Explored)
            {
                text.Add("Above: " + Terrains.GetWithBoundsChecked(x, y, za).Name);
                if (Covers.GetWithBoundsChecked(x, y, za) != Cover.NoCover)
                {
                    text.Add("Cover: " + Covers.GetWithBoundsChecked(x, y, za).Name);
                }
                //text.Add("Lighting: " + GetLighting(x, y, za));
                t = Creatures.GetWithBoundsChecked(x, y, za);
                if (t != null)
                {
                    text.Add("Creature: " + t.Describe());
                }
                t = Features.GetWithBoundsChecked(x, y, za);
                if (t != null)
                {
                    text.Add("Feature: " + t.Describe());
                }
                t = Items.GetWithBoundsChecked(x, y, za);
                if (t != null)
                {
                    text.Add("Item: " + t.Describe());
                }
                t = Tasks.GetWithBoundsChecked(x, y, za);
                if (t != null)
                {
                    text.Add("Task: " + t.Describe() + " " + Resource.Format((t as Task)!.Ingredients));
                }
                text.Add(" ");
            }
            else if (Tasks.GetWithBoundsChecked(x, y, za) != null)
            {
                t = Tasks.GetWithBoundsChecked(x, y, za);
                if (t != null)
                {
                    text.Add("Above: " + t.Describe() + " " + Resource.Format((t as Task)!.Ingredients));
                }
            }
            if (Explored.Contains(below) || HecatombOptions.Explored)
            {
                text.Add("Below: " + Terrains.GetWithBoundsChecked(x, y, zb).Name);
                if (Covers.GetWithBoundsChecked(x, y, zb) != Cover.NoCover)
                {
                    text.Add("Cover: " + Covers.GetWithBoundsChecked(x, y, zb).Name);
                }
                //text.Add("Lighting: " + GetLighting(x, y, zb));
                t = Creatures.GetWithBoundsChecked(x, y, zb);
                if (t != null)
                {
                    text.Add("Creature: " + t.Describe());
                }
                t = Features.GetWithBoundsChecked(x, y, zb);
                if (t != null)
                {
                    text.Add("Feature: " + t.Describe());
                }
                t = Items.GetWithBoundsChecked(x, y, zb);
                if (t != null)
                {
                    text.Add("Item: " + t.Describe());
                }
                t = Tasks.GetWithBoundsChecked(x, y, zb);
                if (t != null)
                {
                    text.Add("Task: " + t.Describe() + " " + Resource.Format((t as Task)!.Ingredients));
                }
                text.Add(" ");
            }
            else if (Tasks.GetWithBoundsChecked(x, y, zb) != null)
            {
                t = Tasks.GetWithBoundsChecked(x, y, zb);
                if (t != null)
                {
                    text.Add("Below: " + t.Describe() + " " + Resource.Format((t as Task)!.Ingredients));
                }
            }
            for (int i = 0; i < text.Count; i++)
            {
                text[i].Colors[0] = (i < change) ? main : other;
            }
            return text;
        }
        public static void ShowTileDetails(Coord c)
        {
            var text = GetTileDetails(c);
            Controls.InfoBottom = text;
            DirtifyTextPanels();
        }
    }
}
