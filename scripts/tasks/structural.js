HTomb = (function(HTomb) {
  "use strict";
  let coord = HTomb.Utils.coord;

  let Task = HTomb.Things.Task;

  Task.extend({
    template: "ResearchTask",
    name: "research",
    bg: "#8800BB",
    structure: null,
    researching: null,
    turns: 0,
    workRange: 1,
    fulfilled: false,
    validTile: function(x,y,z) {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      if (HTomb.World.tiles[z][x][y]===HTomb.Tiles.FloorTile) {
        return true;
      }
      return true;
    },
    canAssign: function(cr) {
      if (this.fulfilled) {
        return false;
      } else {
        return Task.canAssign.call(this, cr);
      }
    },
    workOnTask: function() {
      let x = this.x;
      let y = this.y;
      let z = this.z;
      //cancel the task if something weird happened to the tile
      if (this.validTile(x,y,z)!==true) {
        this.cancel();
      }
      if (this.begun()!==true) {
        this.expend();
        this.begin();
        this.unassign();
      }
    },
    work: function() {
      this.turns-=1;
      if (this.turns<=0) {
        this.finish();
        this.complete();
      }
    },
    begun: function() {
      return this.fulfilled;
    },
    begin: function() {
      this.fulfilled = true;
    },
    finish: function() {
      //!!!odd place to put this logic
      let template = HTomb.Things[this.researching];
      if (template.parent==="Spell") {
        HTomb.GUI.alert("You have completed research on the spell '" + template.describe()+".'");
        let spells = this.assigner.caster.spells.map(function(s) {return s.template;});
        if (spells.indexOf(template.template)===-1) {
          this.assigner.caster.spells.push(HTomb.Things[template.template].spawn({caster: this.assigner.caster}));
        }
      }
      if (this.assigner.master.researched.indexOf(template.template)===-1) {
        this.assigner.master.researched.push(template.template);
      }
    },
    onDespawn: function() {
      this.structure.research.current = null;
    }
  });

  Task.extend({
    template: "HaulTask",
    name: "haul",
    bg: "#773366",
    storage: null,
    workRange: 0,
    priority: 2,
    item: null,
    validTile: function(x,y,z) {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      if (HTomb.World.tiles[z][x][y]!==HTomb.Tiles.FloorTile) {
        return false;
      }
      let f = HTomb.World.features[coord(x,y,z)];
      if (f && f.structure && f.structure.storage===this.storage) {
        return true;
      }
      else {
        return false;
      }
    },
    act: function() {
      let cr = this.assignee;
      if (cr.actor.acted) {
        return;
      }
      HTomb.Behaviors.FetchItem.act(cr.actor, {task: this, item: this.item});
      if (cr.actor.acted) {
        return;
      }
      if (this.x===cr.x && this.y===cr.y && this.z===cr.z) {
        if (cr.inventory.items.contains(this.item)) {
          cr.inventory.drop(this.item);
          this.complete();
        }
      } else {
        cr.actor.walkToward(this.x,this.y,this.z, {
          searcher: cr,
          searchee: this,
          searchTimeout: 10
        });
      }
    },
    onDespawn: function() {
      let tasks = this.storage.tasks;
      tasks.splice(tasks.indexOf[this],1,null);
    }
  });

  Task.extend({
    template: "ProduceTask",
    name: "produce",
    bg: "#336699",
    producer: null,
    makes: null,
    labor: 20,
    started: false,
    dormancy: 4,
    workRange: 0,
    canAssign: function(cr) {
      if (this.isPlaced()===false) {
        return false;
      }
      let x = this.x;
      let y = this.y;
      let z = this.z;
      if (this.validTile(x,y,z) && HTomb.Tiles.isReachableFrom(x,y,z,cr.x,cr.y,cr.z, {
        searcher: cr,
        searchee: this,
        canPass: cr.movement.boundMove(),
        searchTimeout: 10
      })) {
        // cancel this task if you can't find the ingredients
        if (HTomb.Tiles.canFindAll(x,y,z,this.ingredients,{
          searcher: cr,
          ownedOnly: (this.assignee===HTomb.Player) ? true : false,
          respectClaims: (this.assignee===HTomb.Player) ? true : false
        })===false) {
          this.cancel();
          return false;
        } else {
          return true;
        }
      } else {
        return false;
      }
    },
    validTile: function(x,y,z) {
      return true;
    },
    begun: function() {
      return this.started;
    },
    begin: function() {
      this.expend();
      this.started = true;
      this.labor = HTomb.Things[this.makes].labor || this.labor;
      HTomb.GUI.pushMessage(this.blurb());
    },
    workOnTask: function(x,y,z) {
      let assignee = this.assignee;
      if (this.begun()!==true) {
        this.begin(this.assignee);
      }
      let labor = assignee.worker.getLabor();
      this.labor-=labor;
      this.assignee.actor.acted = true;
      this.assignee.actor.actionPoints-=16;
      if (this.labor<=0) {
        this.finish();
        this.complete();
      }
    },
    finish: function() {
      let x = this.x;
      let y = this.y;
      let z = this.z;
      let item = HTomb.Things[this.makes].spawn().place(x,y,z);
      item.owned = true;
      HTomb.Events.publish({type: "Complete", task: this});
      this.producer.occupied = null;
      HTomb.GUI.pushMessage(this.assignee.describe({capitalized: true, article: "indefinite"}) + " finishes making " + HTomb.Things[this.makes].describe({article: "indefinite"}));
    },
    onDespawn: function() {
      this.producer.task = null;
      this.producer.nextGood();
    }
  });

  Task.extend({
    template: "TradeTask",
    name: "trade",
    structure: null,
    offer: null,
    turns: 100,
    workRange: 0,
    bg: "#999922",
    validTile: function(x,y,z) {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      if (HTomb.World.tiles[z][x][y]===HTomb.Tiles.FloorTile) {
        return true;
      }
      return true;
    },
    begun: function() {
      return false;
    },
    workOnTask: function() {
      this.expend();
      this.finish();
      this.complete();
    },
    finish: function() {
      if (this.structure) {
        this.structure.awaiting.push({offer: this.offer, turns: this.turns});
      }
    },
    onDespawn: function() {
      if (this.structure && this.structure.task) {
        this.structure.task = null;
      }
    }
  });

return HTomb;
})(HTomb);