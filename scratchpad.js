

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



   



    let Biome = HTomb.Thing.Thing.extend({
      template: "Biome",
      name: "biome",
      x0: null,
      y0: null,
      z0: null,
      x1: null,
      y1: null,
      z1: null,
      corner: [null,null],
      modifyElevations: function() {

      }
    });

    Biome.extend({
      template: "Mountains",
      name: "mountains",
      modifyElevations: function() {
        let noise = new ROT.Noise.Simplex();
        for (let x=x0; x<x1; x++) {
          for (let y=y0; y<y1; y++) {
            let z0 = HTomb.Tiles.groundLevel(x,y);
            let r = Math.sqrt(Math.pow(x-this.corner[0],2) + Math.pow(y-this.corner[1]));
            if (r<16) {
              for (let z=z0; z<4; z++) {
                HTomb.World.tiles[z] = HTomb.Tiles.WallTile;
              }
            } else if (r<32) {
              for (let z=z0; z<3; z++) {
                HTomb.World.tiles[z] = HTomb.Tiles.WallTile;
              }
            } else if (r<48) {
              for (let z=z0; z<2; z++) {
                HTomb.World.tiles[z] = HTomb.Tiles.WallTile;
              }
            } else if (r<64) {
              for (let z=z0; z<1; z++) {
                HTomb.World.tiles[z] = HTomb.Tiles.WallTile;
              }
            }
          }
        }
      }
    });

    function generateBiomes() {
      let corners = ["Mountain","Swamp","Forest","Ocean"];
      corners = HTomb.Utils.shuffle(corners);
      let b;
      b = HTomb.Things[corners[0]].spawn({
        x0: 1,
        y0: 1,
        z0: NLEVELS-1,
        x1: LEVELW/4,
        y1: LEVELH/4,
        z1: 45,
        corner: [1,1]
      });
      b.modifyElevations;
    }


    var hscale1 = 256;
    var vscale1 = 3;
    var hscale2 = 64;
    //var hscale2 = 128;
    var vscale2 = 2;
    //var hscale3 = 32;
    var hscale3 = 64;
    var vscale3 = 1;
    var noise = new ROT.Noise.Simplex();
    var grid = [];
    var mx = 0, mn = NLEVELS;
    for (var x=0; x<LEVELW; x++) {
      grid.push([]);
      for (var y=0; y<LEVELH; y++) {
        grid[x][y] = ground;
        grid[x][y]+= noise.get(x/hscale1,y/hscale1)*vscale1;
        grid[x][y]+= noise.get(x/hscale2,y/hscale2)*vscale2;
        grid[x][y]+= noise.get(x/hscale3,y/hscale3)*vscale3;
        grid[x][y] = parseInt(grid[x][y]);
        mx = Math.max(mx,grid[x][y]);
        mn = Math.min(mn,grid[x][y]);
        if (x>0 && x<LEVELW-1 && y>0 && y<LEVELH-1) {
          for (var z=grid[x][y]; z>=0; z--) {
            HTomb.World.tiles[z][x][y] = HTomb.Tiles.WallTile;
          }
          z = grid[x][y]+1;
          HTomb.World.tiles[z][x][y] = HTomb.Tiles.FloorTile;
          HTomb.World.exposed[x][y] = z;
          elevTrack[z] = elevTrack[z]+1 || 1;
        }
      }
    }