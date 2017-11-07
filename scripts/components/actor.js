// ****** This module implements Components, which are the basic units of functionality for creatures, items, and features
HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var coord = HTomb.Utils.coord;

  let Component = HTomb.Things.Component;
  
  Component.extend({
    template: "Actor",
    name: "actor",
    // unimplemented
    target: null,
    // unimplemented
    team: "AnimalTeam",
    //allegiance: null,
    angered: false,
    acted: false,
    actionPoints: 16,
    priority: null,
    alert: null,
    goals: null,
    fallback: null,
    regainPoints: function() {
      this.acted = false;
      this.actionPoints+=16;
    },
    isHostile: function(thing) {
      if (thing.actor===undefined || thing.actor.team===null || this.team===null) {
        return false;
      } else if (HTomb.Types[this.team].vendettas.indexOf(thing)!==-1) {
        return true;
      } else if (HTomb.Types[thing.actor.team].vendettas.indexOf(this.entity)!==-1) {
        return true;
      } else {
        return HTomb.Types[this.team].isHostile(thing.actor.team);
      }
    },
    // We may want to save a path for the entity
    onAdd: function(){
      this.path = [];
      this.alert = HTomb.Behaviors.CheckForHostile;
      var goals = this.goals || [];
      this.goals = [];
      for (var i=0; i<goals.length; i++) {
        this.goals.push(HTomb.Behaviors[goals[i]]);
      }
      HTomb.Events.subscribe(this,"Destroy");
      this.fallback = HTomb.Behaviors.WanderAimlessly;
      this.setTeam(this.team);
    },
    setTeam: function(team) {
      this.team = team;
      let myTeam = HTomb.Types.Team.teams[team];
      if (myTeam===undefined) {
        console.log(team);
      }
      if (myTeam.members.indexOf(this.entity)===-1) {
        myTeam.members.push(this.entity);
      }
    },
    onDespawn: function() {
      let myTeam = HTomb.Types.Team.teams[this.team];
      if (myTeam.members.indexOf(this.entity)!==-1) {
        myTeam.members.splice(myTeam.members.indexOf(this.entity),1);
      }
    },
    onDestroy: function(event) {
      if (event.entity===this.target) {
        this.target = null;
      }
    },
    act: function() {
      // If the entity is the player, don't choose for it...maybe this should be a Component?
      //if (this.entity===HTomb.Player) {
      //  return false;
      //}
      // If the creature has already acted, bail out
      HTomb.Events.publish({type: "Act", actor: this});
      if (this.acted===false) {
        this.alert.act(this);
      }
      for (var i=0; i<this.goals.length; i++) {
        if (this.acted===false) {
          this.goals[i].act(this);
        }
      }
      if (this.acted===false) {
        this.fallback.act(this);
      }
      if (this.acted===false) {
        if (this.actionPoints>0) {
          console.log(this.entity.describe() + " failed to act!");
        }
         //throw new Error("Creature failed to act!");
      }
      // Reset activity for next turn
      if (this.acted===false) {
        this.actionPoints-=16;
        this.acted = true;
      }
      //this.acted = false;
      //this.actionPoints-=16;
    },
    // A patrolling creature tries to stay within a certain orbit of a target square
    patrol: function(x,y,z, options) {
      options = options || {};
      let min = options.min || 1;
      let max = options.max || 4;
      if (!this.entity.movement) {
        return false;
      }
      if (z===null) {
        console.log("why problem with patrol???");
      }
      var dist = HTomb.Path.distance(this.entity.x,this.entity.y,x,y);
      if (dist<=min) {
        this.acted = this.walkAway(x,y,z);
      } else if (dist>=max) {
        this.acted = this.walkToward(x,y,z, {
          searcher: options.searcher,
          searchee: options.searchee,
          searchTimeout: options.searchTimeout
        });
      } else {
        this.acted = this.walkRandom();
      }
    },
    // A wandering creature walks randomly...so far it won't scale slopes
    wander: function() {
      if (!this.entity.movement) {
        return false;
      }
      this.acted = this.walkRandom();
    },
    walkRandom: function() {
      var r = Math.floor(ROT.RNG.getUniform()*26);
      var dx = HTomb.dirs[26][r][0];
      var dy = HTomb.dirs[26][r][1];
      var dz = HTomb.dirs[26][r][2];
      return this.tryStep(dx,dy,dz);
    },
    // Walk along a path toward the target
    walkToward: function(x,y,z,options) {
      options = options || {};
      var x0 = this.entity.x;
      var y0 = this.entity.y;
      var z0 = this.entity.z;
      //var path = HTomb.Path.aStar(x0,y0,z0,x,y,z,{useLast: false});
      if (options.useLast===undefined) {
        options.useLast = false;
      }
      if (options.canMove===undefined) {
        options.canPass = this.entity.movement.boundMove();
      }
      var path = HTomb.Path.aStar(x0,y0,z0,x,y,z,options);
      if (path!==false) {
        var square = path[0];
        if (path.length===0) {
          square = [x,y,z];
        }
        return this.tryStep(square[0]-x0,square[1]-y0,square[2]-z0);
      }
      return false;
    },
    // Walk straight away from the target
    walkAway: function(x,y) {
      var x0 = this.entity.x;
      var y0 = this.entity.y;
      var line = HTomb.Path.line(x0,y0,x,y);
      if (line.length<=1) {
        return this.walkRandom();
      }
      var dx = line[1][0] - x0;
      var dy = line[1][1] - y0;
      return this.tryStep(-dx,-dy,0);
    },
    // Try to step in a certain direction
    tryStep: function(dx, dy, dz) {
      var backoffs = HTomb.dirs.getBackoffs(dx, dy, dz);
      var x = this.entity.x;
      var y = this.entity.y;
      var z = this.entity.z;
      // probably need to put feature attacks in here now?
      for (var i=0; i<backoffs.length; i++) {
        var dir = backoffs[i];
        let c = coord(x+dir[0],y+dir[1],z+dir[2]);
        var cr = HTomb.World.creatures[c];
        let f = HTomb.World.features[c];
        if (this.entity.movement.canMove(x+dir[0],y+dir[1],z+dir[2])===false) {
          continue;
        // logic for pushing or displacing friendly creatures
        } else if (cr) {
          if (cr.actor && cr.actor.isHostile(this.entity)===false && cr.player===undefined && cr.movement) {
            let t;
            if (cr.worker && cr.worker.task) {
              t = cr.worker.task;
            }
            // much more reluctant to displace a creature if it is "working"
            if (t && HTomb.Tiles.isTouchableFrom(t.x,t.y,t.z,cr.x,cr.y,cr.z)) {
              // try to push the creature into a space from which it can still work
              let squares = HTomb.Tiles.getNeighborsWhere(cr.x,cr.y,cr.z, function(x0,y0,z0) {
                if (cr.movement && cr.movement.canPass(x0,y0,z0)!==true) {
                  return false;
                } else {
                  return HTomb.Tiles.isTouchableFrom(t.x,t.y,t.z,x0,y0,z0);
                }
              });
              if (squares.length>0) {
                squares = HTomb.Utils.shuffle(squares);
                let s = squares[0];
                this.entity.movement.displaceCreature(cr,s[0],s[1],s[2]);
                return true;
              } else {
                // if the creature can work after trading places, do that
                if (HTomb.Tiles.isTouchableFrom(t.x,t.y,t.z,x,y,z)) {;
                  this.entity.movement.displaceCreature(cr);
                }
                // will not push a working creature away from its task
                return false;
              }
            } else {
              if (ROT.RNG.getUniform()<=0.5) {
                this.entity.movement.displaceCreature(cr);
                return true;
              } else {
                continue;
              }      
            }
          } else {
            continue;
          }
        } else {
          this.entity.movement.stepTo(x+dir[0],y+dir[1],z+dir[2]);
          return true;
        }
      }
      return false;
    }
  });

  return HTomb;
})(HTomb);
