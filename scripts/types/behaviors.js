// ****** This module implements Components, which are the basic units of functionality for creatures, items, and features
HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var coord = HTomb.Utils.coord;

  let Type = HTomb.Types.Type;

  let Behavior = Type.extend({
    template: "Behavior",
    name: "behavior",
    act: function(actor, args) {
      if (false) {
        actor.acted = true;
      }
    }
  });

  // Fetch ingredients (not specific items)
  Behavior.extend({
    template: "FetchItems",
    name: "fetch items",
    act: function(actor, args) {
      if (HTomb.Debug.noingredients) {
        return;
      }
      args = args || {};
      let cr = actor.entity;
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
          task.onFetchFail();
        }
        return;
      }
      let inventory = cr.inventory.items.asIngredients();
      // check whether the target is a needed ingredient
      if (cr.actor.target) {
        // if not, change targets
        let t = cr.actor.target.template;
        if (!ingredients[t] || inventory[t]>=ingredients[t]) {
          cr.actor.target = null;
        // if the item has been picked up or destroyed, change targets
        } else if (cr.actor.target.isPlaced()!==true) {
          if (claims && task) {
            task.unclaim(cr.actor.target);
          }
          cr.actor.target = null;
        } else if (claims && task) {
          // edge case, reclaim if item was targetted by some other task or method
          if (task.hasClaimed(cr.actor.target)===false) {
            let item = cr.actor.target;
            let n = ingredients[cr.actor.target.template];
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
      if (cr.actor.target===null && claims && task) {
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
        if (items.length>0) {
          cr.actor.target = items[0];
        }
      }
      // slightly harder otherwise, or if the claimed item was moved
      if (cr.actor.target===null) {
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
        items = HTomb.Path.closest(cr.x,cr.y,cr.z,items);
        if (items.length>0) {
          cr.actor.target = items[0];
        }
        if (cr.actor.target && claims) {
          let item = cr.actor.target;
          let n = ingredients[item.template];
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
      let t = cr.actor.target;
      if (t===null) {
        throw new Error("FetchItems somehow skipped a check");
      }
      if (t===undefined) {
        throw new Error("You are f*cking kidding me...");
      }
      // if we are standing on it, pick up as many as we need / can
      if (cr.x===t.x && cr.y===t.y && cr.z===t.z) {
        let n = ingredients[t.template];
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
        cr.actor.target = null;
      // otherwise walk toward it
      } else {
        cr.actor.walkToward(t.x,t.y,t.z, {
          searcher: cr,
          searchee: t,
          searchTimeout: 10
        });
      }
    }
  });

  //fetch a specific item
  Behavior.extend({
    template: "FetchItem",
    name: "fetch item",
    act: function(actor, args) {
      args = args || {};
      let cr = actor.entity;
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
        cr.actor.target = item;
      }
      let t = cr.actor.target;
      // if we are standing on it, pick it up
      if (cr.x===t.x && cr.y===t.y && cr.z===t.z) {
        if (task) {
          task.unclaim(t);
        }
        cr.inventory.pickup(t);
        cr.actor.target = null;
      // otherwise walk toward it
      } else {
        cr.actor.walkToward(t.x,t.y,t.z, {
           searcher: cr,
           searchee: t,
           searchTimeout: 10
        });
      }
    }
  });

  Behavior.extend({
    template: "ServeMaster",
    name: "serve master",
    act: function(actor) {
      if (actor.entity.minion===undefined) {
        return;
      }
      let cr = actor.entity;
      // Drop items not relevant to current task
      if (cr.inventory) {
        let items = cr.inventory.items;
        for (let i=0; i<items.length; i++) {
          // drop any item that is not relevant to the current task
          // ...eventually we'll want to keep equipment and certain other items
          // For now just drop items if there is no task at all?
          if (!cr.worker.task || cr.worker.task.template==="PatrolTask") {
            cr.inventory.drop(items[i]);
            actor.acted = true;
            break;
          }
        }
      }
      if (cr.worker && cr.worker.task) {
        actor.entity.worker.task.act();
      } else if (HTomb.Player.master) {
        actor.patrol(actor.entity.minion.master.x,actor.entity.minion.master.y,actor.entity.minion.master.z, {
          searcher: actor.entity,
          searchee: actor.entity.minion.master,
          searchTimeout: 10
        });
      }
    }
  });

  Behavior.extend({
    template: "WanderAimlessly",
    name: "wander aimlessly",
    act: function(actor) {
      actor.wander();
    }
  });


  Behavior.extend({
    template: "CheckForHostile",
    name: "check for hostile",
    act: function(actor) {
      // this performance might be okay
      let cr = actor.entity;
      if (actor.target && actor.target.isPlaced()===false) {
        actor.target = null;
      }
      if (actor.target===null || actor.target.parent!=="Creature" || actor.isHostile(actor.target)!==true || actor.target.isPlaced()===false || HTomb.Path.quickDistance(cr.x,cr.y,cr.z,actor.target.x,actor.target.y,actor.target.z)>10) {
        if (actor.team===undefined) {
          console.log("what in the world???");
        }
        let matrix = HTomb.Types.Team.hostilityMatrix.matrix;
        let m = matrix[actor.entity.spawnId];
        let hostiles = [];
        let doors = [];
        let ids = HTomb.Things.Thing.spawnIds;
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
          actor.target = hostiles[0];
        } else {
          // let's try to attack doors next
          doors = Object.keys(HTomb.World.blocks).map(function(e,i,a) {return HTomb.World.blocks[e];});
          doors = doors.filter(function(door) {return (HTomb.Path.quickDistance(cr.x,cr.y,cr.z,door.x,door.y,door.z)<=10)});
          doors = doors.filter(function(door) {return (!door.owner || actor.isHostile(door.owner));});
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
          actor.target = doors[0];
        }
      }
      // I'm still not clear on how we ever reach this logic
      if (actor.target && actor.target.isPlaced()===false) {
        actor.target = null;
        return;
      }
      // need to handle doors as well
      if (actor.target && actor.target.parent==="Creature" && actor.isHostile(actor.target)) {
        if (HTomb.Tiles.isTouchableFrom(actor.target.x, actor.target.y,actor.target.z, cr.x, cr.y, cr.z)) {
          actor.entity.attacker.attack(actor.target);
          actor.acted = true;
        } else {
          actor.walkToward(actor.target.x,actor.target.y,actor.target.z,{
            canPass: cr.movement.boundMove(),
            searcher: cr,
            searchee: actor.target,
            searchTimeout: 10
          });
        }
      // probably don't want separate logic for doors
      } else if (actor.target && actor.target.parent==="Feature" && actor.target.defender && (!actor.target.owner || actor.isHostile(actor.target.owner))) {
        if (HTomb.Tiles.isTouchableFrom(actor.target.x, actor.target.y,actor.target.z, cr.x, cr.y, cr.z)) {
          actor.entity.attacker.attack(actor.target);
          actor.acted = true;
        } else {
          actor.walkToward(actor.target.x,actor.target.y,actor.target.z,{
            canPass: cr.movement.boundMove(),
            searcher: cr,
            searchee: actor.target,
            searchTimeout: 10
          });
        }
      }
    }
  });

  Behavior.extend({
    template: "LongRangeRoam",
    name: "long range roam",
    act: function(actor) {
      if (actor.target===null) {
        let x = ROT.RNG.getUniformInt(1,LEVELW-2);
        let y = ROT.RNG.getUniformInt(1,LEVELH-2);
        let z = HTomb.Tiles.groundLevel(x,y);
        let cr = actor.entity;
        let canMove = cr.movement.canMove.bind(cr);
        if (HTomb.Tiles.isReachableFrom(x,y,z,cr.x,cr.y,cr.z, {
          canPass: canMove
        })) {
          actor.target = HTomb.Tiles.getTileDummy(x,y,z) || null;
        }
      }
      if (actor.target && HTomb.Tiles.isTouchableFrom(actor.target.x, actor.target.y, actor.target.z, actor.entity.x, actor.entity.y, actor.entity.z)) {
        actor.target = null;
        return;
      }
      if (actor.target!==null) {
        actor.walkToward(actor.target.x, actor.target.y, actor.target.z, {
          searcher: actor.entity,
          searchee: actor.target,
          cacheAfter: 25,
          cacheTimeout: 20
        });
      }
    }
  });

  Behavior.extend({
    template: "HuntPlayer",
    name: "hunt player",
    act: function(actor) {
      let c = actor.entity;
      let canMove = c.movement.boundMove();
      // so...this tends to lock onto one thing and seek it out relentlessly.
      // we want to it be able to acquire other targets
      if (actor.target===null) {
        let p = HTomb.Player;
        if (HTomb.Tiles.isReachableFrom(c.x,c.y,c.z,p.x,p.y,p.z,{canPass: canMove})) {
          actor.target = p;
        } else {
          let minions = HTomb.Player.master.minions;
          if (minions.length>0) {
            for (let m of minions) {
              if (HTomb.Tiles.isReachableFrom(c.x,c.y,c.z,m.x,m.y,m.z,{canPass: canMove})) {
                actor.target = m;
                break;
              }
            }
          }
        }
        if (actor.target===null) {
          for (let block in HTomb.World.blocks) {
            let b = HTomb.World.blocks[block];
            if (b.owner===HTomb.Player && HTomb.Tiles.isReachableFrom(c.x,c.y,c.z,b.x,b.y,b.z,{canPass: canMove})) {
              actor.target = b;
              break;
            }
          }
        }
        if (actor.target && HTomb.Tiles.isTouchableFrom(actor.target.x, actor.target.y, actor.target.z, actor.entity.x, actor.entity.y, actor.entity.z)) {
          actor.target = null;
          return;
        }
        if (actor.target!==null) {
          actor.walkToward(actor.target.x, actor.target.y, actor.target.z, {
            searcher: actor.entity,
            searchee: actor.target,
            cacheAfter: 25,
            cacheTimeout: 20
          });
        }
      }
    }
  });

  Behavior.extend({
    template: "HuntDeadThings",
    name: "hunt dead things",
    act: function(actor) {
      // should this hunt in sight range first?
      if (actor.target===null) {
        var zombies = HTomb.World.creatures.filter(function(v,k,o) {
          return (v.template==="Zombie" && actor.isHostile(v) && HTomb.Tiles.isEnclosed(v.x,v.y,v.z)===false);
        });
        if (zombies.length>0) {
          var e = actor.entity;
          zombies.sort(function(a,b) {
            return HTomb.Path.quickDistance(e.x,e.y,e.z,a.x,a.y,a.z) - HTomb.Path.quickDistance(e.x,e.y,e.z,b.x,b.y,b.z);
          });
          actor.target = zombies[0];
          console.log("hunting a zombie");
          HTomb.Behaviors.CheckForHostile.act(actor);
        }
      }
    }
  });

  return HTomb;
})(HTomb);
