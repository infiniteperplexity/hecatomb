HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;

  function timeIt(name,callb) {
    console.time(name);
    callb();
    console.timeEnd(name);
  }

  HTomb.World.generate = {};
  let PADDING = 6;
  let NPARCELS = 11;
  let PSIZE = 22;
  HTomb.World.generate.parcels = HTomb.Utils.multiarray(NPARCELS,NPARCELS);
  HTomb.World.generate.elevation = HTomb.Utils.multiarray(LEVELW,LEVELH);
  HTomb.World.generate.rockiness = HTomb.Utils.multiarray(LEVELW,LEVELH);
  HTomb.World.generate.enoise;
  HTomb.World.generate.rnoise;

  let GROUNDLEVEL = 50;
  let SEALEVEL = 48;
  let SNOWLINE = 54;
  let ROCKINESS = 50;
  let TREECHANCE = -45;
  let SHRUBCHANCE = -35;
  let BOULDERCHANCE = -55;
  let LAVALEVEL = 10;
  let OCTAVES = [256,128,64,32,16,8,4,2];

  HTomb.World.generate.revised = function() {
    // assign random noise for elevation
    randomizeElevation();
    // assign random noise for rockiness
    randomizeRockiness();
    // assign random biomes to corners and modify elevation and rockiness
    randomizeBiomes();
    // finalize elevations
    finishElevation();
    // add slopes
    addSlopes();
    // add water but leave out aquifers for now
    waterTable();
    // add grass and snow
    groundCover();
    // randomly populate the map
    dealParcels();
    // fill in trees and boulders
    finishRockiness();
  };

  function randomizeBiomes() {
    let corners = HTomb.Utils.shuffle(["Mountains","Wastes","Forest","Ocean"]);
    let parcels = HTomb.World.generate.parcels = {
      CentralHills: [],
      InnerHills: [],
      OuterHills: [],
      BorderMountains: [],
      DeepMountains: [],
      BorderWastes: [],
      DeepWastes: [],
      BorderForest: [],
      DeepForest: [],
      BorderOcean: [],
      DeepOcean: []
    };
    let biome;
    parcels["CentralHills"].push([5,5]);
    parcels["InnerHills"].push(
      [4,4],[5,4],[6,4],
      [4,5],      [6,5],
      [4,6],[5,6],[6,6]
    );
    parcels["OuterHills"].push(
                        [5,0],
                        [5,1],
                  [4,2],[5,2],[6,2]
            [3,3],[4,3],[5,3],[6,3],[7,3],
            [2,4],[3,4],      [7,4],[8,4],
[0,5],[1,5],[2,5],[3,5],      [7,5],[8,5],[9,5],[10,5],
            [2,6],[3,6],      [7,6],[8,6],
            [3,7],[4,7],[5,7],[6,7],[7,7],
                  [4,8],[5,8],[6,8],
                        [5,9],
                        [5,10]
    );
    biome = corners[0];
    parcels["Deep"+biome].push(
      [0,0],[1,0],[2,0],[3,0],
      [0,1],[1,1],[2,1],[3,1],
      [0,2],[1,2],[2,2],
      [0,3],[1,3]
    );
    parcels["Border"+biome].push(
                  [4,0],
                  [4,1],
                [3,2],
              [2,3],
      [0,4],[1,4]
    );
    biome = corners[1];
    parcels["Deep"+biome].push(
      [7,0],[8,0],[9,0],[10,0],
      [7,1],[8,1],[9,1],[10,1],
            [8,2],[9,2],[10,2],
                  [9,3],[10,3]
    );
    parcels["Border"+biome].push(
      [6,0],
      [6,1],
        [7,2],
          [8,3],
            [9,4],[10,4]
    );
    biome = corners[3];
    parcels["Deep"+biome].push(
      [0,7],[1,7],
      [0,8],[1,8],[2,8],
      [0,9],[1,9],[2,9],[3,9],
      [0,10],[1,10],[2,10],[3,10]
    );
    parcels["Border"+biome].push(
    [0,6],[1,6],
            [3,7],
              [4,8],
                [5,9],
                [5,10]
    );
    biome = corners[2];
    parcels["Deep"+biome].push(
                  [9,7],[10,7],
            [0,8],[9,8],[10,8],
      [0,9],[1,9],[9,9],[10,9],
      [0,10],[1,10],[9,10],[10,10]
    );
    parcels["Border"+biome].push(
            [9,6],[10,6],
          [8,7],
        [7,8],
      [6,9],
      [6,10]
    );
    HTomb.Types[corners[0]].modify(1,1);
    HTomb.Types[corners[1]].modify(LEVELW-2,1);
    HTomb.Types[corners[2]].modify(LEVELW-2,LEVELH-2);
    HTomb.Types[corners[3]].modify(1,LEVELH-2);
  }

  function randomizeElevation() {
    let noise = HTomb.World.generate.enoise = new ROT.Noise.Simplex();
    let base = GROUNDLEVEL;
    let scales = [2,1,1,0.5,0,0,0,0];
    let grid = HTomb.World.generate.elevation;
    for (var x=1; x<LEVELW-1; x++) {
      for (var y=1; y<LEVELH-1; y++) {
        grid[x][y] = base;
        for (let o=0; o<OCTAVES.length; o++) {
          grid[x][y]+= noise.get(x/OCTAVES[o],y/OCTAVES[o])*scales[o];
        }
      }
    }
  }
  function finishElevation() {
    for (let x=1; x<LEVELW-1; x++) {
      for (let y=1; y<LEVELH-1; y++) {
        let z0 = parseInt(HTomb.World.generate.elevation[x][y]);
        z0 = Math.min(z0,NLEVELS-2);
        if (x>0 && x<LEVELW-1 && y>0 && y<LEVELH-1) {
          for (let z=z0; z>0; z--) {
            HTomb.World.tiles[z][x][y] = HTomb.Tiles.WallTile;
          }
          if (z0<NLEVELS-2) {
            HTomb.World.tiles[z0+1][x][y] = HTomb.Tiles.FloorTile;
          }
          HTomb.World.exposed[x][y] = z0+1;
        }
      }
    }
  }

  function addSlopes() {
    var tiles = HTomb.World.tiles;
    var squares;
    var square;
    var slope;
    for (var x=1; x<LEVELW-1; x++) {
      for (var y=1; y<LEVELH-1; y++) {
        for (var z=0; z<=NLEVELS-1; z++) {
          // handle the top level first
          if (tiles[z][x][y]===HTomb.Tiles.FloorTile && z===NLEVELS-1) {
            squares = HTomb.Tiles.neighboringColumns(x,y);
            slope = false;
            for (var i=0; i<squares.length; i++) {
              square = squares[i];
              if (tiles[z][square[0]][square[1]]===HTomb.Tiles.WallTile) {
                slope = true;
              }
            }
            if (slope===true) {
              tiles[z][x][y] = HTomb.Tiles.UpSlopeTile;
            }
            continue;
          }
          // handle normal elevation
          if (tiles[z][x][y]===HTomb.Tiles.FloorTile && tiles[z+1][x][y]===HTomb.Tiles.EmptyTile) {
            // this is kind of weird now right? could I use countNeighborsWhere?
            squares = HTomb.Tiles.neighboringColumns(x,y);
            slope = false;
            for (var i=0; i<squares.length; i++) {
              square = squares[i];
              if (tiles[z][square[0]][square[1]]===HTomb.Tiles.WallTile
                  && tiles[z+1][square[0]][square[1]]===HTomb.Tiles.FloorTile) {
                slope = true;
              }
            }
            if (slope===true) {
              tiles[z][x][y] = HTomb.Tiles.UpSlopeTile;
              tiles[z+1][x][y] = HTomb.Tiles.DownSlopeTile;
            }
          }
        }
      }
    }
  }

  function randomizeRockiness() {
    let noise = HTomb.World.generate.rnoise = new ROT.Noise.Simplex();
    let base = ROCKINESS;
    let scales = [0,0,0,10,10,10,0,0];
    let grid = HTomb.World.generate.rockiness;
    for (var x=1; x<LEVELW-1; x++) {
      for (var y=1; y<LEVELH-1; y++) {
        grid[x][y] = base;
        for (let o=0; o<OCTAVES.length; o++) {
          grid[x][y]+= noise.get(x/OCTAVES[o],y/OCTAVES[o])*scales[o];
        }
      }
    }
  }

  function finishRockiness() {;
    for (let x=1; x<LEVELW-1; x++) {
      for (let y=1; y<LEVELH-1; y++) {
        let z = HTomb.Tiles.groundLevel(x,y);
        if (HTomb.World.features[coord(x,y,z)]) {
          continue;
        }
        if (HTomb.World.covers[z][x][y]===HTomb.Covers.Grass || HTomb.World.covers[z][x][y]===HTomb.Covers.Snow) {
          let v = parseInt(HTomb.World.generate.rockiness[x][y]);
          let r = ROT.RNG.getUniformInt(0,99);
          if (r>v-TREECHANCE) {
            HTomb.Things.Tree.spawn().place(x,y,z);
          } else if (r>v-SHRUBCHANCE) {
            HTomb.Things.Shrub.spawn().place(x,y,z);
          } else if (r<v+BOULDERCHANCE) {
            if (ROT.RNG.getUniformInt(1,3)===1) {
              HTomb.Things.Rock.spawn({n: 1, owned: false}).place(x,y,z);
            } else {
              HTomb.Things.Boulder.spawn().place(x,y,z);
            }
          }
        } else if (HTomb.World.covers[z][x][y]===HTomb.Covers.Water) {
          let v = parseInt(HTomb.World.generate.rockiness[x][y]);
          let r = ROT.RNG.getUniformInt(0,99);
          if (v<=r) {
            HTomb.Things.Seaweed.spawn().place(x,y,z);
            // why no underwater boulders?
          }
        }
      }
    }
  }

  

  function waterTable() {
    for (let x=1; x<LEVELW-1; x++) {
      for (let y=1; y<LEVELH-1; y++) {
        for (let z=SEALEVEL; z>0; z--) {
          if (HTomb.World.tiles[z][x][y]!==HTomb.Tiles.WallTile) {
            HTomb.World.covers[z][x][y] = HTomb.Covers.Water;
          } else {
            break;
          }
        }
      }
    }
  }

  function groundCover() {
    var tiles = HTomb.World.tiles;
    var squares;
    var square;
    var slope;
    for (var x=1; x<LEVELW-1; x++) {
      for (var y=1; y<LEVELH-1; y++) {
        var z = HTomb.Tiles.groundLevel(x,y);
        if (HTomb.World.covers[z][x][y]===HTomb.Covers.NoCover) {
          if (z===SNOWLINE) {
            if (ROT.RNG.getUniform()<SNOWLINE-HTomb.World.generate.elevation[x][y]) {
              HTomb.World.covers[z][x][y] = HTomb.Covers.Grass;
            } else {
              HTomb.World.covers[z][x][y] = HTomb.Covers.Snow;
            }
          } else if (z>SNOWLINE) {
            HTomb.World.covers[z][x][y] = HTomb.Covers.Snow;
          } else {
            HTomb.World.covers[z][x][y] = HTomb.Covers.Grass;
          }
        }
      }
    }
  }

  let Parcel = HTomb.Types.Type.extend({
    template: "Parcel",
    name: "parcel",
    biomes: [],
    deal: function() {
      let parcels = [];
      for (let biome of this.biomes) {
        parcels = parcels.concat(HTomb.World.generate.parcels[biome]);
      }
      parcels = HTomb.Utils.shuffle(parcels);
      console.log(parcels);
      let i = parcels[0][0];
      let j = parcels[0][1];
      this.someMethod(PADDING+i*PSIZE,PADDING+j*PSIZE,PSIZE,PSIZE);
    },
    someMethod: function(x0,y0,w,h) {

    }
  });

  Parcel.extend({
    template: "PlayerParcel",
    name: "player parcel",
    biomes: ["CentralHills"],
    someMethod: function(x0,y0,w,h) {
      let n = 3;
      let xyz;
      let last;
      let mask = {};
      for (let i=0; i<n; i++) {
        let g = HTomb.Things.SmallGraveyard.spawn();
        xyz = g.findPlace(x0,y0,w,h,{mask: mask});
        if (xyz) {
          last = xyz;
          g.place(xyz.x,xyz.y,xyz.z);
          let addMask = HTomb.Tiles.squaresWithinSquare(xyz.x,xyz.y,xyz.z,3);
          for (let s of addMask) {
            mask[coord(s[0],s[1],s[2])] = true;
          }
        } else {
          alert("failure!");
        }
      }
      let p = HTomb.Things.Necromancer.spawn();
      let playerPlace = function(x,y,z) {
        let DIST = 6;
        if (Math.abs(x-last.x)<DIST && Math.abs(y-last.y)<DIST && z===last.z) {
          return true;
        } else {
          return false;
        }
      };
      xyz = p.findPlace(x0,y0,w,h,{validPlace: playerPlace});
      if (xyz) {
        p.place(xyz.x,xyz.y,xyz.z);
        HTomb.Things.Player.spawn().addToEntity(p);
      } else {
        alert("failure!");
      }
    }
  });

  let Footprint = HTomb.Things.Entity.extend({
    template: "Footprint",
    name: "footprint",
    place: function(x,y,z) {
      HTomb.Things.Entity.place.call(this,x,y,z);
      this.despawn();
    }
  });

  // could this be done as an encounter?
  Footprint.extend({
    template: "SmallGraveyard",
    name: "small graveyard",
    validPlace: function(x,y,z) {
      let nvalid = 0;
      let dirs = ROT.DIRS[4];
      for (let d of dirs) {
        if (this.validSquare(x+d[0],y+d[1],z)) {
          nvalid+=1;
        }
      }
      if (nvalid>=2) {
        return true;
      }
    },
    validSquare: function(x,y,z) {
      if (  HTomb.World.features[coord(x,y,z)]===undefined
            && HTomb.World.tiles[z][x][y]===HTomb.Tiles.FloorTile
            && HTomb.World.tiles[z-1][x][y]===HTomb.Tiles.WallTile
            && HTomb.Tiles.countNeighborsWhere(x,y,z, function(x1,y1,z1) {
              return (HTomb.World.tiles[z1-1][x1][y1]!==HTomb.Tiles.WallTile);
            })===0) {
        return true;
      } else {
        return false;
      }
    },
    onPlace: function(x,y,z) {
      let dirs = HTomb.Utils.copy(ROT.DIRS[4]);
      dirs = HTomb.Utils.shuffle(dirs);
      let graves = ROT.RNG.getUniformInt(2,4);
      let placed = 0;
      for (let i=0; i<4; i++) {
        if (placed>=graves) {
          break;
        }
        let d = dirs[i];
        let x1 = x+d[0];
        let y1 = y+d[1];
        if (this.validSquare(x1,y1,z)) {
          HTomb.Things.Tombstone.spawn().place(x1,y1,z);
          HTomb.Things.Corpse.spawn().place(x1,y1,z-1);
          HTomb.World.covers[z-1][x1][y1] = HTomb.Covers.Soil;
          placed+=1;
        }
      }
    }
  })

  function dealParcels() {
    // place the player and several graveyards first
    HTomb.Types.PlayerParcel.deal();
    let deck = [];
    for (let type of HTomb.Types.Parcel.types) {
      if (type.template!=="PlayerParcel") {
        for (let i=0; i<type.frequency; i++) {
          deck.push(type);
        }
      }
    }
    // for now;
    return;
    deck = HTomb.Utils.shuffle(deck);
    let nParcels = 100;
    for (let i=0; i<nParcels; i++) {
      deck[i].deal();
    }
  }

  function placeLava() {
    for (var x=1; x<LEVELW-1; x++) {
      for (var y=1; y<LEVELH-1; y++) {
        for (var z=LAVALEVEL; z>0; z--) {
          if (z<LAVALEVEL) {
            HTomb.World.tiles[z][x][y] = HTomb.Tiles.EmptyTile;
          }
          HTomb.World.covers[z][x][y] = HTomb.Covers.Lava;
        }
      }
    }
  }

  return HTomb;
})(HTomb);
