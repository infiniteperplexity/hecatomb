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
      if (this.behaviors) {
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
    // Describe for an in-game list
    onList: function() {
      return this.describe();
    },
    //details: function() {
    //  return ["This is " + this.describe() + "."];
    //},
    extend: function(args) {
      let child = Object.create(this);
      child.parent = this.template;
      child = Object.assign(child, args);
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

  // define a template for creating things
  HTomb.Things.define = function(args) {
    if (args===undefined || args.template===undefined) {
      //HTomb.Debug.pushMessage("invalid template definition");
      return;
    }
    // Create based on generic thing
    var t;
    if (args.parent===undefined || (args.parent!=="Thing" && HTomb.Things.templates[args.parent]===undefined)) {
      args.parent = "Thing";
    }

    t = Object.create(HTomb.Things.templates[args.parent]);

    // Create a new function...maybe not the best way to do this
    HTomb.Things["define" + args.template] = function(opts) {
      opts.parent = opts.parent || args.template;
      return HTomb.Things.define(opts);
    };
    HTomb.Things[args.template] = function(opts) {
      // Create a shortcut function to create it
      return HTomb.Things.create(args.template, opts);
    };
    // Add the arguments to the template
    for (var arg in args) {
      t[arg] = args[arg];
    }
    // Add to the list of templates
    HTomb.Things.templates[args.template] = t;
    // Don't fire onDefine for the top-level thing
    if (t.onDefine && args.parent!=="Thing") {
      t.onDefine(args);
    }
    return t;
  };


  // Create a new object based on the template
  HTomb.Things.create = function(template, args) {
    if (HTomb.Things.templates[template]===undefined) {
      console.log([template,args]);
    }
    var t = Object.create(HTomb.Things.templates[template]);
    // Copy the arguments onto the thing
    // here's where we went wrong...
    for (var arg in args) {
      t[arg] = args[arg];
    }
    // Do all "on spawn" tasks
    t.spawn();
    //t.acquireSpawnId();
    if (t.onCreate) {
      return t.onCreate(args);
    }
    // return the thing
    return t;
  };

  HTomb.Things.defineByProxy = function(child, parnt) {
    // This has only ever been tested for Behavior/Entity relationships
    let ent = HTomb.Things.templates[parnt];
    let beh = HTomb.Things.templates[child];
    HTomb.Things["define"+child] = function(args) {
      args = args || {};
      let chld = {};
      let parent = HTomb.Things.templates[args.parent] || {};
      // I think this next bit is only for subtypes, like Seed, et cetera
      args.Behaviors = args.Behaviors || {};
      if (parent.Behaviors) {
        for (let arg in parent.Behaviors) {
          args.Behaviors[arg] = HTomb.Utils.copy(parent.Behaviors[arg]);
        }
      }
      // This code makes sure the right arguments get passed to the parent and child
      let eargs = {};
      let onDefine = null;
      for (let arg in args) {
        if (arg==="onDefine") {
          onDefine = args[arg];
        } else if (arg.substr(0,2)==="on" && (arg.substr(2,1)===arg.substr(2,1).toUpperCase())) {
          if (ent[arg]) {
            let func1 = ent[arg];
            let func2 = args[arg];
            eargs[arg] = function() {
              func1.apply(this,arguments);
              return func2.apply(this,arguments);
            };
          } else {
            eargs[arg] = args[arg];
          }
        } else if (ent[arg]!==undefined) {
          eargs[arg] = args[arg];
        } else if (beh[arg]!==undefined) {
          chld[arg] = args[arg];
        } else {
          eargs[arg] = args[arg];
        }
      }
      // I think this is for subtypes again
      if (eargs.Behaviors[child]) {
        for (let arg in chld) {
          eargs.Behaviors[child][arg] = HTomb.Utils.copy(chld[arg]);
        }
      } else {
        eargs.Behaviors[child] = chld;
      }
      let newdef = HTomb.Things["define"+parnt](eargs);
      // Make sure that onDefine works at the level it is supposed to
      if (beh.hasOwnProperty("onDefine")) {
        beh.onDefine(eargs);
      }
      // make the child onDefine kick in after the parent and behavioral onDefines
      if (onDefine) {
        onDefine.call(newdef);
      }
      let create = HTomb.Things[args.template];
      HTomb.Things[args.template] = function(crargs) {
        crargs = crargs || {};
        crargs.Behaviors = crargs.Behaviors || {};
        let par = HTomb.Things.templates[args.template];
        if (par.Behaviors) {
          for (let arg in par.Behaviors) {
            crargs.Behaviors[arg] = HTomb.Utils.copy(par.Behaviors[arg]);
          }
        }
        let entargs = {};
        let bargs = {};
        for (let arg in crargs) {
          if (typeof(crargs[arg])==="function") {
            entargs[arg] = crargs[arg];
          } else if (ent[arg]!==undefined) {
            entargs[arg] = crargs[arg];
          } else if (beh[arg]!==undefined) {
            bargs[arg] = crargs[arg];
          } else {
            entargs[arg] = crargs[arg];
          }
        }
        for (let arg in bargs) {
          entargs.Behaviors[child][arg] = bargs[arg];
        }
        return create(entargs);
      };
      return newdef;
    };
    // Make sure that onDefine does not override its parent
    if (beh.hasOwnProperty("onDefine") && HTomb.Things.templates[beh.parent].hasOwnProperty("onDefine")) {
      HTomb.Things.templates[beh.parent].onDefine(beh);
    }
    return beh;
  };


return HTomb;
})(HTomb);
