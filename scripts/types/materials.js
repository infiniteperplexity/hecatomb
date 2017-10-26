HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;

  //***********Types of material
  let Type = HTomb.Types.Type;

  let Material = Type.extend({
  	template: "Material",
  	name: "material"
  });

  Material.extend({
  	template: "Flesh",
  	name: "flesh"
  });

  Material.extend({
  	template: "Bone",
  	name: "bone"
  });

  Material.extend({
    template: "Wood",
    name: "wood"
  });


  //******Types of damage
  let Damage = Type.extend({
  	template: "Damage",
    plural: "Damage",
  	name: "damage",
    table: {},
    onDefine: function() {
      // don't fire for the parent
      if (this.template==="Damage") {
        return;
      }
      this.table[this.template] = {};
      var types = HTomb.Types.Material.types;
      for (var i=0; i<types.length; i++) {
        if (this[types[i].template]!==undefined) {
          this.table[this.template][types[i].template] = this[types[i].template];
        } else {
          console.log("Warning: No damage/material lookup for " + this.template + "/" + types[i].template + ".");
        }
      }
    }
  });

  Damage.extend({
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

  Damage.extend({
  	template: "Piercing",
  	name: "piercing",
    Bone: -1,
    Flesh: +1,
    Wood: -1
  });


  Damage.extend({
    template: "Crushing",
    name: "crushing",
    Bone: +1,
    Flesh: +0,
    Wood: -1
  });

  Damage.extend({
    template: "Acid",
    name: "acid",
    Bone: +1,
    Flesh: -1,
    Wood: +1
  });

  Damage.extend({
    template: "Decay",
    name: "decay",
    Bone: -1,
    Flesh: +1,
    Wood: +1
  });

  Damage.extend({
    template: "Dismantle",
    name: "dismantle",
    Bone: -1,
    Flesh: +1,
    Wood: +1
  });



  //maybe this should just be a huge table?




  return HTomb;
})(HTomb);
