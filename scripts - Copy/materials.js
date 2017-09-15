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
    template: "Blood",
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
        if (this[types[i].template]) {
          this.table[this.template][types[i].template] = this[types[i].template];
        } else {
          HTomb.Debug.pushMessage("Warning: No damage/material lookup for " + this.template + "/" + types[i].template + ".");
        }
      }
    }
  });

  HTomb.Types.defineDamage({
  	template: "Slashing",
  	name: "slashing",
    Bone: 0.5,
    Flesh: 1,
    Blood: 1.5,
    Wood: 1.5
  });

  HTomb.Types.defineDamage({
  	template: "Piercing",
  	name: "piercing",
    Bone: 0.5,
    Flesh: 1.5,
    Blood: 1,
    Wood: 0.5
  });


  HTomb.Types.defineDamage({
    template: "Crushing",
    name: "crushing",
    Bone: 1.5,
    Flesh: 1,
    Blood: 0.5,
    Wood: 1
  });

  HTomb.Types.defineDamage({
    template: "Acid",
    name: "acid",
    Bone: 1.5,
    Flesh: 1,
    Blood: 0.5,
    Wood: 1.5
  });



  //maybe this should just be a huge table?




  return HTomb;
})(HTomb);
