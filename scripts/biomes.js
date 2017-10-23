HTomb = (function(HTomb) {
  "use strict";
  let NLEVELS = HTomb.Constants.NLEVELS;
  let LEVELW = HTomb.Constants.LEVELW;
  let LEVELH = HTomb.Constants.LEVELH;

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
      for (let x=this.x0; x<LEVELW-1; x++) {
        for (let y=this.y0; y<LEVELH-1; y++) {
          let z0 = HTomb.Tiles.groundLevel(x,y);
          let r = Math.sqrt(Math.pow(x-this.corner[0],2) + Math.pow(y-this.corner[1],2));
          r = 2*(2*NLEVELS-r)/(NLEVELS/2);
          HTomb.World.elevations[x][y]+=r;
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
