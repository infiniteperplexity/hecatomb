// ****** This module implements Behaviors, which are the basic units of functionality for creatures, items, and features
HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;

  let Behavior = HTomb.Things.Behavior;

  Behavior.extend({
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
      HTomb.GUI.alert("%c{red}You have died!",1000);
      setTimeout(function(){
        HTomb.GUI.Views.parentView = HTomb.GUI.Views.startup;
        let context = HTomb.GUI.Contexts.new();
        context.clickTile = context.rightClickTile = context.keydown = context.clickAlert = context.clickAt = function() {
          HTomb.GUI.closeAlert();
          HTomb.GUI.reset();
        };
        HTomb.GUI.Contexts.active = context;
      },1000);
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

  let player = HTomb.Things.Player;
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

  Behavior.extend({
    template: "PointLight",
    name: "pointlight",
    level: 255,
    range: 8,
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
    },
    illuminate: function() {
      let e = this.entity;
      HTomb.FOV.pointIlluminate(e.x,e.y,e.z,this.range);
    }
  });

  Behavior.extend({
    template: "StructureLight",
    name: "structurelight",
    level: 255,
    range: 3,
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
    },
    illuminate: function(x,y,z) {
      let e = this.entity;
      for (let i=-this.range; i<=this.range; i++) {
        for (let j=-this.range; j<this.range; j++) {
          HTomb.World.lit[e.z][e.x+i][e.y+j] = this.level;
        }
      }
    }
  });


  Behavior.extend({
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
  Behavior.extend({
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
  Behavior.extend({
    template: "Inventory",
    name: "inventory",
    capacity: 10,
    onSpawn: function() {
      this.items = HTomb.Things.Items(this);
    },
    pickup: function(item) {
      var e = this.entity;
      if (this.entity===HTomb.Player || (this.entity.minion && this.entity.minion.master===HTomb.Player)) {
        item.owned = true;
      }
      item.remove();
      HTomb.GUI.sensoryEvent(this.entity.describe({capitalized: true, article: "indefinite"}) + " picks up " + item.describe({article: "indefinite"})+".",e.x,e.y,e.z);
      this.items.addItem(item);
      this.entity.ai.acted = true;
      this.entity.ai.actionPoints-=16;
    },
    pickupOne: function(i_or_t) {
      var e = this.entity;
      var items = HTomb.World.items[coord(e.x,e.y,e.z)] || HTomb.Things.Items();
      var item = items.take(i_or_t);
      if (item) {
        this.pickup(item);
      }
    },
    pickupSome: function(i_or_t,n) {
      var e = this.entity;
      var items = HTomb.World.items[coord(e.x,e.y,e.z)] || HTomb.Things.Items();
      var item = items.take(i_or_t,n);
      if (item) {
        this.pickup(item);
      }
    },
    drop: function(item) {
      var e = this.entity;
      this.items.removeItem(item);
      item.place(e.x,e.y,e.z);
      HTomb.GUI.sensoryEvent(this.entity.describe({capitalized: true, article: "indefinite"}) + " drops " + item.describe({article: "indefinite"})+".",e.x,e.y,e.z);
      this.entity.ai.acted = true;
      this.entity.ai.actionPoints-=16;
    },
    push: function(item) {
      this.addItem(item);
    },
    unshift: function(item) {
      this.addItem(item);
    },
    addItem: function(item) {
      this.items.addItem(item);
    },
    count: function(arg) {
      return this.items.count(arg);
    },
    contains: function(item) {
      return this.items.contains(item);
    },
    getItem: function(arg) {
      return this.items.getItem(arg);
    },
    take: function(arg,n) {
      return this.items.take(arg,n);
    },
    takeItems: function(ings) {
      return this.items.takeItems(ings);
    },
    removeItem: function(item) {
      return this.items.removeItem(item);
    },
    hasAll: function(ingredients) {
      return this.items.hasAll(ingredients);
    },
    asIngredients: function() {
      return this.items.asIngredients();
    },
    // !!!! should there be an "onEntityDestroyed"?
    onDespawn: function() {
      //should probably drop all the items, right?
      if (this.entity.isPlaced()) {
        for (let item of this.items) {
          this.drop(item);
        }
      }
    },
    // !!!not used anymore?
    canFindAll: function(ingredients) {
      // this doesn't actually check for the path...
      if (HTomb.Debug.noingredients) {
        return true;
      }
      if (!this.entity.minion || !this.entity.minion.master || !this.entity.minion.master.master) {
        return false;
      }
      // this looks ridiculous because we are accessing the master property of a creature that is a master
      let master = this.entity.minion.master.master;
      let that = this;
      for (let ingredient in ingredients) {
        let items = master.ownedItems().filter(function(item) {
          if (item.template!==ingredient) {
            return false;
          } else if (item.isOnGround()===true) {
            return true;
          } else if (that.items.contains(item)) {
            return true;
          } else {
            console.log("where the heck is " + item.describe() + " then???");
            console.log(item);
            return false;
          }
        });
        for (let item of items) {
          if (item.n<ingredients[ingredient]) {
          // if any ingredients are missing, do not assign
            return false;
          }
        }     
      }
      return true;
    }
  });


  // The Minion behavior allows a creature to serve a master and take orders
  Behavior.extend({
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
        this.detachFromEntity();
        this.despawn();
      }
    }
  });

  Behavior.extend({
    template: "Worker",
    name: "worker",
    labor: 1,
    task: null,
    allowedTasks: ["EquipTask","DigTask","BuildTask","PatrolTask","FurnishTask","HaulTask","ConstructTask","ProduceTask","DismantleTask","RepairTask","TradeTask","ResearchTask"],
    onAssign: function(tsk) {
      if (this.task!==null) {
        this.task.unassign();
        throw new Error("What just happened?!?!");
      }
      this.task = tsk;
      HTomb.Debug.pushMessage(this.entity.describe({capitalized: true, article: "indefinite"}) + " was assigned " + tsk.describe());
    },
    unassign: function() {
      if (this.task===null) {
        return;
      }
      HTomb.Debug.pushMessage(this.entity.describe({capitalized: true, article: "indefinite"}) + " was unassigned from " + this.task.describe());
      this.task = null;
    },
    getLabor: function() {
      let e = this.entity.equipper;
      if (e && e.slots.MainHand && e.slots.MainHand.equipment.labor>this.labor) {
        return e.slots.MainHand.equipment.labor;
      }
      return this.labor;
    }
  });


  Behavior.extend({
    template: "Master",
    name: "master",
    minions: null,
    taskList: null,
    workshops: null,
    tasks: null,
    onSpawn: function(options) {
      options = options || {};
      this.tasks = options.tasks || [];
      this.minions = [];
      this.taskList = [];
      this.structures = [];
      HTomb.Events.subscribe(this, "Destroy");
      return this;
    },
    onDestroy: function(event) {
      if (this.minions.indexOf(event.entity)!==-1) {
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
        RepairTask: 1,
        HaulTask: 2,
        PatrolTask: 3,
        ResearchTask: 1,
        TradeTask: 1
      };
      HTomb.Utils.shuffle(this.taskList);
      //count down dormant tasks
      for (let i=0; i<this.taskList.length; i++) {
        let task = this.taskList[i];
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
          // look for labor tools
          let labor = minion.worker.getLabor();
          let invenTools = this.ownedItems().filter(function(e,i,a) {
            return (e.equipment && e.claimed < e.n && e.equipment.labor>labor && minion.inventory && minion.inventory.items.indexOf(e)!==-1);
          });
          let groundTools = this.ownedItems().filter(function(e,i,a) {
            return (e.equipment && e.claimed < e.n && e.equipment.labor>labor && e.isOnGround());
          });
          // sort by labor value
          let comp = function(a,b) {
            if (a.equipment.labor>b.equipment.labor) {
              return 1;
            } else if (a.equipment.labor<b.equipment.labor) {
              return -1;
            } else {
              return 0;
            }
          };
          invenTools.sort(comp);
          groundTools.sort(comp);
          // equip best ones
          if (invenTools.length>0) {
            minion.equipper.equipItem(invenTools[0]);
            continue;
          } else if (groundTools.length>0) {
            let task = HTomb.Things.EquipTask.spawn({
              assigner: this.entity,
              item: groundTools[0],
              name: "equip " + groundTools[0].describe()
            });
            task.assignTo(minion);
            continue;
          }
          let MAXPRIORITY = 3;
          for (let j=0; j<=MAXPRIORITY; j++) {
            let tasks = this.taskList.filter(function(task,i,a) {return (priorities[task.template]===j && !(task.dormant>0) && task.assignee===null)});
            // for hauling tasks, this gives misleading results...but I could fix that
            tasks = HTomb.Path.closest(minion.x, minion.y, minion.z,tasks);
            for (let k=0; k<tasks.length; k++) {
              let task = tasks[k];
              if (minion.worker.task===null && minion.worker.allowedTasks.indexOf(task.template)!==-1 && task.canAssign(minion)) {
                task.assignTo(minion);
                //very ad hoc
                j = MAXPRIORITY+1;
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
        tasks.push(HTomb.Things[this.tasks[i]]);
      }
      return tasks;
    },
    ownedItems: function() {
      // should this return an Items list?
      return HTomb.Utils.where(HTomb.World.things, function(item) {return (item.parent==="Item" && item.owned);});
    },
    ownsAllIngredients: function(ingredients) {
      if (HTomb.Debug.noingredients) {
        return true;
      }
      let ownedItems = this.ownedItems();
      let owned = {};
      for (let i=0; i<ownedItems.length; i++) {
        let temp = ownedItems[i].template
        if (ingredients[temp]>0) {
          owned[temp] = owned[temp] || 0;
          // should respect claims
          let n = ownedItems[i].n-ownedItems[i].claimed || 1;
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
  Behavior.extend({
    template: "SpellCaster",
    name: "caster",
    baseEntropy: 20,
    entropy: 20,
    onSpawn: function(options) {
      options = options || {};
      options.spells = options.spells || [];
      this.spells = [];
      for (let i=0; i<options.spells.length; i++) {
        this.spells.push(HTomb.Things[options.spells[i]].spawn({caster: this}));
        this.spells[i].caster = this;
      }
      HTomb.Events.subscribe(this,"TurnBegin");
      return this;
    },
    onTurnBegin: function() {
      if (this.entropy<this.getMaxEntropy() && Math.random()<(1/10)) {
        this.entropy+=1;
      }
    },
    getMaxEntropy: function() {
      let ent = this.baseEntropy;
      if (this.entity.master) {
        for (let s of this.entity.master.structures) {
          if (s.template==="Sanctum") {
            ent+=5;
          }
        }
      }
      return ent;
    },
    cast: function(sp) {
      let cost = sp.getCost();
      if (this.entropy>=cost) {
        sp.cast();
      }
    }
  });

  // The Movement behavior allows the creature to move
  Behavior.extend({
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
      this.stepTo(x,y,z);
      if (cr.movement) {
        cr.movement.stepTo(x0,y0,z0);
      } else {
        cr.place(x0,y0,z0);
      }
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
      let cost = 16;
      if (HTomb.World.items[coord(x,y,z)] && !this.flies) {
        cost = 25;
      }
      if (this.entity.ai) {
        this.entity.ai.acted = true;
        this.entity.ai.actionPoints-=cost;
      }
      HTomb.Events.publish({type: "Step", creature: this.entity});
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
    boundMove: function() {
      return this.canMove.bind(this);
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
        if (task && task.template==="ForbidTask" && task.assigner===e.minion.master) {
          return false;
        }
      }
      // can't go through solid feature
      var feature = HTomb.World.features[c];
      if (feature && feature.solid===true && this.phases!==true) {
        if (!feature.owner || feature.owner.ai.team!==e.ai.team) {
          return false;
        }
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


  let Damage = HTomb.Types.Damage;

  Behavior.extend({
    template: "Attacker",
    name: "attacker",
    damage: {
      type: "Slashing",
      level: 0,
    },
    accuracy: 0,
    checkTerrain(v,r) {
      let e = this.entity;
      // higher ground
      if (e.z>v.z) {
        r+=1;
      }
      // guard post
      if (v.minion && v.minion.master && v.minion.master.master) {
        let gp = false;
        for (let s of v.minion.master.master.structures) {
          if (s.template==="GuardPost" && s.z===v.z && HTomb.Path.quickDistance(s.x,s.y,s.z,v.x,v.y,v.z)<=s.defenseRange) {
            gp = true;
          }
        }
        if (gp) {
          r-=1;
        }
      }
      return r;
    },
    attack: function(victim) {
      let e = this.entity;
      let evade = (victim.defender) ? victim.defender.evasion - victim.defender.wounds.level : -10;
      let roll = HTomb.Utils.dice(1,20);
      if (e.entity && e.entity.equipper
            && e.entity.equipper.slots.MainHand
            && e.entity.equipper.slots.MainHand.accuracy) {
        roll += e.entity.equipper.slots.MainHand.accuracy;
      }
      roll = this.checkTerrain(victim, roll);
      if (roll+this.accuracy >= 11 + evade) {
        (victim.defender) ? victim.defender.defend(this) : {};       
      } else {
        HTomb.GUI.sensoryEvent(this.entity.describe({capitalized: true, article: "indefinite"}) + " misses " + victim.describe({article: "indefinite"})+".",this.entity.x,this.entity.y,this.entity.z,"yellow");
      }
      if (this.entity.ai) {
        this.entity.ai.acted = true;
        this.entity.ai.actionPoints-=16;
      }
    }
  });

  Behavior.extend({
  	template: "Defender",
  	name: "defender", 
    material: "Flesh",
    evasion: 0,
    armor: {
      material: "Flesh",
      level: 0
    },
    toughness: 0,
    wounds: {
      type: null,
      level: 0 // 2 is mild, 4 is moderate, 6 is severe, 8 is dead
    },
    onSpawn: function() {
      this.wounds = HTomb.Utils.copy(HTomb.Things.Defender.wounds);
    },
    tallyWounds: function() {
      console.log("Wounds: ",this.wounds.level);
      if (this.wounds.level<=0) {
        this.wounds.level = 0;
        this.wounds.type = null;
      }
      if (this.wounds.level>=8) {
        if (this.entity.die) {
          this.entity.die();
        } else {
          this.entity.destroy();
          HTomb.GUI.sensoryEvent(this.entity.describe({article: "indefinite", capitalized: true}) +" is destroyed.",x,y,z,"#FFBB00");
        }
      }
    },
    endure: function(roll, attack) {
      let atype = attack.damage.type;
      let alevel = attack.damage.level;

      if (attack.entity && attack.entity.equipper
            && attack.entity.equipper.slots.MainHand
            && attack.entity.equipper.slots.MainHand.damage) {
        let d = attack.entity.equipper.slots.MainHand;
        if (d.type) {
          atype = d.type;
        }
        if (d.level) {
          alevel = d.level;
        }
      }
      let modifier = Damage.table[atype][this.material];
      let penetrate = Damage.table[atype][this.armor.material];
      let total = roll;
      total += alevel;
      total += modifier;
      total += this.wounds.level;
      if (attack.entity.defender) {
        total -= attack.entity.defender.wounds.level;
      }
      // armor can never leave you worse off
      total -= Math.max(0,this.armor.level-penetrate);
      total -= this.toughness;
      let x = this.entity.x;
      let y = this.entity.y;
      let z = this.entity.z;
      let attacker = attack.entity.describe({capitalized: true, article: "indefinite"});
      let defender = this.entity.describe({article: "indefinite"});
      let type = HTomb.Types[atype].name;
      if (total>=20) {
        HTomb.GUI.sensoryEvent(attacker + " deals critical " + type + " damage to " + defender + ".",x,y,z,"red");
        this.wounds.level = 8;
        this.wounds.type = atype;
      } else if (total>=17) {
        HTomb.GUI.sensoryEvent(attacker + " deals severe " + type + " damage to " + defender + ".",x,y,z,"#FFBB00");
        if (this.wounds.level<6) {
          this.wounds.level = 6;
        } else {
          this.wounds.level = 8;
        }
        this.wounds.type = atype;
      } else if (total>=14) {
        HTomb.GUI.sensoryEvent(attacker + " deals " + type + " damage to " + defender + ".",x,y,z,"orange");
        if (this.wounds.level<4) {
          this.wounds.level = 4;
          this.wounds.type = atype;
        } else if (this.wounds.level===4) {
          this.wounds.level+=2;
          this.wounds.type = atype;
        } else {
          this.wounds.level+=2;
          if (!type) {
            this.wounds.type = atype;
          }
        }
      } else if (total>=8) {
        HTomb.GUI.sensoryEvent(attacker + " deals mild " + type + " damage to " + defender + ".",x,y,z,"#FFBB00");
        if (this.wounds.level<2) {
          this.wounds.level = 2;
          this.wounds.type = atype;
        // cannot die of a mild wound
        } else if (this.wounds.level<7) {
          this.wounds.level+=1;
          if (!atype) {
            this.wounds.type = atype;
          }
        }
      } else {
        HTomb.GUI.sensoryEvent(attacker + " hits " + defender +" but deals no damage.",x,y,z,"yellow");
      }
      this.tallyWounds();
    },
    defend: function(attack) {
      let roll = HTomb.Utils.dice(1,20);
      this.endure(roll, attack);      
    },
    onDescribe: function(options) {
      let pre = options.pre || [];
      if (this.entity.parent==="Feature") {
        let wounds = this.wounds.level;
        if (wounds<=0) {
          return options;
        } else if (wounds<=3) {
          pre.push("mildly damaged");
        } else if (wounds<=5) {
          pre.push("damaged");
        } else if (wounds<=6) {
          pre.push("severely damaged");
        } else {
          pre.push("totaled");
        }
        options.pre = pre;
        options.startsWithVowel = false;
        return options;
      }
      return options;
    }
  });


  Behavior.extend({
    template: "Equipment",
    name: "equipment",
    slot: null,
    labor: 1,
    onEquip: function(equipper) {},
    onUnequip: function(equipper) {}
  });

  Behavior.extend({
    template: "Equipper",
    name: "equipper",
    items: null,
    slots: {
      MainHand: null,
      OffHand: null    
    },
    onSpawn: function(args) {
      this.items = HTomb.Things.Items(this);
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
      let x = this.entity.x;
      let y = this.entity.y;
      let z = this.entity.z;
      HTomb.GUI.sensoryEvent(this.entity.describe({capitalized: true, article: "indefinite"}) + " unequips " + item.describe({article: "indefinite"})+".",x,y,z);
      this.entity.ai.actionPoints-=16;
      this.entity.acted = true;
      return item;
    },
    // should this always use action points?
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
      let x = this.entity.x;
      let y = this.entity.y;
      let z = this.entity.z;
      HTomb.GUI.sensoryEvent(this.entity.describe({capitalized: true, article: "indefinite"}) + " equips " + item.describe({article: "indefinite"})+".",x,y,z);
      this.entity.ai.actionPoints-=16;
      this.entity.acted = true;
    }
  });

  return HTomb;
})(HTomb);
