HTomb = (function(HTomb) {
  "use strict";

  HTomb.Things.defineItem({
    template: "Rock",
    name: "rock",
    symbol: "\u2022",
    fg: "#999999",
    stackable: true,
    randomColor: 15,
    tags: ["Minerals"]
  });

  HTomb.Things.defineItem({
    template: "FlintStone",
    name: "flint stone",
    symbol: "\u2022",
    fg: "#AAAAAA",
    stackable: true,
    randomColor: 15,
    tags: ["Minerals"]
  });

  HTomb.Things.defineItem({
    template: "Corpse",
    name: "corpse",
    symbol: "%",
    //symbol: "\u2620",
    fg: "brown",
    randomColor: 10,
    sourceCreature: null,
    onCreate: function(args) {
      if (this.sourceCreature) {
        if (typeof(this.sourceCreature)==="string") {
          this.sourceCreature = HTomb.Things[this.sourceCreature]();
          this.sourceCreature.despawn();
        }
        this.name = this.sourceCreature.name + " " + this.name;
        this.fg = ROT.Color.interpolate(ROT.Color.fromString(this.fg),ROT.Color.fromString(this.sourceCreature.fg));
        this.fg = ROT.Color.toHex(this.fg);
        this.sourceCreature = this.sourceCreature.template;
      }
      return this;
    }
  });

  HTomb.Things.defineItem({
    template: "Stick",
    name: "stick",
    symbol: "-",
    fg: "brown",
    randomColor: 20
  });

  HTomb.Things.defineItem({
    template: "WoodPlank",
    name: "wooden plank",
    symbol: "=",
    fg: "#BB9922",
    stackable: true,
    maxn: 10,
    tags: ["Wood"]
  });

  HTomb.Things.defineItem({
    template: "WorkAxe",
    name: "work axe",
    symbol: "\u262D",
    fg: "#BBAA88",
    ingredients: {"WoodPlank": 1, "Rock": 1},
    Behaviors: {Equipment: {slot: "MainHand", labor: 2}}
  });


  return HTomb;
})(HTomb);
