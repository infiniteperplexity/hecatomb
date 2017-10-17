HTomb = (function(HTomb) {
  "use strict";
  var coord = HTomb.Utils.coord;

  var thing = {
    template: "Thing",
    maxSpawnId: -1,
    spawnIds: {},
    spawnId: -1,
    Mixins: {},
    behaviors: [],
    resetSpawnIds: function() {
      this.spawnIds = {};
      this.maxSpawnId = -1;
      this.spawnId = -1;
      let things = HTomb.World.things;
      for (let i=0; i<things.length; i++) {
        things[i].acquireSpawnId();
      }
    },
    acquireSpawnId: function() {
      this.spawnId = HTomb.Things.Thing.maxSpawnId+1;
      HTomb.Things.Thing.maxSpawnId = this.spawnId;
      HTomb.Things.Thing.spawnIds[this.spawnId] = this;
    },
    spawn: function(args) {
      args = args || {};
      let o = Object.create(this);
      for (let a in args) {
        // if the thing has complex, instantiated properties, copy them manually
        if (args[a]!==this[a]) {
          o[a] = HTomb.Utils.merge(this[a],args[a]);
        }
      }
      // could instantiate all the arrays...good or bad idea?
      HTomb.Debug.logEvent("spawn",o);
      // Add to the global things table
      HTomb.World.things.push(o);
      o.acquireSpawnId();
      if (o.onSpawn) {
        o.onSpawn(args);
      }
      return o;
    },
    isSpawned: function() {
      if (HTomb.World.things.indexOf(this)===-1) {
        return false;
      } else {
        return true;
      }
    },
    despawn: function() {
      HTomb.Debug.logEvent("despawn",this);
    // remove from the global things table
      let i = HTomb.World.things.indexOf(this);
      if (i!==-1) {
        HTomb.World.things.splice(i,1);
      }
      HTomb.Events.unsubscribeAll(this);
      if (this.onDespawn) {
        this.onDespawn();
      }
    },
    // Describe for an in-game message
    describe: function(options) {
      options = options || {};
      options.name = this.name || "(nameless)";
      // behaviors can augment or alter the description via options
      let nobehaviors = options.nobehaviors || false;
      if (this.onDescribe) {
        options = this.onDescribe(options);
      }
      if (this.behaviors && !nobehaviors) {
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
      let spawnId = options.spawnId || false;
      let prefixes = options.prefixes || [];
      let postfixes = options.postfixes || [];

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
      // postfixes come after pluralization
      for (let post of postfixes) {
        if (!possessive) {
          name = name + " " + post;
        }
      }
      // prefixes come before vowel check
      for (let pre of prefixes) {
        name = pre + " " + name;
      }
      //proper nouns not yet implemented
      if (article==="indefinite") {
        if (plural) {
          // either do nothing or use "some"?
          //name = "some " + name;
        } else
        // e.g. beginsWithVowel is explicitly false for a "unicorn"
        //!!!wait, do we also need to handle begins with Y vowel stuff?
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
      if (spawnId) {
        name = name + " with spawnId=" + this.spawnId;
      }
      return name;
    },
    extend: function(args) {
      let child = Object.create(this);
      // Object.assign should be fine because templates do not get changed after the fact
      child = Object.assign(child, args);
      child.parent = this.template;
      if (this.hasOwnProperty("children")===false) {
        this.children = [];
      }
      this.children.push(child);
      HTomb.Things[args.template] = child;
      //!!!I feel like this is bad news but let's keep it for now...
      //!!!it doesn't know how to climb the prototype chain
      if (child.onDefine) {
        child.onDefine(args);
      }
      // add mixins to template
      for (let m in child.Mixins) {
        let defaults = HTomb.Mixins[m];
        let margs = child.Mixins[m];
        // the way these arguments nest and inherit is pretty subtle...
        for (let key in margs) {
          if (margs.hasOwnProperty(key)) {
            if (margs[key]!==defaults[key]) {
              margs[key] = HTomb.Utils.merge(defaults[key],margs[key]);
            }
          }
        }
        let mixin = HTomb.Mixins[m].create(margs);
        mixin.addToTemplate(child);
      }
      child.behaviors = [];
      return child;
    }
  };
  // The global list of known templates
  HTomb.Things.Thing = thing;

  let Tracker = thing.extend({
    template: "Tracker",
    name: "tracker",
    listens: [],
    resetAll: function(args) {
      for (let tracker in HTomb.World.trackers) {
        HTomb.World.trackers[tracker] = null;
        HTomb.Things[tracker].spawn();
      }
    },
    spawn: function(args) {
      // Trackers are singletons
      if (HTomb.World.trackers[this.template]) {
        return HTomb.World.trackers[this.template];
      }
      let o = thing.spawn.call(this, args);
      for (let type of o.listens) {
        HTomb.Events.subscribe(o, type);
      }
      o.track();
      return o;
    },
    track: function() {
      HTomb.World.trackers[this.template] = this;
    },
    despawn: function(args) {
      HTomb.World.trackers[this.template] = null;
      thing.despawn.call(this,args);
    },
    extend: function(args) {
      let t = HTomb.Things.Thing.extend.call(this, args);
      HTomb.World.trackers[t.template] = null;
    }
  });

  Tracker.extend({
    template: "AngryNatureTracker",
    name: "angry nature tracker",
    listens: ["Destroy"],
    trees: 0,
    shrubs: 0,
    grass: 0,
    onDestroy: function(event) {
      let e = event.entity;
      if (e.template==="Tree") {
        this.trees+=1;
      } else if (e.template==="Shrub") {
        this.shrubs+=1;
      } else if (e.tempalte==="Grass") {
        this.grass+=1;
      }
    }
  });
  HTomb.Things.AngryNatureTracker.spawn();

  // Mixins impersonate Behaviors, but they hang out on the template not the entity?
  // This sounds like a disaster waiting to happen
  let mixin = {
    template: "Mixin",
    name: "mixin",
    // we gotta be careful about layers of nesting and value/reference stuff
    addToTemplate: function(template, args) {
      if (Object.hasOwnProperty(template,"mixins")===false) {
        template.mixins = [];
      }
      template.mixins.push(this);
      template[this.name] = this;
      if (this.onAdd) {
        this.onAdd(template, args);
      }
    },
    create: function(args) {
      let o = Object.create(this);
      // is this really how I want to do it?
      o = Object.assign(o, args);
      return o;
    },
    extend: function(args) {
      if (args===undefined || args.template===undefined) {
        HTomb.Debug.pushMessage("invalid template definition");
        return;
      }
      let child = Object.create(this);
      // ready the pluralized name
      child = Object.assign(child, args);
      child.parent = this.template;
      HTomb.Mixins[child.template] = child;
      return child;
    }
  };

  HTomb.Mixins = {Mixin: mixin};

  // mixin.extend({
  //   template: "Testable",
  //   name: "testable",
  //   foo: 1,
  //   bar: {baz: 2},
  //   f: function() {console.log(3);}
  // });

return HTomb;
})(HTomb);
