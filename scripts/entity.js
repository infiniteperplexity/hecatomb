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
      HTomb.Debug.logEvent("place",this);
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
      HTomb.Debug.logEvent("remove",this);
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
      HTomb.Debug.logEvent("getDestroyed",this);
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
          beh[i].despawn();
        }
      }
      Thing.despawn.call(this);
    },
    spawn: function(args) {
      Thing.spawn.call(this,args);

      for (let b in this.Behaviors) {
        if (!HTomb.Things[b]) {
          console.log(b);
        }
        let beh = HTomb.Things[b](HTomb.Utils.merge(HTomb.Things.templates[b], this.Behaviors[b]));
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

  let Behavior = Thing.extend({
    template: "Behavior",
    name: "behavior",
    entity: null,
    addToEntity: function(ent) {
      HTomb.Debug.logEvent("add",this);
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
      HTomb.Debug.logEvent("detach",this);
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
    onlist: null,
    bulk: 10,
    value: 1,
    tags: [],
    stackSize: [1,0.7,0.4,0.3,0.1],
    owned: true,
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
    isOnGround: function() {
      if (this.onlist && this.onlist.heldby===HTomb.World.items) {
        return true;
      }
      return false;
    },
    inStructure: function() {
      if (this.isOnGround()===false) {
        return false;
      }
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
      if (this.isOnGround() || !this.onlist) {
        return false;
      }
      let parent = this.onlist.heldby;
      if (parent.entity && HTomb.Player.master.minions.indexOf(parent.entity)) {
        return true;
      } else {
        return false;
      }
    },
    despawn: function() {
      if (this.onlist && !this.onlist.contains) {
        console.log("what the heck happened here?");
        console.log(this);
      }
      if (this.onlist && this.onlist.contains(this)) {
        this.onlist.removeItem(this);
      }
      Entity.despawn.call(this);
    },
    carriedByCreature: function() {
      if (this.isOnGround() || !this.onlist) {
        return false;
      }
      let parent = this.onlist.heldby;
      if (parent.entity && parent.entity.parent==="Creature") {
        return true;
      } else {
        return false;
      }
    },
    place: function(x,y,z,args) {
      Entity.place.call(this,x,y,z,args);
      let c = coord(x,y,z);
      var pile = HTomb.World.items[c] || HTomb.Things.Items();
      pile.push(this);
      if (pile.length>0) {
        HTomb.World.items[c] = pile;
      }
      return this;
    },
    remove: function(args) {
      let c = coord(this.x,this.y,this.z);
      var pile = HTomb.World.items[c];
      // remove it from the old pile
      if (pile) {
        if (pile.contains(this)) {
          pile.removeItem(this);
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
    }
  });

  Entity.extend({
    template: "Feature",
    name: "feature",
    solid: false,
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
      if (this.solid) {
        HTomb.World.blocks[c] = this;
      }
    },
    remove: function(args) {
      let c = coord(this.x,this.y,this.z);
      if (HTomb.World.features[c]) {
        delete HTomb.World.features[c];
      }
      if (this.solid && HTomb.World.blocks[c]) {
        delete HTomb.World.blocks[c];
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
            thing.owned = true;
          }
        }
      }
      this.destroy();
    }
  });



  class Items extends Array {
    // normal array constructor arguments forbidden
    // argument is the "holder" of the item list
      // almost always either a behavior or the global item list
    constructor(heldby) {
      super();
      this.heldby = heldby || HTomb.World.items;
    }
    // add a specific item to the array, absorbing it into a stack if need be
    addItem(item) {
      if (item.onlist) {
        item.onlist.removeItem(item);
      }
      for (let it of this) {
        if (it.template===item.template) {
          it.n+=item.n;
          item.despawn();
          return;
        }
      }
      item.onlist = this;
      super.push(item);
    }
    // override normal ways of adding items
    push(item) {
      this.addItem(item);
    }
    unshift(item) {
      this.addItem(item);
    }
    // count how many items of a specific type are in the list
    count(template) {
      if (typeof(template)!=="string" && template.template) {
        template = template.template;
      }
      for (let item of this) {
        if (item.template===template) {
          return item.n;
        }
      }
      return 0;
    }
    // check whether a specific item is in the list
    contains(item) {
      if (this.indexOf(item)!==-1) {
        return true;
      } else {
        return false;
      }
    }
    // return the first item of a given type in the list
    getItem(template) {
      if (typeof(template)!=="string" && template.template) {
        template = template.template;
      }
      for (let item of this) {
        if (item.template===template) {
          return item;
        }
      }
      return null;
    }
    // take n items of a specific type from the list
    take(template,n) {
      n = n || 1;
      if (typeof(template)!=="string" && template.template) {
        template = template.template;
      }
      for (let item of this) {
        if (item.template===template) {
          if (item.n>n) {
            item.n-=n;
            return HTomb.Things[template]({n: n});
          } else {
            return this.removeItem(item);
          }
        }
      }
    }
    // take multiple items of various types from the list
    takeItems(ingredients) {
      let arr = [];
      for (let ingredient in ingredients) {
        let n = ingredients[ingredient];
        arr.push(this.take(ingredient,n));
      }
      return arr;
    }
    // remove a specific item from the list
    removeItem(item) {
      let i = this.indexOf(item);
      super.splice(i,1);
      item.onlist = null;
      if (this.length===0) {
        if (this.heldby===HTomb.World.items) {
          for (let coord in HTomb.World.items) {
            if (HTomb.World.items[coord]===this) {
              delete HTomb.World.items[coord];
              break;
            }
          }
        }
      }
      return item;
    }
    // check whether multiple items of various types are on the list
    hasAll (ingredients) {
      for (let ingredient in ingredients) {
        let n = ingredients[ingredient];
        // if we lack what we need, search for items
        if (this.count(ingredient)<n) {
          return false;
        }
      }
      return true;
    }
    // describe the list
    describe(args) {
      args = args || args;
      let mesg = "";
      for (let i = 0; i<this.length; i++) {
        if (i>0) {
          mesg+=" ";
        }
        mesg+=this[i].describe({article: "indefinite"});
        if (i===this.length-2) {
          mesg = mesg + ", and";
        } else if (i<this.length-1) {
          mesg = mesg + ",";
        }
      }
      return mesg;
    }
    // return the first item on the list
    head() {
      return this[0];
    }
    // return the last item on teh list
    tail() {
      return this[this.length-1];
    }
    // block many methods that normally modify arrays
    pop() {}
    shift() {}
    copyWithin() {}
    concat() {}
    fill() {}
    join() {}
    slice() {}
    splice() {}
  }
  // Expose the constructor
  HTomb.Things.Items = function(heldby) {
    return new Items(heldby);
  };

return HTomb;
})(HTomb);
