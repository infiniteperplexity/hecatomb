// ****** This module implements Behaviors, which are the basic units of functionality for creatures, items, and features
HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var coord = HTomb.Utils.coord;

  let Behavior = HTomb.Things.templates.Behavior;
  
  HTomb.Types.define({
    template: "Routine",
    name: "routine",
    act: function(ai, args) {
      if (false) {
        ai.acted = true;
      }
    }
  });

  HTomb.Types.defineRoutine({
    template: "ShoppingList",
    name: "shopping list",
    act: function(ai, args) {
      var cr = ai.entity;
      var task = cr.worker.task;
      var ingredients = args || task.ingredients;
      if (HTomb.Debug.noingredients) {
        ingredients = {};
      }
      // if no ingredients are required, skip the rest
      if (Object.keys(ingredients).length===0) {
        return false;
      }
      var x = task.x;
      var y = task.y;
      var z = task.z;
      var f = HTomb.World.features[coord(x,y,z)];
      // no need for ingredients if construction has begun
      if (task.begun()) {
        return false;
      }
      //if (f && f.makes.template===task.makes) {
      //  return false;
      //}
      // check to see if we are already targeting an ingredient
      if (cr.ai.target && cr.ai.target.isPlaced()!==true) {
        cr.ai.target = null;
      }
      var t = cr.ai.target;
      // if the target is not an ingredient
      if (t && ingredients[t.template]===undefined) {
        cr.ai.target = null;
      }
      t = cr.ai.target;
      var needy = false;
      // cycle through ingredients to see if we have enough
      if (t===null) {
        for (var ing in ingredients) {
          var n = ingredients[ing];
          // if we lack what we need, search for items
          if (cr.inventory.items.count(ing)<n) {
            needy = true;
            var items = task.assigner.master.ownedItems().filter(function(v) {
              if (v.isOnGround()!==true) {
                return false;
              } else if (v.template===ing) {
                if (HTomb.Tiles.isReachableFrom(v.x,v.y,v.z,cr.x,cr.y,cr.z,{
                  searcher: cr,
                  searchee: v,
                  searchTimeout: 10
                })) {
                  return true;
                }
              }
              return false;
            });
            // if we find an item we need, target it
            if (items.length>0) {
              items = HTomb.Path.closest(cr.x,cr.y,cr.z,items);
              cr.ai.target = items[0];
              break;
            }
          }
        }
      }
      t = cr.ai.target;
      // we have everything we need so skip the rest
      if (needy===false && t===null) {
        return false;
      // failed to find what we needed
      } else if (needy===true && t===null) {
        cr.worker.task.unassign();
        cr.ai.walkRandom();
      } else if (t!==null) {
        if (t.x===cr.x && t.y===cr.y && t.z===cr.z) {
          cr.inventory.pickupSome(t.template,ingredients[t.template]);
          cr.ai.acted = true;
          cr.ai.target = null;
        } else {
          if (t.z===null) {
            console.log("why did this shopping list fail?");
          }
          cr.ai.walkToward(t.x,t.y,t.z, {
            searcher: cr,
            searchee: t,
            searchTimeout: 10
          });
        }
      }
    }
  });

  HTomb.Types.defineRoutine({
    template: "GoToWork",
    name: "go to work",
    act: function(ai, options) {
      options = options || {};
      let useLast = options.useLast || false;
      var cr = ai.entity;
      var task = cr.worker.task;
      if (cr.movement) {
        var x = task.x;
        var y = task.y;
        var z = task.z;
        if (z===null) {
          console.log("why go to work fail?");
        }
        var dist = HTomb.Path.distance(cr.x,cr.y,x,y);
        // Should I instead check for "beginWork"?
        if (useLast===true && x===cr.x && y===cr.y && z===cr.z) {
          task.workOnTask(x,y,z);
        } else if (useLast!==true && HTomb.Tiles.isTouchableFrom(x,y,z,cr.x,cr.y,cr.z)) {
          task.workOnTask(x,y,z);
        } else if (dist>0 || cr.z!==z) {
          cr.ai.walkToward(x,y,z, {
            searcher: cr,
            searchee: task,
            searchTimeout: 10
          });
        } else if (dist===0) {
          cr.ai.walkRandom();
        } else {
          task.unassign();
          cr.ai.walkRandom();
        }
      }
    }
  });

  HTomb.Types.defineRoutine({
    template: "ServeMaster",
    name: "serve master",
    act: function(ai) {
      if (ai.entity.minion===undefined) {
        return;
      }
      let cr = ai.entity;
      // Drop items not relevant to current task
      if (cr.inventory) {
        let items = cr.inventory.items;
        for (let i=0; i<items.length; i++) {
          // drop any item that is not relevant to the current task
          // ...eventually we'll want to keep equipment and certain other items
          // For now just drop items if there is no task at all?
          if (!cr.worker.task || cr.worker.task.template==="PatrolTask") {
            console.log("dropping an unneeded item");
            cr.inventory.drop(items.expose(i));
            ai.acted = true;
            break;
          }
        }
      }
      if (cr.worker && cr.worker.task) {
        ai.entity.worker.task.ai();
      } else {
        // Otherwise, patrol around the creature's master
        // or maybe check for tasks now?
        ai.patrol(ai.entity.minion.master.x,ai.entity.minion.master.y,ai.entity.minion.master.z, {
          searcher: ai.entity,
          searchee: ai.entity.minion.master,
          searchTimeout: 10
        });
      }
    }
  });
  HTomb.Types.defineRoutine({
    template: "WanderAimlessly",
    name: "wander aimlessly",
    act: function(ai) {
      ai.wander();
    }
  });

  HTomb.Types.defineRoutine({
    template: "CheckForHostile",
    name: "check for hostile",
    act: function(ai) {
      // this performance might be okay
      let cr = ai.entity;
      if (ai.target && ai.target.isPlaced()===false) {
        ai.target = null;
      }
      if (ai.target===null || ai.target.parent!=="Creature" || ai.isHostile(ai.target)!==true || ai.target.isPlaced()===false || HTomb.Path.quickDistance(cr.x,cr.y,cr.z,ai.target.x,ai.target.y,ai.target.z)>15) {
        if (ai.team===undefined) {
          console.log("what in the world???");
        }
        let matrix = HTomb.Types.templates.Team.hostilityMatrix.matrix;
        let m = matrix[ai.entity.spawnId];
        let hostiles = [];
        let ids = HTomb.Things.templates.Thing.spawnIds;
        for (let n in m) {
          if (m[n]<=10) {
            hostiles.push(ids[n]);
          }
        }
        let canMove = HTomb.Utils.bind(ai.entity.movement,"canMove");
        hostiles = hostiles.filter(function(e) {
          if (!e.isPlaced()) {
            return false;
          }
          let path = HTomb.Path.aStar(cr.x,cr.y,cr.z,e.x,e.y,e.z, {
            canPass: canMove,
            searcher: cr,
            searchee: e,
            cacheAfter: 40,
            cacheTimeout: 10,
            searchTimeout: 10
          });
          // want a nice gap between cacheAfter and maximum length
          if (path && path.length<=20) {
            return true;
          } else {
            return false;
          }
        });
        if (hostiles.length>0) {
          hostiles = HTomb.Path.closest(ai.entity.x,ai.entity.y,ai.entity.z,hostiles);
          ai.target = hostiles[0];
        }
      }
      if (ai.target && ai.isHostile(ai.target)) {
        if (HTomb.Tiles.isTouchableFrom(ai.target.x, ai.target.y,ai.target.z, cr.x, cr.y, cr.z)) {
          ai.entity.combat.attack(ai.target);
          ai.acted = true;
        } else {
          ai.walkToward(ai.target.x,ai.target.y,ai.target.z,{
            searcher: cr,
            searchee: ai.target,
            searchTimeout: 10
          });
        }
      }
    }
  });

  HTomb.Types.defineRoutine({
    template: "LongRangeRoam",
    name: "long range roam",
    act: function(ai) {
      if (ai.target===null) {
        let x = HTomb.Utils.dice(1,LEVELW-2);
        let y = HTomb.Utils.dice(1,LEVELH-2);
        let z = HTomb.Tiles.groundLevel(x,y);
        let cr = ai.entity;
        if (HTomb.Tiles.isReachableFrom(x,y,z,cr.x,cr.y,cr.z)) {
          ai.target = HTomb.Tiles.getTileDummy(x,y,z) || null;
        }
      }
      if (ai.target && HTomb.Tiles.isTouchableFrom(ai.target.x, ai.target.y, ai.target.z, ai.entity.x, ai.entity.y, ai.entity.z)) {
        ai.target = null;
        return;
      }
      if (ai.target!==null) {
        ai.walkToward(ai.target.x, ai.target.y, ai.target.z, {
          searcher: ai.entity,
          searchee: ai.target,
          cacheAfter: 25,
          cacheTimeout: 20
        });
      }
    }
  });
  HTomb.Types.defineRoutine({
    template: "HuntDeadThings",
    name: "hunt dead things",
    act: function(ai) {
      // should this hunt in sight range first?
      if (ai.target===null) {
        var zombies = HTomb.Utils.where(HTomb.World.creatures,function(v,k,o) {
          return (v.template==="Zombie" && ai.isHostile(v) && HTomb.Tiles.isEnclosed(v.x,v.y,v.z)===false);
        });
        if (zombies.length>0) {
          var e = ai.entity;
          zombies.sort(function(a,b) {
            return HTomb.Path.quickDistance(e.x,e.y,e.z,a.x,a.y,a.z) - HTomb.Path.quickDistance(e.x,e.y,e.z,b.x,b.y,b.z);
          });
          ai.target = zombies[0];
          console.log("hunting a zombie");
          HTomb.Routines.CheckForHostile.act(ai);
        }
      }
    }
  });

  Behavior.extend({
    template: "AI",
    name: "ai",
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
      if (thing.ai===undefined || thing.ai.team===null || this.team===null) {
        return false;
      } else if (HTomb.Types.templates[this.team].vendettas.indexOf(thing)!==-1) {
        return true;
      } else if (HTomb.Types.templates[thing.ai.team].vendettas.indexOf(this.entity)!==-1) {
        return true;
      } else {
        return HTomb.Types.templates[this.team].isHostile(thing.ai.team);
      }
    },
    // We may want to save a path for the entity
    onAdd: function(){
      this.path = [];
      this.alert = HTomb.Routines.CheckForHostile;
      var goals = this.goals || [];
      this.goals = [];
      for (var i=0; i<goals.length; i++) {
        this.goals.push(HTomb.Routines[goals[i]]);
      }
      HTomb.Events.subscribe(this,"Destroy");
      this.fallback = HTomb.Routines.WanderAimlessly;
      this.setTeam(this.team);
    },
    setTeam: function(team) {
      this.team = team;
      let myTeam = HTomb.Types.templates.Team.teams[team];
      if (myTeam===undefined) {
        console.log(team);
      }
      if (myTeam.members.indexOf(this.entity)===-1) {
        myTeam.members.push(this.entity);
      }
    },
    onDespawn: function() {
      let myTeam = HTomb.Types.templates.Team.teams[this.team];
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
      // If the entity is the player, don't choose for it...maybe this should be a Behavior?
      //if (this.entity===HTomb.Player) {
      //  return false;
      //}
      // If the creature has already acted, bail out
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
      let min = options.min || 2;
      let max = options.max || 5;
      if (!this.entity.movement) {
        return false;
      }
      if (z===null) {
        console.log("why problem with patrol???");
      }
      var dist = HTomb.Path.distance(this.entity.x,this.entity.y,x,y);
      if (dist<min) {
        this.acted = this.walkAway(x,y,z);
      } else if (dist>max) {
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
      var r = Math.floor(Math.random()*26);
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
        options.canPass = HTomb.Utils.bind(this.entity.movement,"canMove");
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
      for (var i=0; i<backoffs.length; i++) {
        var dir = backoffs[i];
        var cr = HTomb.World.creatures[coord(x+dir[0],y+dir[1],z+dir[2])];
        // modify this to allow non-player creatures to displace
        if (this.entity.movement.canMove(x+dir[0],y+dir[1],z+dir[2])===false) {
          continue;
        } else if (cr) {
          if (cr.ai && cr.ai.isHostile(this.entity)===false && cr.player===undefined && cr.movement) {
            if (Math.random()<=0.5) {
              this.entity.movement.displaceCreature(cr);
              return true;
            } else {
              continue;
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
    },
  });

  HTomb.Types.define({
    template: "Team",
    name: "team",
    members: null,
    enemies: null,
    allies: null,
    xenophobic: false,
    berserk: false,
    vendettas: null,
    teams: {},
    hostilityMatrix: {
      matrix: {},
      reset: function() {
        this.matrix = {};
      },
      onTurnBegin: function() {
        let matrix = this.matrix = {};
        let teams = HTomb.Types.templates.Team.teams
        let keys = Object.keys(teams);
        for (let i=0; i<keys.length; i++) {
          // handle team-wide vendettas against individuals
          let one = teams[keys[i]];
          for (let j=0; j<one.vendettas.length; j++) {
            for (let m=0; m<one.members.length; m++) {
              let a = one.members[m];
              let s = a.spawnId;
              if (j===0) {
                matrix[s] = matrix[s] || {};
              }
              let b = one.vendettas[j];
              if (a===b) {
                continue;
              }
              let t = b.spawnId;
              let q = HTomb.Path.quickDistance(a.x,a.y,a.z,b.x,b.y,b.z);
              matrix[s][t] = q;
              matrix[t] = matrix[t] || {};
              matrix[t][s] = q;
            }
          }
          // handle inter-team hostility
          for (let j=i; j<keys.length; j++) {
            let two = teams[keys[j]];
            if (one.isHostile(two)) {
              for (let m=0; m<one.members.length; m++) {
                let a = one.members[m];
                let s = a.spawnId;
                for (let n=0; n<two.members.length; n++) {
                  if (n===0) {
                    matrix[s] = matrix[s] || {};
                  }
                  let b = two.members[n];
                  // no creature is ever hostile toward itself, even if it is berserk
                  if (a===b) {
                    continue;
                  }
                  let t = b.spawnId;
                  let q = HTomb.Path.quickDistance(a.x,a.y,a.z,b.x,b.y,b.z);
                  matrix[s][t] = q;
                  matrix[t] = matrix[t] || {};
                  matrix[t][s] = q;
                }
              }
            }
          }
        }
      }
    },
    onDefine: function(args) {
      this.members = this.members || [];
      this.enemies = this.enemies || [];
      this.allies = this.allies || [];
      this.vendettas = this.vendettas || [];
      HTomb.Events.subscribe(this,"Destroy");
      HTomb.Types.templates.Team.teams[this.template] = this;
    },
    onDestroy: function(event) {
      if (this.members.indexOf(event.entity)>-1) {
        this.members.splice(this.members.indexOf(event.entity),1);
      }
      if (this.vendettas.indexOf(event.entity)>-1) {
        this.vendettas.splice(this.vendettas.indexOf(event.entity),1);
      }
    },
    isHostile: function(team) {
      if (team===undefined) {
        return false;
      }
      if (typeof(team)==="string") {
        team = HTomb.Types.templates[team];
      }
      if (this.berserk || team.berserk) {
        return true;
      } else if ((this.xenophobic || team.xenophobic) && (this!==team)) {
        return true;
      } else if (team.enemies.indexOf(this.template)>=0 || this.enemies.indexOf(team.template)>=0) {
        return true;
      } else {
        return false;
      }
    }
  });
  HTomb.Events.subscribe(HTomb.Types.templates.Team.hostilityMatrix,"TurnBegin");


  // the player and affiliated minions
  HTomb.Types.defineTeam({
    template: "PlayerTeam",
    name: "player"
  });

  HTomb.Types.defineTeam({
    template: "DefaultTeam",
    name: "default"
  });

  // non-aggressive animals
  HTomb.Types.defineTeam({
    template: "AnimalTeam",
    name: "animals"
  });

  HTomb.Types.defineTeam({
    template: "GhoulTeam",
    name: "ghouls",
    enemies: ["PlayerTeam"]
  });

  HTomb.Types.defineTeam({
    template: "HungryPredatorTeam",
    name: "predators",
    enemies: ["PlayerTeam"]
    //xenophobic: true
  });

  HTomb.Types.defineTeam({
    template: "AngryNatureTeam",
    name: "angryNature",
    enemies: ["PlayerTeam","GhoulTeam"]
  });

  return HTomb;
})(HTomb);
