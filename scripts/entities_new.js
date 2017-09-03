HTomb = (function(HTomb) {
	"use strict";
	let coord = HTomb.Utils.coord;



	class Things extends Array {
	 reset: function() {}
	}
	HTomb.Things = Things;
	HTomb.Templates = {};


    // Most concrete game objects in Hecatomb are "Things", with the following properties:
    // - They inherit from an abstract "Template"
    // - It has a "template" naming its Template, a "name" that describes it, and "describe" function that formats its name.
    // - When an instance is "spawned", it registers a unique "spawnId", and it is also placed in HTomb.World.things
    // - They can be stringified in a customized way
    // - Derived objects listen for certain method calls, such as "spawn", and respond by hiearchically calling listener methods
    //   (for example, Zombie.spawn() triggers Entity.onSpawn(), Creature.onSpawn(), and Zombie.onSpawn()
    // - Thus far, there are two or three types of Things: Entities, Behaviors, and possibly Spells
  	let thing = {
	  	template: "Thing",
	  	name: "thing",
	  	spawn: function() {
	  		HTomb.World.things.push(this);
	  		this.acquireThingId();
	  		let hierach = [];
	  		let prot = this;
	  		do {
	  			hierarch.unshift(prot);
	  			prot = prot.__proto__;
	  		} while (prot!==thing.__proto__);
	  		for (proto of hierarch) {
	  			if (proto.onSpawn) {
	  				proto.onSpawn();
	  			}
	  		}
	  	},
	  	isSpawned: function() {
	      if (HTomb.World.things.indexOf(this)===-1) {
	        return false;
	      } else {
	        return true;
	      }
	    },
	    despawn: function() {
	      let i = HTomb.World.things.indexOf(this);
	      if (i!==-1) {
	        HTomb.World.things.splice(i,1);
	      }
	      this.releaseThingId();
	      HTomb.Events.unsubscribeAll(this);
	      if (this.onDespawn) {
	        this.onDespawn();
	      }
	    },
	  	acquireThingId: function() {
	  		for (thing of HTomb.Things) {
	  			if ()
	  		}
	  		let i=0;
	  		while (i<HTomb.World.Things) {
	  			if ()
	  		}

	  		if (HTomb.Things.releasedIds.length>0) {
	  			this.thingId = HTomb.Things.releasedIds.shift();
	  		} else {
	  			this.thingId = HTomb.Things.maxThingId+1;
	  			HTomb.Things.maxThingId = this.thingId;
	  		}
	  	}
	  	releaseThingId: function() {
	  		if (this.thingId===HTomb.Things.maxThingId) {
	  			HTomb.Things.maxThingId-=1;
	  		} else {
	  			HTomb.Things.releasedIds.push(this.thingId);
	  			HTomb.Things.releasedIds.sort();
	  		}
	  		this.thingId = null;
	  	},
	  	describe: function(options) {
	      options = options || {};
	      options.name = this.name || "(nameless)";
	      // behaviors can augment or alter the description via options
	      if (this.Behaviors) {
	        this.validateBehaviors();
	        let beh = this.behaviors;
	        for (let i=0; i<beh.length; i++) {
	          if (beh[i].onDescribe) {
	            options = beh[i].onDescribe(options);
	          }
	        }
	      }

	      let article = options.article || "none";
	      // things like the player will always have definite articles, right?
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
	      let name = options.name;

	      let override = options.override || function(s) {return s;}

	      if (plural && irregularPlural) {
	        name = irregularPlural;
	      } else if (plural && this.plural) {
	        name = name;
	      } else if (plural) {
	        let l = name.length;
	        if (name[l-1]==="s" || name[l-1]==="x" || name[l-1]==="s" || name[l-1]==="z" || (
	          name[l-1]==="h" && (name[l-2]==="s" || name[l-2]==="c")
	        )) {
	          name+="e";
	        }
	        name+="s";
	      }
	      if (possessive) {
	        let l = name.length;
	        if (name[l-1]==="s") {
	          name+="'";
	        } else {
	          name+="'s";
	        }
	      }
	      //proper nouns not yet implemented
	      if (article==="indefinite") {
	        if (plural) {
	          // either do nothing or use "some"?
	          //name = "some " + name;
	        } else
	        // e.g. beginsWithVowel is explicitly false for a "unicorn"
	        if (beginsWithVowel===true || (beginsWithVowel!==false &&
	          (name[0]==="a" || name[0]==="e" || name[0]==="i" || name[0]==="o" || name[0]==="u"
	            || name[0]==="A" || name[0]==="E" || name[0]==="I" || name[0]==="O" || name[0]==="U"))) {
	          name = "an " + name;
	        } else {
	          name = "a " + name;
	        }
	      } else if (article==="definite") {
	        name = "the " + name;
	      } else if (article!=="none") {
	        name = article + " " + name;
	      }
	      if (capitalized) {
	        name = name.substr(0,1).toUpperCase() + name.substr(1);
	      }
	      if (atCoordinates) {
	        if (this.entity) {
	          let e = this.entity;
	          if (e.x!==null && e.y!==null && e.z!==null && e.x!==undefined && e.y!==undefined && e.z!==undefined) {
	            name+= " at " + e.x + ", " + e.y + ", " + e.z;
	          }
	        } else if (this.x!==null && this.y!==null && this.z!==null && this.x!==undefined && this.y!==undefined && this.z!==undefined) {
	          name+= " at " + this.x + ", " + this.y + ", " + this.z;
	        }
	      }
	      name = override(name);
	      return name;
	    },
	};

  // Entities are Things that have specific locations in the game world
  // They have x, y, and z coordinates, and "place" and "remove" methods
  // Thus far, there are creatures, features, items, tasks, and structures
  // - Each of these behaves slightly differently when "placed" and appears differently in the game interface
  // - A single entity can only be one of these, though you could have two conceptually linked entities to represent a game object with dual types  
  // They can have additional properties called "Behaviors" that are Things in themselves
  let entity = Object.create(thing);
  entity.template = "Entity";
  entity.name = "entity";
  entity.x = entity.y = entity.z = null;
  entity.place = function(x,y,z) {};
  entity.isPlaced = function() {return (x!==null && y!==null && z!==null);}
  entity.remove = function() {};
  entity.onSpawn = function() {
  	//this.behaviors = {};
  	for (behave in this.Behaviors) {
  		let args = this.Behaviors[behave];
  		let b = HTomb.Behaviors[behave](args);
  		b.addToEntity(this);
  	}
  };
  entity.onDespawn() {
  	if (this.isPlaced) {
  		this.remove();
  	}
  	for (beh of this.behaviors) {
  		beh.despawn();
  	}
  };

  // Creatures are limited to one per tile.
  let creature = Object.create(entity);
  // Features are limited to one per tile.
  let feature = Object.create(entity);
  // Multiple items can exist in one square, and they have special logic for stacking.
  // They are stored in special Array-derived objects called Items.
  let item = Object.create(entity);
  // Tasks are limited to one per tile and have complex behavior for assignment and labor.
  // Abstract tasks have logic for assignment.
  let task = Object.create(entity);
  // Structures do not take up space themselves, but are linked to one or more grouped features.
  let structure = Object.create(entity);
  // Behaviors add additional functionality to a specific Entity.
  // They listen for certain methods called on their entities, such as "spawn" or "place"
  // (It's a bit ambiguous how they can do this while also listening to those events themselves...)
  let behavior = Object.create(thing);
  behavior.addToEntity(entity) {
  	this.entity = entity;
  	entity[this.name] = this;
  	// entity.behaviors.push(this);
  	if (this.onAdd) {
  		this.onAdd(entity);
  	}
  }


  // Items is an Array-derived object for handling lists of items
  class Items extends Array {
  	constructor(heldby) {
  		super();
  		if (heldby!==undefined)
  		this.heldby = heldby;
  	}
  	push: function(item) {
	    if (item.heldby && item.heldby.items) {
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

class Items extends Array {
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
  

  

return HTomb;
})(HTomb);
