HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;

  let type = {
    template: "Type",
    name: "type",
    plural: null,
    Behaviors: {},
    stringify: function() {
      return {"Type" : this.template};
    },
    describe: function() {
      return this.name;
    },
    // "key" is necessary just because this is a JavaScript parser
    parse: function(key, value) {
      return HTomb.Types[this.template][value];
    },
    extend: function(args) {
      if (args===undefined || args.template===undefined) {
        HTomb.Debug.pushMessage("invalid template definition");
        return;
      }
      let child = Object.create(this);
      // I think this is safe because we never modify Types after creation
      child = Object.assign(child, args);
      child.parent = this;
      HTomb.Types[child.template] = child;
      // ready the pluralized name
      if (args.plural) {
        child.plural = args.plural;
      } else {
        child.plural = child.template + "s";
      }
      // add to the registry of types
      if (this!==type) {
        if (!HTomb[this.plural]) {
          HTomb[this.plural] = {};
        }
        HTomb[this.plural][args.template] = child;
        if (this.hasOwnProperty("types")===false) {
          this.types = [];
        }
        this.types.push(child);
      }
      for (let b in child.Behaviors) {
        if (!HTomb.Things[b]) {
          console.log(b);
        }
        let defaults = HTomb.Things[b];
        if (defaults.nospawn!==true) {
          throw Error("Can't add a spawnable behavior to a Type.");
        }
        let bargs = child.Behaviors[b];
        for (let key in bargs) {
          if (bargs.hasOwnProperty(key)) {
            if (bargs[key]!==defaults[key]) {
              bargs[key] = HTomb.Utils.merge(defaults[key],bargs[key]);
            }
          }
        }
        bargs.Template = child;
        let beh = HTomb.Things[b].prespawn(bargs);
        child[beh.name] = beh;
      }
      if (child.onDefine) {
        child.onDefine(args);
      }
      return child;
    }
  }
  // The global list of known templates
  HTomb.Types = {Type: type};

  return HTomb;
})(HTomb);
