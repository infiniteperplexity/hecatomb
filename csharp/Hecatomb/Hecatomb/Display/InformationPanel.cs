using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;

namespace Hecatomb
{

    public class InformationPanel : InterfacePanel
    {
        public bool ScrollState;
        public bool SummaryState;
        public bool HoverState;
        public bool TutorialState;

        public int SelectedMessage;
        public InformationPanel(int x, int y, int w, int h) : base(x, y, w, h)
        {
            LeftMargin = 2;
            RightMargin = 2;
        }

        public void ShowScroll()
        {
            ScrollState = true;
            SummaryState = false;
            HoverState = false;
            TutorialState = false;
            Dirty = true;
        }

        public void ShowSummary()
        {
            ScrollState = false;
            SummaryState = true;
            HoverState = false;
            TutorialState = false;
            Dirty = true;
        }

        public void ShowHover()
        {
            ScrollState = false;
            SummaryState = false;
            HoverState = true;
            TutorialState = false;
            Dirty = true;
        }

        public void ShowTutorial()
        {
            ScrollState = false;
            SummaryState = false;
            HoverState = false;
            TutorialState = true;
            Dirty = true;
        }

        public void ScrollUp()
        {
            if (SelectedMessage > 0)
            {
                SelectedMessage -= 1;
            }
            ShowScroll();
        }

        public void ScrollDown()
        {
            int maxVisible = 4;
            if (SelectedMessage < Game.World.GetState<MessageHandler>().MessageHistory.Count - maxVisible)
            {
                SelectedMessage += 1;
            }
            ShowScroll();
        }

        // this should actually be on the message handler
        public void PushMessage(ColoredText ct)
        {
            int MaxArchive = 100;
            Game.World.GetState<MessageHandler>().MessageHistory.Insert(0, ct);
            while (Game.World.GetState<MessageHandler>().MessageHistory.Count > MaxArchive)
            {
                Game.World.GetState<MessageHandler>().MessageHistory.RemoveAt(MaxArchive);
            }
            SelectedMessage = 0;
            ShowScroll();
        }

        public override void Draw()
        {
            Game.Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
            string txt;
            if (Game.World != null)
            {
                if (ScrollState)
                {
                    Creature p = Game.World.Player;
                    var (X, Y, Z) = Game.World.Player;
                    if (Game.Controls is CameraControls)
                    {
                        X = Game.Camera.XOffset + 12;
                        Y = Game.Camera.YOffset + 12;
                        Z = Game.Camera.Z;
                    }
                    TurnHandler t = Game.World.Turns;
                    string sanity = Game.World.Player.GetComponent<SpellCaster>().Sanity.ToString().PadLeft(3, '0') + '/' + Game.World.Player.GetComponent<SpellCaster>().GetCalculatedMaxSanity().ToString().PadLeft(3, '0');
                    string x = X.ToString().PadLeft(3, '0');
                    string y = Y.ToString().PadLeft(3, '0');
                    string z = Z.ToString().PadLeft(3, '0');
                    string paused = (Game.Time.PausedAfterLoad || Game.Time.AutoPausing) ? "{yellow}Paused" : "      ";
                    string time = "\u263C " + t.Day.ToString().PadLeft(4, '0') + ':' + t.Hour.ToString().PadLeft(2, '0') + ':' + t.Minute.ToString().PadLeft(2, '0');
                    txt = $"Sanity:{sanity}  X:{x} Y:{y} Z:{z} {time}   {paused}";
                    int MaxVisible = Math.Min(Game.World.GetState<MessageHandler>().MessageHistory.Count, 4);
                    List<ColoredText> list = new List<ColoredText> { txt };
                    list = list.Concat(Game.World.GetState<MessageHandler>().MessageHistory.GetRange(SelectedMessage, MaxVisible)).ToList();
                    if (list.Count > 1 && list[1].Colors.Count == 0)
                    {
                        list[1] = new ColoredText(list[1].Text, "cyan");
                    }
                    DrawLines(list);
                }
                else if (SummaryState)
                {
                    var lines = new List<ColoredText>();
                    if (Game.World?.Player != null)
                    {
                        lines.Add($"{Game.World.Player.Describe()}");
                        lines.Add($"Controls {Game.World.GetState<TaskHandler>().Minions.Count} minions.");
                        var stored = new List<Dictionary<string, int>>();
                        var structures = Structure.ListStructures();
                        foreach (Structure s in structures)
                        {
                            stored.Add(s.GetStored());
                        }
                        var total = Item.CombinedResources(stored);
                        if (total.Count > 0)
                        {
                            lines.Add(" ");
                            lines.Add("Stored resources:");
                            foreach (var res in total.Keys)
                            {
                                lines.Add("- " + Resource.Format((res, total[res])));
                            }
                        }
                        DrawLines(lines);
                    }
                }
                else if (HoverState)
                {

                }
                else if (TutorialState)
                {

                }
            }
        }
    }
    public class ScrollPanel : InterfacePanel
    {
        /* 
         * We can expect this panel to take on several states.
         * 1) Event log.
         * 2) Overview of minions and such
         * 3) Hover information.
         * 4) Potentially the tutorial as well.
         * 
         */
        public int SelectedMessage;
        public ScrollPanel(int x, int y, int w, int h) : base(x, y, w, h)
        {
            LeftMargin = 2;
            RightMargin = 2;
        }

        public void ScrollUp()
        {
            if (SelectedMessage > 0)
            {
                SelectedMessage -= 1;
            }
            Dirty = true;
        }

        public void ScrollDown()
        {
            int maxVisible = 4;
            if (SelectedMessage < Game.World.GetState<MessageHandler>().MessageHistory.Count - maxVisible)
            {
                SelectedMessage += 1;
            }
            Dirty = true;
        }

        // this should actually be on the message handler
        public void PushMessage(ColoredText ct)
        {
            int MaxArchive = 100;
            Game.World.GetState<MessageHandler>().MessageHistory.Insert(0, ct);
            while (Game.World.GetState<MessageHandler>().MessageHistory.Count > MaxArchive)
            {
                Game.World.GetState<MessageHandler>().MessageHistory.RemoveAt(MaxArchive);
            }
            SelectedMessage = 0;
        }


        public override void Draw()
        {
            Game.Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
            string txt;
            if (Game.World != null)
            {
                Creature p = Game.World.Player;
                var (X, Y, Z) = Game.World.Player;
                if (Game.Controls is CameraControls)
                {
                    X = Game.Camera.XOffset + 12;
                    Y = Game.Camera.YOffset + 12;
                    Z = Game.Camera.Z;
                }
                TurnHandler t = Game.World.Turns;
                string sanity = Game.World.Player.GetComponent<SpellCaster>().Sanity.ToString().PadLeft(3, '0') + '/' + Game.World.Player.GetComponent<SpellCaster>().GetCalculatedMaxSanity().ToString().PadLeft(3, '0');
                string x = X.ToString().PadLeft(3, '0');
                string y = Y.ToString().PadLeft(3, '0');
                string z = Z.ToString().PadLeft(3, '0');
                string paused = (Game.Time.PausedAfterLoad || Game.Time.AutoPausing) ? "{yellow}Paused" : "      ";
                string time = "\u263C " + t.Day.ToString().PadLeft(4, '0') + ':' + t.Hour.ToString().PadLeft(2, '0') + ':' + t.Minute.ToString().PadLeft(2, '0');
                txt = $"Sanity:{sanity}  X:{x} Y:{y} Z:{z} {time}   {paused}";
                int MaxVisible = Math.Min(Game.World.GetState<MessageHandler>().MessageHistory.Count, 4);
                List<ColoredText> list = new List<ColoredText> { txt };
                list = list.Concat(Game.World.GetState<MessageHandler>().MessageHistory.GetRange(SelectedMessage, MaxVisible)).ToList();
                if (list.Count > 1 && list[1].Colors.Count == 0)
                {
                    list[1] = new ColoredText(list[1].Text, "cyan");
                }
                DrawLines(list);
            }
        }
    }

    public class OtherMenuPanel : InterfacePanel
    {
        public OtherMenuPanel(int x, int y, int w, int h) : base(x, y, w, h)
        {
            LeftMargin = 2;
            RightMargin = 2;
        }
        public override void Draw()
        {
            if (Game.ForegroundPanel.Active)
            {
                // I don't know why this panel needs this and the others don't
                return;
            }
            Game.Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
            var lines = new List<ColoredText>();
            if (Game.World?.Player != null)
            {
                lines.Add($"{Game.World.Player.Describe()}");
                lines.Add($"Controls {Game.World.GetState<TaskHandler>().Minions.Count} minions.");
                var stored = new List<Dictionary<string, int>>();
                var structures = Structure.ListStructures();
                foreach (Structure s in structures)
                {
                    stored.Add(s.GetStored());
                }
                var total = Item.CombinedResources(stored);
                if (total.Count > 0)
                {
                    lines.Add(" ");
                    lines.Add("Stored resources:");
                    foreach (var res in total.Keys)
                    {
                        lines.Add("- " + Resource.Format((res, total[res])));
                    }
                }
                DrawLines(lines);
            }
        }
    }
}
