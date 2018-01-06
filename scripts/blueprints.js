HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;

  let Blueprint = HTomb.Things.Thing.extend({
    template: "Blueprint",
    name: "blueprint",
    kind: "Creature",
    Components: {},
    addComponents: function(args) {
      args = args || {};
      args.Components = args.Components || {};
      for (let comp in this.Components) {
        args.Components[comp] = this.Components[comp];
      }
      return args;
    },
    spawn: function(args) {
      // always overridden
      return HTomb.Things.Thing.spawn(args);
    },
    extend: function(args) {
      // hopefully this never gets used
      let template = HTomb.Things.Thing.extend.call(this, args);
      return template;
    },
    findPlace: function(x0,y0,w,h,options) {
      // this is gonna get iffy...
      return HTomb.Things.Entity.findPlace.call(this,x0,y0,w,h,options);
    },
    validPlace: function(x,y,z) {
      // hopefully it doesn't come to this...
      return HTomb.Things[this.kind].validPlace.call(this,x,y,z);
    },
     chainPlace: function(x,y,z,options) {
       // this one is actually necessary
       return HTomb.Things.Entity.chainPlace.call(this,x,y,z,options);
    }
  });

  Blueprint.extend({
    template: "Zombie",
    name: "zombie",
    symbol: "z",
    fg: "#99FF66",
    Components: {
      Actor: {
        goals: ["ServeMaster"]
      },
      Defender: {
        toughness: 1
      },
      Distinctive: {
        fgRandomRed: 15,
        fgRandomGreen: 15,
        fgRandomBlue: 15
      },
      Decaying: {}
    },
    spawn: function (args) {
      args = this.addComponents(args);
      args.species = args.species || "Human";
      args.symbol = this.symbol;
      args.fg = this.fg;
      args.name = HTomb.Things[args.species].name + " " + this.name;
      args.tags = args.tags || [];
      args.tags.push(this.template);
      return HTomb.Things[args.species].spawn(args);
    }
  });
  
  return HTomb;
})(HTomb);
