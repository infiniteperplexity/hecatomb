

Behaviors.extend({
  template: "Craftable",
  name: "craftable",
  labor: 10,
  ingredients: {},
  dismantle: function() {

  }
});

Behaviors.extend({
  template: "Fixture",
  name: "fixture",
  unplacedFg: null,
  unplacedSymbol: null,
  makeLabor: 10,
  placeLabor: 5,
  ingredients: {},
  dismantle: function() {}
});

Items.extend({
  template: "UnplacedFixture",
  name: "unplaced fixture",
  symbol: "\u25AB",
  fg: "#BB9922",
  makes: null,
  labor: 5
});

Behaviors.extend({
  template: "Harvestable",
  name: "harvestable",
  yields: {},
  harvest: function() {

  }
})




// the initial seed
Math.seed = 6;
 
// in order to work 'Math.seed' must NOT be undefined,
// so in any case, you HAVE to provide a Math.seed
Math.seededRandom = 

HTomb.Utils.seed = 6;






  let type = {
    template: "Type",
    name: "type",
    plural: null,
    stringify: function() {
      return {"Type" : this.template};
    },
    describe: function() {
      return this.name;
    }
  };
  // The global list of known templates
  HTomb.Types.templates = {Type: type};

  // define a template for creating things
  HTomb.Types.define = function(args) {
    if (args===undefined || args.template===undefined) {
      HTomb.Debug.pushMessage("invalid template definition");
      return;
    }
    // default parent is Type
    var t;
    if (args.parent===undefined || (args.parent!=="Type" && HTomb.Types.templates[args.parent]===undefined)) {
      args.parent = "Type";
    }
    if (args.parent==="Type") {
      t = Object.create(type);
      // make an object to hold shortcuts
      if (args.plural===undefined) {
        args.plural = args.template+"s";
      }

      if (HTomb[args.plural]===undefined) {
        HTomb[args.plural] = {};
      }
      // Create a new shortcut function for defining templates within the Type
      HTomb.Types["define" + args.template] = function(opts) {
        opts.parent = args.template;
        return HTomb.Types.define(opts);
      };
      // Create a parsing function for the type
      HTomb.Types["parse" + args.template] = function(key,value) {
        if (typeof(value)==="number") {
          return HTomb.Types.templates[args.template].types[value];
        } else {
          return value;
        }
      }
    } else {
      // define a concrete type
      t = Object.create(HTomb.Types.templates[args.parent]);
      // create an enum-style construct
      var plural = HTomb.Types.templates[args.parent].plural;
      if (HTomb[plural]===undefined) {
        console.log([args.parent,args.template]);
      }
      HTomb[plural][args.template] = t;
      HTomb.Types.templates[args.parent].types.push(t);
      // experiment with adding a parser for the types
    }
    // Add the arguments to the template
    for (var arg in args) {
      t[arg] = args[arg];
    }
    // add to the list of templates
    HTomb.Types.templates[args.template] = t;
    if (args.parent==="Type") {
      // be ready to handle enumeration
      HTomb.Types.templates[args.template].types = [];
    }
    // create a parsing method
    HTomb.Types["parse" + args.template] = function(key,value) {
      if (typeof(value)==="number") {
        return HTomb.Types.templates[args.template].types[value];
      } else {
        return value;
      }
    }
  };




  let mixin = {
    template: "Mixin",
    name: "mixin",
    onAdd: function(thing, args) {},
    extend: function(args) {
      if (args===undefined || args.template===undefined) {
        HTomb.Debug.pushMessage("invalid template definition");
        return;
      }
      let child = Object.create(this);
      // ready the pluralized name
      child = Object.assign(child, args);
      child.parent = this;
    }
  };