// Features are large, typically immobile objects
HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var coord = HTomb.Utils.coord;

  let Cover = HTomb.Types.Type.extend({
    template: "Cover",
    name: "cover",
    stringify: function() {
      return HTomb.Types[this.parent].types.indexOf(this);
    },
    darken: function() {
      if (this.bg===undefined) {
        console.log(this);
        alert("problems in code!");
      }
      var bg = ROT.Color.fromString(this.bg);
      bg = ROT.Color.multiply(bg,[72,128,192]);
      bg = ROT.Color.toHex(bg);
      return bg;
    }
  });

  Cover.extend({
    template: "NoCover",
    name: "none"
  });

  Cover.extend({
    template: "Water",
    name: "water",
    symbol: "~",
    fg: HTomb.Constants.WATERFG || "#3388FF",
    bg: HTomb.Constants.WATERBG || "#1144BB",
    Components: {
      Liquid: {}
    }
  });

  Cover.extend({
    template: "Lava",
    name: "lava",
    symbol: "~",
    flowSymbol: "\u2248",
    liquid: true,
    fg: "#FF8833",
    bg: "#DD4411",
    Components: {
      Liquid: {}
    }
  });

  
  Cover.extend({
    template: "Grass",
    name: "grass",
    symbol: '"',
    fg: HTomb.Constants.GRASSFG ||"#668844",
    bg: HTomb.Constants.GRASSBG || "#334422",
    darken: function() {
      var bg = ROT.Color.fromString(this.bg);
      bg = ROT.Color.multiply(bg,[72,128,128]);
      bg = ROT.Color.toHex(bg);
      return bg;
    }
  });

  Cover.extend({
    template: "Snow",
    name: "snow",
    symbol: ".",  
    fg: "#CCCCEE",
    bg: "#BBBBCC",
    darken: function() {
      var bg = ROT.Color.fromString(this.bg);
      bg = ROT.Color.multiply(bg,[72,128,128]);
      bg = ROT.Color.toHex(bg);
      return bg;
    }
  });

  Cover.extend({
    template: "Muck",
    name: "muck",
    symbol: '"',  
    fg: HTomb.Constants.GRASSBG || "#334422",
    bg: "#223311",
    darken: function() {
      var bg = ROT.Color.fromString(this.bg);
      bg = ROT.Color.multiply(bg,[72,128,128]);
      bg = ROT.Color.toHex(bg);
      return bg;
    }
  });
  

  Cover.extend({
    template: "Road",
    name: "road",
    symbol: '\u25CB',
    fg: HTomb.Constants.WALLFG,
    bg: HTomb.Constants.WALLBG
  });

  Cover.extend({
    template: "Flooring",
    name: "flooring",
    symbol: '\u25CB',
    fg: "#997777",
    bg: "#665544"
  });

  return HTomb;
})(HTomb);
