// Features are large, typically immobile objects
HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var coord = HTomb.Utils.coord;

  HTomb.Things.defineFeature({
    template: "Tombstone",
    name: "tombstone",
    symbol: "\u2670",
    fg: "#AAAAAA",
    randomColor: 5,
    onPlace: function(x,y,z) {
      // Bury a corpse beneath the tombstone
      HTomb.Things.create("Corpse").place(x,y,z-1);
    },
    explode: function(cause) {
      var x = this.x;
      var y = this.y;
      var z = this.z;
      this.destroy();
      HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
      for (var i=0; i<ROT.DIRS[8].length; i++) {
        var x1 = ROT.DIRS[8][i][0]+x;
        var y1 = ROT.DIRS[8][i][1]+y;
        if (HTomb.World.tiles[z][x1][y1].solid!==true) {
          // don't drop rocks on tombstones, it could confuse new players
          let f = HTomb.World.features[coord(x1,y1,z)];
          if (f && f.template==="Tombstone") {
            continue;
          }
          if (Math.random()<0.4) {
            var rock = HTomb.Things.Rock();
            rock.item.n = 1;
            rock.place(x1,y1,z);
            if (cause) {
              rock.item.setOwner(cause);
            }
            //rock.item.setOwner(HTomb.Player);
          }
        }
      }
    }
  });

  HTomb.Things.defineFeature({
    template: "Tree",
    name: "tree",
    //symbol: "\u2663",
    symbol: ["\u2663","\u2660"],
    fg: "#77BB00",
    randomColor: 10,
    integrity: 15,
    yields: {WoodPlank: {n: 1, nozero: true}}
  });

  HTomb.Things.defineFeature({
    template: "Shrub",
    name: "shrub",
    symbol: "\u2698",
    fg: "#779922",
    randomColor: 15
  });

  HTomb.Things.defineFeature({
    template: "Seaweed",
    name: "seaweed",
    plural: true,
    //symbol: ["\u2648","\u2724","\u060F"],
    symbol: ["\u0633","\u2724","\u060F"],
    fg: "#779922",
    randomColor: 20
  });

  HTomb.Things.defineFeature({
    template: "Puddle",
    name: "puddle",
    symbol: "~",
    fg: "#0088DD",
    randomColor: 20
  });

  HTomb.Things.defineFeature({
    template: "Throne",
    name: "throne",
    craftable: true,
    //symbol: "\u2655",
    symbol: "\u265B",
    fg: "#CCAA00",
    ingredients: {GoldOre: 1}
  });

  HTomb.Things.defineFeature({
    template: "ScryingGlass",
    craftable: true,
    name: "scrying glass",
    symbol: "\u25CB",
    fg: "cyan",
    ingredients: {Moonstone: 1}
  });

  HTomb.Things.defineFeature({
    template: "Pentagram",
    craftable: true,
    name: "pentagram",
    symbol: "\u26E7",
    fg: "red",
    ingredients: {Bloodstone: 1}
  });

  HTomb.Things.defineFeature({
    template: "Torch",
    name: "torch",
    craftable: true,
    symbol: "\u2AEF",
    fg: "yellow",
    Behaviors: {PointLight: {}},
    ingredients: {WoodPlank: 1}
  });

  HTomb.Things.defineFeature({
    template: "FakeTorch",
    name: "torch",
    symbol: "\u2AEF",
    fg: "yellow"
  });

  HTomb.Things.defineFeature({
    template: "Door",
    name: "door",
    locked: false,
    symbol: "\u25A5",
    fg: "#BB9922",
    labor: 20,
    craftable: true,
    activate: function() {
      if (this.locked) {
        HTomb.GUI.sensoryEvent("Unlocked " + this.describe()+".",this.x, this.y, this.z);
        this.locked = false;
        this.solid = false;
        this.name = "door";
        this.symbol = "\u25A5";
      } else {
        HTomb.GUI.sensoryEvent("Locked " + this.describe()+".",this.x,this.y,this.z);
        this.locked = true;
        this.solid = true;
        this.name = "locked door";
        this.symbol = "\u26BF";
      }
      HTomb.GUI.reset();
    },
    solid: false,
    opaque: true,
    integrity: 50,
    ingredients: {WoodPlank: 1}
  });

  HTomb.Things.defineFeature({
    template: "Excavation",
    name: "excavation",
    labor: 15,
    incompleteSymbol: "\u2717",
    incompleteFg: HTomb.Constants.BELOWFG,
    onPlace: function(x,y,z) {
      var tiles = HTomb.World.tiles;
      var EmptyTile = HTomb.Tiles.EmptyTile;
      var FloorTile = HTomb.Tiles.FloorTile;
      var WallTile = HTomb.Tiles.WallTile;
      var UpSlopeTile = HTomb.Tiles.UpSlopeTile;
      var DownSlopeTile = HTomb.Tiles.DownSlopeTile;
      var t = tiles[z][x][y];
      let c = HTomb.World.covers[z][x][y];
      // If there is a slope below, dig out the floor
      if (tiles[z-1][x][y]===UpSlopeTile && HTomb.World.explored[z-1][x][y] && (t===WallTile || t===FloorTile)) {
        tiles[z][x][y] = DownSlopeTile;
      // If it's a wall, dig a tunnel
      } else if (t===WallTile) {
        tiles[z][x][y] = FloorTile;
        if (c.mine) {
          c.mine(x,y,z);
        }
      } else if (t===FloorTile) {
        // If it's a floor with a wall underneath dig a trench
        if (tiles[z-1][x][y]===WallTile) {
          tiles[z][x][y] = DownSlopeTile;
          tiles[z-1][x][y] = UpSlopeTile;
          c = HTomb.World.covers[z-1][x][y];
          if (c.mine) {
            c.mine(x,y,z-1);
          }
        // Otherwise just remove the floor
        } else {
          tiles[z][x][y] = EmptyTile;
        }
      // If it's a down slope tile, remove the slopes
      } else if (t===DownSlopeTile) {
        tiles[z][x][y] = EmptyTile;
        tiles[z-1][x][y] = FloorTile;
      // if it's an upward slope, remove the slope
      } else if (t===UpSlopeTile) {
        tiles[z][x][y] = FloorTile;
        if (tiles[z+1][x][y]===DownSlopeTile) {
          tiles[z+1][x][y] = EmptyTile;
        }
      } else if (t===EmptyTile) {
        // this shouldn't happen
      }
      // Eventually this might get folded into mining...
      HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
      if (Math.random()<0.25) {
        var rock = HTomb.Things.Rock();
        rock.item.n = 1;
        if (tiles[z][x][y]===DownSlopeTile) {
          let item = rock.place(x,y,z-1);
          item.item.setOwner(HTomb.Player);
        } else {
          let item = rock.place(x,y,z);
          item.item.setOwner(HTomb.Player);
        }
      }
      HTomb.World.validate.cleanNeighbors(x,y,z);
      this.despawn();
    }
  });


  HTomb.Things.defineFeature({
    template: "Construction",
    name: "construction",
    incompleteSymbol: "\u2692",
    labor: 15,
    incompleteFg: HTomb.Constants.WALLFG,
    onPlace: function(x,y,z) {
      var tiles = HTomb.World.tiles;
      var EmptyTile = HTomb.Tiles.EmptyTile;
      var FloorTile = HTomb.Tiles.FloorTile;
      var WallTile = HTomb.Tiles.WallTile;
      var UpSlopeTile = HTomb.Tiles.UpSlopeTile;
      var DownSlopeTile = HTomb.Tiles.DownSlopeTile;
      var t = tiles[z][x][y];
      HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
      // If it's a floor, build a slope
      if (t===FloorTile) {
        tiles[z][x][y] = UpSlopeTile;
        if (tiles[z+1][x][y]===EmptyTile) {
          tiles[z+1][x][y] = DownSlopeTile;
        }
      // If it's a slope, make it into a wall
    } else if (t===UpSlopeTile) {
        tiles[z][x][y] = WallTile;
        if (tiles[z+1][x][y] === DownSlopeTile) {
          tiles[z+1][x][y] = FloorTile;
        }
      // If it's empty, add a floor
      } else if (t===DownSlopeTile || t===EmptyTile) {
        tiles[z][x][y] = FloorTile;
      }
      HTomb.World.validate.cleanNeighbors(x,y,z);
      this.despawn();
    }
  });

  HTomb.Things.defineFeature({
    template: "IncompleteFeature",
    name: "incomplete feature",
    symbol: "\u25AB",
    fg: "#BB9922",
    makes: null,
    finished: false,
    labor: 5,
    onCreate: function(args) {
      this.makes = args.makes;
      this.labor = this.makes.labor || this.labor;
      this.symbol = this.makes.incompleteSymbol || this.symbol;
      this.fg = this.makes.incompleteFg || this.makes.fg || this.fg;
      this.name = "incomplete "+this.makes.name;
      return this;
    },
    work: function(assignee) {
      let labor = assignee.worker.labor;
      if (assignee.equipper && assignee.equipper.slots.MainHand && assignee.equipper.slots.MainHand.equipment.labor>labor) {
        labor = assignee.equipper.slots.MainHand.equipment.labor;
      }
      // need to account for work axes somehow
      this.labor-=labor;
      if (this.labor<=0) {
        this.finish();
      }
    },
    finish: function() {
      var x = this.x;
      var y = this.y;
      var z = this.z;
      // need to swap over the stack, if necessary...
      this.finished = true;
      this.remove();
      this.makes.place(x,y,z);
      this.despawn();
    }
  });


  HTomb.Types.define({
    template: "Cover",
    name: "cover",
    liquid: false,
    stringify: function() {
      return HTomb.Types.templates[this.parent].types.indexOf(this);
    },
    shimmer: function() {
      var bg = ROT.Color.fromString(this.bg);
      bg = ROT.Color.randomize(bg,[bg[0]/16, bg[1]/16, bg[2]/16]);
      bg = ROT.Color.toHex(bg);
      return bg;
    },
    darken: function() {
      if (this.bg===undefined) {
        console.log(this);
        alert("problems in code!");
      }
      var bg = ROT.Color.fromString(this.bg);
      bg = ROT.Color.multiply(bg,[72,128,192]);
      bg = ROT.Color.toHex(bg);
      return bg;
    },
    flood: function(x,y,z) {
      var t = HTomb.World.covers[z-1][x][y];
      var water;
      if (HTomb.World.tiles[z-1][x][y].solid!==true && t.liquid===undefined) {
        HTomb.World.covers[z][x][y] = this;
        this.flood(x,y,z);
        // if we flood below, don't flood to the sides...should this happen each turn?
        return;
      }
      var neighbors = HTomb.Tiles.neighbors(x,y,4);
      for (var i=0; i<neighbors.length; i++) {
        x = neighbors[i][0];
        y = neighbors[i][1];
        t = HTomb.World.covers[z][x][y];
        if (HTomb.World.tiles[z][x][y].solid===true || t.liquid) {
          continue;
        }
        HTomb.World.covers[z][x][y] = this;
        this.flood(x,y,z);
      }
    }
  });

  HTomb.Types.defineCover({
    template: "NoCover",
    name: "none"
  });

  HTomb.Types.defineCover({
    template: "Water",
    name: "water",
    symbol: "~",
    flowSymbol: "\u2248",
    liquid: true,
    fg: HTomb.Constants.WATERFG || "#3388FF",
    bg: HTomb.Constants.WATERBG || "#1144BB"
  });

  HTomb.Types.defineCover({
    template: "Lava",
    name: "lava",
    symbol: "~",
    flowSymbol: "\u2248",
    liquid: true,
    fg: "#FF8833",
    bg: "#DD4411"
  });

  HTomb.Types.defineCover({
    template: "Grass",
    name: "grass",
    symbol: '"',
    fg: HTomb.Constants.GRASSFG ||"#668844",
    bg: HTomb.Constants.GRASSBG || "#334422",
    darken: function() {
      var bg = ROT.Color.fromString(this.bg);
      bg = ROT.Color.multiply(bg,[72,128,128]);
      bg = ROT.Color.toHex(bg);
      return bg;
    },
    onDefine: function() {
      HTomb.Events.subscribe(this,"TurnBegin");
    },
    onTurnBegin: function() {
      if (HTomb.Time.dailyCycle.turn%50!==0) {
        return;
      }
      for (let x=1; x<LEVELW-1; x++) {
        for (let y=1; y<LEVELH-1; y++) {
          if (Math.random()>=0.1) {
            continue;
          }
          let z = HTomb.Tiles.groundLevel(x,y);
          // don't grow over slopes or features I guess
          if (HTomb.World.tiles[z][x][y]!==HTomb.Tiles.FloorTile || HTomb.World.covers[z][x][y]!==HTomb.Covers.NoCover || HTomb.World.features[coord(x,y,z)]) {
            continue;
          }
          // count adjacent grass
          var n = HTomb.Tiles.countNeighborsWhere(x,y,z,function(x,y,z) {
            return (HTomb.World.covers[z][x][y]===HTomb.Covers.Grass);
          });
          if (n>0) {
            HTomb.World.covers[z][x][y] = HTomb.Covers.Grass;
          }
        }
      }
    }
  });

  HTomb.Types.defineCover({
    template: "Road",
    name: "road",
    symbol: '\u25CB',
    fg: HTomb.Constants.WALLFG,
    bg: HTomb.Constants.WALLBG
  });

  return HTomb;
})(HTomb);
