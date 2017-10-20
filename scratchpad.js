

// the initial seed
Math.seed = 6;
 
// in order to work 'Math.seed' must NOT be undefined,
// so in any case, you HAVE to provide a Math.seed
Math.seededRandom = ;

HTomb.Utils.seed = 6;



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

  let Mineral = HTomb.Types.Type.extend({
    template: "Mineral",
    name: "mineral",
    onDefine: function(args) {
      let symbol = args.symbol || "\u2234";
      this.cover = args.template+((args.metallic) ? "Vein" : "Cluster")
      this.item = args.template+((args.metallic) ? "Ore" : "");
      HTomb.Types.Cover.extend({
        template: this.cover,
        name: args.name+((args.metallic) ? " vein" : " cluster"),
        base: args.template,
        mine: function(x,y,z,owner) {
          HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
          owner = owner || HTomb.Player;
          let base = HTomb.Types[this.base];
          let ore = HTomb.Things[base.item].spawn();
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

   Cover.extend({
    template: "Granite",
    name: "granite",
    earth: true,
    hardness: 3,
    thickness: 12,
    fg: "#AA9999",
    bg: "#776666"
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

   function placeEarths(layers) {
    layers = layers || ["Soil","Limestone","Basalt","Granite","Bedrock"];
    for (let x=1; x<LEVELW-1; x++) {
      for (let y=1; y<LEVELH-1; y++) {
        let z = HTomb.Tiles.groundLevel(x,y);
        for (let i=0; i<layers.length; i++) {
          let layer = HTomb.Covers[layers[i]];
          if (i===layers.length-1) {
            do {
              z-=1;
              if (HTomb.World.covers[z][x][y]===HTomb.Covers.NoCover) {
                HTomb.World.covers[z][x][y] = layer;
              }
            } while (z>0);
          } else {
            for (let j=0; j<layer.thickness; j++) {
              z-=1;
              if (HTomb.World.covers[z][x][y]===HTomb.Covers.NoCover) {
                HTomb.World.covers[z][x][y] = layer;
              }
            }
          }
        }
      }
    }



    FLOORFG: "#777799",
    WALLBG: "#777777",