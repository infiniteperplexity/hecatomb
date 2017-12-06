
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
    template: "WangCavern",
    name: "wang tile cavern",
    tiles: {
T1111:

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


