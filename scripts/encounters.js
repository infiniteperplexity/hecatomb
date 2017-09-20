HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;


  let Entity = HTomb.Things.templates.Entity;

  Entity.extend({
    template: "Encounter",
    name: "encounter",
    creatures: [],
    addCreature: function(creature, args) {
      if (this.creatures===undefined) {
        this.creatures = [];
      }
      this.creatures.push(creature);
      creature.encounter = this;
    },
    place: function(x,y,z,args) {
      Entity.place.call(this,x,y,z,args);
      HTomb.World.encounters.push(this);
      return this;
    },
    remove: function(args) {
      if (HTomb.World.encounters.indexOf(this)!==-1) {
        HTomb.World.encounters.splice(HTomb.World.encounters.indexOf(this),1);
      }
      Entity.remove.call(this,args);
    }
  });

  // a helper function to avoid dropping one creature on top of another.
  function noCreature(x,y,z) {
    if (HTomb.World.creatures[coord(x,y,z)]===undefined) {
      return true;
    } else {
      return false;
    }
  }
  HTomb.Tiles.getEdgeSquare = function(callb) {
    callb = callb || noCreature;
    var TRIES = 100;
    var tries = 0;
    while (true) {
      tries+=1;
      var x = Math.floor(Math.random()*LEVELW);
      var y = Math.floor(Math.random()*LEVELH);
      var r = Math.random();
      if (r<0.5) {
        if (x>LEVELW/2) {
          x = LEVELW-2;
        } else {
          x = 1;
        }
      } else {
        if (y>LEVELH/2) {
          y = LEVELH-2;
        } else {
          y = 1;
        }
      }
      var z = HTomb.Tiles.groundLevel(x,y);
      if (callb(x,y,z)) {
        console.log("returning " + [x,y,z]);
        return [x,y,z];
      }
      if (tries>=TRIES) {
        alert("failed to find ");
        return;
      }
    }
  };



  return HTomb;
})(HTomb);
