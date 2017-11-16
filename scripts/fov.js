// The FOV submodule contains vision algorithms, which should be highly optimized
HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;
  var decoord = HTomb.Utils.decoord;
  var grid;
  var x0,y0,z0,r0;

  var passlight = function(x,y) {
      //constrain to the grid
      if (x<=0 || x>=LEVELW-1 || y<=0 || y>=LEVELH-1) {
        return false;
      }
      //curve the edges
      if (Math.sqrt((x-x0)*(x-x0)+(y-y0)*(y-y0)) > r0) {
        return false;
      }
      //only opaque tiles block light
      //if this ever changes use a different FOV
      if (grid[x][y].opaque===true) {
        return false;
      }
      let f = HTomb.World.features[coord(x,y,z0)];
      if (f && f.opaque===true) {
        return false;
      }
      return true;
  };

  var show = function(x,y,r,v) {
    var visible = HTomb.World.visible;
    var explored = HTomb.World.explored;
    //!!!Experimental
    var tiles = HTomb.World.tiles;
    visible[coord(x,y,z0)] = true;
    explored[z0][x][y] = true;
    if (tiles[z0+1][x][y].zview===-1) {
      explored[z0+1][x][y] = true;
      visible[coord(x,y,z0+1)] = true;
    }
    if (grid[x][y].zview===-1) {
      explored[z0-1][x][y] = true;
      visible[coord(x,y,z0-1)] = true;
    }
  };

  var caster = new ROT.FOV.PreciseShadowcasting(passlight);

  HTomb.FOV.resetVisible = function() {
    var visible = HTomb.World.visible;
    for (var crd in visible) {
      delete visible[crd];
    }
  };
  HTomb.FOV.findVisible = function(x,y,z,r) {
    x0 = x;
    y0 = y;
    r0 = r;
    z0 = z;
    grid = HTomb.World.tiles[z];
    caster.compute(x,y,r,show);
  };

  var darkest = 64;
  // we could make this even faster by resetting only ambient light and handling point lights dynamically
  HTomb.FOV.resetLight = function(coords, zLevel) {
    zLevel = zLevel || 1;
    if (!coords) {
      for (let x=1; x<LEVELW-1; x++) {
        for (let y=1; y<LEVELH-1; y++) {
          for (let z=zLevel; z<NLEVELS-1; z++) {
            HTomb.World.lit[z][x][y] = 0;
          }
        }
      }
    } else {
      for (let crd in coords) {
        let c = decoord(crd);
        let x = c[0];
        let y = c[1];
        for (let z=zLevel; z<NLEVELS-1; z++) {
          HTomb.World.lit[z][x][y] = 0;
        }
      }
    }
  };

  var darkest = 64;
  HTomb.FOV.ambientLight = function(light, coords) {
    let e = HTomb.World.exposed;
    if (!coords) {
      for (let x=1; x<LEVELW-1; x++) {
        for (let y=1; y<LEVELH-1; y++) {
          for (let z=HTomb.World.exposed[x][y]; z<NLEVELS-1; z++) {
            HTomb.World.lit[z][x][y] = Math.max(HTomb.World.lit[z][x][y],light);
            HTomb.World.lit[z][x+1][y] = Math.max(HTomb.World.lit[z][x+1][y],light)
            HTomb.World.lit[z][x-1][y] = Math.max(HTomb.World.lit[z][x-1][y],light)
            HTomb.World.lit[z][x][y+1] = Math.max(HTomb.World.lit[z][x][y+1],light)
            HTomb.World.lit[z][x][y-1] = Math.max(HTomb.World.lit[z][x][y-1],light)
          }
        }
      }
    } else {
      for (let crd in coords) {
        let c = decoord(crd);
        let x = c[0];
        let y = c[1];
        for (let z=HTomb.World.validate.lowestExposed; z<NLEVELS-1; z++) {
          if (z>=HTomb.World.exposed[x][y]) {
            HTomb.World.lit[z][x][y] = Math.max(HTomb.World.lit[z][x][y],light);
          } else {
            for (let i=0; i<4; i++) {
              let dx = x+ROT.DIRS[4][i][0];
              let dy = y+ROT.DIRS[4][i][1];
              if (z >= HTomb.World.exposed[dx][dy]) {
                HTomb.World.lit[z][x][y] = Math.max(HTomb.World.lit[z][x][y],light);
                break;
                if (i===3) {
                  HTomb.World.lit[z][x][y] = Math.max(HTomb.World.lit[z][x][y],darkest);
                }
              }
            }
          }
        }
      }
    }
  };

  var lightLevel = 255;
  HTomb.FOV.discreteLights = function() {
    for (var l=0; l<HTomb.World.lights.length; l++) {
      var light = HTomb.World.lights[l];
      light.illuminate();
   }
  };

  // we need some way for the light to fade over time...
  HTomb.FOV.pointIlluminate = function(x,y,z,r) {
    x0 = x;
    y0 = y;
    r0 = r;
    z0 = z;
    grid = HTomb.World.tiles[z];
    caster.compute(x,y,r,light);
  }

  function light(x,y,r,v) {
    var d = Math.sqrt((x-x0)*(x-x0)+(y-y0)*(y-y0));
    var thisLevel = (r) ? Math.max(lightLevel-(d*10),1) : lightLevel;
    HTomb.World.lit[z0][x][y] = Math.max(HTomb.World.lit[z0][x][y],thisLevel);
    if (HTomb.World.tiles[z0+1][x][y].zview===-1) {
      HTomb.World.lit[z0+1][x][y] = Math.max(HTomb.World.lit[z0+1][x][y],thisLevel);
    }
    if (grid[x][y].zview===-1) {
      HTomb.World.lit[z0-1][x][y] = Math.max(HTomb.World.lit[z0-1][x][y],thisLevel);
    }
  }


  HTomb.World.validate.lighting = function(coords, z) {
    HTomb.FOV.resetLight(coords, z)
    HTomb.FOV.ambientLight(HTomb.Time.dailyCycle.lightLevel(),coords);
    HTomb.FOV.discreteLights(coords);
  };

  HTomb.FOV.shade = function(color,x,y,z) {
    var light = (HTomb.Debug.lit) ? 255 : HTomb.World.lit[z][x][y];
    // We could check whether the critter is on the player's team here instead...
    // that would give more flexibility for shading
    var c = ROT.Color.fromString(color);
    c[0] = Math.max(0,Math.round(c[0]-255+light));
    c[1] = Math.max(0,Math.round(c[1]-255+light));
    c[2] = Math.max(0,Math.round(c[2]-255+light));
    c = ROT.Color.toHex(c);
    return c;
  };

  return HTomb;
})(HTomb);
