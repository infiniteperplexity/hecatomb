// Features are large, typically immobile objects
HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var coord = HTomb.Utils.coord;

  let Entity = HTomb.Things.Entity;
  let Feature = Entity.extend({
    template: "Feature",
    name: "feature",
    solid: false,
    yields: null,
    //i kind of hate this name
    integrity: null,
    tooltip: "A generic feature tooltip",
    validTiles: ["FloorTile"],
    fall: function() {
      var g = HTomb.Tiles.groundLevel(this.x,this.y,this.z);
      if (this.creature) {
        if (HTomb.World.creatures[coord(this.x,this.y,g)]) {
          alert("haven't decided how to handle falling creature collisions");
        } else {
          HTomb.GUI.sensoryEvent(this.describe({capitalized: true, article: "indefinite"}) + " falls " + (this.z-g) + " stories!",this.x,this.y,this.z);
          this.place(this.x,this.y,g);
        }
      }
      if (this.item) {
        HTomb.GUI.sensoryEvent(this.describe({capitalized: true, article: "indefinite"}) + " falls " + (this.z-g) + " stories!",this.x,this.y,this.z);
        this.place(this.x,this.y,g);
      }
      HTomb.GUI.render();
    },
    onDefine: function(args) {
      if (args.craftable===true) {
        let item = HTomb.Utils.copy(args);
        item.template = args.template+"Item";
        item.tags = ["Fixtures"];
        delete item.Components;
        HTomb.Things.Item.extend(item);
        let template = HTomb.Things[args.template];
        // overwrite the item's ingredients
        template.ingredients = {};
        template.ingredients[args.template+"Item"] = 1;
        template.labor = args.labor || 10;
      }
    },
    place: function(x,y,z,args) {
      Entity.place.call(this,x,y,z,args);
      if (this.isPlaced()===false) {
        return;
      }
      let c = coord(x,y,z);
      let f = HTomb.World.features[c];
      if (f) {
        console.log(this);
        console.log(f);
        throw new Error("unhandled feature conflict!");
      }
      HTomb.World.features[c] = this;
      if (this.solid) {
        HTomb.World.blocks[c] = this;
      }
    },
    remove: function(args) {
      let c = coord(this.x,this.y,this.z);
      if (HTomb.World.features[c]) {
        delete HTomb.World.features[c];
      }
      if (this.solid && HTomb.World.blocks[c]) {
        delete HTomb.World.blocks[c];
        HTomb.Path.reset();
      }
      Entity.remove.call(this,args);
    },
    validPlace: function(x,y,z) {
      if (HTomb.World.features[coord(x,y,z)]) {
        return false;
      // some features can be placed on slopes
      } else if (this.validTiles.indexOf(HTomb.World.tiles[z][x][y].template)!==-1) {
        return true;
      } else {
        return false;
      }
    },
    dismantle: function(optionalTask) {
      if (this.integrity===null) {
        this.integrity=5;
      }
      let labor = 1;
      this.integrity-=labor;
      // need to account for work axes somehow
      if (this.integrity<=0) {
        if (this.harvestable) {
          this.harvestable.harvest();
        }
        this.destroy();
      }
    }
  });

  Feature.extend({
    template: "Tombstone",
    name: "tombstone",
    symbol: "\u2670",
    fg: "#AAAAAA",
    validTiles: ["FloorTile","UpSlopeTile"],
    onPlace: function(x,y,z) {
      // Bury a corpse beneath the tombstone
      HTomb.Things.Corpse.spawn().place(x,y,z-1);
    }
  });

  Feature.extend({
    template: "Tree",
    name: "tree",
    fg: "#77BB00",
    integrity: 15,
    validTiles: ["FloorTile","UpSlopeTile"],
    Components: {
      Harvestable: {
        labor: 15,
        yields: {
          WoodPlank: 1
        }
      },
      Distinctive: {
        symbols: ["\u2663","\u2660"],
        fgRandomRed: 15,
        fgRandomGreen: 15,
        fgRandomBlue: 15
      }
    }
  });

  Feature.extend({
    template: "Shrub",
    name: "shrub",
    symbol: "\u2698",
    fg: "#779922",
    validTiles: ["FloorTile","UpSlopeTile"],
    Components: {
      Distinctive: {
        fgRandomRed: 15,
        fgRandomGreen: 15,
        fgRandomBlue: 15
      }
    }
  });

  Feature.extend({
    template: "Seaweed",
    name: "seaweed",
    plural: true,
    fg: "#779922",
    validTiles: ["FloorTile","UpSlopeTile"],
    Components: {
      Distinctive: {
        symbols: ["\u0633","\u2724"],
        fgRandomRed: 20,
        fgRandomGreen: 20,
        fgRandomBlue: 20
      }
    }
  });

  Feature.extend({
    template: "Puddle",
    name: "puddle",
    symbol: "~",
    fg: "#0088DD"
  });

  Feature.extend({
    template: "Throne",
    name: "throne",
    symbol: "\u265B",
    fg: "#CCAA00",
    Components: {
      Craftable: {
        ingredients: {GoldOre: 1}
      },
      Fixture: {}
    }
  });

  Feature.extend({
    template: "ScryingGlass",
    name: "scrying glass",
    symbol: "\u25CB",
    fg: "cyan",
    Components: {
      Craftable: {
        ingredients: {MoonStone: 1}
      },
      Fixture: {}
    }
  });

  Feature.extend({
    template: "Pentagram",
    name: "pentagram",
    symbol: "\u26E7",
    fg: "red",
    Components: {
      Craftable: {
        ingredients: {BloodStone: 1}
      },
      Fixture: {}
    }
  });

  Feature.extend({
    template: "Torch",
    name: "torch",
    symbol: "\u2AEF",
    fg: "yellow",
    Components: {
      PointLight: {},
      Craftable: {
        ingredients: {WoodPlank: 1}
      },
      Fixture: {
        tooltip: "(A furnished torch provides stationary light.)"
      }
    }
  });

  Feature.extend({
    template: "FakeTorch",
    name: "torch",
    symbol: "\u2AEF",
    fg: "yellow"
  });

  Feature.extend({
    template: "Boulder",
    name: "boulder",
    symbol: "\u26AB",
    fg: "#999999",
    validTiles: ["FloorTile","UpSlopeTile"],
    Components: {
      Distinctive: {
        symbols: ["\u2B24","\u25CF","\u25CF","\u2022","\u2022"],
        fgRandomRed: 5,
        fgRandomGreen: 5,
        fgRandomBlue: 5
      },
      Harvestable: {
        labor: 15,
        yields: {Rock: 1}
      }
    }
  });

//2617 is the tombstone looking thing, 26AB, 25CF
  //https://unicode-table.com/en/#2042

  Feature.extend({
    template: "Ramp",
    name: "ramp",
    // for pathing resets
    solid: true,
    onPlace: function(x,y,z) {
      let t = HTomb.World.tiles;
      if (t[z][x][y]===HTomb.Tiles.FloorTile) {
        t[z][x][y] = HTomb.Tiles.UpSlopeTile;
        if (t[z+1][x][y]===HTomb.Tiles.EmptyTile) {
          t[z+1][x][y] = HTomb.Tiles.DownSlopeTile;
        } 
      }
      this.despawn();
    },
    Components: {
      Fixture: {
        labor: 5,
        ingredients: {Rock: 1},
        incompleteSymbol: "\u2692",
        incompleteFg: HTomb.Constants.WALLFG,
        tooltip: "(Creates an upward slope.)"
      }
    }
  });

  Feature.extend({
    template: "Door",
    name: "door",
    solid: true,
    opaque: true,
    locked: false,
    symbol: "\u25A5",
    fg: "#BB9922",
    integrity: 50,
    Components: {
      Defender: {
        material: "Wood",
        toughness: 9,
        evasion: -10
      },
      Craftable: {
        labor: 20,
        ingredients: {WoodPlank: 1}
      },
      Fixture: {
        tooltip: "(A door blocks your enemies but not you or your minions.)",
        unplacedSymbol: "\u25A4"
      }
    }
  });

  Feature.extend({
    template: "SpearTrap",
    name: "spear trap",
    symbol: "\u2963",
    fg: "#CC9944",
    integrity: 10,
    Components: {
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
      },
      Craftable: {
        labor: 10,
        ingredients: {WoodPlank: 2}
      },
      Fixture: {
        unplacedSymbol: "\u2964",
        tooltip: "(A spring-loaded, spiked stick that attacks your enemies.)"
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
      && (!this.owner || !c.actor || this.owner.actor.team!==c.actor.team)
      && !c.movement.flies) {
        this.trap.spring(c.x,c.y,c.z);
      }
    }
  });

  return HTomb;
})(HTomb);
