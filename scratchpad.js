

  let mineral = {
    mine: function(x,y,z,owner) {
      HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
      owner = owner || HTomb.Player;
      let base = HTomb.Types[this.base];
      let ore = HTomb.Things[base.item].spawn();
      ore.place(x,y,z);
      if (owner) {
        ore.owned = true;
      }
    }
  };

  


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




  Type.extend({
    template: "Biome",
    name: "biome",
    strata: []
  });

  Behavior.extend({
    template: "Earth",
    name: "earth",
    nospawn: true,
    hardness: 0,
    // for now
    depth: 0,
    floorSymbol: null,
    wallSymbol: null,
    floorFg: null,
    wallFg: null,
    floorBg: null,
    wallBg: null
  });

   