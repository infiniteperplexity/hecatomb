HTomb = (function(HTomb) {
  "use strict";
  var coord = HTomb.Utils.coord;

  var thing = {
    template: "Thing",
    maxSpawnId: -1,
    spawnIds: {},
    spawnId: -1,
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
      this.spawnId = HTomb.Things.templates.Thing.maxSpawnId+1;
      HTomb.Things.templates.Thing.maxSpawnId = this.spawnId;
      HTomb.Things.templates.Thing.spawnIds[this.spawnId] = this;
    },
    spawn: function(args) {
      HTomb.Debug.logEvent("spawn",this);
      // Add to the global things table
      HTomb.World.things.push(this);
      this.acquireSpawnId();
      if (this.onSpawn) {
        this.onSpawn(args);
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
    //get thingId () {
      // Calculate thingId dynamically
    //  return HTomb.World.things.indexOf(this);
    //},
    //set thingId (arg) {
      // not allowed
    //  HTomb.Debug.pushMessage("Not allowed to set thingId");
    //},
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

      // need to handle vowels somehow
      for (let pre of prefixes) {
        name = pre + " " + name;
      }
      for (let post of postfixes) {
        name = name + " " + post;
      }

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
      if (spawnId) {
        name = name + " with spawnId=" + this.spawnId;
      }
      return name;
    },
    // Describe for an in-game list
    onList: function() {
      return this.describe();
    },
    //details: function() {
    //  return ["This is " + this.describe() + "."];
    //},
    extend: function(args) {
      let child = Object.create(this);
      child = Object.assign(child, args);
      child.parent = this.template;
      if (this.hasOwnProperty("children")===false) {
        this.children = [];
      }
      this.children.push(child);
      HTomb.Things.templates[args.template] = child;
      HTomb.Things[args.template] = function(args2) {
        let o = Object.create(child);
        // !!!should this be a deep merge instead of assign?
        o = Object.assign(o, args2);
        o.spawn();
        if (o.onCreate) {
          return o.onCreate(args2);
        }
        return o;
      };
      //!!!I feel like this is bad news but let's keep it for now...
      //!!!it doesn't know how to climb the prototype chain
      if (child.onDefine) {
        child.onDefine(args);
      }
      return child;
    }
  };
  // The global list of known templates
  HTomb.Things.templates = {Thing: thing};

  // 
return HTomb;
})(HTomb);
