HTomb = (function(HTomb) {
  "use strict";
  var coord = HTomb.Utils.coord;
  // Define a generic entity that occupies a tile space


  HTomb.Things.define({
    template: "Entity",
    name: "entity",
    parent: "Thing",
    x: null,
    y: null,
    z: null,
    Behaviors: {},
    place: function(x,y,z,args) {
      if (this.isPlaced()) {
        this.remove();
      }
      this.x = x;
      this.y = y;
      this.z = z;
      if (this.isPlaced()) {
        var beh = this.behaviors;
        for (var i=0; i<beh.length; i++) {
          if (beh[i].onPlace) {
            beh[i].onPlace(x,y,z);
          }
        }
      }
      if (this.onPlace) {
        this.onPlace(x,y,z);
      }
      return this;
    },
    isPlaced: function() {
      if (this.x===null || this.y===null || this.z===null) {
        return false;
      } else {
        return true;
      }
    },
    validateBehaviors: function() {
      let props = Object.getOwnPropertyNames(this);
      let behaviors = [];
      let b = HTomb.Things.behaviors;
      for (let i=0; i<props.length; i++) {
        if (b.indexOf(props[i])!==-1) {
          behaviors.push(this[props[i]]);
        }
      }
      //return behaviors;
      this.behaviors = behaviors;
    },
    behaviorTemplate: function(beh) {
      let beh1 = HTomb.Utils.copy(this.Behaviors[beh]);
      let beh2 = HTomb.Utils.clone(HTomb.Things.templates[beh]);
      for (let v in beh1) {
        beh2[v] = beh1[v];
      }
      return beh2;
    },
    remove: function() {
      var beh = this.behaviors;
      for (var i=0; i<beh.length; i++) {
        if (beh[i].onRemove) {
          beh[i].onRemove();
        }
      }
      if (this.onRemove) {
        this.onRemove();
      }
      this.x = null;
      this.y = null;
      this.z = null;
    },
    destroy: function() {
      var beh = this.behaviors;
      for (var i=0; i<beh.length; i++) {
        var b = beh[i];
        if (b.destroy) {
          b.destroy();
        }
        b.despawn();
      }
      HTomb.Events.publish({type: "Destroy", entity: this});
      this.despawn();
    },
    onDespawn: function() {
      if (this.isPlaced()) {
        this.remove();
      }
      var beh = this.behaviors;
      for (var i=0; i<beh.length; i++) {
        if (beh[i].onDespawn) {
          beh[i].onDespawn();
        }
      }
    },
    fall: function() {
      var g = HTomb.Tiles.groundLevel(this.x,this.y,this.z);
      if (this.creature) {
        if (HTomb.World.creatures[coord(this.x,this.y,g)]) {
          alert("haven't decided how to handle falling creature collisions");
        } else {
          HTomb.GUI.sensoryEvent(this.describe({capitalized: true, article: "indefinite"}) + " falls " + (this.z-g) + " stories!",this.x,this.y,this.z);
          this.place(this.x,this.y,g);
        }
      }
      if (this.item) {
        HTomb.GUI.sensoryEvent(this.describe({capitalized: true, article: "indefinite"}) + " falls " + (this.z-g) + " stories!",this.x,this.y,this.z);
        this.place(this.x,this.y,g);
      }
      HTomb.GUI.render();
    },
    onCreate: function() {
      // Add behaviors to entity
      for (var b in this.Behaviors) {
        if (typeof(HTomb.Things[b])!=="function") {
            console.log("Problem with behavior " + b + " for " + this.describe());
        }
        //var beh = HTomb.Things[b](this.behaviors[b] || {});
        var beh = HTomb.Things.create(b,this.Behaviors[b] || {});
        beh.addToEntity(this);
      }
      // Randomly choose symbol if necessary
      if (Array.isArray(this.symbol)) {
        this.symbol = this.symbol[Math.floor(Math.random()*this.symbol.length)];
      }
      // Randomly choose  color if necessary
      if (Array.isArray(this.fg)) {
        this.fg = this.fg[Math.floor(Math.random()*this.fg.length)];
      }
      // Randomly perturb color, if necessary
      if (this.randomColor>0 && this.fg) {
        if (this.fg) {
          var c = ROT.Color.fromString(this.fg);
          c = ROT.Color.randomize(c,[this.randomColor, this.randomColor, this.randomColor]);
          c = ROT.Color.toHex(c);
          this.fg = c;
        }
      }
      return this;
    }
  });

  // Define a generic behavior that gets attached to entities
  HTomb.Things.define({
    template: "Behavior",
    name: "behavior",
    parent: "Thing",
    entity: null,
    addToEntity: function(ent) {
      this.entity = ent;
      // This is kind of a weird multi-level inheritance thing...
      if (this.parent==="Behavior") {
        ent[this.name] = this;
      } else {
        let name = HTomb.Things.templates[this.parent].name;
        ent[name] = this;
      }
      if (this.onAdd) {
        this.onAdd(this.options);
      }
      this.entity.validateBehaviors();
    },
    removeFromEntity: function() {
      let ent = this.entity;
      this.entity = null;
      ent[this.name] = undefined;
      this.entity.validateBehaviors();
      //onRemove stuff?
    },
    onDefine: function(args) {
      HTomb.Things.behaviors.push(args.name);
      let beh = HTomb.Things.templates[args.template];
      if (args.parent!=="Behavior") {
        let eargs = {};
        eargs.parent = "Entity";
        eargs.name = args.name;
        if (args.bg) {
          eargs.bg = args.bg;
          delete beh.bg;
        }
        if (args.fg) {
          eargs.fg = args.fg;
          delete beh.fg;
        }
        if (args.symbol) {
          eargs.symbol = args.symbol;
          delete beh.symbol;
        }
        eargs.Behaviors = {};
        eargs.Behaviors[args.template] = {};
        eargs.template = args.template+"Entity";
        HTomb.Things.define(eargs);
        HTomb.Things[args.template] = function(cargs) {
          let entargs = {};
          let bargs = {};
          let entity = HTomb.Things.templates[args.template+"Entity"];
          for (let arg in cargs) {
            if (typeof(cargs[arg])==="function") {
              entargs[arg] = cargs[arg];
            } else if (entity[arg]!==undefined) {
              entargs[arg] = cargs[arg];
            } else if (beh[arg]!==undefined) {
              bargs[arg] = cargs[arg];
            } else {
              entargs[arg] = cargs[arg];
            }
          }
          // this can't work like this...we need to avoid overwriting behaviors...
          // this worked okay for Rocks, didn't it?  Or maybe it overwrote...
          entargs.Behaviors = entargs.Behaviors || {};
          entargs.Behaviors[args.template] = bargs;
          return HTomb.Things[args.template+"Entity"](entargs);
        };
      }
    }
  });
  HTomb.Things.behaviors = [];

  HTomb.Things.defineBehavior({
    template: "Creature",
    name: "creature",
    // this will eventually create corpses with Material values
    leavesCorpse: true,
    die: function() {
      if (this.entity.x!==null && this.entity.y!==null && this.entity.z!==null) {
        HTomb.Particles.addEmitter(this.entity.x, this.entity.y, this.entity.z, HTomb.Particles.Blood, HTomb.Particles.Spray);
        HTomb.GUI.sensoryEvent(this.entity.describe({capitalized: true, article: "indefinite"}) + " dies.",this.entity.x,this.entity.y,this.entity.z,"red");
        if (this.entity.player) {
          this.entity.player.playerDeath();
        } else {
          if (this.leavesCorpse) {
            let corpse = HTomb.Things.Corpse({sourceCreature: this.entity});
            corpse.place(this.entity.x, this.entity.y, this.entity.z);
          }
          this.entity.destroy();
        }
      }
    },
    onPlace: function(x,y,z) {
      let c = coord(x,y,z);
      if (HTomb.World.creatures[c]) {
        HTomb.Debug.pushMessage("Overwrote a creature!");
        let cr = HTomb.World.creatures[c];
        cr.remove();
        cr.despawn();
      }
      HTomb.World.creatures[c] = this.entity;
    },
    onRemove: function() {
      delete HTomb.World.creatures[coord(this.entity.x, this.entity.y, this.entity.z)];
    }
  });

  HTomb.Things.defineBehavior({
    template: "Item",
    name: "item",
    stackable: false,
    n: 1,
    maxn: 10,
    container: null,
    bulk: 10,
    value: 1,
    tags: [],
    stackSize: [1,0.7,0.4,0.3,0.1],
    owner: null,
    unsetOwner: function() {
      if (this.owner) {
        if (this.owner.master.ownedItems.indexOf(this)!==-1) {
          this.owner.master.ownedItems.splice(this.owner.master.ownedItems.indexOf(this.entity),1);
        }
      }
    },
    setOwner: function(cr) {
      this.unsetOwner();
      this.owner = cr;
      if (cr!==null && cr.master) {
        cr.master.ownedItems.push(this.entity)
      }
    },
    // this method is for when an item is forced onto a square but can't fit
    tumble: function(x,y,z) {
      let dirs = HTomb.Utils.copy(ROT.DIRS[8]);
      HTomb.Utils.shuffle(dirs);
      for (let i=0; i<8; i++) {
        let pile = HTomb.World.items[coord(x+dirs[i][0],y+dirs[i][1],z)];
        if (!pile || (pile.canFit(this) && HTomb.World.tiles[z][x+dirs[i][0]][y+dirs[i][1]].solid!==true)) {
          this.place(x+dirs[i][0], y+dirs[i][1], z);
          //make it fall as well?
          break;
        }
        if (i===7) {
          alert("Item crushed due to lack of space!");
          this.despawn();
        }
      }
    },
    getBulk: function(n) {
      n = n || this.n || 1;
      let bulk = 0;
      for (let i=0; i<n; i++) {
        let j = i;
        if (j>=this.stackSize.length) {
          j = this.stackSize.length-1;
        }
        bulk+=this.stackSize[j]*this.bulk;
      }
      return Math.ceil(bulk);
    },
    maxStack: function(b) {
      b = b || HTomb.Things.templates.Container.maxBulk;
      let n=0;
      let blk=0;
      do {
        n+=1;
      } while (this.getBulk(n)<=b);
      n-=1;
      return n;
    },
    isOnGround: function() {
      if (!this.container) {
        return false;
      }
      let parent = this.container.heldby;
      if (typeof(parent)==="number") {
        return true;
      } else {
        return false;
      }
    },
    inStructure: function() {
      if (this.entity.x===null) {
        return false;
      }
      let x = this.entity.x;
      let y = this.entity.y;
      let z = this.entity.z;
      let f = HTomb.World.features(HTomb.Utils.coord(x,y,z));
      if (f && f.structure && f.structure.owner===HTomb.Player) {
        return true;
      }
      return false;
    },
    carriedByMinion: function() {
      if (!this.container) {
        console.log(this.entity);
        return false;
      }
      let parent = this.container.heldby;
      if (parent.entity && HTomb.Player.master.minions.indexOf(parent.entity)) {
        return true;
      } else {
        return false;
      }
    },
    onDespawn: function() {
      if (HTomb.Player.master) {
        let owned = HTomb.Player.master.ownedItems;
        let i = owned.indexOf(this.entity);
        if (i!==-1) {
          owned.splice(i,1);
        }
      }
    },
    carriedByCreature: function() {
      if (!this.container) {
        console.log(this.entity);
        return false;
      }
      let parent = this.container.heldby;
      if (parent.entity && parent.entity.creature) {
        return true;
      } else {
        return false;
      }
    },
    onPlace: function(x,y,z) {
      let c = coord(x,y,z);
      var pile = HTomb.World.items[c] || HTomb.Things.Container({heldby: c});
      pile.push(this.entity);
      if (pile.length>0) {
        HTomb.World.items[c] = pile;
      } else {
        console.log("despawning stupid pile");
        pile.despawn();
      }
    },
    onRemove: function() {
      let c = coord(this.entity.x,this.entity.y,this.entity.z);
      var pile = HTomb.World.items[c];
      // remove it from the old pile
      if (pile) {
        if (pile.contains(this.entity)) {
          pile.remove(this.entity);
        }
      }
      //if there is a haul task for the item here, cancel it
      let task = HTomb.World.tasks[c];
      if (task && task.task.item===this.entity) {
        task.task.cancel();
      }
    },
    onDescribe: function(options) {
      if (this.stackable && this.n>1) {
        options.plural = true;
        options.article = this.n;
      }
      return options;
    },
    makeStack: function() {
      if (this.stackable) {
        this.n = 1+HTomb.Utils.diceUntil(3,3);
      }
    },
    containerXYZ: function() {
      if (this.container && this.container.parent && this.container.parent.entity) {
        let e = this.container.parent.entity;
        return [e.x,e.y,e.z];
      } else {
        return [null,null,null];
      }
    }
  });

  HTomb.Things.define({
    template: "Container",
    name: "container",
    parent: "Thing",
    items: null,
    heldby: null,
    maxBulk: 25,
    totalBulk: function() {
      let total = 0;
      for (let i=0; i<this.items.length; i++) {
        total+=this.items[i].item.getBulk();
      }
      return total;
    },
    onCreate: function() {
      this.items = [];
      return this;
    },
    absorbStack: function(item) {
      if (item.item.container) {
        item.item.container.remove(item);
      }
      //let n = item.item.n;
      let n = Math.min(item.item.n,this.canFit(item));
      let existing = false;
      for (let i=0; i<this.items.length; i++) {
        if (this.items[i].template===item.template) {
          this.items[i].item.n+=n;
          existing = true;
          break;
        }
      }
      if (existing) {
        item.item.n-=n;
        if (item.item.n===0) {
          item.despawn();
          item = null;
        }
        return item;
      } else {
        this.items.push(item);
        item.item.container = this;
        return null;
      }
    },
    canFit: function(item) {
      let one = item.item.n;
      let two = 0;
      let bulk = 0;
      for (let i=0; i<this.items.length; i++) {
        if (item.template===this.items[i].template) {
          two = this.items[i].item.n;
          bulk = this.items[i].item.getBulk();
          break;
        }
      }
      let room = this.maxBulk - this.totalBulk() + bulk;
      let n = 0;
      do {
        n+=1;
      } while (item.item.getBulk(n)<=room);
      n-=1;
      return n;
    },
    push: function(item) {
      if (item.item.stackable) {
        this.absorbStack(item);
      } else {
        if (item.item.container) {
          item.item.container.remove(item);
        }
        this.items.push(item);
        item.item.container = this;
      }
    },
    insert: function(item,i) {
      if (item.item.stackable) {
        this.absorbStack(item);
      } else {
        this.items[i] = item;
        item.item.container = this;
      }
    },
    expose: function(i) {
      return this.items[i];
    },
    exposeItems: function() {
      let arr = [];
      for (let i=0; i<this.items.length; i++) {
        arr.push(this.items[i]);
      }
      return arr;
    },
    contains: function(item) {
      var indx = this.items.indexOf(item);
      if (indx>-1) {
        return true;
      } else {
        return false;
      }
    },
    containsAny: function(template) {
      for (var i=0; i<this.items.length; i++) {
        if (this.items[i].template===template) {
          return true;
        }
      }
      return false;
    },
    countAll: function(template) {
      var tally = 0;
      for (var i=0; i<this.items.length; i++) {
        if (this.items[i].template===template) {
          tally+=this.items[i].item.n;
        }
      }
      return tally;
    },
    getFirst: function(template) {
      for (var i=0; i<this.items.length; i++) {
        if (this.items[i].template===template) {
          return this.items[i];
        }
      }
      return null;
    },
    getLast: function(template) {
      for (var i=this.items.length-1; i>=0; i--) {
        if (this.items[i].template===template) {
          return this.items[i];
        }
      }
      return null;
    },
    takeOne: function(i_or_t) {
      if (typeof(i_or_t)!=="string" && i_or_t.template) {
        i_or_t = i_or_t.template;
      }
      if (HTomb.Things.templates[i_or_t].stackable!==true) {
        var first = this.getFirst(i_or_t);
        return this.remove(first);
      } else {
        var last = this.getLast(i_or_t);
        if (last.item.n===1) {
          this.remove(last);
          return last;
        } else {
          last.item.n-=1;
          var single = HTomb.Things[last.template]();
          single.item.n = 1;
          return single;
        }
      }
    },
    take: function(i_or_t, n) {
      n = n || 1;
      if (typeof(i_or_t)!=="string" && i_or_t.template) {
        i_or_t = i_or_t.template;
      }
      let ing = {};
      ing[i_or_t] = n;
      if (this.hasAll(ing)!==true) {
        return false;
      }
      if (HTomb.Things.templates[i_or_t].Behaviors.Item.stackable!==true) {
        var first = this.getFirst(i_or_t);
        return this.remove(first);
      } else {
        var last = this.getLast(i_or_t);
        if (last.item.n<=n) {
          this.remove(last);
          return last;
        } else {
          last.item.n-=n;
          var taken = HTomb.Things[last.template]();
          taken.item.n = n;
          return taken;
        }
      }
    },
    takeSome: function(i_or_t, n) {
      n = n || 1;
      if (typeof(i_or_t)!=="string" && i_or_t.template) {
        i_or_t = i_or_t.template;
      }
      let ing = {};
      ing[i_or_t] = n;
      if (this.hasAll(ing)!==true) {
        n = this.countAll(i_or_t);
      }
      if (HTomb.Things.templates[i_or_t].Behaviors.Item.stackable!==true) {
        var first = this.getFirst(i_or_t);
        this.remove(first);
        return (first);
      } else {
        var last = this.getLast(i_or_t);
        if (last.item.n<=n) {
          this.remove(last);
          return last;
        } else {
          last.item.n-=n;
          var taken = HTomb.Things[last.template]();
          taken.item.n = n;
          return taken;
        }
      }
    },
    remove: function(item) {
      var indx = this.items.indexOf(item);
      if (indx>-1) {
        item.item.container = null;
        this.items.splice(indx,1);
        // should this only happen if it's on the ground?
        if (typeof(this.heldby)==="number") {
          item.remove();
          if (this.items.length===0) {
            delete HTomb.World.items[this.heldby];
            this.despawn();
          }
        }
        return item;
      }
    },
    takeItems: function(ingredients) {
      let items = [];
      if (this.hasAll(ingredients)!==true) {
        console.log("can't find the ingredients");
        return false;
      }
      for (let item in ingredients) {
        let n = ingredients[item];
        let taken = this.take(item,n);
        // need some error handling?
        items.push(taken);
      }
      return items;
    },
    hasAll: function(ingredients) {
      for (var ing in ingredients) {
        var n = ingredients[ing];
        // if we lack what we need, search for items
        if (this.countAll(ing)<n) {
          return false;
        }
      }
      return true;
    },
    missingIngredients: function(ingredients) {
      var miss = {};
      for (var ing in ingredients) {
        // if we lack what we need, search for items
        let n = ingredients[ing] - this.countAll(ing);
        if (n>0) {
          miss[ing] = n;
        }
      }
      return miss;
    },
    list: function() {
      var mesg = "";
      for (var i = 0; i<this.items.length; i++) {
        if (i>0) {
          mesg+=" ";
        }
        mesg+=this.items[i].describe({article: "indefinite"});
        if (i===this.items.length-2) {
          mesg = mesg + ", and";
        } else if (i<this.items.length-1) {
          mesg = mesg + ",";
        }
      }
      return mesg;
    },
    lineList: function(spacer) {
      var buffer = [];
      for (var i = 0; i<this.items.length; i++) {
        buffer.push([spacer,this.items[i].describe({article: "indefinite"})]);
      }
      return buffer;
    },
    head: function() {
      return this.items[0];
    },
    tail: function() {
      return this.items[this.items.length-1];
    },
    forEach: function(callb) {
      let ret = [];
      for (let i=0; i<this.items.length; i++) {
        ret.push(callb(this.items[i], this, i));
      }
      return ret;
    },
    exposeArray: function() {
      return this.items;
    }
  });
  Object.defineProperty(HTomb.Things.templates.Container,"length",{get: function() {return this.items.length;}});

  HTomb.Things.defineBehavior({
    template: "Feature",
    name: "feature",
    yields: null,
    integrity: null,
    stackedFeatures: null,
    onDefine: function(args) {
      if (args.craftable===true) {
        let item = HTomb.Utils.copy(args);
        item.template = args.template+"Item";
        item.tags = ["Fixtures"];
        delete item.Behaviors;
        HTomb.Things.defineItem(item);
        let template = HTomb.Things.templates[args.template];
        // overwrite the item's ingredients
        template.ingredients = {};
        template.ingredients[args.template+"Item"] = 1;
        template.labor = args.labor || 10;
      }
    },
    onPlace: function(x,y,z) {
      let c = coord(x,y,z);
      let f = HTomb.World.features[c];
      if (f) {
        throw new Error("unhandled feature conflict!");
      }
      HTomb.World.features[c] = this.entity;
    },
    onRemove: function() {
      let c = coord(this.entity.x,this.entity.y,this.entity.z);
      if (HTomb.World.features[c]) {
        delete HTomb.World.features[c];
      }
    },
    dismantle: function(optionalTask) {
      if (this.integrity===null) {
        this.integrity=5;
      }
      let labor = 1;
      this.integrity-=labor;
      // need to account for work axes somehow
      if (this.integrity<=0) {
        this.harvest();
      }
    },
    harvest: function() {
      if (this.yields!==null) {
        var x = this.entity.x;
        var y = this.entity.y;
        var z = this.entity.z;
        for (var template in this.yields) {
          var n = HTomb.Utils.diceUntil(2,2);
          if (this.yields[template].nozero) {
            n = Math.max(n,1);
          }
          for (var i=0; i<n; i++) {
            var thing = HTomb.Things[template]().place(x,y,z);
            thing.item.setOwner(HTomb.Player);
          }
        }
      }
      this.entity.destroy();
    }
  });

  HTomb.Things.defineByProxy("Creature","Entity");
  HTomb.Things.defineByProxy("Item","Entity");
  HTomb.Things.defineByProxy("Feature","Entity");

return HTomb;
})(HTomb);
