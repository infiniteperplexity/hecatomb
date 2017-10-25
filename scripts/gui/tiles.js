HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var FLOORFG = HTomb.Constants.FLOORFG;
  var SHADOWFG = HTomb.Constants.SHADOWFG;
  var WALLFG = HTomb.Constants.WALLFG;
  var BELOWFG = HTomb.Constants.BELOWFG;
  var WALLBG = HTomb.Constants.WALLBG;
  var FLOORBG = HTomb.Constants.FLOORBG;
  var BELOWBG = HTomb.Constants.BELOWBG;
  var TWOBELOWFG = HTomb.Constants.TWOBELOWFG;
  var coord = HTomb.Utils.coord;
  
  let Tiles = HTomb.Tiles;


  Tiles.getSymbol = function(x,y,z) {
    var glyph = Tiles.getGlyph(x,y,z);
    var bg = Tiles.getBackground(x,y,z);
    return [glyph[0],glyph[2],bg];
  }
  Tiles.getBackground = function(x,y,z) {
    if (x<0 || y<0 || z<0 || x>=LEVELW || y>=LEVELH || z>=NLEVELS) {
      return "black";
    }
    var crd = HTomb.Utils.coord(x,y,z);
    var cbelow = HTomb.Utils.coord(x,y,z-1);
    var covers = HTomb.World.covers;
    var creatures = HTomb.World.creatures;
    var features = HTomb.World.features;
    var items = HTomb.World.items;
    var tasks = HTomb.World.tasks;
    var visible = HTomb.World.visible;
    var explored = HTomb.World.explored;
    var tiles = HTomb.World.tiles;
    var tile = tiles[z][x][y];
    var zview = tiles[z][x][y].zview;
    var vis = (visible[crd]===true || HTomb.Debug.visible===true);
    var bg;
    if (tasks[crd]!==undefined && tasks[crd].assigner===HTomb.Player) {
      bg = tasks[crd].bg;
    }
    // ****** If the square has not been explored... ****************
    if (!explored[z][x][y] && HTomb.Debug.explored!==true) {
      // unexplored tiles with an explored floor tile above are rendered as non-visible wall tiles
      if (tiles[z+1][x][y]===Tiles.FloorTile && explored[z+1][x][y]) {
        if (covers[z][x][y].earth && covers[z+1][x][y]===HTomb.Covers.NoCover) {
          //!!!!Arguable.  Maybe only if you've stripped the grass?
          return (bg || covers[z][x][y].bg);
        } else {
          return (bg || WALLBG);
        }
      } else {
        // otherwise paint the tile black
        return (bg || "black");
      }
    }
    // look for explicit highlight colors
    let itembg = null;
    if (HTomb.World.items[crd]) {
      let pile = HTomb.World.items[crd];
      for (let item of pile) {
        if (item.claimed>0) {
          itembg = "#DDBB99";
        }
      }
    }
    if (HTomb.World.creatures[crd] && HTomb.World.creatures[crd].highlightColor) {
      return HTomb.World.creatures[crd].highlightColor;
    } else if (HTomb.World.features[crd] && HTomb.World.features[crd].highlightColor) {
      return HTomb.World.features[crd].highlightColor;
    } else if (itembg) {
      return itembg;
    } else if (HTomb.GUI.Panels.gameScreen.highlitTiles[crd]) {
      return HTomb.GUI.Panels.gameScreen.highlitTiles[crd];
    }
    // *********** Choose the background color *******************************
    if (covers[z][x][y].liquid && tile.solid!==true) {
      if (vis) {
        bg = bg || covers[z][x][y].liquid.shimmer();
      } else {
        bg = bg || covers[z][x][y].darken();
      }
    } else if (features[crd] && features[crd].bg) {
      bg = bg || features[crd].bg;
    } else if (zview===-1 && covers[z-1][x][y]!==HTomb.Covers.NoCover && !covers[z-1][x][y].solid && tiles[z-1][x][y].solid!==true) {
      if (vis && covers[z-1][x][y].liquid) {
        bg = bg || covers[z-1][x][y].liquid.shimmer();
      //!!!!!} else if (!covers[z-1][x][y].solid) {
      } else if (!covers[z-1][x][y].earth) {
        bg = bg || covers[z-1][x][y].darken();
      }
    } else if (creatures[crd] && creatures[crd].bg) {
      bg = bg || creatures[crd].bg;
    } else if (features[crd] && features[crd].bg) {
      bg = bg || features[crd].bg;
    } else if (items[crd] && items[crd].tail().bg) {
      bg = bg || items[crd].tail().bg;
    } else if (zview===-1 && tiles[z-1][x][y].zview===-1 && tiles[z-2][x][y].solid!==true
      && covers[z-2][x][y]!==HTomb.Covers.NoCover && covers[z-2][x][y].liquid) {
      bg = bg || covers[z-2][x][y].darken();
    } else if (covers[z][x][y]!==HTomb.Covers.NoCover) {
      bg = bg || covers[z][x][y].bg;
    }
    // ** An empty tile with an explored floor below...
    if (zview===-1 && HTomb.World.tiles[z-1][x][y]===Tiles.FloorTile && explored[z-1][x][y]) {
      bg = bg || BELOWBG;
    }
    // ** Otherwise, use the tile background
    bg = bg || tile.bg;
    return bg;
  };

  Tiles.getGlyph = function(x,y,z) {
    if (x<0 || y<0 || z<0 || x>=LEVELW || y>=LEVELH || z>=NLEVELS) {
      return [" ","black","black"];
    }
    var crd = HTomb.Utils.coord(x,y,z);
    var cabove = HTomb.Utils.coord(x,y,z+1);
    var cbelow = HTomb.Utils.coord(x,y,z-1);
    var tiles = HTomb.World.tiles;
    var creatures = HTomb.World.creatures;
    var items = HTomb.World.items;
    var features = HTomb.World.features;
    var covers = HTomb.World.covers;
    var tasks = HTomb.World.tasks;
    var visible = HTomb.World.visible;
    var explored = HTomb.World.explored;
    var tile = tiles[z][x][y];
    var zview = tiles[z][x][y].zview;
    var vis = (visible[crd]===true || HTomb.Debug.visible===true);
    var visa = (visible[cabove]===true);
    var visb = (visible[cbelow]===true);
    var sym, fg, shade;
    if (!explored[z][x][y] && HTomb.Debug.explored!==true) {
      // unexplored tiles with an explored floor tile above are rendered as non-visible wall tiles
      if (tiles[z+1][x][y]===Tiles.FloorTile && explored[z+1][x][y]) {
        return [Tiles.WallTile.symbol,SHADOWFG,SHADOWFG];
      } else {
        // otherwise paint the tile black
        return [" ","black","black"];
      }
    }
    if (vis===false) {
      fg = SHADOWFG;
      shade = SHADOWFG;
    }
    //*** Symbol and foreground color
    if (creatures[crd] && vis) {
      sym = creatures[crd].symbol;
      fg = fg || creatures[crd].fg;
    } else if (zview===+1 && creatures[cabove] && (vis || visa)) {
      sym = creatures[cabove].symbol;
      fg = fg || WALLFG;
    } else if (zview===-1 && creatures[cbelow] && (vis || visb)) {
      sym = creatures[cbelow].symbol;
      if (covers[z-1][x][y]!==HTomb.Covers.NoCover && covers[z-1][x][y].liquid) {
        fg = fg || covers[z-1][x][y].fg;
      } else {
        fg = fg || BELOWFG;
      }
    } else if (items[crd]) {
      sym = items[crd].tail().symbol;
      fg = fg || items[crd].tail().fg;
    } else if (features[crd]) {
      sym = features[crd].symbol;
      fg = fg || features[crd].fg;
    } else if (zview===+1 && items[cabove]) {
      sym = items[cabove].tail().symbol;
      fg = fg || WALLFG;
    } else if (zview===-1 && items[cbelow]) {
      sym = items[cbelow].tail().symbol;
      if (zview===-1 && covers[z-1][x][y]!==HTomb.Covers.NoCover && covers[z-1][x][y].liquid) {
        fg = fg || covers[z-1][x][y].fg;
      } else {
        fg = fg || BELOWFG;
      }
    } else if (zview===+1 && features[cabove]) {
      sym = features[cabove].symbol;
      fg = fg || WALLFG;
    // ** Can't see features down through liquids? or maybe we should color it with the liquid instead?
    } else if (zview===-1 && features[cbelow]) {
      sym = features[cbelow].symbol;
      if (covers[z-1][x][y]!==HTomb.Covers.NoCover && covers[z-1][x][y].liquid) {
        fg = fg || covers[z-1][x][y].fg;
      } else {
        fg = fg || BELOWFG;
      }
    } else {
      // *** if the square is empty except for cover, handle the symbol and color separately. ***
      if (covers[z][x][y]!==HTomb.Covers.NoCover) {
        if (covers[z][x][y].liquid || tile===Tiles.FloorTile) {
          fg = fg || covers[z][x][y].fg;
        } else if (covers[z][x][y].mineral) {
          fg = fg || covers[z][x][y].fg;
        } else {
          fg = fg || tile.fg;
        }
      // maybe do show the waterlogged ground?
      } else if (covers[z-1][x][y]!==HTomb.Covers.NoCover && covers[z-1][x][y].liquid && (tile.solid!==true && tile.zview!==+1)) {
        fg = fg || covers[z-1][x][y].fg;
      } else if (covers[z-1][x][y].earth && tile.solid!==true && tile.zview!==+1) {
        // Show the *background* color of the cover below as the foreground
        fg = fg || covers[z-1][x][y].bg
      } else {
        fg = fg || tile.fg;
      }
      // *** symbol ****
      if (tile===Tiles.FloorTile && explored[z-1][x][y] && tiles[z-1][x][y].solid!==true) {
        // explored tunnel below
        sym = "\u25E6";
      } else if ((tile===Tiles.FloorTile || tile===Tiles.EmptyTile) && tiles[z+1][x][y]!==Tiles.EmptyTile) {
        // roof above
        sym = "'";
      } else if (covers[z][x][y]!==HTomb.Covers.NoCover && tile.solid!==true) {
        if (covers[z][x][y].liquid) {
          if (zview===-1 && covers[z-1][x][y]!==HTomb.Covers.NoCover && covers[z-1][x][y].liquid && tiles[z-1][x][y].zmove!==+1) {
          // deeper liquid
            sym = "\u2235";
          } else {
          // submerged liquid
            sym = tile.symbol;
          }
        } else {
          // !!!Experimental - show upward slopes using grass color
          if (tile===Tiles.FloorTile) {
            // non-liquid cover
            sym = covers[z][x][y].symbol;
            // !!!!! Okay...here is where things get weird.
          } else {
            sym = tile.symbol;
          }
        }
      //!!!!} else if (covers[z][x][y].solid && tile===Tiles.WallTile) {
      } else if ((covers[z][x][y].mineral || covers[z][x][y].earth) && tile===Tiles.WallTile) {
        sym = covers[z][x][y].symbol || "#";
      } else if (zview===-1 && covers[z-1][x][y]!==HTomb.Covers.NoCover && covers[z-1][x][y].liquid) {
        // liquid surface
        sym = covers[z-1][x][y].symbol;
      } else {
        // ordinary tile
        sym = tile.symbol;
      }
    }
    sym = sym || "X";
    fg = fg || "white";
    if (creatures[crd]) {
      if (creatures[crd]===HTomb.Player) {
        return [sym,fg,fg];
      }
      if (creatures[crd].minion && creatures[crd].minion.master===HTomb.Player) {
        return [sym,fg,fg];
      }
    }
    shade = shade || HTomb.FOV.shade(fg,x,y,z);
    if (HTomb.Fonts.lookupSymbol[sym]===undefined) {
      if (HTomb.Fonts.charFound(sym,HTomb.Constants.FONTFAMILY)) {
        HTomb.Fonts.lookupSymbol[sym] = sym;
      } else {
        HTomb.Fonts.lookupSymbol[sym] = HTomb.Fonts.getBackup(sym);
      }
    }
    sym = HTomb.Fonts.lookupSymbol[sym];
    return [sym,fg,shade];
  };

  return HTomb;
})(HTomb);
