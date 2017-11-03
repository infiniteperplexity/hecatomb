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
  let ROCKINESS = 75;
  let TREECHANCE = 5;
  let SHRUBCHANCE = 10;
  let BOULDERCHANCE = 5;
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
    let parcels = HTomb.World.generate.parcels;
    let biome;
    parcels[0][5] = "OuterHills";
    parcels[1][5] = "OuterHills";
    parcels[2][5] = "OuterHills";
    parcels[3][5] = "OuterHills";
    parcels[4][5] = "InnerHills";
    parcels[5][5] = "CenterHills";
    parcels[6][5] = "InnerHills";
    parcels[7][5] = "OuterHills";
    parcels[8][5] = "OuterHills";
    parcels[9][5] = "OuterHills";
    parcels[10][5] = "OuterHills";
    parcels[5][0] = "OuterHills";
    parcels[5][1] = "OuterHills";
    parcels[5][2] = "OuterHills";
    parcels[5][3] = "OuterHills";
    parcels[5][4] = "InnerHills";
    parcels[5][6] = "InnerHills";
    parcels[5][7] = "OuterHills";
    parcels[5][8] = "OuterHills";
    parcels[5][9] = "OuterHills";
    parcels[5][10] = "OuterHills";
    biome = corners[0];
    parcels[0][0] = "Deep"+biome;
    parcels[1][0] = "Deep"+biome;
    parcels[2][0] = "Deep"+biome;
    parcels[3][0] = "Deep"+biome;
    parcels[4][0] = "Border"+biome;
    parcels[0][1] = "Deep"+biome;
    parcels[1][1] = "Deep"+biome;
    parcels[2][1] = "Deep"+biome;
    parcels[3][1] = "Deep"+biome;
    parcels[4][1] = "Border"+biome;
    parcels[0][2] = "Deep"+biome;
    parcels[1][2] = "Deep"+biome;
    parcels[2][2] = "Deep"+biome;
    parcels[3][2] = "Border"+biome;
    parcels[4][2] = "OuterHills";
    parcels[0][3] = "Deep"+biome;
    parcels[1][3] = "Deep"+biome;
    parcels[2][3] = "Border"+biome;
    parcels[3][3] = "OuterHills";
    parcels[4][3] = "OuterHills";
    parcels[0][4] = "Border"+biome;
    parcels[1][4] = "Border"+biome;
    parcels[2][4] = "OuterHills";
    parcels[3][4] = "OuterHills";
    parcels[4][4] = "InnerHills";
    biome = corners[1];
    parcels[6][0] = "Border"+biome;
    parcels[7][0] = "Deep"+biome;
    parcels[8][0] = "Deep"+biome;
    parcels[9][0] = "Deep"+biome;
    parcels[10][0] = "Deep"+biome;
    parcels[6][1] = "Border"+biome;
    parcels[7][1] = "Deep"+biome;
    parcels[8][1] = "Deep"+biome;
    parcels[9][1] = "Deep"+biome;
    parcels[10][1] = "Deep"+biome;
    parcels[6][2] = "OuterHills";
    parcels[7][2] = "Border"+biome;
    parcels[8][2] = "Deep"+biome;
    parcels[9][2] = "Deep"+biome;
    parcels[10][2] = "Deep"+biome;
    parcels[6][3] = "OuterHills";
    parcels[7][3] = "OuterHills";
    parcels[8][3] = "Border"+biome;
    parcels[9][3] = "Deep"+biome;
    parcels[10][3] = "Deep"+biome;
    parcels[6][4] = "Border"+biome;
    parcels[7][4] = "Border"+biome;
    parcels[8][4] = "InnerHills";
    parcels[9][4] = "OuterHills";
    parcels[10][4] = "OuterHills";
    biome = corners[3];
    parcels[0][6] = "Border"+biome;
    parcels[1][6] = "Border"+biome;
    parcels[2][6] = "OuterHills";
    parcels[3][6] = "OuterHills";
    parcels[4][6] = "InnerHills";
    parcels[0][7] = "Deep"+biome;
    parcels[1][7] = "Deep"+biome;
    parcels[2][7] = "Border"+biome;
    parcels[3][7] = "OuterHills";
    parcels[4][7] = "OuterHills";
    parcels[0][8] = "Deep"+biome;
    parcels[1][8] = "Deep"+biome;
    parcels[2][8] = "Deep"+biome;
    parcels[3][8] = "Border"+biome;
    parcels[4][8] = "OuterHills";
    parcels[0][9] = "Deep"+biome;
    parcels[1][9] = "Deep"+biome;
    parcels[2][9] = "Deep"+biome;
    parcels[3][9] = "Deep"+biome;
    parcels[4][9] = "Border"+biome;
    parcels[0][10] = "Deep"+biome;
    parcels[1][10] = "Deep"+biome;
    parcels[2][10] = "Deep"+biome;
    parcels[3][10] = "Deep"+biome;
    parcels[4][10] = "Border"+biome;
    biome = corners[2];
    parcels[6][6] = "Border"+biome;
    parcels[7][6] = "Border"+biome;
    parcels[8][6] = "InnerHills";
    parcels[9][6] = "OuterHills";
    parcels[10][6] = "OuterHills";
    parcels[6][7] = "OuterHills";
    parcels[7][7] = "OuterHills";
    parcels[8][7] = "Border"+biome;
    parcels[9][7] = "Deep"+biome;
    parcels[10][7] = "Deep"+biome;
    parcels[6][8] = "OuterHills";
    parcels[7][8] = "Border"+biome;
    parcels[8][8] = "Deep"+biome;
    parcels[9][8] = "Deep"+biome;
    parcels[10][8] = "Deep"+biome;
    parcels[6][9] = "Border"+biome;
    parcels[7][9] = "Deep"+biome;
    parcels[8][9] = "Deep"+biome;
    parcels[9][9] = "Deep"+biome;
    parcels[10][9] = "Deep"+biome;
    parcels[6][10] = "Border"+biome;
    parcels[7][10] = "Deep"+biome;
    parcels[8][10] = "Deep"+biome;
    parcels[9][10] = "Deep"+biome;
    parcels[10][10] = "Deep"+biome;
    HTomb.Types[corners[0]].modify(1,1);
    HTomb.Types[corners[1]].modify(LEVELW-2,1);
    HTomb.Types[corners[2]].modify(LEVELW-2,LEVELH-2);
    HTomb.Types[corners[0]].modify(1,LEVELH-2);
  }

  function randomizeElevation() {
    let noise = HTomb.World.generate.enoise = new ROT.Noise.Simplex();
    let base = GROUNDLEVEL;
    let scales = [2,1,1,0.5,0,0,0,0];
    let grid = HTomb.World.generate.elevation;
    for (var x=1; x<LEVELW; x++) {
      for (var y=1; y<LEVELH; y++) {
        grid[x][y] = base;
        for (let o=0; o<OCTAVES.length; o++) {
          grid[x][y]+= noise.get(x/OCTAVES[o],y/OCTAVES[o])*scales[o];
        }
      }
    }
  }
  function finishElevation() {
    for (let x=0; x<LEVELW-1; x++) {
      for (let y=0; y<LEVELH-1; y++) {
        let z0 = parseInt(HTomb.World.elevations[x][y]);
        z0 = Math.min(z0,NLEVELS-2);
        if (x>0 && x<LEVELW-1 && y>0 && y<LEVELH-1) {
          for (let z=z0; z>0; z--) {
            HTomb.World.tiles[z][x][y] = HTomb.Tiles.WallTile;
          }
          if (z0<NLEVELS-2) {
            HTomb.World.tiles[z0+1][x][y] = HTomb.Tiles.FloorTile;
          }
          HTomb.World.exposed[x][y] = z0+1;
          lowest = Math.min(lowest, z0+1);
        }
      }
    }
  }

  function addSlopes() {
    var tiles = HTomb.World.tiles;
    var squares;
    var square;
    var slope;
    for (var x=0; x<LEVELW; x++) {
      for (var y=0; y<LEVELH; y++) {
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
    for (var x=1; x<LEVELW; x++) {
      for (var y=1; y<LEVELH; y++) {
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
          if (v<=r) {
            HTomb.Things.Tree.spawn().place(x,y,z);
          } else if (v-SHRUBCHANCE<=r) {
            HTomb.Things.Shrub.spawn().place(x,y,z);
          } else if (BOULDERCHANCE+v<r) {
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
            HTomb.World.covers[z][x][y] = HTomb.Tiles.Water;
          } else {
            break;
          }
        }
      }
    }
  }

  function landCover() {
    var tiles = HTomb.World.tiles;
    var squares;
    var square;
    var slope;
    for (var x=1; x<LEVELW-1; x++) {
      for (var y=1; y<LEVELH-1; y++) {
        var z = HTomb.Tiles.groundLevel(x,y);
        if (HTomb.World.covers[z][x][y]===HTomb.Covers.NoCover) {
          if (z===SNOWLINE) {
            if (ROT.RNG.getUniform()<SNOWLINE-HTomb.World.generate.elevations[x][y]) {
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

    }
  });

  Parcel.extend({
    template: "PlayerParcel",
    name: "player parcel",
    biomes: ["CentralHills"],
    deal: function() {
      
    }
  });

  function dealParcels() {
    // place the player and several graveyards first
    HTomb.Types.PlayerParcel.deal() {

    }
    let deck = [];
    for (let type of HTomb.Types.Parcel.types) {
      if (type.template!=="PlayerParcel") {
        for (let i=0; i<type.frequency; i++) {
          deck.push(type);
        }
      }
    }
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
