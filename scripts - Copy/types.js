HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;

  var type = {
  	template: "Type",
  	name: "type",
    stringify: function() {
      return {"Type" : this.template};
    },
  	//parse: function(value) {
    //  console.log(["type is",this]);
    //  return HTomb.Types.templates[this.parent].types[value];
    //},
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
  	// Add to the list of templates
  	HTomb.Types.templates[args.template] = t;
    if (args.parent==="Type") {
      // be ready to handle enumeration
      HTomb.Types.templates[args.template].types = [];
    }
  	// Don't fire onDefine for the top-level thing
  	if (t.onDefine && args.parent!=="Type") {
  		t.onDefine(args);
  	}
  };


  return HTomb;
})(HTomb);
