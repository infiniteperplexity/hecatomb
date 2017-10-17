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
    Trackers: [],
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
      // either create the registry of types, or add to it
      if (this===type) {
        // ready the pluralized name
        if (args.plural) {
          child.plural = args.plural;
        } else {
          child.plural = child.template + "s";
        }
        HTomb[child.plural] = {};
        child.types = [];
      } else {
        HTomb[this.plural][args.template] = child;
        // create an enum-like construct
        // I don't like this syntax
        HTomb.Types[this.template].types.push(child);
      }
      if (child.onDefine) {
        child.onDefine(args);
      }
      //!!!!Anything about Mixins here?
      return child;
    }
  }
  // The global list of known templates
  HTomb.Types = {Type: type};

  return HTomb;
})(HTomb);
