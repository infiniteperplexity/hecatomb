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
            //RightMargin = 2;
            RightMargin = 0;
        }

        public void ScrollUp()
        {
            if (SelectedMessage > 0)
            {
                SelectedMessage -= 1;
            }
        }

        public void ScrollDown()
        {
            int maxVisible = 4;
            if (SelectedMessage < OldGame.World.GetState<MessageHandler>().MessageHistory.Count - maxVisible)
            {
                SelectedMessage += 1;
            }
        }

        // this should actually be on the message handler
        public void PushMessage(ColoredText ct)
        {
            int MaxArchive = 100;
            var m = OldGame.World.GetState<MessageHandler>();
            m.MessageHistory.Insert(0, ct);
            while (OldGame.World.GetState<MessageHandler>().MessageHistory.Count > MaxArchive)
            {
                m.MessageHistory.RemoveAt(MaxArchive);
            }
            SelectedMessage = 0;
            m.Unread = true;
            if (ct.Colors.ContainsKey(0))
            {
                var order = new Dictionary<string, int> { 
                    ["white"] = 0, 
                    ["yellow"] = 1, 
                    ["magenta"] = 2,
                    ["orange"] = 3,
                    ["red"] = 4 
                };
                var c = ct.Colors[0];
                if (order.ContainsKey(c))
                {
                    if (order[c] > order[m.UnreadColor])
                    {
                        m.UnreadColor = c;
                    }
                }
            }
            InterfacePanel.DirtifySidePanels();

        }

        public override void Draw()
        {
            OldGame.Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
            var cp = OldGame.MenuPanel;
            var k = Keyboard.GetState();
            var lines = new List<ColoredText>();
            if (OldGame.World != null && !OldGame.Time.Frozen && OldGame.World.GetState<TutorialHandler>().Visible)
            {
                lines = lines.Concat(OldGame.World.GetState<TutorialHandler>().GetText()).ToList();
            }
            else
            {
                lines = lines.Concat(OldGame.Controls.MenuTop).ToList();
            }
            if (OldGame.Controls.MenuMiddle.Count > 0)
            {
                lines.Add(" ");
                lines = lines.Concat(OldGame.Controls.MenuMiddle).ToList();
            }
            if (OldGame.Controls.MenuBottom.Count > 0)
            {
                lines.Add(" ");
                lines = lines.Concat(OldGame.Controls.MenuBottom).ToList();
            }
            DrawLines(lines);
        }
    }
}
