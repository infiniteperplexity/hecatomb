HTomb = (function(HTomb) {
  "use strict";
  var coord = HTomb.Utils.coord;
  

  let Thing = HTomb.Things.templates.Thing;
  // Define a generic entity that occupies a tile space
  let Entity = Thing.extend({
    template: "Entity",
    name: "entity",
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
      if (this.behaviors) {
        var beh = this.behaviors;
        for (var i=0; i<beh.length; i++) {
          if (beh[i].onPlace) {
             beh[i].onPlace(x,y,z);
          }
        }
      }
      // should this indeed go after the behaviors?
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
    remove: function() {
      if (this.behaviors) {
        var beh = this.behaviors;
        for (var i=0; i<beh.length; i++) {
          if (beh[i].onRemove) {
            beh[i].onRemove();
          }
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
      if (this.behaviors) {
        var beh = this.behaviors;
        for (var i=0; i<beh.length; i++) {
          var b = beh[i];
          if (b.destroy) {
            b.destroy();
          }
          b.despawn();
        }
      }
      HTomb.Events.publish({type: "Destroy", entity: this});
      this.despawn();
    },
    despawn: function() {
      if (this.isPlaced()) {
        this.remove();
      }
      if (this.behaviors) {
      var beh = this.behaviors;
        for (var i=0; i<beh.length; i++) {
          if (beh[i].onDespawn) {
            beh[i].onDespawn();
          }
        }
      }
      Thing.despawn.call(this);
    },
    spawn: function(args) {
      Thing.spawn.call(this,args);
      for (let b in this.Behaviors) {
        let beh = HTomb.Things.create(b,this.Behaviors[b] || {});
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
    },
    highlight: function(bg) {
      this.highlightColor = bg;
    },
    unhighlight: function() {
      if (this.highlightColor) {
        delete this.highlightColor;
      }
    },
  });

  let Behavior = Entity.extend({
    template: "Behavior",
    name: "behavior",
    entity: null,
    addToEntity: function(ent) {
      this.entity = ent;
      ent[this.name] = this;
      if (!ent.behaviors) {
        ent.behaviors = [];
      }
      ent.behaviors.push(this);
      if (this.onAdd) {
        this.onAdd(this.options);
      }
    },
    detachFromEntity: function() {
      let ent = this.entity;
      if (this.onDetach) {
        this.onDetach();
      }
      ent.behaviors.splice(ent.behaviors.indexOf(this),1);
      this.entity = null;
      delete ent[this.name];
    }
  });

  let Item = Entity.extend({
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
          this.owner.master.ownedItems.splice(this.owner.master.ownedItems.indexOf(this),1);
        }
      }
    },
    setOwner: function(cr) {
      this.unsetOwner();
      this.owner = cr;
      if (cr!==null && cr.master) {
        cr.master.ownedItems.push(this)
      }
    },
    fall: function() {
      var g = HTomb.Tiles.groundLevel(this.x,this.y,this.z);
      HTomb.GUI.sensoryEvent(this.describe({capitalized: true, article: "indefinite"}) + " falls " + (this.z-g) + " stories!",this.x,this.y,this.z);
      this.place(this.x,this.y,g);
      HTomb.GUI.render();
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
      if (this.x===null) {
        return false;
      }
      let x = this.x;
      let y = this.y;
      let z = this.z;
      let f = HTomb.World.features(HTomb.Utils.coord(x,y,z));
      if (f && f.structure && f.structure.owner===HTomb.Player) {
        return true;
      }
      return false;
    },
    carriedByMinion: function() {
      if (!this.container) {
        console.log(this);
        return false;
      }
      let parent = this.container.heldby;
      if (parent.entity && HTomb.Player.master.minions.indexOf(parent.entity)) {
        return true;
      } else {
        return false;
      }
    },
    despawn: function() {
      if (HTomb.Player.master) {
        let owned = HTomb.Player.master.ownedItems;
        let i = owned.indexOf(this.entity);
        if (i!==-1) {
          owned.splice(i,1);
        }
      }
      Entity.despawn.call(this);
    },
    carriedByCreature: function() {
      if (!this.container) {
        console.log(this);
        return false;
      }
      let parent = this.container.heldby;
      if (parent.entity && parent.entity.creature) {
        return true;
      } else {
        return false;
      }
    },
    place: function(x,y,z,args) {
      Entity.place.call(this,x,y,z,args);
      let c = coord(x,y,z);
      var pile = HTomb.World.items[c] || HTomb.Things.Container({heldby: c});
      pile.push(this);
      if (pile.length>0) {
        HTomb.World.items[c] = pile;
      } else {
        console.log("despawning stupid pile");
        pile.despawn();
      }
      return this;
    },
    remove: function(args) {
      let c = coord(this.x,this.y,this.z);
      var pile = HTomb.World.items[c];
      // remove it from the old pile
      if (pile) {
        if (pile.contains(this)) {
          pile.remove(this);
        }
      }
      //if there is a haul task for the item here, cancel it
      let task = HTomb.World.tasks[c];
      if (task && task.item===this) {
        task.cancel();
      }
      Entity.remove.call(this, args);
    },
    describe: function(options) {
      options = options || {};
      if (this.stackable && this.n>1) {
        options.plural = true;
        options.article = this.n;
      }
      return Entity.describe.call(this,options);
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
        total+=this.items[i].getBulk();
      }
      return total;
    },
    onCreate: function() {
      this.items = [];
      return this;
    },
    absorbStack: function(item) {
      if (item.container) {
        item.container.remove(item);
      }
      //let n = item.item.n;
      let n = Math.min(item.n,this.canFit(item));
      let existing = false;
      for (let i=0; i<this.items.length; i++) {
        if (this.items[i].template===item.template) {
          this.items[i].n+=n;
          existing = true;
          break;
        }
      }
      if (existing) {
        item.n-=n;
        if (item.n===0) {
          item.despawn();
          item = null;
        }
        return item;
      } else {
        this.items.push(item);
        item.container = this;
        return null;
      }
    },
    canFit: function(item) {
      let one = item.n;
      let two = 0;
      let bulk = 0;
      for (let i=0; i<this.items.length; i++) {
        if (item.template===this.items[i].template) {
          two = this.items[i].n;
          bulk = this.items[i].getBulk();
          break;
        }
      }
      let room = this.maxBulk - this.totalBulk() + bulk;
      let n = 0;
      do {
        n+=1;
      } while (item.getBulk(n)<=room);
      n-=1;
      return n;
    },
    push: function(item) {
      if (item===undefined) {
        throw new Error("what the what?");
      }
      if (item.stackable) {
        this.absorbStack(item);
      } else {
        if (item.container) {
          item.container.remove(item);
        }
        this.items.push(item);
        item.container = this;
      }
    },
    insert: function(item,i) {
      if (item.stackable) {
        this.absorbStack(item);
      } else {
        this.items[i] = item;
        item.container = this;
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
          tally+=this.items[i].n;
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
      if (HTomb.Things.templates[i_or_t].stackable!==true) {
        var first = this.getFirst(i_or_t);
        return this.remove(first);
      } else {
        var last = this.getLast(i_or_t);
        if (last.n<=n) {
          this.remove(last);
          return last;
        } else {
          last.n-=n;
          var taken = HTomb.Things[last.template]();
          taken.n = n;
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
      if (HTomb.Things.templates[i_or_t].stackable!==true) {
        var first = this.getFirst(i_or_t);
        this.remove(first);
        return (first);
      } else {
        var last = this.getLast(i_or_t);
        if (last.n<=n) {
          this.remove(last);
          return last;
        } else {
          last.n-=n;
          var taken = HTomb.Things[last.template]();
          taken.n = n;
          return taken;
        }
      }
    },
    remove: function(item) {
      var indx = this.items.indexOf(item);
      if (indx>-1) {
        item.container = null;
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

  Entity.extend({
    template: "Feature",
    name: "feature",
    yields: null,
    //i kind of hate this name
    integrity: null,
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
    onDefine: function(args) {
      if (args.craftable===true) {
        let item = HTomb.Utils.copy(args);
        item.template = args.template+"Item";
        item.tags = ["Fixtures"];
        delete item.Behaviors;
        HTomb.Things.templates.Item.extend(item);
        let template = HTomb.Things.templates[args.template];
        // overwrite the item's ingredients
        template.ingredients = {};
        template.ingredients[args.template+"Item"] = 1;
        template.labor = args.labor || 10;
      }
    },
    place: function(x,y,z,args) {
      Entity.place.call(this,x,y,z,args);
      let c = coord(x,y,z);
      let f = HTomb.World.features[c];
      if (f) {
        console.log(this);
        console.log(f);
        throw new Error("unhandled feature conflict!");
      }
      HTomb.World.features[c] = this;
    },
    remove: function(args) {
      let c = coord(this.x,this.y,this.z);
      if (HTomb.World.features[c]) {
        delete HTomb.World.features[c];
      }
      Entity.remove.call(this,args);
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
        var x = this.x;
        var y = this.y;
        var z = this.z;
        for (var template in this.yields) {
          var n = HTomb.Utils.diceUntil(2,2);
          if (this.yields[template].nozero) {
            n = Math.max(n,1);
          }
          for (var i=0; i<n; i++) {
            var thing = HTomb.Things[template]().place(x,y,z);
            thing.setOwner(HTomb.Player);
          }
        }
      }
      this.destroy();
    }
  });

return HTomb;
})(HTomb);
