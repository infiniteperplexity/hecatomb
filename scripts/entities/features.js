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
    onPlace: function(x,y,z) {
      // Bury a corpse beneath the tombstone
      HTomb.Things.Corpse.spawn().place(x,y,z-1);
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
            var rock = HTomb.Things.Rock.spawn();
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
    fg: "#77BB00",
    integrity: 15,
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
    Components: {
      Distinctive: {
        symbols: ["\u0633","\u2724","\u060F"],
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
