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
    using static HecatombAliases;

    public class InformationPanel : InterfacePanel
    {
        public InformationPanel(GraphicsDevice g, SpriteBatch sb, ContentManager c, int x, int y, int w, int h) : base(g, sb, c, x, y, w, h)
        {
            LeftMargin = 2;
            RightMargin = 0;
        }

        public override void Prepare()
        {
            var controls = InterfaceState.Controls;
            var lines = new List<ColoredText>();
            if (GameState.World != null && GetState<TutorialHandler>().Visible && !HecatombOptions.NoTutorial)
            {
                lines = lines.Concat(GetState<TutorialHandler>().GetText()).ToList();
            }
            else
            {
                lines = lines.Concat(controls.InfoTop).ToList();
            }
            if (controls.InfoMiddle.Count > 0)
            {
                lines.Add(" ");
                lines = lines.Concat(controls.InfoMiddle).ToList();
            }
            if (controls.InfoBottom.Count > 0)
            {
                lines.Add(" ");
                lines = lines.Concat(controls.InfoBottom).ToList();
            }
            PrepareLines(lines);
        }       
    }
}
