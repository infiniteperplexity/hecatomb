HTomb = (function(HTomb) {
  "use strict";
  let NLEVELS = HTomb.Constants.NLEVELS;

   let Biome = HTomb.Things.Thing.extend({
    template: "Biome",
    name: "biome",
    x0: null,
    y0: null,
    z0: null,
    x1: null,
    y1: null,
    z1: null,
    corner: [null,null],
    modifyElevations: function() {
    }
  });

  Biome.extend({
    template: "Mountains",
    name: "mountains",
    modifyElevations: function() {
      HTomb.Debug.tiles = HTomb.World.tiles;
      for (let x=this.x0; x<this.x1; x++) {
        for (let y=this.y0; y<this.y1; y++) {
          let z0 = HTomb.Tiles.groundLevel(x,y);
          let r = Math.sqrt(Math.pow(x-this.corner[0],2) + Math.pow(y-this.corner[1],2));
          let z1 = z0 + (NLEVELS-r)/(NLEVELS/4);
          for (let z=z0; z<z1; z++) {
            HTomb.World.tiles[z][x][y] = HTomb.Tiles.WallTile;
            HTomb.World.tiles[z+1][x][y] = HTomb.Tiles.FloorTile;
            HTomb.World.exposed[x][y] = z+1;
          }
        }
      }
    }
  });

  Biome.extend({
    template: "Swamp",
    name: "swamp"
  });

  Biome.extend({
    template: "Forest",
    name: "forest"
  });

  Biome.extend({
    template: "Ocean",
    name: "ocean"
  });

  return HTomb;
})(HTomb);
