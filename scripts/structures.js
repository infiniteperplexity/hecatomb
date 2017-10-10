HTomb = (function(HTomb) {
  "use strict";
  let coord = HTomb.Utils.coord;

  // Might like to have animations

  let Entity = HTomb.Things.templates.Entity;
  let Task = HTomb.Things.templates.Task;
  let Behavior = HTomb.Things.templates.Behavior;

  let Structure = Entity.extend({
    template: "Structure",
    name: "structure",
    owner: null,
    height: 3,
    width: 3,
    x: null,
    y: null,
    z: null,
    placed: false,
    squares: [],
    features: [],
    symbols: [],
    fgs: [],
    ingredients: [],
    tooltip: "A generic structure tooltip.",
    setSymbol: function(i,sym) {
      this.features[i].symbol = sym;
    },
    setColor: function(i,fg) {
      this.features[i].fg = fg;
    },
    onDefine: function(args) {
      if ((args.ingredients===undefined || args.ingredients.length===0) && args.height!==null && args.width!==null) {
        let ings = [];
        let h = args.height;
        let w = args.width;
        for (let i=0; i<w*h; i++) {
          ings.push({});
        }
        HTomb.Things.templates[this.template].ingredients = ings;
      }
      HTomb.Things.templates.Feature.extend({template: args.template+"Feature", name: args.name, bg: args.bg});
    },
    spawn: function(args) {
      Entity.spawn.call(this,args);
      this.features = [];
      this.options = HTomb.Utils.copy(this.options);
      return this;
    },
    place: function(x,y,z,args) {
      Entity.place.call(this,x,y,z,args);
      this.owner.master.structures.push(this);
      this.placed = true;
      return this;
    },
    isPlaced: function() {
      return this.placed;
    },
    remove: function() {
      this.owner.master.structures.splice(this.owner.master.structures.indexOf(this),1);
      HTomb.Events.unsubscribeAll(this);
      this.placed = false;
      Entity.remove.call(this);
    },
    highlight: function(bg) {
      for (let i=0; i<this.features.length; i++) {
        this.features[i].highlightColor = bg;
      }
    },
    unhighlight: function() {
      for (let i=0; i<this.features.length; i++) {
        if (this.features[i].highlightColor) {
          delete this.features[i].highlightColor;
        }
      }
    },
    headerText: function() {
      return;
    },
    structureText: function() {
      let txt = [
        "%c{orange}**Esc: Done.**",
        "Wait: NumPad 5 / Control+Space.",
        "Click / Space: Select.",
        "Enter: Toggle Pause.",
        " ",
        "%c{yellow}Structure: "+this.describe({capitalized: true, atCoordinates: true})+".",
      ];
      if (this.behaviors) {
        for (let behavior of this.behaviors) {
          if (behavior.commandsText) {
            txt = txt.concat(behavior.commandsText());
          }
        }
      }
      txt = txt.concat(["Tab: Next structure."," "]);
      if (this.behaviors) {
        for (let behavior of this.behaviors) {
          if (behavior.detailsText) {
            txt = txt.concat(behavior.detailsText());
          }
        }
      }
      return txt;
    },
    totalIngredients: function() {
      let ings = {};
      if (HTomb.Debug.noingredients) {
        return ings;
      }
      for (let i=0; i<this.ingredients.length; i++) {
        let ingr = this.ingredients[i];
        for (let ing in ingr) {
          ings[ing] = ings[ing] || 0;
          ings[ing] += ingr[ing];
        }
      }
      return ings;
    },
    neededIngredients: function() {
      if (this.ingredients.length===0) {
        return {};
      }
      let ings = {};
      if (HTomb.Debug.noingredients) {
        return {};
      }
      for (let i=0; i<this.ingredients.length; i++) {
        if (this.features[i]) {
          continue;
        } else {
          let x = this.squares[i][0];
          let y = this.squares[i][1];
          let z = this.squares[i][2];
          let f = HTomb.World.features[coord(x,y,z)];
          if (f && f.template==="IncompleteFeature" && f.makes===this.template+"Feature") {
            continue;
          }
        }
        let ingr = this.ingredients[i];
        for (let ing in ingr) {
          ings[ing] = ings[ing] || 0;
          ings[ing] += ingr[ing];
        }
      }
      return ings;
    }
  });
  // Add commands with similar behavior dynamically
  for (let command of ["up","down","left","right","more","less","cancel","choice"]) {
    Structure[command+"Command"] = function(args) {
      if (this.behaviors) {
        for (let behavior of this.behaviors) {
          if (behavior[command+"Command"]) {
            behavior[command+"Command"](args);
          }
        }
      }
    };
  }

  Behavior.extend({
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
      this.queue.splice(this.cursor+1,0,[this.makes[i],"finite",1]);
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
    rightCommand: function() {
      let i = this.cursor;
      if (i===-1 || this.queue.length===0) {
        return;
      }
      if (this.queue[i][1]==="finite") {
        this.queue[i][1]=1;
      } else if (parseInt(this.queue[i][1])===this.queue[i][1]) {
        this.queue[i][1]="infinite";
      } else if (this.queue[i][1]==="infinite") {
        this.queue[i][1] = "finite";
      }
    },
    leftCommand: function() {
      let i = this.cursor;
      if (i===-1 || this.queue.length===0) {
        return;
      }
      if (this.queue[i][1]==="finite") {
        this.queue[i][1]="infinite";
      } else if (parseInt(this.queue[i][1])===this.queue[i][1]) {
        this.queue[i][1] = "finite";
      } else if (this.queue[i][1]==="infinite") {
        this.queue[i][1]=1;
      }
    },
    moreCommand: function() {
      let i = this.cursor;
      if (i===-1 && this.task) {
        if (this.queue[0] && this.queue[0][0]===this.task.makes) {
          if (this.queue[0][1]!=="infinite") {
            this.queue[0][2]+=1;
          }
        } else {
          this.queue.splice(this.cursor+1,0,[this.task.makes,"finite",1]);
        }
        return;
      } else if (i===-1 || this.queue.length===0) {
        return;
      }
      if (this.queue[i][1]==="finite") {
        this.queue[i][2]+=1;
      } else if (parseInt(this.queue[i][1])===this.queue[i][1]) {
        this.queue[i][1]+=1;
        this.queue[i][2]+=1;
      }
    },
    lessCommand: function() {
      let i = this.cursor;
      if (i===-1 && this.task) {
        this.cancelCommand();
        return;
      } else if (i===-1 || this.queue.length===0) {
        return;
      }
      if (this.queue[i][1]==="finite" && this.queue[i][2]>1) {
        this.queue[i][2]-=1;
      } else if (parseInt(this.queue[i][1])===this.queue[i][1] && this.queue[i][1]>1) {
        this.queue[i][1]-=1;
        if (this.queue[i][2]>this.queue[i][1]) {
          this.queue[i][2] = this.queue[i][1];
        }
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
      if (this.task===null) {
        if (HTomb.Utils.dice(2,6)===12) {
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
      let ings = HTomb.Utils.copy(HTomb.Things.templates[this.queue[0][0]].ingredients);
      if (HTomb.Debug.noingredients) {
        ings = {};
      }
      if (this.entity.owner.master.ownsAllIngredients(ings)!==true) {
        this.task = null;
        this.queue.push(this.queue.shift());
        return;
      }
      let task = HTomb.Things.templates.ProduceTask.designateTile(this.entity.x,this.entity.y,this.entity.z,this.entity.owner);
      this.task = task;
      task.makes = this.queue[0][0];
      task.producer= this;
      HTomb.GUI.pushMessage("Next good is "+HTomb.Things.templates[task.makes].describe({article: "indefinite"}));
      task.name = "produce "+HTomb.Things.templates[task.makes].name;
      task.ingredients = ings;
      if (this.queue[0][1]==="finite") {
        this.queue[0][2]-=1;
        if (this.queue[0][2]<=0) {
          this.queue.shift();
        }
      } else if (this.queue[0][1]===parseInt(this.queue[0][1])) {
        this.queue[0][2]-=1;
        if (this.queue[0][2]<=0) {
          this.queue[0][2] = this.queue[0][1];
          this.queue.push(this.queue.shift());
        }
      } else if (this.queue[0][1]==="infinite") {
        // do nothing
        // except maybe check to see if there are enough materials???
      }
    },
    commandsText: function() {
      return [
        "Up/Down: Traverse queue.",
        "Left/Right: Alter repeat.",
        "[/]: Alter count.",
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
        let t = HTomb.Things.templates[this.makes[i]];
        let g = t.describe({article: "indefinite"});
        let ings = t.ingredients;
        if (HTomb.Utils.notEmpty(ings)) {
          g+=" ";
          g+=HTomb.Utils.listIngredients(ings);
          if (this.entity.owner && this.entity.owner.master && this.entity.owner.master.ownsAllIngredients(ings)!==true) {
            g = "%c{gray}"+g;
          }
        }
        txt.push(alphabet[i]+") "+g);
      }
      txt.push(" ");
      txt.push("Production Queue:");
      let startQueue = txt.length;
      if (this.task) {
        let s = "@ " + HTomb.Things.templates[this.task.makes].describe({article: "indefinite"});
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
        let item = this.queue[i];
        let s = "- " + HTomb.Things.templates[item[0]].describe({article: "indefinite"}) + ": ";
        if (item[1]==="finite") {
          s+=(" (repeat " + item[2] + ")");
        } else if (item[1]==="infinite") {
          s+=" (repeat infinite)";
        } else if (item[1]===parseInt(item[1])) {
          s+=(" (cycle " + item[2] + ")");
        }
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

  Behavior.extend({
    template: "Research",
    name: "research",
    choices: [],
    library: [],
    current: null,
    onPlace: function(x,y,z,args) {
      HTomb.Events.subscribe(this, "TurnBegin");
    },
    choiceCommand: function(i) {
      if (i<this.choices.length) {
        this.current = HTomb.Things[this.choices[i]]();
      }
    },
    cancelCommand: function() {
      this.current = null;
    },
    onTurnBegin: function() {
      if (this.current) {
        let e = this.entity;
        let cr = HTomb.World.creatures[coord(e.x,e.y,e.z)];
        if (cr && cr.caster && (cr===this.entity.owner || (this.entity.owner.master && this.entity.owner.master.indexOf(cr)!==-1))) {
          this.current.researchable.time-=1;
          if (this.current.researchable.time<=0) {
            this.current.researchable.finish({researcher: this.entity.owner});
            this.current = null;
          }
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
      let dtime = HTomb.Things.templates.Researchable.time;
      for (let i=0; i<choices.length; i++) {
        let choice = HTomb.Things.templates[choices[i]];
        txt.push(alphabet[i] + ") " + choice.name + " (" + (choice.Behaviors.Researchable.time || dtime) + " turns.)");
      }
      txt.push(" ");
      txt.push("Researching:");
      if (this.current===null) {
        txt.push("(nothing)");
      } else {
        txt.push("- " + this.current.name + " (" + this.current.researchable.time + " turns.)");
      }
      return txt;
    }
  });

  Structure.extend({
    template: "Workshop",
    name: "workshop",
    symbols: ["\u25AE","/","\u2699","\u2261","\u25AA",".","\u2692",".","\u25A7"],
    bg: "#665555",
    tooltip: "(The workshop produces basic goods.)",
    fgs: ["#BB9922","#BB9922",HTomb.Constants.FLOORFG,"#BB9922","#BB9922",HTomb.Constants.FLOORFG,HTomb.Constants.FLOORFG,HTomb.Constants.FLOORFG,"#BB9922"],
     ingredients: [
       {WoodPlank: 1},
       {},
       {WoodPlank: 1},
       {},
       {Rock: 2},
       {},
       {WoodPlank: 1},
       {},
       {WoodPlank: 1}
    ],
    Behaviors: {
      Producer: {
        makes: [
          "WorkAxe",
          "DoorItem",
          "TorchItem",
          "SpearTrapItem"
        ]
      },
      StructureLight: {}
    }
  });

  Behavior.extend({
    template: "Storage",
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
          line = HTomb.Things.templates[key].describe({article: "indefinite"});
        } else {
          line = n + " " + HTomb.Things.templates[key].describe({plural: "true"});
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
      let items = this.entity.owner.master.ownedItems();
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
        } else if (HTomb.World.tasks[coord(item.x,item.y,item.z)]!==undefined) {
          continue;
        } else if (HTomb.Tiles.isReachableFrom(this.entity.x, this.entity.y, this.entity.z, item.x, item.y, item.z,
          {canPass: canMove})===false) {
          continue;
        } else if (f && f.structure && f.structure.template===this.entity.template) {
          continue;
        } else {
          let slots = [];
          for (let j=0; j<this.tasks.length; j++) {
            if (this.tasks[j]===null) {
              slots.push(j);
            }
          }
          HTomb.Utils.shuffle(slots);
          let feat = this.entity.features[slots[0]];
          let z = HTomb.Things.HaulTask({
             assigner: this.entity.owner,
             name: "haul " + item.describe()
           });
          z.place(item.x,item.y,item.z);
          z.item = item;
          z.storage = this;
          z.feature = feat;
          this.tasks[slots[0]] = z;
        }
      }
    }
  });

  Task.extend({
    template: "HaulTask",
    name: "haul",
    bg: "#773366",
    item: null,
    feature: null,
    storage: null,
    validTile: function(x,y,z) {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      if (HTomb.World.tiles[z][x][y]===HTomb.Tiles.WallTile || HTomb.World.tiles[z][x][y]===HTomb.Tiles.EmptyTile) {
        return false;
      } else {
        return true;
      }
    },
    canAssign: function(cr) {
      let x = this.x;
      let y = this.y;
      let z = this.z;
      // should this check whether the item is still here?
      if (this.validTile(x,y,z) && HTomb.Tiles.isReachableFrom(x,y,z,cr.x,cr.y,cr.z, {
        canPass: cr.movement.boundMove(),
        searcher: cr,
        searchee: this,
        searchTimeout: 10
      }) && this.item.x===x && this.item.y===y && this.item.z===z) {
        return true;
      } else {
        return false;
      }
    },
    ai: function() {
      let cr = this.assignee;
      let item = this.item;
      let feature = this.feature;
      if (cr.inventory.items.indexOf(this.item)!==-1) {
        cr.ai.target = feature;
        this.place(feature.x, feature.y, feature.z);
      } else if (item.isOnGround())  {
        cr.ai.target = item;
      } else {
        this.cancel();
         return;
      }
      let t = cr.ai.target;
      // this could be either the task square or the item
      if (t.x===cr.x && t.y===cr.y && t.z===cr.z) {
        if (t===feature) {
          cr.inventory.drop(this.item);
          this.complete(this.assignee);
          cr.ai.acted = true;
          return;
        } else {
          // move it to the building;
          this.place(feature.x, feature.y, feature.z);
          cr.inventory.pickup(item);
          cr.ai.acted = true;
          return;
        }
      }
      cr.ai.walkToward(t.x,t.y,t.z, {
        searcher: cr,
        searchee: t,
        searchTimeout: 10,
        useLast: true
      });
      cr.ai.acted = true;
    },
    onDespawn: function() {
      let tasks = this.storage.tasks;
      tasks.splice(tasks.indexOf[this],1,null);
    }
  });

  Structure.extend({
    template: "Stockpile",
    name: "stockpile",
    tooltip: "(The stockpile stores and tallies basic resources.)",
    bg: "#444455",
    symbols: [".",".","\u25AD","\u2234","#","\u2630","\u25A7",".","\u25AF"],
    fgs: ["#BB9922",HTomb.Constants.FLOORFG,HTomb.Constants.FLOORFG,"#BB9922",HTomb.Constants.FLOORFG,"#BB9922",HTomb.Constants.FLOORFG,HTomb.Constants.FLOORFG,"#BB9922"],
    ingredients: [
      {Rock: 1},
      {},
      {Rock: 1},
      {},
      {WoodPlank: 2},
      {},
      {Rock: 1},
      {},
      {Rock: 1}
    ],
    Behaviors: {
      Storage: {
        stores: ["WoodPlank","Rock"]
      },
      StructureLight: {}
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
        if (cr.inventory.canFindAll(this.ingredients)!==true && !HTomb.Debug.noingredients) {
          // Wait...can this cancel the task in the middle of assignment???
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
      if (!HTomb.Debug.noingredients) {
        let test = this.assignee.inventory.items.takeItems(this.ingredients);
      }
      this.started = true;
      this.labor = HTomb.Things.templates[this.makes].labor || this.labor;
      HTomb.GUI.pushMessage(this.blurb());
    },
    workOnTask: function(x,y,z) {
      let assignee = this.assignee;
      if (this.begun()!==true) {
        this.begin(this.assignee);
      }
      let labor = assignee.worker.getLabor();
      this.labor-=labor;
      this.assignee.ai.acted = true;
      this.assignee.ai.actionPoints-=16;
      if (this.labor<=0) {
        this.finish();
        this.complete();
      }
    },
    finish: function() {
      let x = this.x;
      let y = this.y;
      let z = this.z;
      let item = HTomb.Things[this.makes]().place(x,y,z);
      item.owned = true;
      HTomb.Events.publish({type: "Complete", task: this});
      this.producer.occupied = null;
      HTomb.GUI.pushMessage(this.assignee.describe({capitalized: true, article: "indefinite"}) + " finishes making " + HTomb.Things.templates[this.makes].describe({article: "indefinite"}));
    },
    onDespawn: function() {
      this.producer.task = null;
      this.producer.nextGood();
    }
  });

  Task.extend({
    template: "ConstructTask",
    name: "construct",
    description: "create a structure",
    bg: "#553300",
    makes: null,
    structures: ["GuardPost","Workshop","Stockpile","BlackMarket","Sanctum"],
    validTile: function(x,y,z) {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      if (HTomb.World.tiles[z][x][y]!==HTomb.Tiles.FloorTile) {
        return false;
      }
      let f = HTomb.World.features[coord(x,y,z)];
      if (f===undefined || (f.template==="IncompleteFeature" && f.makes===this.makes)) {
        return true;
      }
      else {
        return false;
      }
    },
    designate: function(assigner) {
      var arr = [];
      for (var i=0; i<this.structures.length; i++) {
        //should be pushing the entity, not the structure?
        arr.push(HTomb.Things.templates[this.structures[i]]);
      }
      var that = this;
      HTomb.GUI.choosingMenu("Choose a structure:", arr, function(structure) {
        function placeBox(squares, options) {
          let failed = false;
          let struc = null;
          let mid = Math.floor(squares.length/2);
          for (let i=0; i<squares.length; i++) {
            let crd = squares[i];
            let f = HTomb.World.features[coord(crd[0],crd[1],crd[2])];
            if (HTomb.World.tiles[crd[2]][crd[0]][crd[1]]!==HTomb.Tiles.FloorTile) {
              failed = true;
            // a completed, partial version of the same structure
          } else if (f && f.template===structure.template+"Feature") {
              struc = f.structure;
              if (struc.isPlaced()===true || struc.structure.x!==squares[mid][0] || struc.structure.y!==squares[mid][1]) {
                failed = true;
              }
            // an incomplete version of the same structure
          } else if (f && (f.template!=="IncompleteFeature" || !f.makes || f.makes!==structure.template+"Feature")) {
              failed = true;
            }
          }
          if (failed===true) {
            HTomb.GUI.pushMessage("Can't build there.");
            return;
          }
          let w;
          if (struc!==null) {
            w = struc;
          } else {
            w = HTomb.Things[structure.template]();
            w.owner = assigner;
            w.squares = squares;
            w.x = squares[mid][0];
            w.y = squares[mid][1];
            w.z = squares[mid][2];
          }
          for (let i=0; i<squares.length; i++) {
            let crd = squares[i];
            // this might get weird if you try overlapping a farm with anothern farm...
            if (HTomb.World.features[coord(crd[0],crd[1],crd[2])] && HTomb.World.features[coord(crd[0],crd[1],crd[2])].template===w.template+"Feature") {
              continue;
            }
            this.makes = w.template+"Feature";
            let task = this.designateTile(crd[0],crd[1],crd[2],assigner);
            if (task) {
              //Bugs have occurred here...I may have fixed them...
              task.structure = w;
              task.makes = w.template+"Feature";
              if (w.height!==null && w.width!==null) {
                if (HTomb.Debug.noingredients) {
                  task.ingredients = {};
                } else {
                  if (w.ingredients[i]===undefined) {
                    task.ingredients = {};
                  } else {
                    task.ingredients = HTomb.Utils.clone(w.ingredients[i]);
                  }
                }
              }
              task.position = i;
              task.name = task.name + " " + w.name;
            }
          }
        }
        function myHover(x, y, z, squares) {
          //hacky as all get-out
          let menu = HTomb.GUI.Panels.menu;
          // this may not be used any more because it was for varied-size structures
          if (Array.isArray(x)===false && squares===undefined) {
            if (HTomb.World.explored[z][x][y]!==true) {
              menu.middle = ["%c{orange}Unexplored tile."];
              return;
            }
            if (that.validTile(x,y,z)!==true) {
              menu.middle = ["%c{orange}Cannot build structure here."];
              return;
            }
            menu.middle = ["%c{lime}Build structure starting here."];
            return;
          } else {
            if (Array.isArray(x)) {
              squares = x;
            }
            let valid = true;
            for (var j=0; j<squares.length; j++) {
              let s = squares[j];
              //!! this went out of range?
              if (HTomb.World.explored[s[2]][s[0]][s[1]]!==true || that.validTile(s[0],s[1],s[2])!==true) {
                valid = false;
              }
            }
            if (valid===false) {
              menu.middle = ["%c{orange}Cannot build structure here."];
              return;
            } else {
              menu.middle = ["%c{lime}Build structure here.","","%c{lime}"+structure.tooltip];
            }
          }
        }
        if (structure.height!==null && structure.width!==null) {
          return function() {
            HTomb.GUI.selectBox(structure.width, structure.height, assigner.z,that.designateBox,{
              assigner: assigner,
              context: that,
              bg: that.bg,
              callback: placeBox,
              hover: myHover,
              contextName: "Designate"+that.template
            });
          };
        } else {
          return function() {
            HTomb.GUI.selectSquareZone(assigner.z,that.designateBox,{
              assigner: assigner,
              context: that,
              bg: that.bg,
              callback: placeBox,
              hover: myHover,
              contextName: "Designate"+that.template
            });
          };
        }
      },
      {
        format: function(structure) {
          let g = structure.describe();
          let ings = structure.totalIngredients();
          if (HTomb.Utils.notEmpty(ings)) {
            g+=" ";
            g+=HTomb.Utils.listIngredients(ings);
            if (!assigner || !assigner.master) {
              return g;
            }
            if (assigner.master.ownsAllIngredients(ings)!==true) {
              g = "%c{gray}"+g;
            }
            return g;
          } else {
            return g;
          }
        },
        contextName: "ChooseStructure"
      });
    },
    canAssign: function(cr) {
      if (Task.canAssign.call(this, cr)!==true) {
        return false;
      }
      // more intuitive if ingredient-needing features are built first
      let ings = this.structure.neededIngredients();
      if (HTomb.Utils.notEmpty(ings) && HTomb.Utils.notEmpty(this.ingredients)!==true) {
        return false;
      }
      return true;
    },
    begin: function() {
      Task.begin.call(this);
      let i = this.position;
      let f = HTomb.World.features[coord(this.x, this.y, this.z)];
      f.fg = this.structure.fgs[i] || this.structure.fg;
      HTomb.World.covers[this.z][this.x][this.y] = HTomb.Covers.NoCover;
    },
    finish: function() {
      Task.finish.call(this);
      let x = this.x;
      let y = this.y;
      let z = this.z;
      let f = HTomb.World.features[coord(x,y,z)];
      let p = this.position;
      f.fg = this.structure.fgs[p] || this.structure.fg;
      f.symbol = this.structure.symbols[p] || this.structure.symbol;
      f.structure = this.structure;
      this.structure.features[p] = f;
      for (let i=0; i<this.structure.squares.length; i++) {
        if (!this.structure.features[i]) {
          break;
        }
        if (i===this.structure.squares.length-1) {
          let w = this.structure;
          w.place(w.x, w.y, w.z);
          HTomb.GUI.pushMessage(this.assignee.describe({capitalized: true, article: "indefinite"}) + " finishes building " + w.describe({article: "indefinite", atCoordinates: true}) + ".");
        }
      }
    },
    designateBox: function(squares, options) {
      options = options || {};
      var assigner = options.assigner;
      var callb = options.callback;
      callb.call(options.context,squares,assigner);
    }
  });

  Structure.extend({
    template: "BlackMarket",
    name: "black market",
    tooltip: "(The black market lets you trade for goods from faraway lands.)",
    symbols: ["\u00A3",".",".",".","\u2696","$","\u00A2","\u20AA","\u00A4"],
    fgs: ["#552222",HTomb.Constants.FLOORFG,HTomb.Constants.FLOORFG,HTomb.Constants.FLOORFG,"#888844","#888844","#225522","#333333","#222266"],
    bg: "#555544",
    offers: [],
    awaiting: [],
    onPlace: function() {
      HTomb.Events.subscribe(this,"TurnBegin");
    },
    onTurnBegin: function() {
      if (HTomb.Utils.dice(1,100)===1) {
        let MAXOFFERS = 4;
        let trades = [
          [["Rock",3],["TradeGoods",1]],
          [["WoodPlank",3],["TradeGoods",1]]
        ];
        trades = HTomb.Utils.shuffle(trades);
        this.offers.unshift(trades[0]);
        if (this.offers.length>MAXOFFERS) {
          this.offers.pop();
        }
      }
      let i = 0;
      while (i<this.awaiting.length) {
        let a = this.awaiting[i];
        a[2]-=1;
        if (a[2]<=0) {
          let item = HTomb.Things[a[0]]({n: a[1]});
          item.place(this.x,this.y,this.z);
          this.awaiting.splice(i,1);
        } else {
          i+=1;
        }
      }
    },
    choiceCommand: function(i) {
      if (this.offers.length>i) {
        let o = this.offers[i];
        //need to remove the traded items somehow
        this.awaiting.push([o[1][0],o[1][1],50]);
        this.offers.splice(i,1);
      }
    },
    structureText: function() {
      let txt = [
        "%c{orange}**Esc: Done.**",
        "Wait: NumPad 5 / Control+Space.",
        "Click / Space: Select.",
        "Enter: Toggle Pause.",
        "a-z: Accept an offer.",
        " ",
        "%c{yellow}Structure: "+this.describe({capitalized: true, atCoordinates: true})+".",
      ];
      if (this.behaviors) {
        for (let behavior of this.behaviors) {
          if (behavior.commandsText) {
            txt = txt.concat(behavior.commandsText());
          }
        }
      }
      txt = txt.concat(["Tab: Next structure."," "]);
      txt.push(" ");
      txt.push("Offers:");
      let alphabet = 'abcdefghijklmnopqrstuvwxyz';
      for (let i=0; i<this.offers.length; i++) {
        let o = this.offers[i];
        let name0 = HTomb.Things.templates[o[0][0]].name;
        let name1 = HTomb.Things.templates[o[1][0]].name;
        // !!!gray it out if you don't have it...
        txt.push(alphabet[i] + ") " + o[0][1] + " " + name0 + " : " + o[1][1] + " " + name1 + ".");
      }
      txt.push(" ");
      txt.push("Awaiting:");
      for (let i=0; i<this.awaiting.length; i++) {
        let a = this.awaiting[i];
        let name = HTomb.Things.templates[a[0]].name;
        txt.push("- " + a[1] + " " + name + ".");
      }
      txt.push(" ");
      if (this.behaviors) {
        for (let behavior of this.behaviors) {
          if (behavior.detailsText) {
            txt = txt.concat(behavior.detailsText());
          }
        }
      }
      return txt;
    },
    ingredients: [
      {Rock: 1}, {}, {WoodPlank: 1},
      {}, {TradeGoods: 1}, {},
      {WoodPlank: 1}, {}, {Rock: 1}
    ],
    Behaviors: {
      Storage: {
        stores: ["TradeGoods"]
      },
      StructureLight: {}
    }
  });

  Structure.extend({
    template: "Sanctum",
    name: "sanctum",
    tooltip: "(The sanctum grants you addition entropy and researchable spells.)",
    symbols: ["\u2625",".","\u2AEF",".","\u2135",".","\u2AEF","\u2606","\u263F"],
    fgs: ["magenta",HTomb.Constants.FLOORFG,"cyan",HTomb.Constants.FLOORFG,"green",HTomb.Constants.FLOORFG,"yellow","red","orange"],
    bg: "#222244",
    // do we want some sort of mana activation thing?,
    Behaviors: {
      Research: {
        choices: ["PoundOfFlesh"]
      },
      StructureLight: {}
    },
  });

  //!!!!!Weird that its behavior is defined in defender, not here
  Structure.extend({
    template: "GuardPost",
    name: "guard post",
    defenseRange: 3,
    highlight: function(bg) {
      Structure.highlight.call(this,bg);
      let z = this.z;
      for (let x=-3; x<=3; x++) {
        for (let y=-3; y<=3; y++) {
          if (HTomb.Path.quickDistance(this.x, this.y, z, this.x+x, this.y+y, z) <= 3) {
            HTomb.GUI.Panels.gameScreen.highlitTiles[coord(this.x+x,this.y+y,z)] = "#779944";
          }
        }
      }
    },
    unhighlight: function() {
      let z = this.z;
      for (let x=-3; x<=3; x++) {
        for (let y=-3; y<=3; y++) {
          if (HTomb.Path.quickDistance(this.x, this.y, z, this.x+x, this.y+y, z) <= 3) {
            if (HTomb.GUI.Panels.gameScreen.highlitTiles[coord(this.x+x,this.y+y,z)]) {
              delete HTomb.GUI.Panels.gameScreen.highlitTiles[coord(this.x+x,this.y+y,z)]; 
            }
          }
        }
      }
      Structure.unhighlight.call(this);
    },
    structureText: function() {
      let txt = Structure.structureText();
      txt.splice(6,0,"(Warns of incoming attacks; defense bonus within radius.");
      return txt;
    },
    tooltip: "(The guard post warns of incoming attacks and gives bonuses to defenders fighting near it.)",
    ingredients: [
      {},{},{},
      {},{Rock:1,WoodPlank:1},{},
      {},{},{}
    ],
    symbols: ["\u2694",".","\u2658",".",".",".","\u2658",".","\u2694"],
    fgs: ["#BBBBBB",HTomb.Constants.FLOORFG,"#BBBBBB",HTomb.Constants.FLOORFG,HTomb.Constants.FLOORFG,HTomb.Constants.FLOORFG,"#BBBBBB",HTomb.Constants.FLOORFG,"#BBBBBB"],
    bg: "#555577",
    Behaviors: {
      StructureLight: {}
    }
  });


return HTomb;
})(HTomb);