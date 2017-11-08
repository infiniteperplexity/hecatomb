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
        if (HTomb.World.generate.parcels[biome]===undefined) {
          console.log("Something went wrong here...");
          console.log(biome);
        }
        parcels = parcels.concat(HTomb.World.generate.parcels[biome]);
      }
      parcels = HTomb.Utils.shuffle(parcels);
      if (parcels[0]===undefined) {
        console.log("Something went wrong with " + this.describe());
        console.log(parcels);
        console.log(this.biomes);
      }
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
        // claim nearby items on ground
        for (let pile in HTomb.World.items) {
          for (let item of HTomb.World.items[pile]) {
            if (HTomb.Path.quickDistance(xyz.x,xyz.y,xyz.z,item.x,item.y,item.z)<=10) {
              if (HTomb.World.tiles[coord(item.x,item.y,item.z)]!==HTomb.Tiles.WallTile) {
                item.owned = true;
              }
            }
          }
        }
      } else {
        alert("failure!");
      }
    }
  });


  Parcel.extend({
    template: "GraveyardParcel",
    name: "graveyard parcel",
    frequency: 12,
    biomes: ["InnerHills","OuterHills","BorderForest","DeepForest","BorderWastes","DeepWastes","BorderMountains","BorderOcean"],
    someMethod: function(x0,y0,w,h) {
      let n = ROT.RNG.getUniformInt(2,4);
      let xyz;
      let mask = {};
      for (let i=0; i<n; i++) {
        let g = HTomb.Things.SmallGraveyard.spawn();
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
      for (let i = Math.max(1,x-SIZE); i<=Math.min(LEVELW-2,x+SIZE); i++) {
        for (let j = Math.max(1,y-SIZE); j<=Math.min(LEVELH-2,y+SIZE); j++) {
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


  Parcel.extend({
    template: "RuinsParcel",
    name: "ruins parcel",
    frequency: 6,
    biomes: ["OuterHills","BorderForest","BorderWastes","DeepWastes","BorderMountains","BorderOcean"],
    structures: ["Workshop","GuardPost","BlackMarket","Sanctum"],
    someMethod: function(x0,y0,w,h) {
      let n = ROT.RNG.getUniformInt(2,4);
      let xyz;
      let mask = {};
      for (let i=0; i<n; i++) {
        let structures = HTomb.Utils.shuffle(this.structures);
        let g = HTomb.Things.RuinedStructure.spawn({structure: structures[0]});
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

  Footprint.extend({
    template: "RuinedStructure",
    name: "ruined structure",
    structure: "Workshop",
    validPlace: function(x,y,z) {
      let s = HTomb.Things[this.structure];
      let dx = parseInt(s.width/2);
      let dy = parseInt(s.height/2);
      for (let i=x-dx; i<=x+dx; i++) {
        for (let j=y-dy; j<=y+dy; j++) {
          if (HTomb.World.tiles[z][i][j]!==HTomb.Tiles.FloorTile) {
            return false;
          } else if (HTomb.World.features[coord(i,j,z)]) {
            return false;
          }
        }
      }
      return true;
    },
    onPlace: function(x,y,z) {
      let s = HTomb.Things[this.structure];
      let dx = parseInt(s.width/2);
      let dy = parseInt(s.height/2);
      console.log("placing ruins at ",x,y,z);
      let f = 0;
      for (let j=y-dy; j<=y+dy; j++) {
        for (let i=x-dx; i<=x+dx; i++) {
          if (ROT.RNG.getUniformInt(1,2)===1) {
            let ruins = HTomb.Things.Ruins.spawn();
            ruins.place(i,j,z);
            ruins.symbol = s.symbols[f];
            ruins.name = "ruined " + s.name;
          }
          f+=1;
        }
      }
    }
  });

  return HTomb;
})(HTomb);
