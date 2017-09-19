<<<<<<< HEAD
//conventions...
//....an items member should always be called this.items
//...an item should call the items object it belongs to this.container
class ItemList extends Array {
  push: function(item) {
    // does this properly handle full stacks
    if (item.container) {
      item.container.remove(item);
    }
    let n = Math.min(item.n, this.canFit(item));
    let exists = false;
    for (let it of this) {
      if (it.name===item.name) {
        it.n+=n;
        exists = true;
        break;
      }
    }
    if (exists) {
      item.n-=n;
      if (item.n===0) {
        item.despawn();
        item = null;
      }
    } else {
      this.push(item);
      item.container = this;
      return null;
    }
  }
  canFit: function(item) {
    // for now
    return true;
  }

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
=======
HTomb.World.things = {};
class Thing {
  constructor(args) {
    this.label = null;
    this.thingId = null;
  }
  static resetIds: function() {
    let i = 0;
    let things = Object.values(HTomb.World.things);
    HTomb.World.things = {};
    for (let thing of things) {
      thing.acquireId();
    }
  }
  get name: function() {
    return this.constructor.name;
  }
  acquireId: function() {
    this.thingId = (Math.max(Object.keys(HTomb.World.things)) || 0)+1;
    HTomb.World.things[this.thingId] = this;
  }
  spawn: function() {
    if (this.onSpawn) {
      this.onSpawn();
    }
  }
  despawn: function() {
    if (HTomb.World.things[this.thingId]) {
      delete HTomb.World.things[this.thingId];
    }
    HTomb.Events.unsubscribeAll(this);
    if (this.onDespawn) {
      this.onDespawn();
    }
  }
  //isSpawned: function() {}
  describe: function(options) {
    options = options || {};
    options.label = this.label || "(nameless)";
    let article = options.article || "none";
    if (article==="indefinite" && this.definiteArticle===true) {
      article = "definite";
    }
    let capitalized = options.capitalized || false;
    let plural = options.plural || this.plural || false;
    let possessive = options.possessive || false;
    let beginsWithVowel = this.beginsWithVowel || undefined;
    let properNoun = this.properNoun || false;
    let irregularPlural = this.irregularPlural || false;
    let atCoordinates = options.atCoordinates || false;
    let label = options.label;
    let override = options.override || function(s) {return s;}
    if (plural && irregularPlural) {
      label = irregularPlural;
    } else if (plural && this.plural) {
      label = label;
    } else if (plural) {
      let l = label.length;
      if (label[l-1]==="s" || label[l-1]==="x" || label[l-1]==="s" || label[l-1]==="z" || (
        label[l-1]==="h" && (label[l-2]==="s" || label[l-2]==="c")
      )) {
        label+="e";
      }
      label+="s";
    }
    if (possessive) {
      let l = label.length;
      if (label[l-1]==="s") {
        label+="'";
      } else {
        label+="'s";
      }
    }
    //proper nouns not yet implemented
    if (article==="indefinite") {
      if (plural) {
        // either do nothing or use "some"?
        //label = "some " + label;
      } else
      // e.g. beginsWithVowel is explicitly false for a "unicorn"
      if (beginsWithVowel===true || (beginsWithVowel!==false &&
        (label[0]==="a" || label[0]==="e" || label[0]==="i" || label[0]==="o" || label[0]==="u"
          || label[0]==="A" || label[0]==="E" || label[0]==="I" || label[0]==="O" || label[0]==="U"))) {
        label = "an " + label;
      } else {
        label = "a " + label;
      }
    } else if (article==="definite") {
      label = "the " + label;
    } else if (article!=="none") {
      label = article + " " + label;
    }
    if (capitalized) {
      label = label.substr(0,1).toUpperCase() + label.substr(1);
    }
    if (atCoordinates) {
      if (this.entity) {
        let e = this.entity;
        if (e.x!==null && e.y!==null && e.z!==null && e.x!==undefined && e.y!==undefined && e.z!==undefined) {
          label+= " at " + e.x + ", " + e.y + ", " + e.z;
>>>>>>> f96df913d8628a7bfff351702188e286cd1342cd
        }
      } else if (this.x!==null && this.y!==null && this.z!==null && this.x!==undefined && this.y!==undefined && this.z!==undefined) {
        label+= " at " + this.x + ", " + this.y + ", " + this.z;
      }
<<<<<<< HEAD
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
=======
    }
    label = override(label);
    return label;
  }
}


class Entity extends Thing {
  constructor(args) {
    super(args);
    this.label = "entity";
    this.x = null;
    this.y = null;
    this.z = null;
    this.behaviors = [];
  }
  place(x,y,z) {
    if (this.isPlaced()) {
      this.remove();
    }
    this.x = x;
    this.y = y;
    this.z = z;
    if (this.onPlace) {
      this.onPlace(x,y,z);
    }
    for (let b in this.behaviors) {
      if (b.onPlace) {
        b.onPlace(x,y,z);
      }
    }
  }
  remove() {
    for (let b in this.behaviors) {
      if (b.onRemove) {
        b.onRemove();
      }
    }
    if (this.onRemove) {
      this.onRemove();
    }
  }
  onDespawn() {
    if (this.isPlaced()) {
      this.remove();
    }
    for (let b of this.behaviors) {
      if (b.onDespawn) {
        b.onDespawn();
      }
    }
  }
  isPlaced() {
    if (this.x===null || this.y===null || this.z===null) {
      return false;
    } else {
      return true;
    }
  }
}

class Creature extends Entity {

}

HTomb.Entities.Creature = function(args) {
  args = args || {};
  let creature = new Creature(args);
  if (args.onDefine) {
    args.onDefine.call(creature, args);
  }
  Object.seal(creature);
  HTomb.Entities[creature.name] = function() {
    return new /////something or other....losing steam....
  }
};

class Behavior extends Thing {
  constructor(args) {
    super(args);
    this.label = "behavior";
    this.entity = null;
  }
  addToEntity(entity) {
    this.entity = entity;
    this.entity[this.label] = this;
  }
}

HTomb.Things.Behavior = function(args) {
  args = args || {};
  let behavior = new Behavior(args);
  if (args.onDefine) {
    args.onDefine.call(behavior, args);
  }
  Object.seal(behavior);
  HTomb.Behaviors[behavior.name] = function() {};
};

HTomb.Utils.


