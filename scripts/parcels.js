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
    cavernMin: null,
    cavernMax: null,
    frequency: 1,
    biomes: [],
    logme: false,
    deal: function() {
      if (this.cavernMin===null && this.cavernMax===null) {
        this.dealSurface();
      } else {
        this.dealCavern();
      }
    },
    dealSurface: function() {
      let parcels = [];
      for (let biome of this.biomes) {
        if (HTomb.World.generate.parcels[biome]===undefined) {
          console.log("Something went wrong here...");
          console.log(biome);
        }
        parcels = parcels.concat(HTomb.World.generate.parcels[biome]);
      }
      // this built a list of parcels that have the right biome
      if (parcels.length===0) {
        for (let biome of HTomb.World.generate.parcels) {
          parcels = parcels.concat(HTomb.World.generate.parcels[biome]);
        }
      }
      parcels = HTomb.Utils.shuffle(parcels);
      if (parcels[0]===undefined) {
        console.log("Something went wrong with " + this.describe());
        console.log(parcels);
        console.log(this.biomes);
      }
      let i = parcels[0][0];
      let j = parcels[0][1];
      this.populate(PADDING+i*PSIZE,PADDING+j*PSIZE,PSIZE,PSIZE);
    },
    dealCavern: function() {
      let levels = [];
      for (let key in HTomb.World.caverns) {
        let cavern = HTomb.World.caverns[key];
        if (cavern.level<=this.cavernMax && cavern.level>=this.cavernMin) {
          levels.push(cavern);
        }
      }
      if (levels.length===0) {
        console.log("no valid cavern levels");
        return;
      }
      levels = HTomb.Utils.shuffle(levels);
      let nParcels = 11;
      let i = ROT.RNG.getUniformInt(1,nParcels)-1;
      let j = ROT.RNG.getUniformInt(1,nParcels)-1;
      this.populate(PADDING+i*PSIZE,PADDING+j*PSIZE,PSIZE,PSIZE,levels[0]);
    },
    populate: function(x0,y0,w,h,optionalLevel) {

    }
  });



  Parcel.extend({
    template: "PlayerParcel",
    name: "player parcel",
    biomes: ["CentralHills"],
    populate: function(x0,y0,w,h) {
      console.log("populating player parcel");
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
          if (HTomb.World.covers[z][x][y].liquid) {
            return false;
          } else {
            return true;
          }
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
            if (HTomb.Path.quickDistance(xyz.x,xyz.y,xyz.z,item.x,item.y,item.z)<=10 && Math.abs(xyz.z-item.z<=3)) {
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
    template: "TroglodyteLair",
    name: "troglodyte lair",
    cavernMin: 30,
    cavernMax: 50,
    frequency: 10,
    populate: function(x0,y0,w,h,level) {
      let trogs = 5;
      let trolls = 2;
      let xyz = HTomb.Things.Troglodyte.findPlace(x0,y0,w,h,{cavern: level});
      if (xyz) {
        let placed = HTomb.Things.Troglodyte.chainPlace(xyz.x,xyz.y,xyz.z,
          { n: trogs,
            cavern: level,
            callback: function(cr) {cr.actor.dormant=true;}
          });
        let last = placed[0];
        xyz = HTomb.Things.Troll.findPlace(last.x-3,last.y-3,6,6,{cavern: level});
        if (xyz) {
          HTomb.Things.Troll.chainPlace(xyz.x,xyz.y,xyz.z,{
            n: trolls,
            cavern: level,
            callback: function(cr) {cr.actor.dormant=true;}
          });
        }
      }
    }
  });

  Parcel.extend({
    template: "GraveyardParcel",
    name: "graveyard parcel",
    frequency: 12,
    biomes: ["InnerHills","OuterHills","BorderForest","DeepForest","BorderWastes","DeepWastes","BorderMountains","BorderOcean"],
    populate: function(x0,y0,w,h) {
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
    place: function(x,y,z,args) {
      HTomb.Things.Entity.place.call(this,x,y,z,args);
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
            && !HTomb.World.covers[z][x][y].liquid
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
    populate: function(x0,y0,w,h) {
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


  Parcel.extend({
    template: "HerbyCavern",
    name: "herby cavern",
    frequency: 18,
    cavernMin: 20,
    cavernMax: 50,
    populate: function(x0,y0,w,h,level) {
      let herbs = HTomb.Utils.shuffle(["Skullcap","Lichen","Agaric"]);
      let n = ROT.RNG.getUniformInt(2,3);
      let xyz;
      let mask = {};
      for (let i=0; i<n; i++) {
        let g = HTomb.Things.MushroomPatch.spawn({herb: herbs[0]});
        xyz = g.findPlace(x0,y0,w,h,{mask: mask, cavern: level});
        if (xyz) {
          g.place(xyz.x,xyz.y,xyz.z,{cavern: level});
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
      HTomb.Things[this.herb + "Plant"].chainPlace(x,y,z, {
        min: 1,
        max: 4,
        n: 12
      });
    }
  });

  Footprint.extend({
    template: "MushroomPatch",
    name: "mushroom patch",
    herb: "Skullcap",
    validPlace: function(x,y,z) {
      if (HTomb.World.features[coord(x,y,z)]) {
        return false;
      } else if (HTomb.World.tiles[z][x][y]!==HTomb.Tiles.FloorTile && HTomb.World.tiles[z][x][y]!==HTomb.Tiles.UpSlopeTile) {
        return false;
      } else if (HTomb.World.covers[z][x][y]!==HTomb.Covers.Water) {
        return true;
      }
      return false;
    },
    validSquare: function(x,y,z) {
      if (HTomb.World.features[coord(x,y,z)]) {
        return false;
      } else if (HTomb.World.tiles[z][x][y]!==HTomb.Tiles.FloorTile && HTomb.World.tiles[z][x][y]!==HTomb.Tile.UpSlopeTile) {
        return false;
      } else if (HTomb.World.covers[z][x][y]!==HTomb.Covers.Water) {
        return true;
      }
      return false;
    },
    onPlace: function(x,y,z,options) {
      options = options || {};
      let level = options.cavern || null;
      HTomb.Things[this.herb + "Plant"].chainPlace(x,y,z, {
        min: 1,
        max: 4,
        n: 12,
        cavern: level
      });
    }
  });

  Parcel.extend({
    template: "RuinsParcel",
    name: "ruins parcel",
    frequency: 6,
    biomes: ["OuterHills","BorderForest","BorderWastes","DeepWastes","BorderMountains","BorderOcean"],
    structures: ["Workshop","GuardPost","BlackMarket","Sanctum"],
    populate: function(x0,y0,w,h) {
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
          // this check appears to fail at its purpose sometimes
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

  Parcel.extend({
    template: "HunterParcel",
    name: "hunter parcel",
    frequency: 2,
    biomes: ["OuterHills","BorderWastes","BorderOcean","BorderMountains","BorderForest","DeepForest"],
    populate: function(x0,y0,w,h) {
      let foot = HTomb.Things.HunterShack.spawn();
      let xyz = foot.findPlace(x0,y0,w,h);
      if (xyz) {
        foot.place(xyz.x,xyz.y,xyz.z);
      }
    }
  });

  Footprint.extend({
    template: "HunterShack",
    name: "hunter shack",
    validPlace: function(x,y,z) {
      if (HTomb.World.features[coord(x,y,z)]) {
        return false;
      } else {
        return true;
      }
    },
    onPlace: function(x,y,z) {
      HTomb.Things.Shack.spawn().place(x,y,z);
      let hunter = HTomb.Things.Hunter.spawn().place(x,y,z);
      let traps = ROT.RNG.getUniformInt(1,4);
      let RADIUS = 10;
      for (let i=0; i<traps; i++) {
        let trap = HTomb.Things.SpearTrap.spawn({owner: hunter});
        let xyz = trap.findPlace(x-RADIUS,y-RADIUS,2*RADIUS,2*RADIUS);
        if (xyz) {
          trap.place(xyz.x,xyz.y,xyz.z);
        }
      }
    }
  });

  // could be two kinds, red and black, at odds
  // normal ants are a, soldiers are 00E4, queen is 00E3
  Parcel.extend({
    template: "AntParcel",
    name: "ant parcel",
    biomes: ["OuterHills","BorderWastes","DeepWastes","BorderForest","BorderOcean"],
    frequency: 2,
    logme: true,
    populate: function(x0,y0,w,h) {
      let xyz = HTomb.Things.AntColony.findPlace(x0,y0,w,h);
      if (xyz) {
        HTomb.Things.AntColony.spawn().place(xyz.x,xyz.y,xyz.z);
      }
    }
  });

  Footprint.extend({
    template: "AntColony",
    name: "ant colony",
    radius: 9,
    roomSize: 3,
    // this should be handled differnetly to reuse less code
    findPlace: function(x0,y0,w,h, options) {
      options = options || {};
      let mask = options.mask || {};
      let callback = options.validPlace || this.validPlace.bind(this);
      let valid = false;
      let x ;
      let y;
      let z;
      let tries = 0;
      let TRIES = 100;
      while (valid===false && tries<TRIES) {
        x = ROT.RNG.getUniformInt(x0,x0+w);
        y = ROT.RNG.getUniformInt(y0,y0+h);
        z = HTomb.Tiles.groundLevel(x,y)-1;
        if (mask[coord(x,y,z)]) {
          continue;
        }
        valid = callback(x,y,z);
        tries+=1;
      }
      if (!valid) {
        console.log(this.describe() + "placement failed.");
        return null;
      }
      return {x: x, y: y, z: z};
    },
    validPlace: function(x,y,z) {
      let radius = this.radius;
      for (let i=x-radius; i<=x+radius; i++) {
        for (let j=y-radius; j<=y+radius; j++) {
          if (i<=0 || i>=LEVELW-1 || j<=0 || j>=LEVELH-1) {
            return false;
          } else if (HTomb.World.tiles[z][i][j]!==HTomb.Tiles.WallTile) {
            return false;
          }
        }
      }
      return true;
    },
    onPlace: function(x,y,z) {
      //z shouldn't be on ground level
      let radius = this.radius;
      let roomSize = this.roomSize;
      // create the "throne room"
      for (let i=x-roomSize; i<=x+roomSize; i++) {
        for (let j=y-roomSize; j<=y+roomSize; j++) {
          if (HTomb.Path.quickDistance(i,j,z,x,y,z)<=3) {
      //       // should I "dig it out?"
            HTomb.World.tiles[z][i][j] = HTomb.Tiles.FloorTile;
            HTomb.World.covers[z][i][j] = HTomb.Covers.NoCover;
          }
        }
      }
      let queen = HTomb.Things.AntQueen.spawn();
      queen.place(x,y,z);
      let maze = new ROT.Map.EllerMaze(2*radius+1, 2*radius+1);
      maze.create(function(x1,y1,val) {
        // wait, what's the x+1 y+1 thing for?
        // for now just dig out the whole thing
        if (val===0 && HTomb.World.tiles[z][x1+x-radius][y1+y-radius]===HTomb.Tiles.WallTile) {
          // HTomb.Things.DigTask.spawn({assigner: queen}).place(x1+x-radius,y1+y-radius,z);
          HTomb.World.tiles[z][x1+x-radius][y1+y-radius] = HTomb.Tiles.FloorTile;
          HTomb.World.covers[z][x1+x-radius][y1+y-radius] = HTomb.Covers.NoCover;
        }
      });
      let nants = ROT.RNG.getUniformInt(2,5);
      let ants = [];
      let a=0;
      while(a<nants) {
        let ant = HTomb.Things.WorkerAnt.spawn();
        // this works wrong...it places them on the ground level...
        let xyz = ant.findPlace(x-roomSize, y-roomSize, 2*roomSize, 2*roomSize);
        if (xyz) {
          ant.place(xyz.x,xyz.y,xyz.z);
          HTomb.Things.Minion.spawn().addToEntity(ant);
          queen.master.addMinion(ant);
          ants.push(ant);
        }
        a+=1;
      }
      let f = [x,y];
      // would be better to traverse this randomly
      for (let i=x-radius; i<=x+radius; i++) {
        for (let j=y-radius; j<=y+radius; j++) {
          if (HTomb.World.tiles[z][i][j]===HTomb.Tiles.FloorTile) {
            if (HTomb.Path.quickDistance(x,y,z,i,j,z) > HTomb.Path.quickDistance(x,y,z,f[0],f[1],z)) {
              if (HTomb.World.tiles[z+1][i][j]===HTomb.Tiles.FloorTile) { 
                f = [i,j];
              }
            }
          }
        }
      }
      HTomb.World.tiles[z][f[0]][f[1]] = HTomb.Tiles.UpSlopeTile;
      HTomb.World.tiles[z+1][f[0]][f[1]] = HTomb.Tiles.DownSlopeTile;
      HTomb.World.covers[z+1][f[0]][f[1]] = HTomb.Covers.NoCover;
     }
  });


  Parcel.extend({
    template: "TempleParcel",
    name: "temple parcel",
    biomes: ["OuterHills","BorderMountains","BorderWastes","BorderForest","BorderOcean"],
    frequency: 2,
    populate: function(x0,y0,w,h) {
      let mask = {};
      let xyz = HTomb.Things.TempleFootprint.findPlace(x0,y0,w,h);
      if (xyz) {
        HTomb.Things.TempleFootprint.spawn().place(xyz.x,xyz.y,xyz.z);
        // probably want some method for masking, and bounding boxes
      }
      let ngraves = ROT.RNG.getUniformInt(1,3);
      // should maybe do some masks for this?
      for (let i=0; i<ngraves; i++) {
        xyz = HTomb.Things.SmallGraveyard.findPlace(x0,y0,w,h);
        if (xyz) {
          HTomb.Things.SmallGraveyard.spawn().place(xyz.x,xyz.y,xyz.z);
        }
      }
      let nfounts = ROT.RNG.getUniformInt(0,2);
      for (let i=0; i<ngraves; i++) {
        xyz = HTomb.Things.Fountain.findPlace(x0,y0,w,h);
        if (xyz) {
          HTomb.Things.Fountain.spawn().place(xyz.x,xyz.y,xyz.z);
        }
      }
    }
  });

  Footprint.extend({
    template: "TempleFootprint",
    name: "temple footprint",
    width: 3,
    height: 3,
    symbols: ["\u2657",".","\u2628",".","\u26EA",".","\u2628",".","\u2657"],
    validPlace: function(x,y,z) {
      for (let i=x-1; i<=x+1; i++) {
        for (let j=y-1; j<=y+1; j++) {
          if (HTomb.World.features[coord(i,j,z)]) {
            return false;
          } else if (HTomb.World.tiles[z][i][j]!==HTomb.Tiles.FloorTile) {
            return false;
          }
        }
      }
      return true;
    },
    onPlace: function(x,y,z) {
      console.log("placing temple at ",x,y,z);
      let p = 0;
      for (let j=y-1; j<=y+1; j++) {
        for (let i=x-1; i<=x+1; i++) {
          let t = HTomb.Things.Temple.spawn();
          t.place(i,j,z);
          t.symbol = this.symbols[p];
          p+=1;
        } 
      }
      let priest = HTomb.Things.Priest.spawn();
      let xyz = priest.findPlace(x-2,y-2,5,5);
      if (xyz) {
        priest.place(xyz.x,xyz.y,xyz.z);
      }
    }
  });


  Parcel.extend({
    template: "WarpedObelisksParcel",
    name: "warped obelisks parcel",
    frequency: 1,
    biomes: ["DeepMountains","DeepForest","DeepWastes"],
    populate: function(x0,y0,w,h) {
      let valid = HTomb.Things.WarpedObelisk.validPlace.bind(HTomb.Things.WarpedObelisk);
      let one = HTomb.Things.WarpedObelisk.findPlace(x0,y0,w,h);
      let p = HTomb.Player;
      if (one) {
        let two = HTomb.Things.WarpedObelisk.findPlace(1,1,LEVELW-1,LEVELH-1, {
          function(x,y,z) {
            if (!valid(x,y,z)) {
              return false;
            } else if (HTomb.Path.quickDistance(one.x,one.y,one.z,x,y,z)<=50) {
              return false
            } else if (HTomb.Path.quickDistance(p.x,p.y,p.z,x,y,z)<=25) {
              return false;
            }
            return true;
          }
        });
        if (two) {
          let o1 = HTomb.Things.WarpedObelisk.spawn().place(one.x,one.y,one.z);
          let o2 = HTomb.Things.WarpedObelisk.spawn().place(two.x,two.y,two.z);
          o1.linked = o2;
          o2.linked = o1;
          o2.symbol = "\u260B";
        }
      }
    }
  });

  Parcel.extend({
    template: "HotSpringsParcel",
    name: "hot springs parcel",
    frequency: 1,
    biomes: ["DeepMountains","DeepWastes"],
    populate: function(x0,y0,w,h) {
      let springs = ROT.RNG.getUniformInt(2,5);
      // should these be clustered more?
      for (let i=0; i<springs; i++) {
        let xyz = HTomb.Things.HotSprings.findPlace(x0,y0,w,h);
        if (xyz) {
          HTomb.Things.HotSprings.spawn().place(xyz.x,xyz.y,xyz.z);
        }
      }
    }
  });

  Parcel.extend({
    template: "RadiantObeliskParcel",
    name: "radiant obelisk parcel",
    frequency: 1,
    biomes: ["DeepMountains","DeepWastes","DeepForest","BorderOcean","BorderForest","BorderWastes","OuterHills"],
    populate: function(x0,y0,w,h) {
      let xyz = HTomb.Things.RadiantObelisk.findPlace(x0,y0,w,h);
      if (xyz) {
        HTomb.Things.RadiantObelisk.spawn().place(xyz.x,xyz.y,xyz.z);
      }
    }
  });

  Parcel.extend({
    template: "OvergrownObeliskParcel",
    name: "overgrown obelisk parcel",
    frequency: 1,
    biomes: ["DeepMountains","DeepWastes","DeepForest","BorderOcean","BorderForest","BorderWastes","OuterHills"],
    populate: function(x0,y0,w,h) {
      let xyz = HTomb.Things.RadiantObelisk.findPlace(x0,y0,w,h);
      if (xyz) {
        HTomb.Things.RadiantObelisk.spawn().place(xyz.x,xyz.y,xyz.z);
      }
    }
  });


  Parcel.extend({
    template: "FirefliesParcel",
    name: "fireflies parcel",
    frequency: 4,
    biomes: ["BorderOcean","InnerHills","OuterHills","BorderForest","DeepForest"],
    populate: function(x0,y0,w,h) {
      let ffs = ROT.RNG.getUniformInt(4,8);
      // should these be clustered more?
      for (let i=0; i<ffs; i++) {
        let xyz = HTomb.Things.Firefly.findPlace(x0,y0,w,h);
        if (xyz) {
          HTomb.Things.Firefly.spawn().place(xyz.x,xyz.y,xyz.z);
        }
      }
    }
  });



  return HTomb;
})(HTomb);
