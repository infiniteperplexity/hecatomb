HTomb = (function(HTomb) {
  "use strict";
  let coord = HTomb.Utils.coord;
  
  let Entity = HTomb.Things.Entity;

  let Item = Entity.extend({
    template: "Item",
    name: "item",
    n: 1,
    maxn: 10,
    onlist: null,
    bulk: 10,
    value: 1,
    tags: [],
    stackSize: [1,0.7,0.4,0.3,0.1],
    owned: true,
    claimed: 0,
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
      this.claimed = 0;
      Entity.remove.call(this, args);
    },
    describe: function(options) {
      options = options || {};
      if (this.n>1) {
        options.plural = true;
        options.article = this.n;
      }
      if (this.claimed) {
        options.postfixes = options.postfixes || [];
        if (this.claimed===1 && this.n===1) {
          options.postfixes.push("(claimed)");
        } else {
          options.postfixes.push("(" + this.claimed + " claimed)");
        }
      }
      return Entity.describe.call(this,options);
    }
  });

  class Items extends Array {
    // normal array constructor arguments forbidden
    // argument is the "holder" of the item list
      // almost always either a component or the global item list
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
            return HTomb.Things[template].spawn({n: n});
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
    asIngredients() {
      let ingredients = {};
      for (let item of this) {
        ingredients[item.template] = item.n;
      }
      return ingredients;
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
