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
        }
      } else if (this.x!==null && this.y!==null && this.z!==null && this.x!==undefined && this.y!==undefined && this.z!==undefined) {
        label+= " at " + this.x + ", " + this.y + ", " + this.z;
      }
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
  HTomb.Entities[creature.name] = function() {
    return new /////something or other....losing steam....
  }
}

class Behavior extends Thing {

}

class Type {

}


let foo = {a: 1, b: 2};
Object.seal(foo);
let bar = Object.create(foo);
bar.c = 3