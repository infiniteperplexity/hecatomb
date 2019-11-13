/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/8/2018
 * Time: 1:08 PM
 */
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace Hecatomb
{
    // this may or may not be used ultimately; it's a shell for showing tile hover details when the tutorial is active
    public class ExamineTileControls : ControlContext
    {
        Coord tile;

        public ExamineTileControls(Coord c) : base()
        {
            tile = c;
            RefreshContent();
        }

        public override void RefreshContent()
        {
            MenuTop = Game.World.GetTileDetails(tile);
        }

        public override void HoverTile(Coord c)
        {
            Reset();
        }
    }
}
