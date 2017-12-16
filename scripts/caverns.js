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
    groundLevels: null,
    squares: null,
    generate: function() {
      this.squares = {};
      HTomb.World.caverns.push(this);
      this.groundLevels = HTomb.Utils.multiarray(LEVELW,LEVELH);
      for (let x=0; x<LEVELW; x++) {
        for (let y=0; y<LEVELH; y++) {
          if (x===0 || y===0 || x===LEVELW-1 || y===LEVELH-1) {
            this.groundLevels[x][y] = null;
          }
        }
      }
      this.algorithm();
    },
    algorithm: function() { 
    },
    breach: function() {
      this.breached = true;
      for (let square in this.squares) {
        let [x,y,z] = HTomb.Utils.decoord(square);
        let cr = HTomb.World.creatures[square]
        if (cr && cr.actor && cr.actor.dormant) {
          cr.actor.dormant = false;
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
    algorithm: function() {
      let TILE = 8;
      this.floodTiles(0,0);
      let grid = HTomb.Utils.multiarray(LEVELW,LEVELH);
      for (let i=0; i<LEVELW/TILE; i++) {
        for (let j=0; j<LEVELH/TILE; j++) {
          let key = "t"+this.grid[i][j].join("");
          let squares = this.tiles[key].squares;
          let square = squares[ROT.RNG.getUniformInt(1,squares.length)-1];
          for (let m=0; m<TILE; m++) {
            for (let n=0; n<TILE; n++) {
              let x = i*TILE+m;
              let y = j*TILE+n;
              if (x>0 || y>0 || x<LEVELW-1 || y<LEVELH-1) {
                grid[x][y] = square[m][n];
              }
            }
          }
        }
      }
      let zoneKeys = {};
      let zoneList = [[]];
      let zone = 0; 
      HTomb.Debug.grid = grid;
      HTomb.Debug.zoneKeys = zoneKeys;
      let _flood = function(i,j,depth) {
        if (i<=0 || i>=LEVELW-1 || j<=0 || j>=LEVELH-1) {
          return;
        }
        if (grid[i][j]===0 && zoneKeys[coord(i,j,0)]===undefined) {
          zoneKeys[coord(i,j,0)] = zone;
          zoneList[zone].push([i,j]);
          for (let dir of ROT.DIRS[4]) {
            let [x,y] = dir;
            // the recursion error comes in the midst of this stuff, not bailing out
            //  depth is extremely high
            // and when it happens, zone is zero all the way untiul the end
            // if (depth>250) {
            //   console.log("went deep...");
            //   console.log(i,x,j,y);
            //   console.log(depth);
            //   console.log(zone);
            // }
            _flood(i+x,j+y,depth+1);
          }
          if (depth===0) {
            zone+=1;
            zoneList.push([]);
          }
        }
      };
      for (let i=1; i<LEVELW-1; i++) {
        for (let j=1; j<LEVELH-1; j++) {
          _flood(i,j,0);
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
            let zn = zoneKeys[coord(x,y,0)];
            if (zoneList[zn].length<100) {
              //again, weird
              this.groundLevels[x][y] = z0;
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
              this.squares[coord(x,y,z)] = true;
            }
            this.groundLevels[x][y] = z;
          } else {
            // this is weird...
            this.groundLevels[x][y] = z0;
          }
        }
      }
      for (let x=1; x<LEVELW-1; x++) {
        for (let y=1; y<LEVELH-1; y++) {
          let z = this.groundLevels[x][y];
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
              this.squares[coord(x,y,z)] = true;
              this.squares[coord(x,y,z+1)] = true;
            }
          }
        }
      }
    }
  });

  return HTomb;
})(HTomb);


