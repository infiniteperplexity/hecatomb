HTomb = (function(HTomb) {
  "use strict";
  let coord = HTomb.Utils.coord;

  let Component = HTomb.Things.Component;

  Component.extend({
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

  Component.extend({
    template: "Producer",
    name: "producer",
    makes: [],
    queue: null,
    task: null,
    cursor: -1,
    onPlace: function(x,y,z,args) {
      this.queue = [];
      HTomb.Events.subscribe(this, "TurnBegin");
    },
    onRemove: function() {
      for (let i=0; i<this.queue.length; i++) {
        this.task.cancel();
      }
      HTomb.Events.unsubscribeAll(this);
    },
    choiceCommand: function(i) {
      if (this.makes.length<=i) {
        return;
      }
      this.queue.splice(this.cursor+1,0,this.makes[i]);
      if (this.task===null) {
        this.nextGood();
      }
      if (this.cursor<this.queue.length-1) {
        this.cursor+=1;
      }
    },
    upCommand: function() {
      this.cursor-=1;
      if (this.cursor<-1) {
        this.cursor = this.queue.length-1;
      }
    },
    downCommand: function() {
      this.cursor+=1;
      if (this.cursor>this.queue.length-1) {
        this.cursor = -1;
      }
    },
    cancelCommand: function() {
      if (this.cursor===-1) {
        if (this.task) {
          this.task.cancel();
        }
      } else if (this.queue.length>0 && this.cursor>=0) {
        this.queue.splice(this.cursor,1);
      }
      if (this.cursor>=this.queue.length) {
        this.cursor = this.queue.length-1;
      }
    },
    onTurnBegin: function() {
      // is this still useful???
      if (this.task===null) {
        if (ROT.RNG.getUniformInt(1,36)===1) {
          this.nextGood();
        }
      }
    },
    nextGood: function() {
      if (this.queue.length===0) {
        return;
      } else if (HTomb.World.tasks[HTomb.Utils.coord(this.entity.x,this.entity.y,this.entity.z)]) {
        HTomb.GUI.pushMessage("Structure tried to create new task but there was already a zone.");
        return;
      }
      // this is a good place to check for ingredients
      let ings = HTomb.Utils.copy(HTomb.Things[this.queue[0]].craftable.ingredients);
      if (this.entity.owner.owner.ownsAllIngredients(ings)!==true) {
        this.task = null;
        this.queue.push(this.queue.shift());
        return;
      }
      let task = HTomb.Things.ProduceTask.designateTile(this.entity.x,this.entity.y,this.entity.z,this.entity.owner);
      this.task = task;
      task.labor = HTomb.Things[this.queue[0]].craftable.labor;
      task.makes = this.queue[0];
      task.producer= this;
      HTomb.GUI.pushMessage("Next good is "+HTomb.Things[task.makes].describe({article: "indefinite"}));
      task.name = "produce "+HTomb.Things[task.makes].describe();
      task.ingredients = ings;
      this.queue.shift();
    },
    commandsText: function() {
      return [
        "Up/Down: Traverse queue.",
        "a-z: Insert good below the >.",
        "Backspace/Delete: Remove good."
      ];
    },
    detailsText: function() {
      if (this.cursor>=this.queue.length) {
        this.cursor = this.queue.length-1;
      }
      let txt = ["Goods:"];
      let alphabet = 'abcdefghijklmnopqrstuvwxyz';
      for (let i=0; i<this.makes.length; i++) {
        let makes = HTomb.Things[this.makes[i]];
        let g = (makes.makes) ? HTomb.Things[makes.makes] : makes;
        g = g.describe({article: "indefinite"});
        let ings;
        if (makes.makes) {
          ings = (HTomb.Debug.noingredients) ? {} : HTomb.Things[makes.makes].craftable.ingredients;
        } else {
          ings = (HTomb.Debug.noingredients) ? {} : makes.craftable.ingredients;
        }
        if (Object.keys(ings).length>0) {
          g+=" ";
          g+=HTomb.Utils.listIngredients(ings);
          if (this.entity.owner && this.entity.owner.owner && this.entity.owner.owner.ownsAllIngredients(ings)!==true) {
            g = "%c{gray}"+g;
          }
        }
        txt.push(alphabet[i]+") "+g);
      }
      txt.push(" ");
      txt.push("Production Queue:");
      let startQueue = txt.length;
      if (this.task) {
        let s = "@ " + HTomb.Things[this.task.makes].describe({article: "indefinite"});
        if (this.task.assignee) {
          s+=": (active: "+this.task.assignee.describe({article: "indefinite"})+")";
        } else {
          s+=": (unassigned)";
        }
        txt.push(s);
      } else {
        txt.push("@ (none)");
      }
      for (let i=0; i<this.queue.length; i++) {
        let making = HTomb.Things[this.queue[i]];
        let item = (making.makes) ? HTomb.Things[making.makes] : making;
        let s = "- " + item.describe({article: "indefinite"});
        txt.push(s);
      }
      if (this.queue.length>0 && this.cursor>-1) {
        let s = txt[this.cursor+1+startQueue];
        s = ">" + s.substr(1);
        txt[this.cursor+1+startQueue] = s;
      } else {
        let s = txt[startQueue];
        s = ">" + s.substr(1);
        txt[startQueue] = s;
      }
      return txt;
    }
  });

  Component.extend({
    template: "Storage",
    name: "storage",
    dormant: 0,
    dormancy: 10,
    tasks: null,
    stores: [],
    onPlace: function() {
      HTomb.Events.subscribe(this,"TurnBegin");
    },
    onRemove: function() {
      HTomb.Events.unsubscribeAll(this);
    },
    onAdd: function() {
      this.tasks = [];
      for (let i=0; i<this.entity.width*this.entity.height; i++) {
        this.tasks.push(null);
      }
      return this;
    },
    detailsText: function() {
      let txt = ["Contents:"];
      let totalItems = {};
      for (let i=0; i<this.entity.squares.length; i++) {
        let s = this.entity.squares[i];
        let items = HTomb.World.items[coord(s[0],s[1],s[2])];
        if (items) {
          for (let j=0; j<items.length; j++) {
            let item = items[j];
            totalItems[item.template] = totalItems[item.template] || 0;
            totalItems[item.template] += item.n;
          }
        }
      }
      for (let key in totalItems) {
        let n = totalItems[key];
        let line;
        if (n===1) {
          line = HTomb.Things[key].describe({article: "indefinite"});
        } else {
          line = n + " " + HTomb.Things[key].describe({plural: "true"});
        }
        txt.push(line);
      }
      return txt;
    },
    onTurnBegin: function() {
      // this probably shouldn't happen every turn...maybe have a countdown?
      if (this.dormant>0) {
        this.dormant-=1;
        return;
      }
      this.dormant = this.dormancy;
      let items = this.entity.owner.owner.ownedItems();
      for (let i=0; i<items.length; i++) {
        //if ever we run out of task space, break the loop
        if (this.tasks.indexOf(null)===-1) {
          return;
        }
        let item = items[i];
        let f = HTomb.World.features[coord(item.x,item.y,item.z)];
        // This is a bit tricky...maybe a "bestMove" function returned from Master?
        let canMove = this.entity.owner.movement.boundMove();
        if (!item.isOnGround()) {
          continue;
        } else if (this.stores.indexOf(item.template)===-1) {
          continue;
        } else if (item.claimed>=item.n) {
          continue;
        } else if (HTomb.World.tasks[coord(item.x,item.y,item.z)]!==undefined) {
          continue;
        } else if (HTomb.Tiles.isReachableFrom(this.entity.x, this.entity.y, this.entity.z, item.x, item.y, item.z,
          {canPass: canMove})===false) {
          continue;
        } else if (f && f.structure && f.structure.template===this.entity.template) {
          continue;
        } else {
          this.spawnHaulTask(item);
        }
      }
    },
    spawnHaulTask: function(item) {
      let slots = [];
      for (let j=0; j<this.tasks.length; j++) {
        if (this.tasks[j]===null) {
          slots.push(j);
        }
      }
      HTomb.Utils.shuffle(slots);
      let f = this.entity.features[slots[0]];
      let t = HTomb.Things.HaulTask.spawn({
         assigner: this.entity.owner,
         name: "haul " + item.name
       });
      t.claim(item, item.n-item.claimed);
      t.item = item;
      t.storage = this;
      this.tasks[slots[0]] = t;
      t.place(f.x,f.y,f.z);
    }
  });

  Component.extend({
    template: "Research",
    name: "research",
    choices: [],
    library: [],
    current: null,
    onPlace: function(x,y,z,args) {
      HTomb.Events.subscribe(this, "TurnBegin");
    },
    choiceCommand: function(i) {
      let choices = this.choices;
      choices = choices.filter(e => this.entity.owner.owner.researched.indexOf(e)===-1);
      if (i<choices.length) {
        if (this.current) {
          if (confirm("Really cancel current research?")) {
            this.current.cancel();
          } else {
            return;
          }
        }
        let choice = HTomb.Things[choices[i]];
        this.current = HTomb.Things.ResearchTask.spawn({
          assigner: this.entity.owner,
          name: "research " + choice.name,
          structure: this.entity,
          researching: choices[i],
          turns: choice.researchable.turns,
          ingredients: choice.researchable.ingredients,
          fulfilled: (HTomb.Debug.noingredients || Object.keys(choice.researchable.ingredients).length===0) ? true : false
        });
        this.current.place(this.entity.x, this.entity.y, this.entity.z);
      }
    },
    cancelCommand: function() {
      if (this.current && confirm("Really cancel research?")) {
        this.current.cancel();
      }
    },
    onTurnBegin: function() {
      if (this.current && this.current.fulfilled) {
        let e = this.entity;
        let cr = HTomb.World.creatures[coord(e.x,e.y,e.z)];
        if (cr && cr.caster && (cr===e.owner || (e.owner.master && e.owner.master.minions.indexOf(cr)!==-1))) {
          this.current.work();
        }
      }
      // think about how to handle the "library"?
    },
    commandsText: function() {
      return [
        "a-z: Begin research on lore.",
        "Delete: Cancel current research.",
        "(Research takes place only if the necromancer occupies the building.)"
      ];
    },
    detailsText: function() {
      let txt = ["Research choices:"];
      let alphabet = "abcdefghijklmnopqrstuvwxyz";
      let choices = this.choices;
      choices = choices.filter(e => this.entity.owner.owner.researched.indexOf(e)===-1);
      for (let i=0; i<choices.length; i++) {
        let choice = HTomb.Things[choices[i]];
        let msg = choice.name + " " + HTomb.Utils.listIngredients(choice.researchable.ingredients);
        if (this.entity.owner.owner.ownsAllIngredients(choice.researchable.ingredients)!==true) {
          msg = "%c{gray}" + msg;
        }
        msg = alphabet[i] + ") " + msg;
        txt.push(msg);
      }
      txt.push(" ");
      txt.push("Researching:");
      if (this.current===null) {
        txt.push("(nothing)");
      } else {
        txt.push("- " + this.current.name + " (" + this.current.turns + " turns.)");
      }
      return txt;
    }
  });

return HTomb;
})(HTomb);