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

  var Tiles = HTomb.Tiles;
  // Define a generic tile
  HTomb.Types.define({
    template: "Tile",
    name: "tile",
    symbol: " ",
    stringify: function() {
      return HTomb.Types.templates[this.parent].types.indexOf(this);
    },
    specialTileName: function(x,y,z) {
      return this.name;
    }
  });

  // Define specific types of tiles
  HTomb.Types.defineTile({
    template: "VoidTile",
    name: "boundary",
    symbol: " ",
    opaque: true,
    solid: true,
    immutable: true
  });

  HTomb.Types.defineTile({
    template: "EmptyTile",
    name: "empty",
    //symbol: "\u25CB",
    //symbol: "\u25E6",
    symbol: "\u2024",
    zview: -1,
    //fg: BELOW,
    fg: TWOBELOWFG,
    //bg: HTomb.Constants.BELOWBG,
    bg: "black",
    fallable: true,
    specialTileName: function(x,y,z) {
      if (HTomb.World.covers[z-1][x][y].liquid && !HTomb.World.covers[z][x][y].liquid) {
        return "surface";
      } else {
        return this.name;
      }
    }
  });
  HTomb.Types.defineTile({
    template: "FloorTile",
    name: "floor",
    symbol: ".",
    fg: FLOORFG,
    bg: FLOORBG,
    specialTileName: function(x,y,z) {
      if (HTomb.World.explored[z+1][x][y] && HTomb.World.tiles[z+1][x][y].zview!==-1) {
        return "roofed floor";
      } else {
        return this.name;
      }
    }
  });
  HTomb.Types.defineTile({
    template: "WallTile",
    name: "wall",
    symbol: "#",
    fg: WALLFG,
    opaque: true,
    solid: true,
    bg: WALLBG
  });

  HTomb.Types.defineTile({
    template: "UpSlopeTile",
    name: "upward slope",
    symbol: "\u02C4",
    fg: WALLFG,
    zview: +1,
    zmove: +1,
    bg: WALLBG
  });

  HTomb.Types.defineTile({
    template: "DownSlopeTile",
    name: "downward slope",
    symbol: "\u02C5",
    zview: -1,
    zmove: -1,
    fg: BELOWFG,
    bg: BELOWBG
  });

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
        return (bg || WALLBG);
      } else {
        // otherwise paint the tile black
        return (bg || "black");
      }
    }
    // look for explicit highlight colors
    if (HTomb.World.creatures[crd] && HTomb.World.creatures[crd].highlightColor) {
      return HTomb.World.creatures[crd].highlightColor;
    } else if (HTomb.World.features[crd] && HTomb.World.features[crd].highlightColor) {
      return HTomb.World.features[crd].highlightColor;
    }
    // *********** Choose the background color *******************************
    if (covers[z][x][y].liquid && tile.solid!==true) {
      if (vis) {
        bg = bg || covers[z][x][y].shimmer();
      } else {
        bg = bg || covers[z][x][y].darken();
      }
    } else if (features[crd] && features[crd].bg) {
      bg = bg || features[crd].bg;
    } else if (zview===-1 && covers[z-1][x][y]!==HTomb.Covers.NoCover && !covers[z-1][x][y].solid && tiles[z-1][x][y].solid!==true) {
      if (vis && covers[z-1][x][y].liquid) {
        bg = bg || covers[z-1][x][y].shimmer();
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
          } else {
            sym = tile.symbol;
          }
        }
      } else if (covers[z][x][y].mineral && tile===Tiles.WallTile) {
        sym = covers[z][x][y].symbol;
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


  HTomb.Tiles.getSquare = function(x,y,z) {
    var square = {};
    var crd = HTomb.Utils.coord(x,y,z);
    try {
    if (HTomb.World.tiles[z][x]===undefined) {
      console.log("weird thing going on, tried to examine square: "+x +", " + y +", "+z);
    }} catch(e) {HTomb.Debug.myError = e;}
    square.terrain = HTomb.World.tiles[z][x][y];
    square.creature = HTomb.World.creatures[crd];
    square.items = HTomb.World.items[crd];
    square.feature = HTomb.World.features[crd];
    square.task = HTomb.World.tasks[crd];
    square.cover = HTomb.World.covers[z][x][y];
    square.explored = HTomb.World.explored[z][x][y];
    square.visible = HTomb.World.visible[crd];
    //square.visible = HTomb.World.visible[z][x][y];
    // until we get the real code in place...
    if (z>=1) {
      square.visibleBelow = (square.visible && square.terrain.zview===-1) || HTomb.Debug.visible || false;
      square.exploredBelow = ((square.explored && square.terrain.zview===-1) || HTomb.World.explored[z-1][x][y]) || HTomb.Debug.explored || false;
    } else {
      square.visibleBelow = false;
      square.exploredBelow = false;
    }
    if (z<NLEVELS-1) {
      square.visibleAbove = (square.visible && (square.terrain.zview===+1 || HTomb.World.tiles[z+1][x][y].zview===-1)) || HTomb.Debug.visible || false;
      square.exploredAbove = ((square.explored && (square.terrain.zview===+1 || HTomb.World.tiles[z+1][x][y].zview===-1)) || HTomb.World.explored[z+1][x][y]) || HTomb.Debug.explored || false;
    } else {
      square.visibleAbove = false;
      square.exploredAbove = false;
    }
    square.x = x;
    square.y = y;
    square.z = z;
    return square;
  };


  function defaultPassable(x,y,z) {
    let t = HTomb.World.tiles[z][x][y];
    return (t.solid===undefined && t.fallable===undefined);
  }

  Tiles.randomEmptyNeighbor = function(x,y,z) {
    var d = [
      [ 0, -1],
      [ 1, -1],
      [ 1,  0],
      [ 1,  1],
      [ 0,  1],
      [-1,  1],
      [-1,  0],
      [-1, -1]
    ].randomize();
    var square;
    for (var j=0; j<d.length; j++) {
      square = HTomb.Tiles.getSquare(x+d[j][0],y+d[j][1],z);
      if (square.terrain.solid===undefined && square.creature===undefined) {
        return [x+d[j][0],y+d[j][1],z];
      }
    }
    return false;
  };
  Tiles.fill = function(x,y,z) {
    // check for more stuff in a while
    if (HTomb.World.features[coord(x,y,z)]) {
      HTomb.World.features[coord(x,y,z)].despawn();
    }
    HTomb.World.tiles[z][x][y] = HTomb.Tiles.WallTile;
    if (HTomb.World.tiles[z+1][x][y]===HTomb.Tiles.EmptyTile) {
      HTomb.World.tiles[z+1][x][y] = HTomb.Tiles.FloorTile;
    }
    HTomb.World.validate.cleanNeighbors(x,y,z);
  };
  // I actually hate the way this works
  Tiles.excavate = function(x,y,z,options) {
    options = options || {};
    if (HTomb.World.tiles[z][x][y]===HTomb.Tiles.VoidTile) {
      HTomb.GUI.pushMessage("Can't dig here!");
      return;
    }
    // If the ceiling is removed and there no solid tile above...
    if (options.removeCeiling===true && HTomb.World.tiles[z+1][x][y].solid!==true) {
      HTomb.World.tiles[z+1][x][y] = HTomb.Tiles.EmptyTile;
    }
    // Check whether there is a solid tile below...
    if (HTomb.World.tiles[z-1][x][y].solid!==true) {
      HTomb.World.tiles[z][x][y] = HTomb.Tiles.EmptyTile;
    } else {
      HTomb.World.tiles[z][x][y] = HTomb.Tiles.FloorTile;
    }
    HTomb.World.validate.cleanNeighbors(x,y,z);
  };
  Tiles.neighbors = function(x,y,n) {
    n = n||8;
    var squares = [];
    var dirs = ROT.DIRS[n];
    var x1, y1;
    for (var i=0; i<n; i++) {
      x1 = x+dirs[i][0];
      y1 = y+dirs[i][1];
      if (x1>=0 && x1<LEVELW && y1>=0 && y1<LEVELH) {
        squares.push([x1,y1]);
      }
    }
    return squares;
  };
  Tiles.groundLevel = function(x,y,zoption) {
    zoption = NLEVELS-2 || zoption;
    for (var z=zoption; z>0; z--) {
      if (HTomb.World.tiles[z][x][y].fallable!==true && HTomb.World.tiles[z][x][y].zmove!==-1) {
        return z;
      }
    }
  };

  Tiles.countNeighborsWhere = function(x,y,z,callb) {
    var dirs = ROT.DIRS[8];
    var x1, y1;
    var tally = 0;
    for (var i=0; i<8; i++) {
      x1 = x+dirs[i][0];
      y1 = y+dirs[i][1];
      if (x1>=0 && x1<LEVELW && y1>=0 && y1<LEVELH) {
        if (callb(x1,y1,z)===true) {
          tally+=1;
        }
      }
    }
    return tally;
  };
  Tiles.getNeighborsWhere = function(x,y,z,callb) {
    var dirs = ROT.DIRS[8];
    var x1, y1;
    var squares = [];
    for (var i=0; i<8; i++) {
      x1 = x+dirs[i][0];
      y1 = y+dirs[i][1];
      if (x1>=0 && x1<LEVELW && y1>=0 && y1<LEVELH) {
        if (callb(x1,y1,z)===true) {
          squares.push([x1,y1,z]);
        }
      }
    }
    return squares;
  };
  Tiles.getNeighbors = function(x,y,z) {
    var dirs = ROT.DIRS[8];
    var x1, y1;
    var neighbors = [];
    for (var i=0; i<8; i++) {
      x1 = x+dirs[i][0];
      y1 = y+dirs[i][1];
      if (x1>=0 && x1<LEVELW && y1>=0 && y1<LEVELH) {
        neighbors.push([x1,y1,z]);
      }
    }
    return neighbors;
  };
  Tiles.explore = function(x,y,z) {
    HTomb.World.explored[z][x][y] = true;
  };

  // any tile that can be touched by a worker from a square
  HTomb.Tiles.touchableFrom = function(x,y,z,options) {
    options = options || {};
    var touchable = [];
    //sideways
    var t, x1, y1;
    for (var i=0; i<ROT.DIRS[8].length; i++) {
      x1 = x+ROT.DIRS[8][i][0];
      y1 = y+ROT.DIRS[8][i][1];
      touchable.push([x1,y1,z]);
      t = HTomb.World.tiles[z][x1][y1];
      if (t.zmove===-1 || t.fallable) {
        touchable.push([x1,y1,z-1]);
      }
    }
    t = HTomb.World.tiles[z][x][y];
    if (t.zmove===+1) {
      touchable.push([x,y,z+1]);
    } else if (t.zmove===-1 || t.fallable) {
      touchable.push([x,y,z-1]);
    }
    return touchable;
  };
  HTomb.Tiles.isTouchableFrom = function(x1,y1,z1,x0,y0,z0,options) {
    options = options || {};
    if(x1===x0 && y1===y0 && z1===z0) {
      return true;
    }
    if (HTomb.Utils.arrayInArray([x1,y1,z1],HTomb.Tiles.touchableFrom(x0,y0,z0,options))>-1) {
      return true;
    } else {
      return false;
    }
  };
  HTomb.Tiles.isReachableFrom = function(x1,y1,z1,x0,y0,z0, options) {
    if (HTomb.Tiles.isTouchableFrom(x1,y1,z1,x0,y0,z0, options)) {
      return true;
    }
    options = options || {};
    options.useLast = options.useLast || false;
    var path = HTomb.Path.aStar(x0,y0,z0,x1,y1,z1,options);
    if (path!==false) {
      return true;
    } else {
      return false;
    }
  };

  HTomb.Tiles.isEnclosed = function(x,y,z,callb) {
    callb = callb || defaultPassable;
    var dirs = ROT.DIRS[8];
    for (let i=0; i<dirs.length; i++) {
      let dx = x+dirs[i][0];
      let dy = y+dirs[i][1];
      if (callb(dx,dy,z,x,y,z)) {
        return false;
      }
    }
    if (HTomb.World.tiles[z+1][x][y].zmove===-1) {
      if (callb(x,y,z+1,x,y,z)) {
        return false;
      }
    }
    if (HTomb.World.tiles[z-1][x][y].zmove===+1) {
      if (callb(x,y,z-1,x,y,z)) {
        return false;
      }
    }
    return true;
  };

  HTomb.Tiles.getTileDummy = function(x,y,z) {
    let e = {
      x: x,
      y: y,
      z: z,
      spawnId: "xyz"+coord(x,y,z),
      isPlaced: function() {return true;}
    }
    return e;
  };
  //
  HTomb.Tiles.squaresWithinSquare = function(x,y,z,n) {
    var squares = [];
    for (var i=-n; i<=n; i++) {
      for (var j=-n; j<=n; j++) {
        squares.push([x+i,y+j,z]);
      }
    }
    return squares;
  };
  return HTomb;
})(HTomb);
