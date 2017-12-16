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

  let NPARCELS = 11;
  HTomb.World.generate = {};
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
    timeIt("elevation",randomizeElevation);
    //randomizeElevation();
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
    // rock strata
    rockStrata();
    // place ores
    placeOres();
    // place lava
    placeLava();
    // place caverns
    timeIt("cavesn",generateCaverns);
    //generateCaverns();
    // randomly populate the map
    timeIt("parcels",dealParcels);
    //dealParcels();
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
                  [4,2],[5,2],[6,2],
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

  function rockStrata() {
    let SOIL = 1;
    let LIMESTONE = 5;
    let BASALT = 12;
    let GRANITE = 12;
    let BEDROCK = 64;
    let layers = [];
    for (let i=0; i<SOIL; i++) {
      layers.push(HTomb.Covers.Soil);
    }
    for (let i=0; i<LIMESTONE; i++) {
      layers.push(HTomb.Covers.Limestone);
    }
    for (let i=0; i<BASALT; i++) {
      layers.push(HTomb.Covers.Basalt);
    }
    for (let i=0; i<GRANITE; i++) {
      layers.push(HTomb.Covers.Granite);
    }
    for (let i=0; i<BEDROCK; i++) {
      layers.push(HTomb.Covers.Bedrock);
    }
    for (let x=1; x<LEVELW-1; x++) {
      for (let y=1; y<LEVELH-1; y++) {
        let z = HTomb.Tiles.groundLevel(x,y)-1;
        for (let i=0; z-i>0; i++) {
          HTomb.World.covers[z-i][x][y] = layers[i];
          // how do we place out-of-depth rock types?
          if (ROT.RNG.getUniformInt(1,16)===1) {
            if (i<SOIL) {
              HTomb.World.covers[z-i][x][y] = HTomb.Covers.Limestone;
            } else if (i<SOIL+LIMESTONE) {
              HTomb.World.covers[z-i][x][y] = HTomb.Covers.Basalt;
            } else {
              HTomb.World.covers[z-i][x][y] = layers[i+BASALT];
            }
          }
        }
      }
    }
  }


  
  function dealParcels() {
    // place the player and several graveyards first
    HTomb.Types.PlayerParcel.deal();
    let deck = [];
    for (let type of HTomb.Types.Parcel.types) {
      if (type.template!=="PlayerParcel" && type.cavernMin===null && type.cavernMax===null) {
        for (let i=0; i<type.frequency; i++) {
          deck.push(type);
        }
      }
    }
    // for now;
    deck = HTomb.Utils.shuffle(deck);
    let nParcels = Math.min(100,deck.length);
    for (let i=0; i<nParcels; i++) {
      if (deck[i].logme) {
        console.log("dealing "+deck[i].describe());
      }
      deck[i].deal();
    }
    let caverns = [];
    for (let type of HTomb.Types.Parcel.types) {
      if (type.cavernMin!==null && type.cavernMax!==null) {
        for (let i=0; i<type.frequency; i++) {
          caverns.push(type);
        }
      }
    }
    caverns = HTomb.Utils.shuffle(caverns);
    let cParcels = Math.min(100,caverns.length);
    for (let i=0; i<cParcels; i++) {
      if (caverns[i].logme) {
        console.log("dealing "+caverns[i].describe());
      }
      //caverns[i].deal();
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

  function placeOres() {
    let NVEINS = 512;
    let SEGMAX = 6;
    let SEGMIN = 3;
    let SEGLEN = 2;
    let CURVE = 1;
    let noise = new ROT.Noise.Simplex();
    let ores = {
      Soil: {FlintCluster: 3, CoalSeam: 2},
      Limestone: {CopperVein: 3, TinVein: 1, CoalSeam: 2},
      Basalt: {IronVein: 3, SilverVein: 1},
      Granite: {TitaniumVein: 1, CobaltVein: 1, GoldVein: 1, IronVein: 1, SilverVein: 1},
      Bedrock: {AdamantVein: 1, UraniumVein: 1, TitaniumVein: 1, CobaltVein: 1}
    };
    for (let z=1; z<NLEVELS-1; z++) {
      for (let i=0; i<NVEINS; i++) {
        let x0 = ROT.RNG.getUniformInt(1,LEVELW-2);
        let y0 = ROT.RNG.getUniformInt(1,LEVELH-2);
        let cover = HTomb.World.covers[z][x0][y0];
        if (!(cover.template in ores)) {
          continue;
        }
        let ratios = ores[cover.template];
        let choices = [];
        for (let ore in ratios) {
          for (let j=0; j<ratios[ore]; j++) {
            choices.push(ore);
          }
        }
        choices = HTomb.Utils.shuffle(choices);
        let displace = ROT.RNG.getUniform()*256;
        let angle = 2*Math.PI*ROT.RNG.getUniform();
        let segs = ROT.RNG.getUniformInt(SEGMIN, SEGMAX);
        for (let j=0; j<segs; j++) {
          angle += CURVE*noise.get(displace+x0,displace+y0);
          let x1 = x0+Math.cos(angle)*SEGLEN;
          let y1 = y0+Math.sin(angle)*SEGLEN;
          let line = HTomb.Path.line(parseInt(x0),parseInt(y0),parseInt(x1),parseInt(y1));
          for (let square of line) {
            let [x,y] = square;
            if (x>0 && x<LEVELW-1 && y>0 && y<LEVELH-1) {
              if (ROT.RNG.getUniform()<0.75 && HTomb.World.covers[z][x][y].mineral) {
                HTomb.World.covers[z][x][y] = HTomb.Covers[choices[0]];
              }
            }
          }
          x0 = x1;
          y0 = y1;
        }
      }
    }
  }

  HTomb.World.caverns = [];
  function generateCaverns() {
    let levels = [46, 41, 35, 29, 23, 16];
    // this is kind of sloppy
    for (let level of levels) {
      //let cavern = HTomb.Things.Cavern.spawn({level: level});
      //let cavern = HTomb.Things.HerringboneCavern.spawn({level: level});
      let cavern = HTomb.Things.WangTileCavern.spawn({level: level});
      cavern.generate();
    }
  }

  return HTomb;
})(HTomb);
