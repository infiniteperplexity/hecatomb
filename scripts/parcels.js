HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;
  let PADDING = 6;
  let PSIZE = 22;

  let Parcel = HTomb.Types.Type.extend({
    template: "Parcel",
    name: "parcel",
    frequency: 1,
    biomes: [],
    deal: function() {
      let parcels = [];
      for (let biome of this.biomes) {
        parcels = parcels.concat(HTomb.World.generate.parcels[biome]);
      }
      parcels = HTomb.Utils.shuffle(parcels);
      let i = parcels[0][0];
      let j = parcels[0][1];
      this.someMethod(PADDING+i*PSIZE,PADDING+j*PSIZE,PSIZE,PSIZE);
    },
    someMethod: function(x0,y0,w,h) {

    }
  });

  Parcel.extend({
    template: "PlayerParcel",
    name: "player parcel",
    biomes: ["CentralHills"],
    someMethod: function(x0,y0,w,h) {
      let n = 3;
      let xyz;
      let last;
      let mask = {};
      for (let i=0; i<n; i++) {
        let g = HTomb.Things.SmallGraveyard.spawn();
        xyz = g.findPlace(x0,y0,w,h,{mask: mask});
        if (xyz) {
          last = xyz;
          g.place(xyz.x,xyz.y,xyz.z);
          let addMask = HTomb.Tiles.squaresWithinSquare(xyz.x,xyz.y,xyz.z,3);
          for (let s of addMask) {
            mask[coord(s[0],s[1],s[2])] = true;
          }
        } else {
          alert("failure!");
        }
      }
      let p = HTomb.Things.Necromancer.spawn();
      let playerPlace = function(x,y,z) {
        let DIST = 6;
        if (Math.abs(x-last.x)<DIST && Math.abs(y-last.y)<DIST && z===last.z) {
          return true;
        } else {
          return false;
        }
      };
      xyz = p.findPlace(x0,y0,w,h,{validPlace: playerPlace});
      if (xyz) {
        p.place(xyz.x,xyz.y,xyz.z);
        HTomb.Things.Player.spawn().addToEntity(p);
      } else {
        alert("failure!");
      }
    }
  });

  let Footprint = HTomb.Things.Entity.extend({
    template: "Footprint",
    name: "footprint",
    place: function(x,y,z) {
      HTomb.Things.Entity.place.call(this,x,y,z);
      this.despawn();
    }
  });

  // could this be done as an encounter?
  Footprint.extend({
    template: "SmallGraveyard",
    name: "small graveyard",
    validPlace: function(x,y,z) {
      let nvalid = 0;
      let dirs = ROT.DIRS[4];
      for (let d of dirs) {
        if (this.validSquare(x+d[0],y+d[1],z)) {
          nvalid+=1;
        }
      }
      if (nvalid>=2) {
        return true;
      }
    },
    validSquare: function(x,y,z) {
      if (  HTomb.World.features[coord(x,y,z)]===undefined
            && HTomb.World.tiles[z][x][y]===HTomb.Tiles.FloorTile
            && HTomb.World.tiles[z-1][x][y]===HTomb.Tiles.WallTile
            && HTomb.World.covers[z][x][y].liquid!==true
            && HTomb.Tiles.countNeighborsWhere(x,y,z, function(x1,y1,z1) {
              return (HTomb.World.tiles[z1-1][x1][y1]!==HTomb.Tiles.WallTile);
            })===0) {
        return true;
      } else {
        return false;
      }
    },
    onPlace: function(x,y,z) {
      let dirs = HTomb.Utils.copy(ROT.DIRS[4]);
      dirs = HTomb.Utils.shuffle(dirs);
      let graves = ROT.RNG.getUniformInt(2,4);
      let placed = 0;
      for (let i=0; i<4; i++) {
        if (placed>=graves) {
          break;
        }
        let d = dirs[i];
        let x1 = x+d[0];
        let y1 = y+d[1];
        if (this.validSquare(x1,y1,z)) {
          HTomb.Things.Tombstone.spawn().place(x1,y1,z);
          HTomb.Things.Corpse.spawn({owned: false}).place(x1,y1,z-1);
          HTomb.World.covers[z-1][x1][y1] = HTomb.Covers.Soil;
          placed+=1;
        }
      }
    }
  });


  Parcel.extend({
    template: "HerbyParcel",
    name: "herby parcel",
    biomes: ["InnerHills","OuterHills","BorderForest","DeepForest","BorderMountains","BorderWastes","BorderOcean"],
    frequency: 6,
    someMethod: function(x0,y0,w,h) {
      let herbs = HTomb.Utils.shuffle(["Wolfsbane","Bloodwort","Mandrake"]);
      let n = ROT.RNG.getUniformInt(2,3);
      let xyz;
      let mask = {};
      for (let i=0; i<n; i++) {
        let g = HTomb.Things.HerbPatch.spawn({herb: herbs[0]});
        xyz = g.findPlace(x0,y0,w,h,{mask: mask});
        if (xyz) {
          g.place(xyz.x,xyz.y,xyz.z);
          let addMask = HTomb.Tiles.squaresWithinSquare(xyz.x,xyz.y,xyz.z,3);
          for (let s of addMask) {
            mask[coord(s[0],s[1],s[2])] = true;
          }
        } else {
          alert("failure!");
        }
      }
    }
  });

  // could this be done as an encounter?
  Footprint.extend({
    template: "HerbPatch",
    name: "herb patch",
    herb: "Wolfsbane",
    validPlace: function(x,y,z) {
      if (HTomb.World.features[coord(x,y,z)]) {
        return false;
      } else if (HTomb.World.covers[z][x][y]==HTomb.Covers.Grass) {
        return true;
      }
      return false;
    },
    validSquare: function(x,y,z) {
      if (HTomb.World.features[coord(x,y,z)]) {
        return false;
      } else if (HTomb.World.covers[z][x][y]==HTomb.Covers.Grass) {
        return true;
      }
      return false;
    },
    onPlace: function(x,y,z) {
      let SIZE = 10;
      for (let i = x-SIZE; i<=x+SIZE; i++) {
        for (let j = y-SIZE; j<=y+SIZE; j++) {
          let z = HTomb.Tiles.groundLevel(i,j);
          if (!HTomb.World.features[coord(i,j,z)] && HTomb.World.covers[z][x][y]===HTomb.Covers.Grass) {
            let closeness = SIZE - Math.sqrt(Math.pow(i-x,2)+Math.pow(j-y,2));
            let r = ROT.RNG.getUniformInt(0,99);
            let rockiness = HTomb.World.generate.rockiness[i][j];
            let FREQ = -60;
            if (r>rockiness-FREQ-10*closeness) {
              if (ROT.RNG.getUniformInt(1,3)===1) {
                HTomb.Things[this.herb + "Plant"].spawn().place(i,j,z);
              }
            }
          }
        }
      }
    }
  });

  return HTomb;
})(HTomb);
