HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;

  //***********Types of material
  HTomb.Types.define({
  	template: "Material",
  	name: "material"
  });

  HTomb.Types.defineMaterial({
  	template: "Flesh",
  	name: "flesh"
  });

  HTomb.Types.defineMaterial({
  	template: "Bone",
  	name: "bone"
  });

  HTomb.Types.defineMaterial({
    template: "Wood",
    name: "wood"
  });


  //******Types of damage
  HTomb.Types.define({
  	template: "Damage",
    plural: "Damage",
  	name: "damage",
    table: {},
    onDefine: function() {
      this.table[this.template] = {};
      var types = HTomb.Types.templates.Material.types;
      for (var i=0; i<types.length; i++) {
        if (this[types[i].template]!==undefined) {
          this.table[this.template][types[i].template] = this[types[i].template];
        } else {
          console.log("Warning: No damage/material lookup for " + this.template + "/" + types[i].template + ".");
        }
      }
    }
  });

  HTomb.Types.defineDamage({
  	template: "Slashing",
  	name: "slashing",
    hits: {
      2: "grazes",
      4: "slashes",
      6: "cleaves",
      8: "massacres"
    },
    wounded: {
      2: "grazed",
      4: "slashed",
      6: "bleeding"
    },
    Bone: -1,
    Flesh: +0,
    Wood: +1,
  });

  HTomb.Types.defineDamage({
  	template: "Piercing",
  	name: "piercing",
    Bone: -1,
    Flesh: +1,
    Wood: -1
  });


  HTomb.Types.defineDamage({
    template: "Crushing",
    name: "crushing",
    Bone: +1,
    Flesh: +0,
    Wood: -1
  });

  HTomb.Types.defineDamage({
    template: "Acid",
    name: "acid",
    Bone: +1,
    Flesh: -1,
    Wood: +1
  });

  HTomb.Types.defineDamage({
    template: "Wither",
    name: "wither",
    Bone: -1,
    Flesh: +1,
    Wood: +1
  });



  //maybe this should just be a huge table?




  return HTomb;
})(HTomb);
