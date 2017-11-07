// The Events submodule, thus far unused, handles events and messaging
HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;

  // *********** String handling *******************
  HTomb.Utils.lineBreak = function(str,len) {
    let pat = /%c{[#\w]*}|%b{[#\w]*}/g;
    let match;
    let formats = [];
    while (match = pat.exec(str)) {
      formats[match.index] = match[0];
    }
    let clean = HTomb.Utils.cleanText(str);
    let tokens = clean.split(" ");
    let lines = [tokens[0]];
    tokens.splice(0,1);
    let i = 0;
    for (let word of tokens) {
      if (lines[i].length + 1 + word.length > len) {
        i+=1;
        lines.push(word);
      } else {   
        lines[i] = lines[i] + " " + word;
      }
    }
    let fgpat = /%c{[#\w]*}/g;
    let bgpat = /%b{[#\w]*}/g;
    let fg = "";
    let bg = "";
    // for each saved format...
    let linef = [];
    for (let f in formats) {
      let format = formats[f];
      // keep track of how many characters we have passed
      let tally = 0;
      let ind = f-tally;
      // iterate through the lines
      for (let j=0; j<lines.length; j++) {
        let line = lines[j];
        let len = lines[j].length;
        if (tally+lines[j].length>f) {
          // if the end of this line is after the format starts...
          if (tally<=f) {
            // ...but the format starts after the beginning of the line...
            line = lines[j].substr(0,ind-tally) + format + lines[j].substr(ind-tally);
            len = line.length;
          } else {
            // ...otherwise it affects the whole line.
            len = lines[j].length;
            line = lines[j];
            if (j===0) {
              len+=format.length;
            }
          }
          // replace with the altered line
          lines[j] = line;
          if (format.match(fgpat)) {
            fg = format;
          } else if (format.match(bgpat)) {
            bg = format;
          }
          linef[j+1] = fg+bg;
        }  
        tally+=len;
        tally+=1;
      }
    }
    for (let i=1; i<lines.length; i++) {
      lines[i] = linef[i] + lines[i];
    }
    return lines;
  };
  HTomb.Utils.cleanText = function(str) {
    let fgpat = /%c{[#\w]*}/g;
    let bgpat = /%b{[#\w]*}/g;
    return str.replace(fgpat,"").replace(bgpat,"");
  };
  HTomb.Utils.splitPropCase = function(s) {
    let caps = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    for (let i=0; i<s.length; i++) {
      if (caps.indexOf(s[i])!==-1) {
        s = s.substr(0,i-1)+" "+s.substr(i);
      }
    }
    return s;
  }


// ********************** Coordinates ******************
  HTomb.Utils.coord = function(x,y,z) {
    return z*LEVELH*LEVELW + x*LEVELH + y;
  }

  HTomb.Utils.decoord = function(c) {
    var x=0, y=0, z=0;
    while(c-LEVELH*LEVELW>=0) {
      c-=LEVELH*LEVELW;
      z+=1;
    }
    while(c-LEVELH>=0) {
      c-=LEVELH;
      x+=1;
    }
    y = parseInt(c);
    return [x,y,z];
  }

  // **************** RGB Manipulation ************
  HTomb.Utils.alphaHex = function(newc,oldc,alpha) {
    var combined = [];
    for (var i=0; i<3; i++) {
      combined[i] = alpha*newc[i]+(1-alpha)*oldc[i];
    }
    return combined;
  }

  HTomb.Utils.alphaString = function(newc,oldc,alpha) {
    var oldc = ROT.Color.fromString(oldc);
    var newc = ROT.Color.fromString(newc);
    var combined = [];
    for (var i=0; i<3; i++) {
      combined[i] = alpha*newc[i]+(1-alpha)*oldc[i];
    }
    combined = ROT.Color.toHex(combined);
    return combined;
  }

  // *********** Merging and copying *******************
  HTomb.Utils.merge = function(obj, newer) {
    //if one or the other is undefined, return the other
    if (obj===undefined) {
      return HTomb.Utils.copy(newer);
    } else if (newer===undefined) {
      return HTomb.Utils.copy(obj);
    } else if (newer===null) {
      return null;
    } else if (obj!==null && typeof(obj)!==typeof(newer)) {
      throw new Error("Malformed deep merge arguments.");
    } else if (obj===null) {
      return HTomb.Utils.copy(newer);
    } else if (typeof(obj)==="object") {
      let nobj;
      // for arrays, do not recursively merge, instead keep newer
      if (Array.isArray(newer)) {
        nobj = [];
        for (let i=0; i<newer.length; i++) {
          if (typeof(newer[i]!=="function")) {
            nobj[i] = HTomb.Utils.copy(newer[i]);
          }
        }
      } else if (Array.isArray(obj)) {
        nobj = [];
        for (let i=0; i<obj.length; i++) {
          if (typeof(nobj[i]!=="function")) {
            nobj[i] = HTomb.Utils.copy(obj[i]);
          }
        }
      // pass specialized objects by reference
      } else if (Object.getPrototypeOf(newer)!==Object.getPrototypeOf({})) {
        return newer;
      } else if (Object.getPrototypeOf(obj)!==Object.getPrototypeOf({})) {
        return obj;
      } else {
        let keys = [];
        for (let key of Object.keys(obj)) {
          if (obj.hasOwnProperty(key) && typeof(obj[key])!=="function") {
            keys.push(key);
          }
        }
        for (let key of Object.keys(newer)) {
          if (newer.hasOwnProperty(key) && keys.indexOf(key)===-1 && typeof(newer[key]!=="function")) {
            keys.push(key);
          }
        }
        nobj = {};
        for (let key of keys) {
          nobj[key] = HTomb.Utils.merge(obj[key],newer[key]);
        }
      }
      return nobj;
    } else {
      return HTomb.Utils.copy(newer);
    }
  };
  HTomb.Utils.coalesce = function() {
    let template = {};
    for (let i = 0; i<arguments.length; i++) {
      let args = arguments[i];
      for (let arg in args) {
        template[arg] = args[arg];
      }
    }
    return template;
  };

  // like clone but don't keep the prototype
  HTomb.Utils.copy = function(obj) {
    if (typeof(obj)==="object") {
      if (obj===null) {
        return null;
      }
      // recursively copy arrays and objects
      let nobj;
      if (Array.isArray(obj)) {
        nobj = [];
        for (let i=0; i<obj.length; i++) {
          // ignore functions
          if (typeof(nobj[i])!=="function") {
            nobj[i] = HTomb.Utils.copy(obj[i]);
          }
        }
      // pass complex objects by reference
      } else if (Object.getPrototypeOf(obj)!==Object.getPrototypeOf({})) {
        return obj;
      } else {
        nobj = {};
        for (let key in obj) {
          // ignore functions
          if (obj.hasOwnProperty(key)) {
            if (typeof(nobj[key])!=="function") {
              nobj[key] = HTomb.Utils.copy(obj[key]);
            }
          }
        }
      }
      return nobj;
    // pass primitives by value and functions by reference
    } else {
      return obj;
    }
  };
  // clone actually uses the same prototype
  HTomb.Utils.clone = function(obj) {
    if (typeof(obj)==="object") {
      if (obj===null) {
        return null;
      }
      // recursively copy arrays and objects
      let nobj = Object.create(obj);
      if (Array.isArray(obj)) {
        for (let i=0; i<obj.length; i++) {
          // ignore and inherit functions
          if (typeof(nobj[i])!=="function") {
            nobj[i] = HTomb.Utils.clone(obj[i]);
          }
        }
      // pass specialized objects by reference
      } else if (Object.getPrototypeOf(obj)!==Object.getPrototypeOf({})) {
        return obj;
      } else {
        for (let key in obj) {
          nobj[key] = HTomb.Utils.clone(obj[key]);
          // ignore and inherit functions
          if (typeof(nobj[key])!=="function") {
            nobj[key] = HTomb.Utils.clone(obj[key]);
          }
        }
      }
      return nobj;
    // pass primitives by value
    } else {
      return obj;
    }
  };

  
  // ***************** Array handling *********

  HTomb.Utils.multiarray = function() {
    let arr = [];
    let args = [...arguments];
    if (args.length<=1) {
      return arr;
    }
    let a = args.shift();
    for (let i=0; i<a; i++) {
      arr.push(HTomb.Utils.multiarray(...args));
    }
    return arr;
  };

  HTomb.Utils.shuffle = function(arr) {
    //Fisher-Yates
    var i = arr.length;
    if ( i == 0 ) return arr;
    while ( --i ) {
       var j = Math.floor( ROT.RNG.getUniform() * ( i + 1 ) );
       var tempi = arr[i];
       var tempj = arr[j];
       arr[i] = tempj;
       arr[j] = tempi;
     }
     return arr;
  };

  // ******* Oddballs ************************
  // this feels kind of misplaced but I'm not sure where it goes...
  HTomb.Utils.listIngredients = function(ingredients) {
    let keys = Object.keys(ingredients);
    if (keys.length===0) {
      return "";
    }
    let s = "($: ";
    for (let key of keys) {
      s+=ingredients[key];
      s+=" ";
      s+=HTomb.Things[key].name;
      if (key!==keys[keys.length-1]) {
        s+=", ";
      } else {
        s+=")";
      }
    }
    return s;
  };

  return HTomb;
})(HTomb);
