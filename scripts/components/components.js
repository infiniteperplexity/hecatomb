// ****** This module implements Components, which are the basic units of functionality for creatures, items, and features
HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;

  let Component = HTomb.Things.Component;

  Component.extend({
    template: "PointLight",
    name: "pointlight",
    level: 255,
    range: 8,
    onPlace: function() {
      if (HTomb.World.lights.indexOf(this)===-1) {
        HTomb.World.lights.push(this);
      }
      HTomb.World.validate.lighting();
    },
    onRemove: function() {
      if (HTomb.World.lights.indexOf(this)!==-1) {
        HTomb.World.lights.splice(HTomb.World.lights.indexOf(this),1);
        HTomb.World.validate.lighting();
      }
    },
    illuminate: function() {
      let e = this.entity;
      HTomb.FOV.pointIlluminate(e.x,e.y,e.z,this.range);
    }
  });


  Component.extend({
    template: "Senses",
    name: "senses",
    sightRange: 10,
    audioRange: 10,
    smellRange: 10,
    darkSight: false,
    onAdd: function(options) {
      options = options || {};
      this.sightRange = options.sightRange || this.sightRange;
      this.audioRange = options.audioRange || this.audioRange;
      this.smellRange = options.smellRange || this.smellRange;
      this.darkSight = options.darkSight || this.darkSight;
    },
    canSee: function(x,y,z) {
      return true;
    },
    getSeen: function() {
      var squares = [];
      return squares;
    },
    canSmell: function(x,y,z) {
      return true;
    },
    getSmelled: function() {
      var squares = [];
      return squares;
    },
    canHear: function(x,y,z) {
      return true;
    },
    getHeard: function() {
      var squares = [];
      return squares;
    }
  });

  // The Sight bomponent allows a creature to see
  Component.extend({
    template: "Sight",
    name: "sight",
    range: 10,
    onAdd: function(options) {
      options = options || {};
      if (options.range) {
        this.range = options.range;
      }
    },
    getSeen: function() {

    }
  });

  // The SpellCaster bomponent maintains a list of castable spells
  Component.extend({
    template: "SpellCaster",
    name: "caster",
    baseSanity: 20,
    sanity: 20,
    onSpawn: function(options) {
      options = options || {};
      options.spells = options.spells || [];
      this.spells = [];
      for (let i=0; i<options.spells.length; i++) {
        this.spells.push(HTomb.Things[options.spells[i]].spawn({caster: this}));
        this.spells[i].caster = this;
      }
      HTomb.Events.subscribe(this,"TurnBegin");
      return this;
    },
    onTurnBegin: function() {
      if (this.sanity<this.getMaxSanity() && Math.random()<(1/10)) {
        this.sanity+=1;
      }
    },
    getMaxSanity: function() {
      let ent = this.baseSanity;
      if (this.entity.master) {
        for (let s of this.entity.owner.structures) {
          if (s.template==="Sanctum") {
            ent+=5;
          }
        }
      }
      return ent;
    },
    cast: function(sp) {
      let cost = sp.getCost();
      if (this.sanity>=cost) {
        sp.cast();
      }
    }
  });

  Component.extend({
    template: "Researchable",
    name: "researchable",
    turns: 48,
    nospawn: true,
    ingredients: {},
    finish: function() {
      //!!!odd that we have no logic here
    }
  });


  Component.extend({
    template: "Craftable",
    name: "craftable",
    nospawn: true,
    labor: 15,
    ingredients: {WoodPlank: 1}
  });

  Component.extend({
    template: "Harvestable",
    name: "harvestable",
    labor: 10,
    yields: {WoodPlank: 1},
    harvest: function() {
      let x = this.entity.x;
      let y = this.entity.y;
      let z = this.entity.z;
      for (let ing in this.yields) {
        HTomb.Things[ing].spawn({owned: true, n: this.yields[ing]}).place(x,y,z);
      }
    }
  });

  Component.extend({
    template: "Fixture",
    name: "fixture",
    nospawn: true,
    labor: 5,
    // this will play out kind of weirdly...
    tooltip: null,
    unplacedSymbol: null,
    unplacedFg: null,
    incompleteSymbol: null,
    incompleteFg: null,
    onPrespawn(args) {
      // Craftable Fixtures work much differently than non-Craftable Fixtures
      if (this.Template.Components.Craftable) {
        let iargs = {
          template: "Unplaced" + this.Template.template,
          name: "unplaced " + this.Template.name,
          symbol: this.unplacedSymbol || this.Template.symbol,
          fg: this.unplacedFg || this.Template.fg,
          makes: this.Template.template,
          Components: {
            Craftable: {
              ingredients: this.Template.Components.Craftable.ingredients || HTomb.Things.Craftable.ingredients
            }
          }
        };
        HTomb.Things.Item.extend(iargs);
        this.ingredients = {};
        this.ingredients["Unplaced" + this.Template.template] = 1;
      }
    }
    // ,dismantle() // should it be easier to dismantle?  into the item version?
  });

  Component.extend({
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
        this.entity.symbol = HTomb.Things[this.entity.template].symbol;
      }
      // This should be onDescribe I think
      this.entity.name = HTomb.Things[this.entity.template].name;
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

  Component.extend({
    template: "Liquid",
    name: "liquid",
    nospawn: true,
    flowSymbol: "\u2248",
    shimmer: function() {
      let bg = ROT.Color.fromString(this.Template.bg);
      bg = ROT.Color.randomize(bg,[bg[0]/16, bg[1]/16, bg[2]/16]);
      bg = ROT.Color.toHex(bg);
      return bg;
    },
    flood: function(x,y,z) {
      let t = HTomb.World.covers[z-1][x][y];
      if (HTomb.World.tiles[z-1][x][y].solid!==true && !t.liquid) {
        HTomb.World.covers[z-1][x][y] = this.Template;
        this.flood(x,y,z-1);
        // if we flood below, don't flood to the sides...should this happen each turn?
        return;
      }
      let neighbors = HTomb.Tiles.neighboringColumns(x,y,4);
      for (var i=0; i<neighbors.length; i++) {
        x = neighbors[i][0];
        y = neighbors[i][1];
        t = HTomb.World.covers[z][x][y];
        if (HTomb.World.tiles[z][x][y].solid===true || t.liquid) {
          continue;
        }
        HTomb.World.covers[z][x][y] = this.Template;
        this.flood(x,y,z);
      }
    }
  });

  Component.extend({
    template: "Distinctive",
    name: "distinctive",
    fg: null,
    fgs: null,
    fgRandomRed: 0,
    fgRandomGreen: 0,
    fgRandomBlue: 0,
    bg: null,
    bgs: null,
    bgRandomRed: 0,
    bgRandomGreen: 0,
    bgRandomBlue: 0,
    symbol: null,
    symbols: null,
    onAdd: function() {
      let e = this.entity;
      if (this.fgs) {
        e.fg = this.fgs[Math.floor(Math.random()*this.fgs.length)];
        this.fg = e.fg;
      }
      if (this.fgRandomRed || this.fgRandomGreen || this.fgRandBlue) {
        var c = ROT.Color.fromString(e.fg);
        c = ROT.Color.randomize(c,[this.fgRandomRed, this.fgRandomGreen, this.fgRandomBlue]);
        c = ROT.Color.toHex(c);
        e.fg = c;
        this.fg = e.fg;
      }
      if (this.bgs) {
        e.bg = this.bgs[Math.floor(Math.random()*this.bgs.length)];
        this.bg = e.bg;
      }
      if (this.bgRandomRed || this.bgRandomGreen || this.bgRandBlue) {
        var c = ROT.Color.fromString(e.bg);
        c = ROT.Color.randomize(c,[this.bgRandomRed, this.bgRandomGreen, this.bgRandomBlue]);
        c = ROT.Color.toHex(c);
        e.bg = c;
        this.bg = e.bg;
      }
      if (this.symbols) {
        e.symbol = this.symbols[Math.floor(Math.random()*this.symbols.length)];
        this.symbol = e.symbol;
      }
    }
  });

  return HTomb;
})(HTomb);
