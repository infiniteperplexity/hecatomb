// ****** This module implements Components, which are the basic units of functionality for creatures, items, and features
HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;

  let Component = HTomb.Things.Component;

  Component.extend({
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

  // The Minion bomponent allows a creature to serve a master and take orders
  Component.extend({
    template: "Minion",
    name: "minion",
    master: null,
    setMaster: function(cr) {
      this.master = cr;
      this.entity.actor.setTeam(cr.actor.team);
      HTomb.Events.subscribe(this,"Destroy");
    },
    onDestroy: function(event) {
      if (event.entity===this.master) {
        this.detachFromEntity();
        this.despawn();
      }
    }
  });

  Component.extend({
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

  Component.extend({
    template: "Master",
    name: "master",
    minions: null,
    taskList: null,
    tasks: null,
    onSpawn: function(options) {
      options = options || {};
      this.tasks = options.tasks || [];
      this.minions = [];
      this.taskList = [];
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
    designate: function(tsk) {
      tsk.designate(this.entity);
    },
    assignTasks: function() {
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
          // !!!! this is dependent on owner?
          // look for labor tools
          let labor = minion.worker.getLabor();
          let invenTools = this.entity.owner.ownedItems().filter(function(e,i,a) {
            return (e.equipment && e.claimed < e.n && e.equipment.labor>labor && minion.inventory && minion.inventory.items.indexOf(e)!==-1);
          });
          let groundTools = this.entity.owner.ownedItems().filter(function(e,i,a) {
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
            let tasks = this.taskList.filter(function(task,i,a) {return (task.priority===j && !(task.dormant>0) && task.assignee===null)});
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
    }
  });


  Component.extend({
    template: "Owner",
    name: "owner",
    structures: null,
    researched: null,
    onSpawn: function(options) {
      options = options || {};
      this.structures = [];
      this.researched = [];
      return this;
    },
    addWorkshop: function(w) {
      this.workshops.push(w);
    },
    removeWorkshop: function(w) {
      this.workshops.splice(this.workshops.indexOf(w,1));
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

  return HTomb;
})(HTomb);
