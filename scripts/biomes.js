HTomb = (function(HTomb) {
  "use strict";
  let NLEVELS = HTomb.Constants.NLEVELS;
  let LEVELW = HTomb.Constants.LEVELW;
  let LEVELH = HTomb.Constants.LEVELH;

  let OCTAVES = [256,128,64,32,16,8,4,2];

  let Biome = HTomb.Types.Type.extend({
    template: "Biome",
    name: "biome",
    width: LEVELW/2,
    height: LEVELH/2,
    modify: function(x,y) {
      let x0 = (x===1) ? x : this.width;
      let x1 = (x===1) ? this.width : LEVELW-1;
      let y0 = (y===1) ? y : this.height;
      let y1 = (y===1) ? this.height : LEVELH-1;
      for (let i=x0; i<x1; i++) {
        for (let j=y0; j<y1; j++) {
          this.modifyTerrain(i,j,x,y);
        }
      }
    },
    modifyTerrain: function(i,j,x,y) {
    }
  });

  Biome.extend({
    template: "Mountains",
    name: "mountains",
    modifyTerrain: function(i,j,x,y) {
      let r = Math.sqrt(Math.pow(i-x,2) + Math.pow(j-y,2));
      r = 2*(2*NLEVELS-r)/(NLEVELS/3);
      r = Math.max(r,0);
      HTomb.World.generate.elevation[i][j]+=r;
      let scales = [0,0,0,0.05,0.05,0.025,0.025,0];
      for (let o=0; o<OCTAVES.length; o++) {
        HTomb.World.generate.elevation[i][j] += 1.5*r*scales[o]*HTomb.World.generate.enoise.get(i/OCTAVES[o],j/OCTAVES[o]);
      }
      HTomb.World.generate.rockiness[i][j]+=1.25*r;
    }
  });

  Biome.extend({
    template: "Wastes",
    name: "wastes",
    modifyTerrain: function(i,j,x,y) {
      let r = Math.sqrt(Math.pow(i-x,2) + Math.pow(j-y,2));
      r = 5*(2*NLEVELS-r)/(NLEVELS/3);
      r = Math.max(r,0);
      HTomb.World.generate.rockiness[i][j]+=r;
      let scales = [0,0,0,0.05,0.05,0.025,0.025,0];
      for (let o=0; o<OCTAVES.length; o++) {
        HTomb.World.generate.elevation[i][j] += r*scales[o]*HTomb.World.generate.enoise.get(i/OCTAVES[o],j/OCTAVES[o]);
      }
    }
  });

  Biome.extend({
    template: "Forest",
    name: "forest",
    modifyTerrain: function(i,j,x,y) {
      let r = Math.sqrt(Math.pow(i-x,2) + Math.pow(y-j,2));
      r = 10*(2*NLEVELS-r)/(NLEVELS/3);
      HTomb.World.generate.rockiness[i][j]-=Math.max(0,r);
    }
  });

  Biome.extend({
    template: "Ocean",
    name: "ocean",
    modifyTerrain: function(i,j,x,y) {
      let r = Math.sqrt(Math.pow(i-x,2) + Math.pow(j-y,2));
      r = 2*(2*NLEVELS-r)/(NLEVELS/3);
      HTomb.World.generate.elevation[i][j]-=Math.max(0,r);
    }
  });

  return HTomb;
})(HTomb);
