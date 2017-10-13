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

  // Fetch ingredients (not specific items)
  HTomb.Types.defineRoutine({
    template: "FetchItems",
    name: "fetch items",
    act: function(ai, args) {
      if (HTomb.Debug.noingredients) {
        return;
      }
      args = args || {};
      let cr = ai.entity;
      let claims = false;
      // if this is a minion of the player, be ready to stake claims
      if (cr.minion && cr.minion.master === HTomb.Player) {
        claims = true;
      }
      if (cr.inventory===undefined) {
        return;
      }
      // this mostly only matters for claims and defaults
      let task = args.task || null;
      if (task && task.begun()) {
        return;
      }
      // decide what the ingredients are
      let ingredients = (HTomb.Debug.noingredients) ? {} : args.ingredients;
      if (ingredients===undefined && task) {
        ingredients = task.ingredients;
      }
      ingredients = ingredients || {};
      if (Object.keys(ingredients).length===0) {
        return;
      }
      // if all ingredients are carried, skip the rest
      if (cr.inventory.items.hasAll(ingredients)) {
        return;
      }
      // if not all ingredients are available, fall back on something else
      if (HTomb.Tiles.canFindAll(cr.x, cr.y, cr.z, ingredients, {
        searcher: cr,
        respectClaims: (claims) ? true : false,
        ownedOnly: (claims) ? true : false,
        claimedItems: (task) ? task.claimedItems : false
      })===false) {
        if (task) {
          console.log("fetch failed?");
          task.onFetchFail();
        }
        return;
      }
      let inventory = cr.inventory.items.asIngredients();
      // check whether the target is a needed ingredient
      if (cr.ai.target) {
        console.log("already targetting" + cr.ai.target.describe());
        console.log(cr.ai.target);
        // if not, change targets
        let t = cr.ai.target.template;
        if (!ingredients[t] || inventory[t]>=ingredients[t]) {
          console.log("target wasn't an ingredient");
          cr.ai.target = null;
        // if the item has been picked up or destroyed, change targets
        } else if (cr.ai.target.isPlaced()!==true) {
          console.log("target wasn't placed");
          if (claims && task) {
            task.unclaim(cr.ai.target);
          }
          cr.ai.target = null;
        } else if (claims && task) {
          // edge case, reclaim if item was targetted by some other task or method
          if (task.hasClaimed(cr.ai.target)===false) {
            console.log("hadn't claimed it yet.")
            let item = cr.ai.target;
            let n = ingredients[cr.ai.target.template];
            if (n<=item.n-item.claimed) {
              task.claim(item,n);
            } else {
              task.claim(item,item.n-item.claimed);
            }
          }
        }
        // otherwise continue
      }
      // choosing targets is easier with claims and tasks
      if (cr.ai.target===null && claims && task) {
        console.log("trying to target a claimed ingredient");
        let items = task.claimedItems.filter(function(e,i,a) {
          // if it is still needed and is reachable
          if (e[0].isPlaced()
              && (!inventory[e[0].template] || inventory[e[0].template]<=ingredients[e[0].template])
              && HTomb.Tiles.isReachableFrom(e[0].x,e[0].y,e[0].z,cr.x,cr.y,cr.z,{
                canPass: cr.movement.boundMove(),
                searcher: cr,
                searchee: e[0],
                searchTimeout: 10
              })) {
            return true;
          } else {
            return false;
          }
        });
        items = items.map(function(e,i,a) {
          return e[0];
        });
        items = HTomb.Path.closest(cr.x,cr.y,cr.z,items);
        console.log("there are " + items.length + " items to choose from.");
        console.log(items);
        if (items.length>0) {
          cr.ai.target = items[0];
          console.log(items[0].describe() + " was closest");
          console.log(items[0]);
        }
      }
      // slightly harder otherwise, or if the claimed item was moved
      if (cr.ai.target===null) {
        console.log("trying to claim a new item instead");
        let items = [];
        for (let crd in HTomb.World.items) {
          let pile = HTomb.World.items[crd];
          for (let item of pile) {
            if (ingredients[item.template]) {
              // if it is not needed
              if (inventory[item.template]>=ingredients[item.template]) {
                break;
              // otherwise
              } else if (HTomb.Tiles.isReachableFrom(item.x,item.y,item.z,cr.x,cr.y,cr.z,{
                  canPass: cr.movement.boundMove(),
                  searcher: cr,
                  searchee: item,
                  searchTimeout: 10
                })) {
                items.push(item);
              }
            }
          }
        }
        console.log("there are " + items.length + " items to choose from.");
        console.log(items);
        items = HTomb.Path.closest(cr.x,cr.y,cr.z,items);
        if (items.length>0) {
          cr.ai.target = items[0];
          console.log(items[0].describe() + " was closest");
          console.log(items[0]);
        }
        if (cr.ai.target && claims) {
          let item = cr.ai.target;
          let n = ingredients[item.template];
          console.log("going to try to claim " + n + " from " + item.describe());
          if (n<=item.n-item.claimed) {
            if (task) {
              task.claim(item,n);
            } else {
              item.claimed+=n;
            }
          } else {
            if (task) {
              task.claim(item,item.n-item.claimed);
            } else {
              item.claimed = item.n;
            }
          }
        }
      }
      // if there is no target at this point, something has gone wrong
      let t = cr.ai.target;
      if (t===null) {
        throw new Error("FetchItems somehow skipped a check");
      }
      if (t===undefined) {
        throw new Error("You are f*cking kidding me...");
      }
      // if we are standing on it, pick up as many as we need / can
      if (cr.x===t.x && cr.y===t.y && cr.z===t.z) {
        let n = ingredients[t.template];
        console.log("trying to pick up " + n + " from " + t.describe());
        if (inventory[t.template]) {
          n-=inventory[t.template];
        }
        if (claims) {
          if (task) {
            task.unclaim(t);
          }
          if (n<=t.n-t.claimed) {
            cr.inventory.pickupSome(t.template,n);
            t.claimed-=n;
          } else {
            cr.inventory.pickup(t);
          }
          if (task && task.claimedItems.indexOf(t)!==-1) {
            task.claimedItems.splice(task.claimedItems.indexOf(t),1);
          }
        } else {
          cr.inventory.pickUpSome(t.template,n);
        }
        cr.ai.target = null;
      // otherwise walk toward it
      } else {
        console.log("walking towards " + t.describe());
        cr.ai.walkToward(t.x,t.y,t.z, {
          searcher: cr,
          searchee: t,
          searchTimeout: 10
        });
      }
    }
  });

  //fetch a specific item
  HTomb.Types.defineRoutine({
    template: "FetchItem",
    name: "fetch item",
    act: function(ai, args) {
      args = args || {};
      let cr = ai.entity;
      let claims = false;
      // if this is a minion of the player, be ready to stake claims
      if (cr.minion && cr.minion.master === HTomb.Player) {
        claims = true;
      }
      if (cr.inventory===undefined) {
        return;
      }
      // this mostly only matters for claims and defaults
      let task = args.task || null;
      let item = args.item;
      if (item===undefined && task) {
        item = task.item || null;
      }
      if (item===null) {
        return;
      }
      // if we already have it
      if (cr.inventory.items.contains(item))  {
        return;
      }
      // if someone else picked it up
      if (!item.isPlaced() || !HTomb.Tiles.isReachableFrom(item.x,item.y,item.z,cr.x,cr.y,cr.z,{
            canPass: cr.movement.boundMove(),
            searcher: cr,
            searchee: item,
            searchTimeout: 10
        })) {
        task.cancel();
        return;
      } else {
        cr.ai.target = item;
      }
      let t = cr.ai.target;
      // if we are standing on it, pick it up
      if (cr.x===t.x && cr.y===t.y && cr.z===t.z) {
        if (task) {
          task.unclaim(t);
        }
        cr.inventory.pickup(t);
        cr.ai.target = null;
      // otherwise walk toward it
      } else {
        cr.ai.walkToward(t.x,t.y,t.z, {
           searcher: cr,
           searchee: t,
           searchTimeout: 10
        });
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
            cr.inventory.drop(items[i]);
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
    // This whole method is an awful mess.
    // Right now we can acquire features as targets but we can't keep them as targets.
    act: function(ai) {
      // this performance might be okay
      let cr = ai.entity;
      if (ai.target && ai.target.isPlaced()===false) {
        ai.target = null;
      }
      if (ai.target===null || ai.target.parent!=="Creature" || ai.isHostile(ai.target)!==true || ai.target.isPlaced()===false || HTomb.Path.quickDistance(cr.x,cr.y,cr.z,ai.target.x,ai.target.y,ai.target.z)>10) {
        if (ai.team===undefined) {
          console.log("what in the world???");
        }
        let matrix = HTomb.Types.templates.Team.hostilityMatrix.matrix;
        let m = matrix[ai.entity.spawnId];
        let hostiles = [];
        let doors = [];
        let ids = HTomb.Things.templates.Thing.spawnIds;
        for (let n in m) {
          if (m[n]<=10) {
            hostiles.push(ids[n]);
          }
        }
        hostiles = hostiles.filter(function(e) {
          if (!e.isPlaced()) {
            return false;
          }
          let path = HTomb.Path.aStar(cr.x,cr.y,cr.z,e.x,e.y,e.z, {
            canPass: cr.movement.boundMove(),
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
          hostiles = HTomb.Path.closest(cr.x,cr.y,cr.z,hostiles);
          ai.target = hostiles[0];
        } else {
          // let's try to attack doors next
          doors = Object.keys(HTomb.World.blocks).map(function(e,i,a) {return HTomb.World.blocks[e];});
          doors = doors.filter(function(door) {return (HTomb.Path.quickDistance(cr.x,cr.y,cr.z,door.x,door.y,door.z)<=10)});
          doors = doors.filter(function(door) {return (!door.owner || ai.isHostile(door.owner));});
          doors = doors.filter(function(e) {
            if (!e.isPlaced()) {
              return false;
            }
            let path = HTomb.Path.aStar(cr.x,cr.y,cr.z,e.x,e.y,e.z, {
              canPass: cr.movement.boundMove(),
              searcher: cr,
              searchee: e,
              useLast: false,
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
        }
        if (doors.length>0) {
          doors = HTomb.Path.closest(cr.x,cr.y,cr.z,doors);
          ai.target = doors[0];
        }
      }
      // I'm still not clear on how we ever reach this logic
      if (ai.target && ai.target.isPlaced()===false) {
        ai.target = null;
        return;
      }
      // need to handle doors as well
      if (ai.target && ai.target.parent==="Creature" && ai.isHostile(ai.target)) {
        if (HTomb.Tiles.isTouchableFrom(ai.target.x, ai.target.y,ai.target.z, cr.x, cr.y, cr.z)) {
          ai.entity.attacker.attack(ai.target);
          ai.acted = true;
        } else {
          ai.walkToward(ai.target.x,ai.target.y,ai.target.z,{
            canPass: cr.movement.boundMove(),
            searcher: cr,
            searchee: ai.target,
            searchTimeout: 10
          });
        }
      // probably don't want separate logic for doors
      } else if (ai.target && ai.target.parent==="Feature" && ai.target.defender && (!ai.target.owner || ai.isHostile(ai.target.owner))) {
        if (HTomb.Tiles.isTouchableFrom(ai.target.x, ai.target.y,ai.target.z, cr.x, cr.y, cr.z)) {
          ai.entity.attacker.attack(ai.target);
          ai.acted = true;
        } else {
          ai.walkToward(ai.target.x,ai.target.y,ai.target.z,{
            canPass: cr.movement.boundMove(),
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
        let canMove = cr.movement.canMove.bind(cr);
        if (HTomb.Tiles.isReachableFrom(x,y,z,cr.x,cr.y,cr.z, {
          canPass: canMove
        })) {
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
    template: "HuntPlayer",
    name: "hunt player",
    act: function(ai) {
      let c = ai.entity;
      let canMove = c.movement.boundMove();
      // so...this tends to lock onto one thing and seek it out relentlessly.
      // we want to it be able to acquire other targets
      if (ai.target===null) {
        let p = HTomb.Player;
        if (HTomb.Tiles.isReachableFrom(c.x,c.y,c.z,p.x,p.y,p.z,{canPass: canMove})) {
          ai.target = p;
        } else {
          let minions = HTomb.Player.master.minions;
          if (minions.length>0) {
            for (let m of minions) {
              if (HTomb.Tiles.isReachableFrom(c.x,c.y,c.z,m.x,m.y,m.z,{canPass: canMove})) {
                ai.target = m;
                break;
              }
            }
          }
        }
        if (ai.target===null) {
          for (let block in HTomb.World.blocks) {
            let b = HTomb.World.blocks[block];
            if (b.owner===HTomb.Player && HTomb.Tiles.isReachableFrom(c.x,c.y,c.z,b.x,b.y,b.z,{canPass: canMove})) {
              ai.target = b;
              break;
            }
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
      // probably need to put feature attacks in here now?
      for (var i=0; i<backoffs.length; i++) {
        var dir = backoffs[i];
        let c = coord(x+dir[0],y+dir[1],z+dir[2]);
        var cr = HTomb.World.creatures[c];
        let f = HTomb.World.features[c];
        // modify this to allow non-player creatures to displace

        ////!!!!Okay, hold on...this currently forbids attacking into walls, or attacking doors

        //if (f && f.defend && (!f.owner || this.isHostile(f.owner))) {

        //}
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

  HTomb.Types.defineTeam({
    template: "HumanityTeam",
    name: "humanity",
    enemies: ["PlayerTeam","GhoulTeam","AngryNatureTeam"]
  });

  return HTomb;
})(HTomb);
