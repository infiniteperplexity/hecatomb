HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;

  HTomb.World.things = [];
  HTomb.World.tiles = HTomb.Utils.multiarray(NLEVELS,LEVELW,LEVELH);
  HTomb.World.explored = HTomb.Utils.multiarray(NLEVELS,LEVELW,LEVELH);
  HTomb.World.exposed = [];
  for (let x=0; x<LEVELW; x++) {
    HTomb.World.exposed.push([]);
  }
  HTomb.World.lit = HTomb.Utils.multiarray(NLEVELS,LEVELW,LEVELH);
  HTomb.World.lights = [];
  HTomb.World.visible = [];
  HTomb.World.creatures = [];
  HTomb.World.items = [];
  HTomb.World.features = [];
  HTomb.World.tasks = [];
  HTomb.World.encounters = [];
  HTomb.World.blocks = [];
  HTomb.World.covers = HTomb.Utils.multiarray(NLEVELS,LEVELW,LEVELH);
  HTomb.World.trackers = [];

  HTomb.World.reset = function() {
    HTomb.Things.Player.delegate = null;
    HTomb.Time.dailyCycle.reset();
    HTomb.Events.reset();
    HTomb.Achievements.reset();
    HTomb.Path.reset();
    HTomb.Tutorial.reset();
    HTomb.Types.Team.hostilityMatrix.reset();
    HTomb.World.validate.reset();
    while(HTomb.World.things.length>0) {
      HTomb.World.things.pop().despawn();  
    }
    HTomb.Things.Tracker.resetAll();
    var oldkeys;
    oldkeys = Object.keys(HTomb.World.creatures);
    for (let i=0; i<oldkeys.length; i++) {
      delete HTomb.World.creatures[oldkeys[i]];
    }
    //for (let key in HTomb.World.creatures) {
    //  delete HTomb.World.creatures[key];
    //}
    oldkeys = Object.keys(HTomb.World.features);
    for (let i=0; i<oldkeys.length; i++) {
      delete HTomb.World.features[oldkeys[i]];
    }
    oldkeys = Object.keys(HTomb.World.items);
    for (let i=0; i<oldkeys.length; i++) {
      delete HTomb.World.items[oldkeys[i]];
    }
    oldkeys = Object.keys(HTomb.World.tasks);
    for (let i=0; i<oldkeys.length; i++) {
      delete HTomb.World.tasks[oldkeys[i]];
    }
    oldkeys = Object.keys(HTomb.World.blocks);
    for (let i=0; i<oldkeys.length; i++) {
      delete HTomb.World.blocks[oldkeys[i]];
    }
    for (let i=0; i<HTomb.World.encounters.length; i++) {
      delete HTomb.World.encounters[i];
    }
    for (let i=0; i<HTomb.World.lights.length; i++) {
      delete HTomb.World.lights[i];
    }
    HTomb.World.fillTiles();
  };
  HTomb.World.init = function() {
    this.reset();
    HTomb.World.generate.revised();
    HTomb.World.validate.all();
    HTomb.Time.unlockTime();
  };
  // Add void tiles to the boundaries of the level
  HTomb.World.fillTiles = function() {
    for (var x=0; x<LEVELW; x++) {
      for (var y=0; y<LEVELH; y++) {
        for (var z=0; z<NLEVELS; z++) {
          if (x===0 || x===LEVELW-1 || y===0 || y===LEVELH-1 || z===0 || z===NLEVELS-1) {
            HTomb.World.tiles[z][x][y] = HTomb.Tiles.VoidTile;
            HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
          } else {
            HTomb.World.tiles[z][x][y] = HTomb.Tiles.EmptyTile;
            HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
          }
          HTomb.World.lit[z][x][y] = 0;
        }
      }
    }
  };
  // Run this to make sure the basic rules of adjacent terrain are followed

  HTomb.World.validate = {
    dirty: {},
    dirtyColumns: {},
    cleaned: {},
    cleanedColumns: {},
    lowestExposed: NLEVELS-2,
    trackNesting: 0
  };
  HTomb.World.validate.teams = function() {
    let teams = HTomb.Types.Team.types;
    let types = {};
    for (let i=0; i< teams.length; i++) {
      teams[i].members = [];
      types[teams[i].template] = teams[i];
    }
    for (let c in HTomb.World.creatures) {
      let cr = HTomb.World.creatures[c];
      if (cr.actor && cr.actor.team) {
        types[cr.actor.team].members.push(cr);
      }
    }
    HTomb.Types.Team.teams = types;
  };
  HTomb.World.validate.reset = function() {
    this.dirty = {};
    this.dirtyColumns = {};
    this.cleaned = {};
    this.cleanedColumns = {};
    this.lowestExposed = NLEVELS-2;
    this.trackNesting = 0;
    for (let x=0; x<HTomb.World.exposed.length; x++) {
      for (let y=0; y<HTomb.World.exposed[x].length; y++) {
        HTomb.World.exposed[x][y] = NLEVELS-2;
      }
    }
  };
  HTomb.World.validate.clean = function() {
    this.trackNesting+=1;
    if (this.trackNesting>1) {
      console.log("Note: Validation checks are running " + this.trackNesting + " layers deep.");
    }
    //lighting can only be done all at once?
    for (var crd in this.dirty) {
      if (this.cleaned[crd]) {
        continue;
      }
      let d = HTomb.Utils.decoord(crd);
      let x = d[0];
      let y = d[1];
      let z = d[2];
      this.dirtyColumns[coord(x,y,0)]=true;
      this.square(x,y,z);
    }
    for (let xy in this.dirtyColumns) {
      if (this.cleanedColumns[xy]) {
        continue;
      }
      let d = HTomb.Utils.decoord(xy);
      let x = d[0];
      let y = d[1];
      this.column(x,y);
    }
    if (Object.keys(this.dirtyColumns).length>0) {
      HTomb.World.validate.lighting(this.dirtyColumns);
    }
    this.dirty = {};
    this.dirtyColumns = {};
    this.cleaned = {};
    this.cleanedColumns = {};
    this.trackNesting = 0;
  };
  HTomb.World.validate.exposure = function(x,y) {
    let blocked = false;
    for (let z=NLEVELS-2; z>0; z--) {
      if (blocked===false && HTomb.World.tiles[z][x][y].zview!==-1) {
        blocked = true;
        HTomb.World.exposed[x][y] = z;
        if (z<this.lowestExposed) {
          this.lowestExposed = z;
        }
        return;
      }
    }
    HTomb.World.exposed[x][y] = 1;
    this.lowestExposed = 1;
  };
  HTomb.World.validate.square = function(x,y,z) {
    this.slopes(x,y,z);
    this.floors(x,y,z);
    this.falling(x,y,z);
    this.liquids(x,y,z);
    this.cleaned[coord(x,y,z)] = true;
  };
  HTomb.World.validate.column = function(x,y) {
    this.exposure(x,y);
    this.cleanedColumns[coord(x,y,0)] = true;
  };
  HTomb.World.validate.all = function() {
    this.dirty = {};
    for (var x=1; x<LEVELW-1; x++) {
      for (var y=1; y<LEVELH-1; y++) {
        for (var z=1; z<NLEVELS-1; z++) {
          this.square(x,y,z);
        }
        this.column(x,y);
      }
    }
    HTomb.World.validate.teams();
    HTomb.World.validate.lighting();
  };
  HTomb.World.validate.dirtify = function(x,y,z) {
    this.dirty[coord(x,y,z)] = true;
  };
  HTomb.World.validate.dirtyNeighbors = function(x,y,z) {
    this.dirtify(x,y,z);
    var dx;
    var dy;
    var dz;
    var dirs = HTomb.dirs[26];
    for (var i=0; i<dirs.length; i++) {
      dx = x+dirs[i][0];
      dy = y+dirs[i][1];
      dz = z+dirs[i][2];
      this.dirtify(dx,dy,dz);
    }
  }
  HTomb.World.validate.cleanNeighbors = function(x,y,z) {
    this.dirtyNeighbors(x,y,z);
    this.clean();
  }
  HTomb.World.validate.slopes = function(x,y,z) {
    // validate.all slopes
    var t = HTomb.World.tiles[z][x][y];
    if (t===HTomb.Tiles.UpSlopeTile) {
      if (HTomb.World.tiles[z+1][x][y].fallable===true) {
        HTomb.World.tiles[z+1][x][y] = HTomb.Tiles.DownSlopeTile;
      }
    } else if (t===HTomb.Tiles.DownSlopeTile) {
      t = HTomb.World.tiles[z-1][x][y];
      if (t!==HTomb.Tiles.UpSlopeTile) {
        if (t.solid) {
          HTomb.World.tiles[z][x][y] = HTomb.Tiles.FloorTile;
        } else {
          HTomb.World.tiles[z][x][y] = HTomb.Tiles.EmptyTile;
        }
      }
    }
  };
  HTomb.World.validate.floors = function(x,y,z) {
    if (HTomb.World.tiles[z][x][y]===HTomb.Tiles.EmptyTile && HTomb.World.tiles[z-1][x][y].solid) {
      HTomb.World.tiles[z][x][y] = HTomb.Tiles.FloorTile;
    }
  };
  HTomb.World.validate.falling = function(x,y,z) {
    if (HTomb.World.tiles[z][x][y].fallable) {
      var creature = HTomb.World.creatures[coord(x,y,z)];
      if (creature && creature.movement.flies!==true) {
        console.log(creature.describe() + " fell");
        creature.fall();
      }
      var items = HTomb.World.items[coord(x,y,z)] || [];
      while (items && items.length>0) {
        items.head().fall();
      }
    }
  };
  HTomb.World.validate.liquids = function(x,y,z) {
    var t = HTomb.World.covers[z][x][y];
    if (t===undefined) {
      console.log(x,y,z);
    }
    if (t.liquid) {
      t.liquid.flood(x,y,z);
    }
  };


  //callback is optional
  HTomb.World.creaturesWithin = function(x,y,z,r,callb) {
    var creatures = [];
    for (var c in HTomb.World.creatures) {
      var cr = HTomb.World.creatures[c];
      if (callb && callb(cr)===false) {
        continue;
      } else {
        if (HTomb.Path.distance(x,y,cr.x,cr.y) && Math.abs(z-cr.z)<=1) {
          creatures.push(cr);
        }
      }
    }
    return creatures;
  };

   HTomb.World.creaturesWithin = function(x,y,z,r,callb) {
     var creatures = [];
     for (var i=-r; i<=r; i++) {
       for (var j=-r; j<=r; j++) {
         var cr = HTomb.World.creatures[coord(x+i,y+j,z)];
         if (cr && (callb===undefined || callb(cr))) {
           creatures.push(cr);
         }
       }
     }
     return creatures;
   };



  return HTomb;
})(HTomb);
