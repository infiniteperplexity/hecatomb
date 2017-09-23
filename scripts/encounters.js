HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;


  let Entity = HTomb.Things.templates.Entity;

  let Encounter = Entity.extend({
    template: "Encounter",
    name: "encounter",
    listens: [],
    creatures: null,
    observed: true,
    blurb: "There is some kind of encounter going on!",
    alert: function() {
      HTomb.GUI.alert(this.blurb);
      //HTomb.GUI.Views.Main.tacticalMode();
    },
    isPlaced: function() {
      if (this.creatures && this.creatures.length>0) {
        return true;
      }
      return false;
    },
    onDefine: function(args) {
      args = args || {};
      let listens = args.listens || this.listens;
      for (let ev of listens) {
        HTomb.Events.subscribe(this,ev);
      }
    },
    addCreature: function(creature, args) {
      if (this.creatures===null) {
        this.creatures = [];
      }
      this.creatures.push(creature);
      creature.encounter = this;
    },
    removeCreature: function(creature, args) {
      if (this.creatures && this.creatures.indexOf(creature)!==-1) {
        this.creatures.splice(this.creatures.indexOf(this),1);
      } 
      delete creature.encounter;
      // despawn the encounter if every creature is dead
      if (this.creatures && this.creatures.length===0) {
        this.despawn();
      }
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

  Encounter.extend({
    template: "PeasantMob",
    name: "angry peasant mob",
    listens: ["TurnBegin"],
    blurb: "A mob of angry peasants approaches, determined to end your foul research.",
    onTurnBegin: function(event) {
      if (HTomb.Time.dailyCycle.turn===999) {
        let e = HTomb.Tiles.getEdgeSquare();
        this.spawn().place(e.x,e.y,e.z);
      }
    },
    onSpawn: function(args) {
      //this.addCreature(HTomb.Things.Peasant());
    },
    onPlace: function(x,y,z,args) {
      this.creatures[0].place(x,y,z);
    }
  });
  
  Encounter.extend({
    template: "AngryDryads",
    name: "angry dryads",
    listens: ["Destroy"],
    blurb: "Dryads emerge from the trees, angered by your desecration of the forest!",
    onDestroy: function(event) {
      // no dryads in the extremely early game
      if (HTomb.Time.dailyCycle.turn<500) {
        return;
      }
      let t = event.entity;
      if (t.template==="Tree") {
        let x = t.x;
        let y = t.y;
        let z = t.z;
        if (HTomb.Utils.dice(1,25)===1) {
          // really ought to standardize this...
          HTomb.Things[this.template]().place(x,y,z);
        }
      }
    },
    onSpawn: function() {
      this.addCreature(HTomb.Things.Dryad());
    },
    onPlace: function(x,y,z) {
      let dryad = this.creatures[0];
      let trees = HTomb.Utils.where(HTomb.World.features,
      function(e) {
        let d = HTomb.Path.quickDistance(e.x,e.y,e.z,x,y,z);
          return (e.template==="Tree" && d>=5 && d<=9);
        }
      );
      if (trees.length>0) {
        let tree = HTomb.Path.closest(x,y,z,trees)[0];
        if (HTomb.World.creatures[coord(x,y,z)]) {
          return;
        }
        dryad.place(tree.x,tree.y,tree.z);
        HTomb.Particles.addEmitter(tree.x,tree.y,tree.z,HTomb.Particles.SpellTarget, HTomb.Particles.DryadEffect);
        HTomb.GUI.sensoryEvent("An angry dryad emerges from a nearby tree!",tree.x,tree.y,tree.z,"red");
        this.alert();
      } else {
        this.despawn();
      }
    }
  });

  return HTomb;
})(HTomb);
