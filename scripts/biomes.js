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
    modifyTerrain: function() {
    }
  });

  Biome.extend({
    template: "Mountains",
    name: "mountains",
    modifyTerrain: function() {
      for (let x=this.x0; x<this.x1; x++) {
        for (let y=this.y0; y<this.y1; y++) {
          let r = Math.sqrt(Math.pow(x-this.corner[0],2) + Math.pow(y-this.corner[1],2));
          r = 2*(2*NLEVELS-r)/(NLEVELS/3);
          HTomb.World.elevations[x][y]+=Math.max(0,r);
        }
      }
    }
  });

  Biome.extend({
    template: "Swamp",
    name: "swamp",
    // modifyTerrain: function() {
    //   for (let x=this.x0; x<this.x1; x++) {
    //     for (let y=this.y0; y<this.y1; y++) {
    //       let r = Math.sqrt(Math.pow(x-this.corner[0],2) + Math.pow(y-this.corner[1],2));
    //       r = 10*(2*NLEVELS-r)/(NLEVELS/3);
    //       HTomb.World.vegetation[x][y]-=Math.max(0,r);
    //     }
    //   }
    // }
  });

  Biome.extend({
    template: "Forest",
    name: "forest",
    modifyTerrain: function() {
      for (let x=this.x0; x<this.x1; x++) {
        for (let y=this.y0; y<this.y1; y++) {
          let r = Math.sqrt(Math.pow(x-this.corner[0],2) + Math.pow(y-this.corner[1],2));
          r = 10*(2*NLEVELS-r)/(NLEVELS/3);
          HTomb.World.vegetation[x][y]+=Math.max(0,r);
        }
      }
    }
  });

  Biome.extend({
    template: "Ocean",
    name: "ocean",
    modifyTerrain: function() {
      for (let x=this.x0; x<this.x1; x++) {
        for (let y=this.y0; y<this.y1; y++) {
          let r = Math.sqrt(Math.pow(x-this.corner[0],2) + Math.pow(y-this.corner[1],2));
          r = 2*(2*NLEVELS-r)/(NLEVELS/3);
          HTomb.World.elevations[x][y]-=Math.max(0,r);
        }
      }
    }
  });

  return HTomb;
})(HTomb);
