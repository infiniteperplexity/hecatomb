using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class NoControls : ControlContext
    {
        public NoControls() : base()
        {
        }

        public override void RefreshContent()
        {
        }

        public override void ClickTile(Coord c)
        {
        }

        public override void HoverTile(Coord c)
        {

        }
    }
}
