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

  // Define a generic tile
  let Tile = HTomb.Types.Type.extend({
    template: "Tile",
    name: "tile",
    symbol: " ",
    stringify: function() {
      return HTomb.Types[this.parent].types.indexOf(this);
    },
    specialTileName: function(x,y,z) {
      return this.name;
    }
  });

  // Define specific types of tiles
  Tile.extend({
    template: "VoidTile",
    name: "boundary",
    symbol: " ",
    opaque: true,
    solid: true,
    immutable: true
  });

  Tile.extend({
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
  Tile.extend({
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
  Tile.extend({
    template: "WallTile",
    name: "wall",
    symbol: "#",
    fg: WALLFG,
    opaque: true,
    solid: true,
    bg: WALLBG
  });

  Tile.extend({
    template: "UpSlopeTile",
    name: "upward slope",
    symbol: "\u02C4",
    fg: WALLFG,
    zview: +1,
    zmove: +1,
    bg: WALLBG
  });

  Tile.extend({
    template: "DownSlopeTile",
    name: "downward slope",
    symbol: "\u02C5",
    zview: -1,
    zmove: -1,
    fg: BELOWFG,
    bg: BELOWBG
  });

  let Tiles = HTomb.Tiles;

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
  // This is a deeply weird function...should it be "neighboringColumns?"
  Tiles.neighboringColumns = function(x,y,n) {
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
      if (HTomb.World.tiles[z]===undefined || HTomb.World.tiles[z][x]===undefined || HTomb.World.tiles[z][x][y]===undefined) {
        debugger;
      }
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
    let squares = HTomb.Tiles.touchableFrom(x0,y0,z0,options);
    for (let i=0; i<squares.length; i++) {
      let s = squares[i];
      squares[i] = HTomb.Utils.coord(s[0],s[1],s[2]);
    }
    if (squares.indexOf(HTomb.Utils.coord(x1,y1,z1))!==-1) {
      return true;
    }
    return false;
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

  HTomb.Tiles.canFindAll = function(x,y,z,ingredients, options) {
    if (HTomb.Debug.noingredients) {
      return true;
    }
    options = options || {};
    // should always be a creature
    let searcher = options.searcher || null;
    // used for tasks that have already claimed items
    let claimedItems = options.claimedItems || [];
    // usually true for minions
    let ownedOnly = (options.ownedOnly===false) ? false : true;
    // usually true for minions
    let respectClaims = (options.respectClaims===false) ? false : true;
    let items = [];
    for (let ingredient in ingredients) {
      let n = ingredients[ingredient];
      if (ownedOnly) {
        // search items the player owns
        for (let item of HTomb.Player.owner.ownedItems()) {
          if (item.template===ingredient) {
            items.push(item);
          }
        }
      } else {
        // search all things
        for (let thing of HTomb.World.things) {
          if (thing.template===ingredient) {
            items.push(thing);
          }
        }
      }
      // make a list of claimed items
      let claimed = claimedItems.map(function(e) {return e[0];});
      // now we have a list of candidate items
      for (let item of items) {
        // if the item is held by somebody else
        if (!item.isPlaced() && (!searcher || !searcher.inventory || searcher.inventory.items.contains(item)===false)) {
          continue;
        }
        // if the item is not reachable
        if (item.isPlaced() && !HTomb.Tiles.isReachableFrom(item.x,item.y,item.z,x,y,z,{
            searcher: searcher || undefined,
            searchee: item,
            canMove: (searcher) ? searcher.movement.canMove : undefined,
            searchTimeOut: 10,
            useLast: true
          })) {
          continue;
        }
        if (respectClaims) {
          // check claims status
          if (claimed.indexOf(item)!==-1) {
            n-=(item.n-item.claimed+claimedItems[claimed.indexOf(item)][1]);
          } else {
            n-=(item.n-item.claimed);
          }
        } else {
          n-=item.n;
        }
        // if we found enough, skip the rest
        if (n<=0) {
          break;
        }
      }
      // if we did not find enough, return false
      if (n>0) {
        return false;
      }
    }
    // if we got this far without failing, return true
    return true;
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


  HTomb.Tiles.getSquaresWithinRange = function(x,y,z,n,callb) {
    callb = callb || function(x1,y1,z1,n1) {
      z1 = HTomb.Tiles.groundLevel(x1,y1);
      if (HTomb.Path.quickDistance(x,y,z,x1,y1,z1)<=n1) {
        return [x1,y1,z1];
      } else {
        return null;
      }
    }
    let squares = [];
    for (let dx = -n; dx<n; dx++) {
      for (let dy = -n; dy<n; dy++) {
        for (let dz = -n; dz<n; dz++) {
          let x1 = x+dx;
          let y1 = y+dy;
          let z1 = z+dz;
          if (HTomb.Path.quickDistance(x,y,z,x1,y1,z1)<=n) {
            let s = callb(x1,y1,z1,n);
            if (s) {
              s = coord(s[0],s[1],s[2]);
              if (squares.indexOf(s)===-1) {
                squares.push(s);
              }
            }
          }
        }
      }
    }
    return squares.map(HTomb.Utils.decoord);
  };

  HTomb.Tiles.getRandomWithinRange = function(x,y,z,n,callb) {
    let squares = HTomb.Tiles.getSquaresWithinRange(x,y,z,n,callb);
    if (squares.length>0) {
      let r = ROT.RNG.getUniformInt(1,squares.length);
      return squares[r-1];
    }
    return null;
  };

  return HTomb;
})(HTomb);
