using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Threading;

namespace Hecatomb8
{

    public class InformationPanel : InterfacePanel
    {
        //public bool ScrollState;
        //public bool SummaryState;
        //public bool HoverState;
        //public bool TutorialState;

        public int SelectedMessage;
        public InformationPanel(GraphicsDevice g, SpriteBatch sb, ContentManager c, int x, int y) : base(g, sb, c, x, y)
        {
            LeftMargin = 2;
            //RightMargin = 2;
            RightMargin = 0;
        }

        public override void PrepareLines(List<ColoredText> lines, int leftMargin = 0, int topMargin = 0)
        {
            lines = lines.Concat(InterfaceState.Controls.MenuTop).ToList();
        }       
    }
    //public void ScrollUp()
    //{
    //    if (SelectedMessage > 0)
    //    {
    //        SelectedMessage -= 1;
    //    }
    //}

    //public void ScrollDown()
    //{
    //    int maxVisible = 4;
    //    if (SelectedMessage < Game.World.GetState<MessageHandler>().MessageHistory.Count - maxVisible)
    //    {
    //        SelectedMessage += 1;
    //    }
    //}

    // this should actually be on the message handler
    //public void PushMessage(ColoredText ct)
    //{
    //    int MaxArchive = 100;
    //    var m = Game.World.GetState<MessageHandler>();
    //    m.MessageHistory.Insert(0, ct);
    //    while (Game.World.GetState<MessageHandler>().MessageHistory.Count > MaxArchive)
    //    {
    //        m.MessageHistory.RemoveAt(MaxArchive);
    //    }
    //    SelectedMessage = 0;
    //    m.Unread = true;
    //    if (ct.Colors.ContainsKey(0))
    //    {
    //        var order = new Dictionary<string, int>
    //        {
    //            ["white"] = 0,
    //            ["yellow"] = 1,
    //            ["magenta"] = 2,
    //            ["orange"] = 3,
    //            ["red"] = 4
    //        };
    //        var c = ct.Colors[0];
    //        if (order.ContainsKey(c))
    //        {
    //            if (order[c] > order[m.UnreadColor])
    //            {
    //                m.UnreadColor = c;
    //            }
    //        }
    //    }
    //    InterfacePanel.DirtifySidePanels();
    //}

    //public override void Draw()
    //{
    //    Game.Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
    //    var cp = Game.MenuPanel;
    //    var k = Keyboard.GetState();
    //    var lines = new List<ColoredText>();
    //    if (Game.World != null && !Game.Time.Frozen && Game.World.GetState<TutorialHandler>().Visible)
    //    {
    //        lines = lines.Concat(Game.World.GetState<TutorialHandler>().GetText()).ToList();
    //    }
    //    else
    //    {
    //        lines = lines.Concat(Game.Controls.MenuTop).ToList();
    //    }
    //    if (Game.Controls.MenuMiddle.Count > 0)
    //    {
    //        lines.Add(" ");
    //        lines = lines.Concat(Game.Controls.MenuMiddle).ToList();
    //    }
    //    if (Game.Controls.MenuBottom.Count > 0)
    //    {
    //        lines.Add(" ");
    //        lines = lines.Concat(Game.Controls.MenuBottom).ToList();
    //    }
    //    DrawLines(lines);
    //}
}
