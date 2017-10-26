HTomb = (function(HTomb) {
  "use strict";
  
  let Item = HTomb.Things.Item;

  Item.extend({
    template: "Rock",
    name: "rock",
    symbol: "\u2022",
    fg: "#999999",
    tags: ["Minerals"]
  });

  Item.extend({
    template: "Corpse",
    name: "corpse",
    symbol: "%",
    fg: "brown",
    sourceCreature: null,
    // !!! should change to onDescribe;
    onSpawn: function(args) {
      if (this.sourceCreature) {
        if (typeof(this.sourceCreature)==="string") {
          this.sourceCreature = HTomb.Things[this.sourceCreature].spawn();
          this.sourceCreature.despawn();
        }
        this.name = this.sourceCreature.name + " " + this.name;
        this.fg = ROT.Color.interpolate(ROT.Color.fromString(this.fg),ROT.Color.fromString(this.sourceCreature.fg));
        this.fg = ROT.Color.toHex(this.fg);
        this.sourceCreature = this.sourceCreature.template;
      }
      return this;
    },
    Components: {
      Decaying: {
        suspended: true
      }
    }
  });

  Item.extend({
    template: "WoodPlank",
    name: "wooden plank",
    symbol: "=",
    fg: "#BB9922",
    tags: ["Wood"]
  });

  Item.extend({
    template: "TradeGoods",
    name: "trade goods",
    symbol: "\u2696",
    fg: "#AAAA44",
    plural: true
  });

  Item.extend({
    template: "Ectoplasm",
    name: "ectoplasm",
    symbol: "\u2697",
    fg: "cyan"
  });

  Item.extend({
    template: "Fuel",
    name: "fuel",
    symbol: "\u269B",
    fg: "cyan"
  });


  Item.extend({
    template: "WorkAxe",
    name: "work axe",
    symbol: "\u262D",
    fg: "#BBAA88",
    Components: {
      Equipment: {
        slot: "MainHand",
        labor: 2,
        accuracy: 0,
        damage: {
          type: "Slashing",
          level: 1
        }
      },
      Craftable: {
        ingredients: {"WoodPlank": 1, "Rock": 1}
      }
    }
  });


  return HTomb;
})(HTomb);
