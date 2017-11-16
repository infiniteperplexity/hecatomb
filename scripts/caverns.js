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


  return HTomb;
})(HTomb);
