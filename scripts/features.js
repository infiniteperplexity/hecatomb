// Features are large, typically immobile objects
HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var coord = HTomb.Utils.coord;

  let Feature = HTomb.Things.templates.Feature;
  let Behavior = HTomb.Things.templates.Behavior;
  let Item = HTomb.Things.templates.Item;

  Feature.extend({
    template: "Tombstone",
    name: "tombstone",
    symbol: "\u2670",
    fg: "#AAAAAA",
    randomColor: 5,
    onPlace: function(x,y,z) {
      // Bury a corpse beneath the tombstone
      HTomb.Things.Corpse().place(x,y,z-1);
    },
    explode: function(args) {
      var x = this.x;
      var y = this.y;
      var z = this.z;
      this.destroy();
      let buriedItems = HTomb.World.items[coord(x,y,z-1)] || HTomb.Things.Items();
      for (let item of buriedItems) {
        item.owned = true;
      }
      HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
      for (let i=0; i<ROT.DIRS[8].length; i++) {
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
            rock.n = 1;
            rock.place(x1,y1,z);
            if (args) {
              rock.owned = true;
            }
          }
        }
      }
    }
  });

  Feature.extend({
    template: "Tree",
    name: "tree",
    //symbol: "\u2663",
    symbol: ["\u2663","\u2660"],
    fg: "#77BB00",
    randomColor: 10,
    integrity: 15,
    yields: {WoodPlank: {n: 1, nozero: true}}
  });

  Feature.extend({
    template: "Shrub",
    name: "shrub",
    symbol: "\u2698",
    fg: "#779922",
    randomColor: 15
  });

  Feature.extend({
    template: "Seaweed",
    name: "seaweed",
    plural: true,
    //symbol: ["\u2648","\u2724","\u060F"],
    symbol: ["\u0633","\u2724","\u060F"],
    fg: "#779922",
    randomColor: 20
  });

  Feature.extend({
    template: "Puddle",
    name: "puddle",
    symbol: "~",
    fg: "#0088DD",
    randomColor: 20
  });

  Feature.extend({
    template: "Throne",
    name: "throne",
    craftable: true,
    //symbol: "\u2655",
    symbol: "\u265B",
    fg: "#CCAA00",
    ingredients: {GoldOre: 1}
  });

  Feature.extend({
    template: "ScryingGlass",
    craftable: true,
    name: "scrying glass",
    symbol: "\u25CB",
    fg: "cyan",
    ingredients: {Moonstone: 1}
  });

  Feature.extend({
    template: "Pentagram",
    craftable: true,
    name: "pentagram",
    symbol: "\u26E7",
    fg: "red",
    ingredients: {Bloodstone: 1}
  });

  Feature.extend({
    template: "Torch",
    name: "torch",
    tooltip: "(A furnished torch provides stationary light.)",
    craftable: true,
    symbol: "\u2AEF",
    fg: "yellow",
    Behaviors: {PointLight: {}},
    ingredients: {WoodPlank: 1}
  });

  Feature.extend({
    template: "FakeTorch",
    name: "torch",
    symbol: "\u2AEF",
    fg: "yellow"
  });

  Feature.extend({
    template: "Ramp",
    name: "ramp",
    tooltip: "(Creates an upward slope.)",
    incompleteSymbol: "\u2692",
    incompleteFg: HTomb.Constants.WALLFG,
    labor: 5,
    // for pathing resets
    solid: true,
    // !!!!very ad hoc solution
    placeholder: true,
    ingredients: {Rock: 1},
    onPlace: function(x,y,z) {
      let t = HTomb.World.tiles;
      if (t[z][x][y]===HTomb.Tiles.FloorTile) {
        t[z][x][y] = HTomb.Tiles.UpSlopeTile;
        if (t[z+1][x][y]===HTomb.Tiles.EmptyTile) {
          t[z+1][x][y] = HTomb.Tiles.DownSlopeTile;
        } 
      }
    }
  });

  Feature.extend({
    template: "Door",
    name: "door",
    solid: true,
    opaque: true,
    locked: false,
    tooltip: "(A door blocks your enemies but not you or your minions.)",
    symbol: "\u25A5",
    fg: "#BB9922",
    labor: 20,
    craftable: true,
    integrity: 50,
    ingredients: {WoodPlank: 1},
    Behaviors: {
      Defender: {
        material: "Wood",
        toughness: 9,
        evasion: -10
      }
    }
  });



// U+27F1
// U+290B
// U+2927
// U+2963
// U+2964
// U+2ADD

  Feature.extend({
    template: "SpearTrap",
    name: "spear trap",
    symbol: "\u2963",
    fg: "#CC9944",
    tooltip: "(A spring-loaded, spiked stick that attacks your enemies.)",
    labor: 10,
    craftable: true,
    integrity: 10,
    sprung: false,
    ingredients: {WoodPlank: 2},
    Behaviors: {
      Attacker: {
        damage: {
          type: "Piercing",
          level: 2
        },
        accuracy: 2
      },
      Trap: {
        sprungSymbol: "\u2964",
        rearmCost: {WoodPlank: 1}
      }
    },
    onSpring: function(x,y,z) {
      let c = HTomb.World.creatures[coord(x,y,z)];
      this.attacker.attack(c);
    },
    onStep: function(event) {
      if (this.trap.sprung) {
        return;
      }
      let c = event.creature;
      if (c.x===this.x && c.y===this.y && c.z===this.z
      && (!this.owner || !c.ai || this.owner.ai.team!==c.ai.team)
      && !c.movement.flies) {
        this.trap.spring(c.x,c.y,c.z);
      }
    }
  });

  Behavior.extend({
    template: "Trap",
    name: "trap",
    listens: ["Step"],
    sprung: false,
    rearmLabor: 5,
    rearmEffort: 0,
    sprungSymbol: null,
    unsprungSymbol: null,
    onAdd: function() {
      for (let type of this.listens) {
        HTomb.Events.subscribe(this.entity,type);
      }
    },
    rearm: function() {
      this.sprung = false;
      if (this.unsprungSymbol) {
        this.entity.symbol = this.unsprungSymbol;
      } else {
        this.entity.symbol = HTomb.Things.templates[this.entity.template].symbol;
      }
      this.entity.name = HTomb.Things.templates[this.entity.template].name;
    },
    spring: function(x,y,z) {
      this.sprung = true;
      this.entity.onSpring(x,y,z);
      if (this.sprungSymbol) {
        this.entity.symbol = this.sprungSymbol;
      }
      this.entity.name = "sprung " + this.entity.name;
      this.entity.labor = this.rearmLabor;
      this.entity.effort = this.rearmEffort;
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

  HTomb.Types.defineCover({
    template: "Flooring",
    name: "flooring",
    symbol: '\u25CB',
    fg: "#997777",
    bg: "#665544"
  });


  return HTomb;
})(HTomb);
