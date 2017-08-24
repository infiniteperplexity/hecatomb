HTomb = (function(HTomb) {
  "use strict";
  let coord = HTomb.Utils.coord;

  // Might like to have animations

  HTomb.Things.defineBehavior({
    template: "Structure",
    name: "structure",
    owner: null,
    height: null,
    width: null,
    x: null,
    y: null,
    z: null,
    squares: [],
    features: [],
    symbols: [],
    fgs: [],
    options: [],
    ingredients: [],
    cursor: -1,
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
      HTomb.Things.templates.Behavior.onDefine.call(this,args);
      HTomb.Things.defineFeature({template: args.template+"Feature", name: args.name, bg: args.bg});
    },
    onCreate: function() {
      this.features = [];
      this.options = HTomb.Utils.copy(this.options);
      return this;
    },
    onPlace: function() {
      this.owner.master.structures.push(this.entity);
      if (this.onTurnBegin) {
        HTomb.Events.subscribe(this,"TurnBegin");
      }
    },
    onRemove: function() {
      this.owner.master.structures.splice(this.owner.master.structures.indexOf(this.entity),1);
      HTomb.Events.unsubscribeAll(this);
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
      return "%c{yellow}Structure: "+this.entity.describe({capitalized: true, atCoordinates: true})+".";
    },
    detailsText: function() {
      let txt = this.commandsText();
      txt.concat(this.optionsText());
      return txt;
    },
    commandsText: function() {
      let txt = [
        "%c{orange}**Esc: Done.**",
        "Wait: NumPad 5 / Control+Space.",
        "Click / Space: Select.",
        "Enter: Toggle Pause.",
        " ",
        this.headerText(),
        "a-z: Toggle option.",
        "Tab: Next structure.",
        " "
      ];
      txt = txt.concat(this.optionsText());
      return txt;
    },
    updateOptions: function() {

    },
    optionsHeading: function() {

    },
    noOptionsText: function() {
      return "(No options.)";
    },
    optionsText: function() {
      this.updateOptions();
      if (this.options.length===0) {
        return this.noOptionsText();
      }
      let txt = [this.optionsHeading()];
      let alphabet = "abcdefghijklmnopqrstuvwxyz";
      for (let i=0; i<this.options.length; i++) {
        let opt = this.options[i];
        let s = "";
        if (opt.active) {
          s = "%c{white}";
        } else {
          s = "%c{gray}";
        }
        s+=alphabet[i];
        s+=") ";
        if (opt.selected) {
          s += "[X] ";
        } else {
          s += "[ ] ";
        }
        s+=opt.text;
        s+=".";
        txt.push(s);
      }
      return txt;
    },
    choiceCommand: function(i) {
      if (this.options[i]!==undefined) {
        this.options[i].selected = !this.options[i].selected;
      }
    },
    upCommand: function() {
    },
    downCommand: function() {
    },
    leftCommand: function() {
    },
    rightCommand: function() {
    },
    moreCommand: function() {
    },
    lessCommand: function() {
    },
    cancelCommand: function() {
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

  HTomb.Things.defineStructure({
    template: "Workshop",
    makes: [],
    queue: null,
    task: null,
    height: 3,
    width: 3,
    onPlace: function(x,y,z) {
      HTomb.Things.templates.Structure.onPlace.call(this,x,y,z);
      this.queue = [];
    },
    onRemove: function() {
      HTomb.Things.templates.Structure.onRemove.call(this);
      for (let i=0; i<this.queue.length; i++) {
        this.task.cancel();
      }
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
        if (this.queue[0] && this.queue[0][0]===this.task.task.makes) {
          if (this.queue[0][1]!=="infinite") {
            this.queue[0][2]+=1;
          }
        } else {
          this.queue.splice(this.cursor+1,0,[this.task.task.makes,"finite",1]);
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
          this.task.task.cancel();
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
          console.log("trying next good");
          this.nextGood();
        }
      }
    },
    nextGood: function() {
      if (this.queue.length===0) {
        return;
      } else if (HTomb.World.tasks[HTomb.Utils.coord(this.x,this.y,this.z)]) {
        HTomb.GUI.pushMessage("Workshop tried to create new task but there was already a zone.");
        return;
      }
      // this is a good place to check for ingredients
      let ings = HTomb.Utils.copy(HTomb.Things.templates[this.queue[0][0]].ingredients);
      if (HTomb.Debug.noingredients) {
        ings = {};
      }
      if (this.owner.master.ownsAllIngredients(ings)!==true) {
        this.task = null;
        this.queue.push(this.queue.shift());
        return;
      }
      let task = HTomb.Things.templates.ProduceTask.designateTile(this.x,this.y,this.z,this.owner);
      this.task = task;
      task.task.makes = this.queue[0][0];
      task.task.workshop = this;
      HTomb.GUI.pushMessage("Next good is "+HTomb.Things.templates[task.task.makes].describe({article: "indefinite"}));
      task.name = "produce "+HTomb.Things.templates[task.task.makes].name;
      task.task.ingredients = ings;
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
    detailsText: function() {
      if (this.cursor>=this.queue.length) {
        this.cursor = this.queue.length-1;
      }
      let txt = [
        "%c{orange}**Esc: Done.**",
        "Wait: NumPad 5 / Control+Space.",
        "Click / Space: Select.",
        "Enter: Toggle Pause.",
        " ",
        "%c{yellow}Workshop: "+this.describe({capitalized: true, atCoordinates: true})+".",
        "Up/Down: Traverse queue.",
        "Left/Right: Alter repeat.",
        "[/]: Alter count.",
        "a-z: Insert good below the >.",
        "Backspace/Delete: Remove good.",
        "Tab: Next structure.",
        " ",
        "Goods:"
      ];
      let alphabet = 'abcdefghijklmnopqrstuvwxyz';
      for (let i=0; i<this.makes.length; i++) {
        let t = HTomb.Things.templates[this.makes[i]];
        let g = t.describe({article: "indefinite"});
        let ings = t.ingredients;
        if (HTomb.Utils.notEmpty(ings)) {
          g+=" ";
          g+=HTomb.Utils.listIngredients(ings);
          if (this.owner && this.owner.master && this.owner.master.ownsAllIngredients(ings)!==true) {
            g = "%c{gray}"+g;
          }
        }
        txt.push(alphabet[i]+") "+g);
      }
      txt.push(" ");
      txt.push("Production Queue:");
      let startQueue = txt.length;
      if (this.task) {
        let s = "@ " + HTomb.Things.templates[this.task.task.makes].describe({article: "indefinite"});
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


  HTomb.Things.defineWorkshop({
    template: "Carpenter",
    name: "carpenter",
    symbols: ["\u25AE","/","\u2699","\u2261","\u25AA",".","\u2692",".","\u25A7"],
    bg: "#665555",
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
    makes: [
      "WorkAxe",
      "DoorItem",
      "TorchItem"
    ]
  });

  HTomb.Things.defineStructure({
    template: "Storage",
    height: 3,
    width: 3,
    dormant: 0,
    dormancy: 10,
    tasks: null,
    stores: function(item) {return false;},
    onCreate: function() {
      this.tasks = [];
      for (let i=0; i<this.width*this.height; i++) {
        this.tasks.push(null);
      }
      return this;
    },
    detailsText: function() {
      let txt = [
        "%c{orange}**Esc: Done.**",
        "%c{yellow}Storage: "+this.describe({capitalized: true, atCoordinates: true})+".",
        " ",
        "Contents:"
      ];
      let totalItems = {};
      for (let i=0; i<this.squares.length; i++) {
        let s = this.squares[i];
        let items = HTomb.World.items[coord(s[0],s[1],s[2])];
        if (items) {
          items = items.items;
          for (let j=0; j<items.length; j++) {
            let item = items[j];
            totalItems[item.template] = totalItems[item.template] || 0;
            totalItems[item.template] += item.item.n;
          }
        }
      }
      for (let key in totalItems) {
        let n = totalItems[key];
        let line;
        if (n===1) {
          line = HTomb.Things.templates[key].describe({article: "indefinite"});
          console.log(line);
        } else {
          line = n + " " + HTomb.Things.templates[key].describe({plural: "true"});
          console.log(line);
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
      let items = this.owner.master.ownedItems;
      for (let i=0; i<items.length; i++) {
        //if ever we run out of task space, break the loop
        if (this.tasks.indexOf(null)===-1) {
          return;
        }
        let item = items[i];
        let f = HTomb.World.features[coord(item.x,item.y,item.z)];
        if (!item.item.isOnGround()) {
          continue;
        } else if (this.stores(item)===false) {
          continue;
        } else if (HTomb.World.tasks[coord(item.x,item.y,item.z)]!==undefined) {
          continue;
        } else if (HTomb.Tiles.isReachableFrom(this.x, this.y, this.z, item.x, item.y, item.z)===false) {
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
          let feat = this.features[slots[0]];
          let z = HTomb.Things.HaulTask({
             assigner: this.owner,
             name: "haul " + item.describe()
           });
          z.place(item.x,item.y,item.z);
          z.task.item = item;
          z.task.storage = this;
          z.task.feature = feat;
          this.tasks[slots[0]] = z;
        }
      }
    }
  });

  HTomb.Things.defineTask({
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
      let x = this.entity.x;
      let y = this.entity.y;
      let z = this.entity.z;
      // should this check whether the item is still here?
      if (this.validTile(x,y,z) && HTomb.Tiles.isReachableFrom(x,y,z,cr.x,cr.y,cr.z, {
        searcher: cr,
        searchee: this.entity,
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
      if (cr.inventory.items.items.indexOf(this.item)!==-1) {
        cr.ai.target = feature;
        this.entity.place(feature.x, feature.y, feature.z);
      } else if (item.item.isOnGround())  {
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
          this.completeWork(this.assignee);
          cr.ai.acted = true;
          return;
        } else {
          // move it to the building;
          this.entity.place(feature.x, feature.y, feature.z);
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
      HTomb.Things.templates.Task.onDespawn.call(this);
      let tasks = this.storage.tasks;
      tasks.splice(tasks.indexOf[this.entity],1,null);
    }
  });

  HTomb.Things.defineStorage({
    template: "Stockpile",
    name: "stockpile",
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
    stores: function(item) {
      if (item.template==="WoodPlank" || item.template==="Rock") {
        return true;
      } else {
        return false;
      }
    }
  });

  HTomb.Things.defineTask({
    template: "ProduceTask",
    name: "produce",
    bg: "#336699",
    workshop: null,
    makes: null,
    labor: 20,
    started: false,
    dormancy: 4,
    canAssign: function(cr) {
      if (this.entity.isPlaced()===false) {
        return false;
      }
      let x = this.entity.x;
      let y = this.entity.y;
      let z = this.entity.z;
      if (this.validTile(x,y,z) && HTomb.Tiles.isReachableFrom(x,y,z,cr.x,cr.y,cr.z, {
        searcher: cr,
        searchee: this.entity,
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
    workBegun: function() {
      return this.started;
    },
    beginWork: function() {
      if (!HTomb.Debug.noingredients) {
        let test = this.assignee.inventory.items.takeItems(this.ingredients);
      }
      this.started = true;
      this.labor = HTomb.Things.templates[this.makes].labor || this.labor;
      HTomb.GUI.pushMessage(this.beginMessage());
    },
    work: function(x,y,z) {
      let assignee = this.assignee;
      if (this.workBegun()!==true) {
        this.beginWork(this.assignee);
      }
      let labor = assignee.worker.labor;
      if (assignee.equipper && assignee.equipper.slots.MainHand && assignee.equipper.slots.MainHand.equipment.labor>labor) {
        labor = assignee.equipper.slots.MainHand.equipment.labor;
      }
      this.labor-=labor;
      this.assignee.ai.acted = true;
      this.assignee.ai.actionPoints-=16;
      if (this.labor<=0) {
        this.completeWork(assignee);
      }
    },
    completeWork: function() {
      let x = this.entity.x;
      let y = this.entity.y;
      let z = this.entity.z;
      let item = HTomb.Things[this.makes]().place(x,y,z);
      item.item.setOwner(HTomb.Player);
      HTomb.Events.publish({type: "Complete", task: this});
      this.workshop.occupied = null;
      HTomb.GUI.pushMessage(this.assignee.describe({capitalized: true, article: "indefinite"}) + " finishes making " + HTomb.Things.templates[this.makes].describe({article: "indefinite"}));
      this.entity.despawn();
    },
    onDespawn: function() {
      HTomb.Things.templates.Task.onDespawn.call(this);
      this.workshop.task = null;
      this.workshop.nextGood();
    }
  });

  HTomb.Things.defineTask({
    template: "ConstructTask",
    name: "construct",
    longName: "create a structure",
    bg: "#553300",
    makes: null,
    structures: ["Carpenter","Stockpile","BlackMarket","Sanctum"],
    validTile: function(x,y,z) {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      if (HTomb.World.tiles[z][x][y]!==HTomb.Tiles.FloorTile) {
        return false;
      }
      let f = HTomb.World.features[coord(x,y,z)];
      if (f===undefined || (f.template==="IncompleteFeature" && f.makes.template===this.makes)) {
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
            // a completed, partial version of the same workshop
          } else if (f && f.template===structure.template+"Feature") {
              struc = f.structure;
              if (struc.isPlaced()===true || struc.structure.x!==squares[mid][0] || struc.structure.y!==squares[mid][1]) {
                failed = true;
              }
            // an incomplete version of the same workshop
          } else if (f && (f.template!=="IncompleteFeature" || !f.makes || f.makes.template!==structure.template+"Feature")) {
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
            w.structure.owner = assigner;
            w.structure.squares = squares;
            w.structure.x = squares[mid][0];
            w.structure.y = squares[mid][1];
            w.structure.z = squares[mid][2];
          }
          for (let i=0; i<squares.length; i++) {
            let crd = squares[i];
            // this might get weird if you try overlapping a farm with anothern farm...
            if (HTomb.World.features[coord(crd[0],crd[1],crd[2])] && HTomb.World.features[coord(crd[0],crd[1],crd[2])].template===w.template+"Feature") {
              continue;
            }
            this.makes = w.structure.template+"Feature";
            let task = this.designateTile(crd[0],crd[1],crd[2],assigner);
            if (task) {
              //Bugs have occurred here...I may have fixed them...
              task.task.structure = w;
              task.task.makes = w.structure.template+"Feature";
              if (w.structure.height!==null && w.structure.width!==null) {
                if (HTomb.Debug.noingredients) {
                  task.task.ingredients = {};
                } else {
                  task.task.ingredients = HTomb.Utils.clone(w.structure.ingredients[i]);
                }
              }
              task.task.position = i;
              task.name = task.name + " " + w.structure.name;
            }
          }
        }
        function myHover(x, y, z, squares) {
          //hacky as all get-out
          let menu = HTomb.GUI.Panels.menu;
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
              if (HTomb.World.explored[s[2]][s[0]][s[1]]!==true || that.validTile(s[0],s[1],s[2])!==true) {
                valid = false;
              }
            }
            if (valid===false) {
              menu.middle = ["%c{orange}Cannot build structure here."];
              return;
            } else {
              menu.middle = ["%c{lime}Build structure here."];
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
      if (HTomb.Things.templates.Task.canAssign.call(this, cr)!==true) {
        return false;
      }
      // more intuitive if ingredient-needing features are built first
      let ings = this.structure.structure.neededIngredients();
      if (HTomb.Utils.notEmpty(ings) && HTomb.Utils.notEmpty(this.ingredients)!==true) {
        return false;
      }
      return true;
    },
    beginWork: function() {
      HTomb.Things.templates.Task.beginWork.call(this);
      let i = this.position;
      let f = HTomb.World.features[coord(this.entity.x, this.entity.y, this.entity.z)];
      f.fg = this.structure.structure.fgs[i] || this.structure.fg;
      f.makes.fg = this.structure.structure.fgs[i] || this.structure.fg;
      f.makes.symbol = this.structure.structure.symbols[i] || this.structure.symbol;
      HTomb.World.covers[this.entity.z][this.entity.x][this.entity.y] = HTomb.Covers.NoCover;
    },
    completeWork: function() {
      let x = this.entity.x;
      let y = this.entity.y;
      let z = this.entity.z;
      let f = HTomb.World.features[coord(x,y,z)];
      f.structure = this.structure;
      this.structure.structure.features[this.position] = f;
      for (let i=0; i<this.structure.structure.squares.length; i++) {
        if (!this.structure.structure.features[i]) {
          break;
        }
        if (i===this.structure.structure.squares.length-1) {
          let w = this.structure;
          w.place(w.structure.x, w.structure.y, w.structure.z);
          HTomb.GUI.pushMessage(this.assignee.describe({capitalized: true, article: "indefinite"}) + " finishes building " + w.describe({article: "indefinite", atCoordinates: true}) + ".");
        }
      }
      HTomb.Things.templates.Task.completeWork.call(this);
    },
    designateBox: function(squares, options) {
      options = options || {};
      var assigner = options.assigner;
      var callb = options.callback;
      callb.call(options.context,squares,assigner);
    }
  });

  HTomb.Things.defineStructure({
    template: "BlackMarket",
    name: "black market",
    height: 3,
    width: 3,
    symbols: ["\u00A3",".",".",".","\u2696","$","\u00A2","\u20AA","\u00A4"],
    fgs: ["#552222",HTomb.Constants.FLOORFG,HTomb.Constants.FLOORFG,HTomb.Constants.FLOORFG,"#888844","#888844","#225522","#333333","#222266"],
    bg: "#555544"
  });

  HTomb.Things.defineStructure({
    template: "BlackMarket",
    name: "black market",
    height: 3,
    width: 3,
    symbols: ["\u00A3",".",".",".","\u2696","$","\u00A2","\u20AA","\u00A4"],
    fgs: ["#552222",HTomb.Constants.FLOORFG,HTomb.Constants.FLOORFG,HTomb.Constants.FLOORFG,"#888844","#888844","#225522","#333333","#222266"],
    bg: "#555544"
  });

  HTomb.Things.defineStructure({
    template: "Sanctum",
    name: "sanctum",
    height: 3,
    width: 3,
    symbols: ["\u2625",".","\u2AEF",".","\u2135",".","\u2AEF","\u2606","\u263F"],
    fgs: ["magenta",HTomb.Constants.FLOORFG,"cyan",HTomb.Constants.FLOORFG,"green",HTomb.Constants.FLOORFG,"yellow","red","orange"],
    bg: "#222244"
  });


return HTomb;
})(HTomb);