HTomb = (function(HTomb) {
  "use strict";

  let Item = HTomb.Things.templates.Item;

  let mineral = {
    mine: function(x,y,z,owner) {
      HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
      owner = owner || HTomb.Player;
      let base = HTomb.Types.templates[this.base];
      let ore = HTomb.Things[base.item]();
      ore.place(x,y,z);
      if (owner) {
        ore.owned = true;
      }
    }
  };

  let Mineral = HTomb.Types.templates.Type.extend({
    template: "Mineral",
    name: "mineral",
    onDefine: function(args) {
      let symbol = args.symbol || "\u2234";
      this.cover = args.template+((args.metallic) ? "Vein" : "Cluster")
      this.item = args.template+((args.metallic) ? "Ore" : "");
      HTomb.Types.templates.Cover.extend({
        template: this.cover,
        name: args.name+((args.metallic) ? " vein" : " cluster"),
        base: args.template,
        mine: function(x,y,z,owner) {
          HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
          owner = owner || HTomb.Player;
          let base = HTomb.Types.templates[this.base];
          let ore = HTomb.Things[base.item]();
          ore.place(x,y,z);
          if (owner) {
            ore.owned = true;
          }
        },
        fg: args.fg,
        bg: HTomb.Constants.WALLBG,
        symbol: symbol,
        metallic: args.metallic,
        mineral: true,
        solid: true
      });
      Item.extend({
        template: this.item,
        name: args.name+((args.metallic) ? " ore" : ""),
        plural: (args.metallic) ? true : false,
        base: args.template,
        fg: args.fg,
        symbol: symbol,
        tags: ["Minerals"],
        metallic: args.metallic
      });
    }
  });

  Mineral.extend({
    template: "Iron",
    name: "iron",
    symbol: "\u2234",
    fg: "#C0C0C0",
    metallic: true
  });

  Mineral.extend({
    template: "Coal",
    name: "coal",
    symbol: "\u2234",
    fg: "#333333"
  });


  Mineral.extend({
    template: "Bloodstone",
    name: "bloodstone",
    fg: "red"
  });

  Mineral.extend({
    template: "Gold",
    name: "gold",
    fg: "yellow",
    metallic: true
  });

  Mineral.extend({
    template: "Moonstone",
    name: "moonstone",
    fg: "cyan"
  });

  Mineral.extend({
    template: "Jade",
    name: "jade",
    fg: "green"
  });

  let Cover = HTomb.Types.templates.Cover;

  Cover.extend({
    template: "Soil",
    name: "soil",
    earth: true,
    hardness: 0,
    thickness: 1,
    fg: "#BBBBAA",
    bg: "#888877"
  });

  Cover.extend({
    template: "Limestone",
    name: "limestone",
    earth: true,
    hardness: 1,
    thickness: 5,
    fg: "#AAAAAA",
    bg: "#777777"
  });

  Cover.extend({
    template: "Basalt",
    name: "basalt",
    earth: true,
    hardness: 2,
    thickness: 12,
    fg: "#999999",
    bg: "#666666"
  });

  Cover.extend({
    template: "Granite",
    name: "granite",
    earth: true,
    hardness: 3,
    thickness: 12,
    fg: "#AA9999",
    bg: "#776666"
  });

  Cover.extend({
    template: "Bedrock",
    name: "bedrock",
    earth: true,
    hardness: 4,
    fg: "#888888",
    bg: "#555555"
  });

return HTomb;
})(HTomb);
