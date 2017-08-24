HTomb = (function(HTomb) {
  "use strict";

  let mineral = {
    mine: function(x,y,z,owner) {
      HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
      owner = owner || HTomb.Player;
      let base = HTomb.Types.templates[this.base];
      let ore = HTomb.Things[base.item]();
      ore.place(x,y,z);
      ore.item.setOwner(owner);

    }
  }
  HTomb.Types.define({
    template: "Mineral",
    name: "mineral",
    onDefine: function(args) {
      let symbol = args.symbol || "\u2234";
      this.cover = args.template+((args.metallic) ? "Vein" : "Cluster")
      this.item = args.template+((args.metallic) ? "Ore" : "");

      HTomb.Types.defineCover({
        template: this.cover,
        name: args.name+((args.metallic) ? " vein" : " cluster"),
        base: args.template,
        mine: mineral.mine,
        fg: args.fg,
        bg: HTomb.Constants.WALLBG,
        symbol: symbol,
        metallic: args.metallic,
        mineral: true,
        solid: true
      });

      HTomb.Things.defineItem({
        template: this.item,
        name: args.name+((args.metallic) ? " ore" : ""),
        plural: (args.metallic) ? true : false,
        base: args.template,
        fg: args.fg,
        symbol: symbol,
        tags: ["Minerals"],
        metallic: args.metallic
      });
      //defineMaterial
    }
  });

  HTomb.Types.defineMineral({
    template: "Iron",
    name: "iron",
    symbol: "\u2234",
    fg: "#C0C0C0",
    metallic: true
  });

  HTomb.Types.defineMineral({
    template: "Coal",
    name: "coal",
    symbol: "\u2234",
    fg: "#333333"
  });


  HTomb.Types.defineMineral({
    template: "Bloodstone",
    name: "bloodstone",
    fg: "red"
  });

  HTomb.Types.defineMineral({
    template: "Gold",
    name: "gold",
    fg: "yellow",
    metallic: true
  });

  HTomb.Types.defineMineral({
    template: "Moonstone",
    name: "moonstone",
    fg: "cyan"
  });

  HTomb.Types.defineMineral({
    template: "Jade",
    name: "jade",
    fg: "green"
  });

  HTomb.Types.defineCover({
    template: "Basalt",
    name: "basalt",
    symbol: "#",
    fg: "#BB9999",
    bg: "#776666"
  });

  HTomb.Types.defineCover({
    template: "Aquifer",
    name: "aquifer",
    symbol: "~",
    fg: HTomb.Constants.WATERFG || "#3388FF",
    bg: "#666677"
  });

return HTomb;
})(HTomb);
