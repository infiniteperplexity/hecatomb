HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;
  let OCTAVES = [256,128,64,32,16,8,4,2];

  let Cavern = HTomb.Things.Thing.extend({
    template: "Cavern",
    name: "cavern",
    breached: false,
    level: null,
    generate: function() {
      let caves = new ROT.Map.Cellular(LEVELW-2,LEVELH-2,{connected: true});
      caves.randomize(0.5);
      for (let i=0; i<6; i++) {
        caves.create();
      }
      let noise = new ROT.Noise.Simplex();
      let scales = [2,1,1,0.5,0,0,0,0];
      let grid = HTomb.Utils.multiarray(LEVELW-2,LEVELH-2);
      let level = this.level;
      caves.create(function(x,y,val) {  
        let z = level;
        for (let o=0; o<OCTAVES.length; o++) {
          z+= noise.get(x/OCTAVES[o],y/OCTAVES[o])*scales[o];
        }
        z = parseInt(z);
        grid[x][y] = z;
        if (val) {
          HTomb.World.tiles[z][x+1][y+1] = HTomb.Tiles.FloorTile;
          let cover = HTomb.World.covers[z][x+1][y+1];
          if (cover.mineral) {
            cover.mineral.mine(x+1,y+1,z);
          }
          HTomb.World.validate.dirtify(x+1,y+1,z);
        }
      });
      let tiles = HTomb.World.tiles;
      for (let x=1; x<LEVELW-1; x++) {
        for (let y=1; y<LEVELH-1; y++) {
          let z = grid[x-1][y-1];
          if (tiles[z][x][y]===HTomb.Tiles.FloorTile) {
            let squares = HTomb.Tiles.neighboringColumns(x,y);
            let slope = false;
            for (let s of squares) {
              if (tiles[z+1][s[0]][s[1]]===HTomb.Tiles.FloorTile) {
                slope = true;
                break;
              }
            }
            if (slope===true) {
              tiles[z][x][y] = HTomb.Tiles.UpSlopeTile;
              let cover = HTomb.World.covers[z+1][x][y];
              if (cover.mineral) {
                cover.mineral.mine(x,y,z+1);
              }
              tiles[z+1][x][y] = HTomb.Tiles.DownSlopeTile;
            }
          }
        }
      }
    }
  });

  Cavern.extend({
    template: "HerringboneCavern",
    name: "herringbone cavern",
    tiling: ["N","E","W","S"],
    tiles: {
      N: [1, 1, 1, 1, 1, 1, 1, 1,
          1, 1, 1, 1, 1, 1, 1, 1,
          1, 1, 1, 1, 1, 1, 1, 1,
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0,
          1, 1, 1, 0, 0, 1, 1, 1,
          1, 1, 1, 0, 0, 1, 1, 1,
          1, 1, 1, 0, 0, 1, 1, 1],
      S: [1, 1, 1, 0, 0, 1, 1, 1,
          1, 1, 1, 0, 0, 1, 1, 1,
          1, 1, 1, 0, 0, 1, 1, 1,
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0,
          1, 1, 1, 1, 1, 1, 1, 1,
          1, 1, 1, 1, 1, 1, 1, 1,
          1, 1, 1, 1, 1, 1, 1, 1],
      E: [1, 1, 1, 0, 0, 1, 1, 1,
          1, 1, 1, 0, 0, 1, 1, 1,
          1, 1, 1, 0, 0, 1, 1, 1,
          0, 0, 0, 0, 0, 1, 1, 1,
          0, 0, 0, 0, 0, 1, 1, 1,
          1, 1, 1, 0, 0, 1, 1, 1,
          1, 1, 1, 0, 0, 1, 1, 1,
          1, 1, 1, 0, 0, 1, 1, 1],
      W: [1, 1, 1, 0, 0, 1, 1, 1,
          1, 1, 1, 0, 0, 1, 1, 1,
          1, 1, 1, 0, 0, 1, 1, 1,
          1, 1, 1, 0, 0, 0, 0, 0,
          1, 1, 1, 0, 0, 0, 0, 0,
          1, 1, 1, 0, 0, 1, 1, 1,
          1, 1, 1, 0, 0, 1, 1, 1,
          1, 1, 1, 0, 0, 1, 1, 1]
    },
    generate: function() {
      let grid = HTomb.Utils.multiarray(LEVELW,LEVELH);
      let tileSize = Math.sqrt(this.tiles.N.length);
      let nTiles = LEVELW/tileSize;
      for (let i=0; i<nTiles; i++) {
        let whichTile = i%this.tiling.length;
        for (let j=0; j<nTiles; j++) {
          let thisTile = this.tiling[whichTile];
          if (ROT.RNG.getUniformInt(1,4)===1) {
            thisTile = this.tiling[ROT.RNG.getUniformInt(0,3)];
          }
          for (let m=0; m<tileSize; m++) {
            for (let n=0; n<tileSize; n++) {
              let ind = m+n*tileSize;
              grid[i*tileSize+m][j*tileSize+n] = this.tiles[thisTile][ind];
            }
          }
          whichTile = (whichTile+1)%4;
        }
      }
      let noise = new ROT.Noise.Simplex();
      let scales = [2,1,1,0.5,0,0,0,0];
      let z = this.level;
      for (let x=1; x<LEVELW-2; x++) {
        for (let y=1; y<LEVELH-2; y++) {
          if (grid[x][y]===0) {
            HTomb.World.tiles[z][x][y] = HTomb.Tiles.FloorTile;
            let cover = HTomb.World.covers[z][x][y];
            if (cover.mineral) {
              cover.mineral.mine(x,y,z);
            }
          }
        }
      }
    }
  });

  Cavern.extend({
    template: "WangTileCavern",
    name: "wang tile cavern",
    tiles: HTomb.wangtiles,
    P1: 0.50,
    P2: 0.75,
    grid: HTomb.Utils.multiarray(32,32),
    floodTiles: function(i,j) {
      let TILE = 8;
      let WIDTH = LEVELW;
      let HEIGHT = LEVELH;
      let grid = this.grid;
      grid[i][j] = [];
      if (i>0 && grid[i-1][j]) {
        grid[i][j][0] = grid[i-1][j][2];
        grid[i][j][7] = grid[i-1][j][3];
        grid[i][j][6] = grid[i-1][j][4];
      }
      if (i<WIDTH/TILE-1 && grid[i+1][j]) {
        grid[i][j][2] = grid[i+1][j][0];
        grid[i][j][3] = grid[i+1][j][7];
        grid[i][j][4] = grid[i+1][j][6];
      }
      if (j>0 && grid[i][j-1]) {
        grid[i][j][2] = grid[i][j-1][4];
        grid[i][j][1] = grid[i][j-1][5];
        grid[i][j][0] = grid[i][j-1][6];
      }
      if (j<HEIGHT/TILE-1 && grid[i][j+1]) {
        grid[i][j][4] = grid[i][j+1][2];
        grid[i][j][5] = grid[i][j+1][1];
        grid[i][j][6] = grid[i][j+1][0];
      }
      for (let k=0; k<8; k++) {
        if (grid[i][j][k]===undefined) {
          let P = (k===1 || k===3 || k===5 || k===7) ? this.P1 : this.P2;
          grid[i][j][k] = (ROT.RNG.getUniform()<P) ? 1 : 0;
        }
      }
      if (i>0 && !grid[i-1][j]) {
        this.floodTiles(i-1,j);
      }
      if (i<WIDTH/TILE-1 && !grid[i+1][j]) {
        this.floodTiles(i+1,j);
      }
      if (j>0 && !grid[i][j-1]) {
        this.floodTiles(i,j-1);
      }
      if (j<HEIGHT/TILE-1 && !grid[i][j+1]) {
        this.floodTiles(i,j+1);
      }
    },
    generate: function() {
      let TILE = 8;
      this.floodTiles(0,0);
      let grid = HTomb.Utils.multiarray(256,256);
      for (let i=0; i<LEVELW/TILE; i++) {
        for (let j=0; j<LEVELH/TILE; j++) {
          let key = "t"+this.grid[i][j].join("");
          let squares = this.tiles[key].squares;
          let square = squares[ROT.RNG.getUniformInt(1,squares.length)-1];
          for (let m=0; m<TILE; m++) {
            for (let n=0; n<TILE; n++) {
              grid[i*TILE+m][j*TILE+n] = square[m][n];
            }
          }
        }
      }
      let zoneKeys = {};
      let zoneList = [[]];
      let zone = 0; 
      let _flood = function(i,j,topLevel) {
        if (i<=0 || i>=LEVELW-1 || j<=0 || j>=LEVELH-1) {
          return;
        }
        if (grid[i][j]===0 && zoneKeys[i+":"+j]===undefined) {
          zoneKeys[i+":"+j] = zone;
          zoneList[zone].push([i,j]);
          for (let dir of ROT.DIRS[8]) {
            let [x,y] = dir;
            _flood(i+x,j+y,false);
          }
          if (topLevel) {
            zone+=1;
            zoneList.push([]);
          }
        }
      };
      for (let i=1; i<LEVELW-1; i++) {
        for (let j=1; j<LEVELW-1; j++) {
          _flood(i,j,true);
        }
      }
      let z0 = this.level;
      let OCTAVES = [256,128,64,32,16,8,4,2];
      let scales = [1,0.5,0.5,0,0,0,0,0];;
      let noise = HTomb.World.generate.enoise;
      let tiles = HTomb.World.tiles;
      for (let x=1; x<LEVELW-1; x++) {
        for (let y=1; y<LEVELH-1; y++) {
          if (grid[x][y]===0) {
            let zn = zoneKeys[x+":"+y];
            if (zoneList[zn].length<100) {
              continue;
            }
            let z = z0;  
            for (let o=0; o<OCTAVES.length; o++) {
              z+=noise.get(x/OCTAVES[o],y/OCTAVES[o])*scales[o];
            }
            z = parseInt(z);
            if (tiles[z][x][y]===HTomb.Tiles.WallTile) {
              tiles[z][x][y] = HTomb.Tiles.FloorTile;
              let cover = HTomb.World.covers[z][x][y];
              if (cover.mineral) {
                cover.mineral.mine(x,y,z);
              }
            }
            //ugly, ad hoc way of saving this for the next step
            grid[x][y] = z;
          } else {
            grid[x][y] = z0;
          }
        }
      }
      for (let x=1; x<LEVELW-1; x++) {
        for (let y=1; y<LEVELH-1; y++) {
          let z = grid[x][y];
          if (tiles[z][x][y]===HTomb.Tiles.FloorTile) {
            let squares = HTomb.Tiles.neighboringColumns(x,y);
            let slope = false;
            for (let s of squares) {
              if (tiles[z+1][s[0]][s[1]]===HTomb.Tiles.FloorTile) {
                slope = true;
                break;
              }
            }
            if (slope===true) {
              tiles[z][x][y] = HTomb.Tiles.UpSlopeTile;
              let cover = HTomb.World.covers[z+1][x][y];
              if (cover.mineral) {
                cover.mineral.mine(x,y,z+1);
              }
              tiles[z+1][x][y] = HTomb.Tiles.DownSlopeTile;
            }
          }
        }
      }
      // for (let zn of zoneList) {
      //   console.log("zone size was "+zn.length);
      // }
    }
  });

  return HTomb;
})(HTomb);


