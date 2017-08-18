// ****** This module implements Behaviors, which are the basic units of functionality for creatures, items, and features
HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;

  HTomb.Things.defineBehavior({
    template: "Player",
    name: "player",
    controlling: null,
    onAdd: function() {
      HTomb.Player = this.entity;
    },
    onDescribe: function(options) {
      if (options.article==="indefinite") {
        options.article = "definite";
      }
      return options;
    },
    playerDeath: function() {
      HTomb.Time.lockTime();
      HTomb.GUI.Contexts.locked=true;
      HTomb.GUI.pushMessage("%c{red}You have died!");
      setTimeout(function(){
        HTomb.GUI.Views.parentView = HTomb.GUI.Views.startup;
        let context = HTomb.GUI.Contexts.new();
        context.clickTile = context.rightClickTile = context.keydown = function() {
          HTomb.GUI.reset();
        };
        HTomb.GUI.Contexts.active = context;
        HTomb.GUI.Contexts.locked = false;
      },2000);
    },
    visibility: function() {
      let p = this.entity;
      HTomb.FOV.resetVisible();
      if (p.sight) {
        HTomb.FOV.findVisible(p.x, p.y, p.z, p.sight.range);
      }
      if (p.master) {
        for (let i=0; i<p.master.minions.length; i++) {
          let cr = p.master.minions[i];
          if (cr.sight) {
            HTomb.FOV.findVisible(cr.x,cr.y,cr.z, cr.sight.range);
          }
        }
      }
    }
  });

  let player = HTomb.Things.templates.Player;
  let delegate = null;
  Object.defineProperty(player,"delegate", {
    get: function() {
      if (delegate===null) {
        delegate = HTomb.Player;
      }
      return delegate;
    },
    set: function(d) {
      delegate = d;
    }
  });

  HTomb.Things.defineBehavior({
    template: "PointLight",
    name: "pointlight",
    point: null,
    level: 255,
    range: 8,
    onAdd: function() {
      this.point = this.entity;
    },
    onPlace: function() {
      if (HTomb.World.lights.indexOf(this)===-1) {
        HTomb.World.lights.push(this);
      }
      HTomb.World.validate.lighting();
    },
    onRemove: function() {
      if (HTomb.World.lights.indexOf(this)!==-1) {
        HTomb.World.lights.splice(HTomb.World.lights.indexOf(this),1);
        HTomb.World.validate.lighting();
      }
    }
  })

  HTomb.Things.defineBehavior({
    template: "Senses",
    name: "senses",
    sightRange: 10,
    audioRange: 10,
    smellRange: 10,
    darkSight: false,
    onAdd: function(options) {
      options = options || {};
      this.sightRange = options.sightRange || this.sightRange;
      this.audioRange = options.audioRange || this.audioRange;
      this.smellRange = options.smellRange || this.smellRange;
      this.darkSight = options.darkSight || this.darkSight;
    },
    canSee: function(x,y,z) {
      return true;
    },
    getSeen: function() {
      var squares = [];
      return squares;
    },
    canSmell: function(x,y,z) {
      return true;
    },
    getSmelled: function() {
      var squares = [];
      return squares;
    },
    canHear: function(x,y,z) {
      return true;
    },
    getHeard: function() {
      var squares = [];
      return squares;
    }
  });

  // The Sight behavior allows a creature to see
  HTomb.Things.defineBehavior({
    template: "Sight",
    name: "sight",
    range: 10,
    onAdd: function(options) {
      options = options || {};
      if (options.range) {
        this.range = options.range;
      }
    },
    getSeen: function() {

    }
  });

  // The Inventory behavior allows a creature to carry things
  HTomb.Things.defineBehavior({
    template: "Inventory",
    name: "inventory",
    capacity: 10,
    onAdd: function() {
      this.items = HTomb.Things.Container({heldby: this});
    },
    pickup: function(item) {
      var e = this.entity;
      if (this.entity.minion && this.entity.minion.master) {
        item.item.owner = this.entity.minion.master;
      }
      item.remove();
      HTomb.GUI.sensoryEvent(this.entity.describe({capitalized: true, article: "indefinite"}) + " picks up " + item.describe({article: "indefinite"})+".",e.x,e.y,e.z);
      this.add(item);
      this.entity.ai.acted = true;
      this.entity.ai.actionPoints-=16;
    },
    pickupOne: function(i_or_t) {
      var e = this.entity;
      var items = HTomb.World.items[coord(e.x,e.y,e.z)];
      var item = items.takeOne(i_or_t);
      if (item) {
        this.pickup(item);
      }
    },
    pickupSome: function(i_or_t,n) {
      var e = this.entity;
      var items = HTomb.World.items[coord(e.x,e.y,e.z)];
      var item = items.takeSome(i_or_t,n);
      if (item) {
        this.pickup(item);
      }
    },
    drop: function(item) {
      var e = this.entity;
      this.items.remove(item);
      item.place(e.x,e.y,e.z);
      HTomb.GUI.sensoryEvent(this.entity.describe({capitalized: true, article: "indefinite"}) + " drops " + item.describe({article: "indefinite"})+".",e.x,e.y,e.z);
      this.entity.ai.acted = true;
      this.entity.ai.actionPoints-=16;
    },
    add: function(item) {
      if (this.items.length>=this.capacity) {
        HTomb.GUI.pushMessage("Can't pick that up.");
      } else {
        this.items.push(item);
      }
    },
    hasAll: function(ingredients) {
      for (var ing in ingredients) {
        var n = ingredients[ing];
        // if we lack what we need, search for items
        if (this.items.countAll(ing)<n) {
          return false;
        }
      }
      return true;
    },
    onDespawn: function() {
      if (this.items) {
        this.items.despawn();
      }
    },
    canFindAll: function(ingredients) {
      if (!this.entity.minion || !this.entity.minion.master || !this.entity.minion.master.master) {
        return false;
      }
      let master = this.entity.minion.master.master;
      for (let item in ingredients) {
        let items = master.ownedItems.filter(function(it) {
          if (it.template===item && it.item.isOnGround()===true) {
            return true;
          } else {
            return false;
          }
        });
        let n = 0;
        for (let i=0; i<items.length; i++) {
          n+= (items[i].item.n || 1);
        }
        if (n<ingredients[item]) {
          // if any ingredients are missing, do not assign
          return false;
        }
      }
      return true;
    }
  });

  // Not yet functional
  HTomb.Things.defineBehavior({
    template: "Attacker",
    name: "attack"
  });
  // The Minion behavior allows a creature to serve a master and take orders
  HTomb.Things.defineBehavior({
    template: "Minion",
    name: "minion",
    master: null,
    setMaster: function(cr) {
      this.master = cr;
      this.entity.ai.setTeam(cr.ai.team);
      HTomb.Events.subscribe(this,"Destroy");
    },
    onDestroy: function(event) {
      if (event.entity===this.master) {
        this.removeFromEntity();
        this.despawn();
      }
    }
  });

  HTomb.Things.defineBehavior({
    template: "Worker",
    name: "worker",
    task: null,
    allowedTasks: ["DigTask","BuildTask","PatrolTask","FurnishTask","HaulTask","ConstructTask","ProduceTask","DismantleTask","HarvestFarmTask","ConvergeTask"],
    onAssign: function(tsk) {
      this.task = tsk;
      HTomb.Debug.pushMessage(this.entity.describe({capitalized: true, article: "indefinite"}) + " was assigned " + tsk.describe());
    },
    unassign: function() {
      if (this.task===null) {
        return;
      }
      HTomb.Debug.pushMessage(this.entity.describe({capitalized: true, article: "indefinite"}) + " was unassigned from " + this.task.describe());
      this.task = null;
    }
  });

  HTomb.Things.defineBehavior({
    template: "Master",
    name: "master",
    minions: null,
    taskList: null,
    workshops: null,
    tasks: null,
    ownedItems: null,
    onCreate: function(options) {
      options = options || {};
      this.tasks = options.tasks || [];
      this.minions = [];
      this.taskList = [];
      this.structures = [];
      this.ownedItems = [];
      HTomb.Events.subscribe(this, "Destroy");
      return this;
    },
    onDestroy: function(event) {
      if (this.minions.indexOf(event.entity)>-1) {
        HTomb.GUI.sensoryEvent(this.entity.describe({capitalized:true, article:"indefinite"}) + " mourns the death of " + event.entity.describe({article: "indefinite"})+".",this.entity.x,this.entity.y,this.entity.z);
        this.minions.splice(this.minions.indexOf(event.entity),1);
      }
    },
    addMinion: function(cr) {
      this.minions.push(cr);
      cr.minion.setMaster(this.entity);
    },
    removeMinion: function(cr) {
      this.minions.splice(this.minions.indexOf(cr,1));
    },
    addWorkshop: function(w) {
      this.workshops.push(w);
    },
    removeWorkshop: function(w) {
      this.workshops.splice(this.workshops.indexOf(w,1));
    },
    designate: function(tsk) {
      tsk.designate(this.entity);
    },
    assignTasks: function() {
      let priorities = {
        DigTask: 1,
        BuildTask: 1,
        ConstructTask: 1,
        DismantleTask: 1,
        FurnishTask: 1,
        ProduceTask: 1,
        HaulTask: 2,
        PatrolTask: 3
      };
      HTomb.Utils.shuffle(this.taskList);
      //count down dormant tasks
      for (let i=0; i<this.taskList.length; i++) {
        let task = this.taskList[i].task;
        if (task.dormant>0) {
          task.dormant-=1;
        }
      }
      let failed = [];
      for (let i=0; i<this.minions.length; i++) {
        let minion = this.minions[i];
        if (minion.worker===undefined) {
          continue;
        } else if (minion.worker.task!==null) {
          continue;
        } else {
          let MAXPRIORITY = 3;
          for (let j=0; j<=MAXPRIORITY; j++) {
            let tasks = this.taskList.filter(function(e,i,a) {return (priorities[e.task.template]===j && !e.dormant && e.task.assignee===null)});
            // for hauling tasks, this gives misleading results...but I could fix that
            tasks = HTomb.Path.closest(minion.x, minion.y, minion.z,tasks);
            for (let k=0; k<tasks.length; k++) {
              let task = tasks[k].task;
              if (minion.worker.allowedTasks.indexOf(task.template)!==-1 && task.canAssign(minion)) {
                task.assignTo(minion);
                //very ad hoc
                let j = MAXPRIORITY+1;
                break;
              } else if (failed.indexOf(task)===-1) {
                failed.push(task);
              }
            }
          }
        }
      }
      for (let i=0; i<failed.length; i++) {
        let task = failed[i];
        task.dormant = HTomb.Utils.perturb(task.dormancy);
      }
    },
    listTasks: function() {
      var tasks = [];
      for (var i=0; i<this.tasks.length; i++) {
        tasks.push(HTomb.Things.templates[this.tasks[i]]);
      }
      return tasks;
    },
    ownsAllIngredients: function(ingredients) {
      let owned = {};
      for (let i=0; i<this.ownedItems.length; i++) {
        let temp = this.ownedItems[i].template
        if (ingredients[temp]>0) {
          owned[temp] = owned[temp] || 0;
          let n = this.ownedItems[i].n || 1;
          owned[temp]+=n;
        }
      }
      for (let ing in ingredients) {
        if (!owned[ing] || owned[ing]<ingredients[ing]) {
          return false;
        }
      }
      return true;
    }
  });

  // The SpellCaster behavior maintains a list of castable spells
  HTomb.Things.defineBehavior({
    template: "SpellCaster",
    name: "caster",
    maxmana: 20,
    mana: 20,
    onCreate: function(options) {
      options = options || {};
      options.spells = options.spells || [];
      this.spells = [];
      for (let i=0; i<options.spells.length; i++) {
        this.spells.push(HTomb.Things[options.spells[i]]({caster: this}));
        //this.spells[i].caster = this;
      }
      HTomb.Events.subscribe(this,"TurnBegin");
      return this;
    },
    onTurnBegin: function() {
      if (this.mana<this.maxmana && Math.random()<(1/10)) {
        this.mana+=1;
      }
    },
    cast: function(sp) {
      let cost = sp.getCost();
      if (this.mana>=cost) {
        sp.cast();
      }
    }
  });

  // The Movement behavior allows the creature to move
  HTomb.Things.defineBehavior({
    template: "Movement",
    name: "movement",
    // flags for different kinds of movement
    walks: true,
    climbs: true,
    displaceCreature: function(cr) {
      var x0 = this.entity.x;
      var y0 = this.entity.y;
      var z0 = this.entity.z;
      var x = cr.x;
      var y = cr.y;
      var z = cr.z;
      cr.remove();
      this.entity.place(x,y,z);
      cr.place(x0,y0,z0);
      //HTomb.GUI.sensoryEvent(this.entity.describe({capitalized: true, article: "indefinite"}) + " displaces " + cr.describe({article: "indefinite"}) + ".",x,y,z);
      if (this.entity.ai) {
        this.entity.ai.acted = true;
        this.entity.ai.actionPoints-=16;
      }
      if (cr.ai) {
        cr.ai.acted = true;
        this.entity.ai.actionPoints-=16;
      }
    },
    stepTo: function(x,y,z) {
      this.entity.place(x,y,z);
      if (this.entity.ai) {
        this.entity.ai.acted = true;
        this.entity.ai.actionPoints-=16;
      }
      // unimplemented...use action points?
    },
    // If the square is crossable and unoccupied
    canPass: function(x,y,z) {
      if (this.canMove(x,y,z)===false) {
        return false;
      }
      var square = HTomb.Tiles.getSquare(x,y,z);
      if (square.creature) {
        return false;
      }
      return true;
    },
    // If the square is crossable for this creature
    canMove: function(x,y,z,x0,y0,z0) {
      if (x<0 || x>=LEVELW || y<0 || y>=LEVELH || z<0 || z>=NLEVELS) {
        return false;
      }
      var c = coord(x,y,z);
      ////Passability independent of position
      // can't go through solid terrain
      var terrain = HTomb.World.tiles[z][x][y];
      if (terrain.solid===true && this.phases!==true) {
        return false;
      }
      // can't walk over a pit
      if (terrain.fallable===true && this.flies!==true) {
        return false;
      }
      var cover = HTomb.World.covers[z][x][y];
      if (cover!==HTomb.Covers.NoCover && cover.liquid && this.swims!==true) {
        return false;
      }
      let e = this.entity;
      // can't go through a zone your master forbids
      if (e.minion) {
        let task = HTomb.World.tasks[c];
        if (task && task.task.template==="ForbidTask" && task.task.assigner===e.minion.master) {
          return false;
        }
      }
      // can't go through solid feature
      var feature = HTomb.World.features[c];
      if (feature && feature.solid===true && this.phases!==true) {
        return false;
      }
      ////Passability dependent on position
      var dx = x-(x0 || e.x);
      var dy = y-(y0 || e.y);
      var dz = z-(z0 || e.z);
      // a way to check whether the square itself is allowed, for certain checks
      if (dx===0 && dy===0 && dz===0) {
        return true;
      }
      // non-flyers can't climb diagonally
      if (this.flies!==true && dz!==0 && (dx!==0 || dy!==0)) {
        return false;
      // non-flyers need a slope in order to go up
      }
      var t = HTomb.World.tiles[z-dz][x-dx][y-dy];
      if (dz===+1 && this.flies!==true && t.zmove!==+1) {
        return false;
      }
      var tu = HTomb.World.tiles[z+1-dz][x-dx][y-dy];
      // non-phasers can't go through a ceiling
      if (dz===+1 && this.phases!==true && tu.fallable!==true && tu.zmove!==-1) {
        return false;
      }
      // non-phasers can't go down through a floor
      if (dz===-1 && t.fallable!==true && t.zmove!==-1 && this.phases!==true) {
        return false;
      }
      if (this.walks===true) {
        return true;
      }
      if (this.flies===true) {
        return true;
      }
      if (this.swims===true && cover && cover.liquid) {
        return true;
      }
      return false;
    }
  });


  HTomb.Things.defineBehavior({
  	template: "Body",
  	name: "body",
  	materials: null,
    armor: null,
  	endure: function(attack) {
      let damage = attack.damage;
      for (let d in damage) {
        for (let m in this.materials) {
          let dice = damage[d];
          let n = HTomb.Utils.dice(dice[0],dice[1]);
          if (dice[2]) {
            n+=dice[2];
          }
          var adjusted = Math.round(n*HTomb.Types.templates.Damage.table[d][m]);
          this.materials[m].has-=adjusted;
          if (adjusted>1) {
            HTomb.Particles.addEmitter(this.entity.x,this.entity.y,this.entity.z,HTomb.Particles.Blood);
          }
        }
        let died = false;
        for (var m in this.materials) {
          //how do we decide how to die first?  just do it in order I guess...
          if (this.materials[m].has < this.materials[m].needs) {
            died = true;
          }
        }
        if (died) {
          this.entity.creature.die();
        }
      }
    },
    onCreate: function(options) {
      this.materials = {};
      options = options || {};
      for (var m in options.materials) {
        this.materials[m] = {};
        // if there's just one number, fall back on a default
        if (typeof(options.materials[m])==="number") {
          this.materials[m].max = options.materials[m];
          this.materials[m].has = options.materials[m];
          this.materials[m].needs = Math.floor(options.materials[m]/2);
        } else {
        // otherwise expect maximum and minimum
          this.materials[m].max = options.materials[m].max;
          this.materials[m].has = options.materials[m].max;
          this.materials[m].needs = options.materials[m].needs;
        }
      }
      return this;
    }
  });

  HTomb.Things.defineBehavior({
  	template: "Combat",
  	name: "combat",
    accuracy: 0,
    evasion: 0,
    armor: 0,
    damage: null,
    onCreate: function(options) {
      return this;
    },
  	// worry about multiple attacks later
  	attack: function(thing) {
      // if it's a combatant, you might miss
      var evade = (thing.combat) ? thing.combat.evasion : 0;
      var accuracy = this.accuracy;
      // basic hit roll
      var roll = HTomb.Utils.dice(1,20);
      if (roll+accuracy >= 10+evade) {
        HTomb.GUI.sensoryEvent(this.entity.describe({capitalized: true, article: "indefinite"}) + " hits " + thing.describe({article: "indefinite"})+".",this.entity.x,this.entity.y,this.entity.z,"orange");
        //apply armor in some way?
        thing.body.endure(this);
      } else {
        HTomb.GUI.sensoryEvent(this.entity.describe({capitalized: true, article: "indefinite"}) + " misses " + thing.describe({article: "indefinite"})+".",this.entity.x,this.entity.y,this.entity.z,"yellow");
      }
      this.entity.ai.acted = true;
      this.entity.ai.actionPoints-=16;
  	},
  	//should be on the damage packet..//hit: function() {},
  	defend: function() {
      // do nothing for now
  	}
  });


  HTomb.Things.defineBehavior({
    template: "Equipment",
    name: "equipment",
    slot: null,
    labor: 1,
    onEquip: function(equipper) {},
    onUnequip: function(equipper) {}
  });

  HTomb.Things.defineBehavior({
    template: "Equipper",
    name: "equipper",
    items: null,
    slots: {
      MainHand: null,
      OffHand: null    
    },
    onCreate: function(args) {
      this.items = HTomb.Things.Container();
      this.slots = HTomb.Utils.copy(this.slots);
      return this;
    },
    unequipItem: function(slot_or_item) {
      let item;
      if (typeof(slot_or_item)==="string") {
        item = this.slots[slot];
      } else {
        item = slot_or_item;
      }
      if (item) {
        this.items.remove(item);
        item.equipment.onUnequip(this);
        if (this.entity.inventory) {
          this.entity.inventory.add(item);
        } else {
          let e = this.entity;
          item.place(e.x,e.y,e.z);
        }
        this.entity.slot = null;
      }
      return item;
    },
    equipItem: function(item) {
      let e = item.equipment;
      if (e) {
        let slot = e.slot;
        if (this.slots[slot]!==undefined) {
          if (this.slots[slot]!==null) {
            this.unequipItem(slot);
          }
          this.items.push(item);
          e.onEquip(this);
          this.slots[slot] = item;
        }
      }
    },
    onDespawn: function() {
      if (this.items) {
        this.items.despawn();
      }
    }
  });

  return HTomb;
})(HTomb);
