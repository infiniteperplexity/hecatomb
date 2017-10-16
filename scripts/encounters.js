HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;


  let Entity = HTomb.Things.Entity;

  let Encounter = Entity.extend({
    template: "Encounter",
    name: "encounter",
    listens: [],
    creatures: null,
    observed: true,
    blurb: "There is some kind of encounter going on!",
    alert: function() {
      let structures = HTomb.Player.master.structures;
      for (let s of structures) {
        if (s.template==="GuardPost") {
          HTomb.GUI.alert(this.blurb);
          //HTomb.GUI.Views.Main.tacticalMode();
          return;
        }
      }
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
    // this "spawns" the encounter, since the name "spawn" is already taken
    muster: function(x,y,z,args) {
      let encounter = HTomb.Things[this.template]();
      encounter.place(x,y,z,args);
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
    },
    alongEdge: function(n, len, callb) {
      // n is the number of squares needed, len is the length of the line to try, callb is the rule
      len = len || 3*n;
      // default rule is ground level, no liquid, no creatures
      callb = callb || function(x,y) {
        let z = HTomb.Tiles.groundLevel(x,y);
        if (HTomb.World.covers[z][x][y].liquid) {
          return false;
        }
        if (HTomb.World.creatures[coord(x,y,z)]===undefined) {
          return [x,y,z];
        }
        return false;
      };
      let TRIES = 100;
      let tries = 0;
      let x;
      let y;
      let z;
      while (tries<TRIES) {
        let edge = HTomb.Utils.dice(1,4);
        let span;
        if (edge===1 || edge===3) {
          span = LEVELH;
        } else {
          span = LEVELW;
        }
        let center = HTomb.Utils.dice(1,span-2-len)+Math.floor(len/2);
        let line = [];
        for (let i=0; i<len; i++) {
          line.push(center-Math.floor(len/2)+i);
        }
        let coords = [];
        if (edge===1) {
          x = 1;
          for (let s of line) {
            coords.push([x,s]);
          }
        } else if (edge===2) {
          y = 1;
          for (let s of line) {
            coords.push([s,y]);
          }
        } else if (edge===3) {
          x = LEVELW-1;
          for (let s of line) {
            coords.push([x,s]);
          }
        } else if (edge===4) {
          y = LEVELH-1;
          for (let s of line) {
            coords.push([s,y]);
          }
        }
        coords = HTomb.Utils.shuffle(coords);
        let squares = [];
        for (let i of coords) {
          if (callb(i[0],i[1])) {
            squares.push(callb(i[0],i[1]));
          }
          if (squares.length>=n) {
            return squares;
          }
        } 
      }
      return false;
    }
  });

  Encounter.extend({
    template: "PeasantMob",
    name: "angry peasant mob",
    listens: ["TurnBegin"],
    blurb: "A mob of angry peasants approaches, determined to end your foul research!",
    onTurnBegin: function(event) {
      if (HTomb.Time.dailyCycle.turn===1800) {
        this.muster();
      }
    },
    onSpawn: function(args) {
      let r = HTomb.Utils.dice(1,4)+2;
      for (let i=0; i<r; i++) {
        this.addCreature(HTomb.Things.Peasant.spawn());
      }
      HTomb.Debug.peasants = this.creatures;
    },
    onPlace: function(x,y,z,args) {
      let squares = this.alongEdge(this.creatures.length);
      if (squares) {
        this.alert();
        for (let i=0; i<this.creatures.length; i++) {
          let s = squares[i];
          this.creatures[i].place(s[0],s[1],s[2]);
        }
      } else {
        for (let c of this.creatures) {
          c.despawn();
        }
        this.despawn();
      }
    }
  });
  
  Encounter.extend({
    template: "AngryDryads",
    name: "angry dryads",
    listens: ["Destroy"],
    blurb: "Dryads emerge from the trees, angered by your desecration of the forest!",
    onDestroy: function(event) {
      // no dryads in the extremely early game
      if (HTomb.Time.dailyCycle.turn<1800) {
        return;
      }
      let t = event.entity;
      if (t.template==="Tree") {
        let x = t.x;
        let y = t.y;
        let z = t.z;
        if (HTomb.Utils.dice(1,50)===1) {
          this.muster(x,y,z,{});
        }
      }
    },
    onSpawn: function() {
      // should add multiple dryads
      this.addCreature(HTomb.Things.Dryad.spawn());
    },
    onPlace: function(x,y,z,args) {
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
