HTomb = (function(HTomb) {
  "use strict";

  let Item = HTomb.Things.Item;
  let Component = HTomb.Things.Component;
  let Cover = HTomb.Types.Cover;

  Component.extend({
    template: "Mineral",
    name: "mineral",
    // should be layer, vein, or cluster
    yields: {Rock: 0.35},
    symbol: "#",
    nospawn: true,
    hardness: 0,
    mine: function(x,y,z,owner) {
      HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
      owner = owner || HTomb.Player;
      for (let i in this.yields) {
        // may not drop if n<1
        if (ROT.RNG.getUniform()<this.yields[i]) {
          let item = HTomb.Things[i].spawn({n: (this.yields[i]<1) ? 1 : this.yields[i]});
          item.place(x,y,z);
          // is this always true?
          item.owned = true;
        }
      }
    },
    onPrespawn: function(args) {
      args = args || {};
      if (args.yields) {
        for (let item in args.yields) {
          if (HTomb.Things[item]===undefined) {
            let it = {
              template: item,
              name: HTomb.Utils.splitPropCase(item).toLowerCase(),
              fg: args.fg || this.Template.fg,
              symbol: args.symbol || this.Template.symbol
            };
            if (args.plural) {
              it.plural = args.plural;
            }
            Item.extend(it);
          }
        }
      }
    }
  });




  Cover.extend({
    template: "Soil",
    name: "soil",
    fg: "#BBBBAA",
    bg: "#888877",
    Components: {
      Mineral: {
        hardness: 0
      }
    }
  });

  Cover.extend({
    template: "Limestone",
    name: "limestone",
    earth: true,
    hardness: 1,
    thickness: 5,
    fg: "#999999",
    bg: "#666666",
    Components: {
      Mineral: {
        hardness: 1
      }
    }
  });

  Cover.extend({
    template: "Basalt",
    name: "basalt",
    earth: true,
    hardness: 2,
    thickness: 12,
    fg: "#9999BB",
    bg: "#666688",
    Components: {
      Mineral: {
        hardness: 2
      }
    }
  });

  Cover.extend({
    template: "Granite",
    name: "granite",
    fg: "#AA9999",
    bg: "#776666",
    Components: {
      Mineral: {
        hardness: 3
      }
    }
  });

  Cover.extend({
    template: "Bedrock",
    name: "bedrock",
    fg: "#778877",
    bg: "#445544",
    Components: {
      Mineral: {
        hardness: 4
      }
    }
  });

  Cover.extend({
    template: "IronVein",
    name: "iron vein",
    symbol: "\u2234",
    fg: "#C0C0C0",
    Components: {
      Mineral: {
        hardness: 2,
        plural: true,
        yields: {IronOre: 1}
      }
    }
  });

  Cover.extend({
    template: "GoldVein",
    name: "gold vein",
    symbol: "\u2234",
    fg: "yellow",
    Components: {
      Mineral: {
        hardness: 1,
        yields: {GoldOre: 1}
      }
    }
  });

  Cover.extend({
    template: "CoalSeam",
    name: "coal seam",
    fg: "#C0C0C0",
    symbol: "\u2234",
    Components: {
      Mineral: {
        hardness: 0,
        plural: true,
        yields: {Coal: 1}
      }
    }
  });

  Cover.extend({
    template: "BloodstoneCluster",
    name: "bloodstone cluster",
    fg: "red",
    symbol: "\u2234",
    Components: {
      Mineral: {
        hardness: 0,
        symbol: "\u2666",
        yields: {Bloodstone: 1}
      }
    }
  });

  Cover.extend({
    template: "MoonstoneCluster",
    name: "moonstone cluster",
    fg: "cyan",
    symbol: "\u2234",
    Components: {
      Mineral: {
        hardness: 0,
        symbol: "\u2666",
        yields: {Moonstone: 1}
      }
    }
  });

  Cover.extend({
    template: "JadeCluster",
    name: "jade cluster",
    fg: "green",
    symbol: "\u2234",
    Components: {
      Mineral: {
        symbol: "\u2666",
        hardness: 0,
        yields: {Jade: 1}
      }
    }
  });



  // //!!! failed experiment, revisit later
  // HTomb.Covers.Soil.extend({
  //   template: "Sand",
  //   name: "sand",
  //   fg: "#BBBB88",
  //   bg: "#888855"
  // });

return HTomb;
})(HTomb);
