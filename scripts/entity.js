HTomb = (function(HTomb) {
  "use strict";
  let LEVELW = HTomb.Constants.LEVELW;
  let LEVELH = HTomb.Constants.LEVELH;
  var coord = HTomb.Utils.coord;
  
  let Thing = HTomb.Things.Thing;
  // Define a generic entity that occupies a tile space
  let Entity = Thing.extend({
    template: "Entity",
    name: "entity",
    x: null,
    y: null,
    z: null,
    unstaged: null,
    Components: {},
    place: function(x,y,z,args) {
      HTomb.Debug.logEvent("place",this);
      if (this.isPlaced()) {
        this.remove();
      }
      this.x = x;
      this.y = y;
      this.z = z;
      if (this.components) {
        var comp = this.components;
        for (var i=0; i<comp.length; i++) {
          if (comp[i].onPlace) {
             comp[i].onPlace(x,y,z,args);
          }
        }
      }
      // should this indeed go after the components?
      if (this.onPlace) {
        this.onPlace(x,y,z,args);
      }
      return this;
    },
    isPlaced: function() {
      if (this.x===null || this.y===null || this.z===null) {
        return false;
      } else {
        return true;
      }
    },
    findPlace: function(x0,y0,w,h, options) {
      options = options || {};
      let mask = options.mask || {};
      let callback = options.validPlace || this.validPlace.bind(this);
      let level = options.cavern || null;
      if (level===undefined) {
        console.trace();
        console.log("what's going on here?");
      }
      let valid = false;
      let x;
      let y;
      let z;
      let tries = 0;
      let TRIES = 50;
      while (valid===false && tries<TRIES) {
        x = ROT.RNG.getUniformInt(x0,x0+w);
        y = ROT.RNG.getUniformInt(y0,y0+h);
        if (x>LEVELW-2) {
          x = LEVELW-2;
        }
        if (y>LEVELH-2) {
          y = LEVELH-2;
        }
        try {
          z = (level===null) ? HTomb.Tiles.groundLevel(x,y) : level.groundLevels[x][y];
        } catch(e) {
          console.log(x);
          console.log(y);
          console.log(options);
          console.log(level);
          throw e;
        }
        if (mask[coord(x,y,z)]) {
          continue;
        }
        valid = callback(x,y,z);
        tries+=1;
      }
      if (!x && !y && !z) {
        console.log(this.describe() + "placement failed.");
        return null;
      }
      return {x: x, y: y, z: z};
    },
    validPlace: function(x,y,z) {
      return true;
    },
    remove: function() {
      HTomb.Debug.logEvent("remove",this);
      if (this.components) {
        var comp = this.components;
        for (var i=0; i<comp.length; i++) {
          if (comp[i].onRemove) {
            comp[i].onRemove();
          }
        }
      }
      if (this.onRemove) {
        this.onRemove();
      }
      this.x = null;
      this.y = null;
      this.z = null;
    },
    unstage: function() {
      let x = this.x;
      let y = this.y;
      let z = this.z;
      this.remove();
      this.unstaged = {
        x: x,
        y: y,
        z: z
      };
    },
    stage: function() {
      let {x, y, z} = this.unstaged;
      this.unstaged = null;
      this.place(x,y,z);
    },
    destroy: function() {
      HTomb.Debug.logEvent("getDestroyed",this);
      HTomb.Events.publish({type: "Destroy", entity: this});
      this.despawn();
    },
    despawn: function() {
      if (this.isPlaced()) {
        this.remove();
      }
      if (this.components) {
      var comp = this.components;
        for (var i=0; i<comp.length; i++) {
          comp[i].despawn();
        }
      }
      Thing.despawn.call(this);
    },
    spawn: function(args) {
      args = args || {};
      let o = Thing.spawn.call(this,args);

      for (let b in o.Components) {
        if (!HTomb.Things[b]) {
          console.log(b);
        }
        let defaults = HTomb.Things[b];
        if (defaults.nospawn) {
          //!!!For now, these don't even get added to the components list
          //!!!That means they can't listen for events other than onDefine
          //!!!We may revisit that in the future
          continue;
        }
        let cargs = o.Components[b];
        // the way these arguments nest and inherit is pretty subtle...
        for (let key in cargs) {
          if (cargs.hasOwnProperty(key)) {
            if (cargs[key]!==defaults[key]) {
              cargs[key] = HTomb.Utils.merge(defaults[key],cargs[key]);
            }
          }
        }
        let comp = HTomb.Things[b].spawn(cargs);
        comp.addToEntity(o);
      }
      return o;
    },
    highlight: function(bg) {
      this.highlightColor = bg;
    },
    unhighlight: function() {
      if (this.highlightColor) {
        delete this.highlightColor;
      }
    },
    extend: function(args) {
      // this logic makes it so the Zombie template has behavior directly attached
      // I think this is only needed in edge cases
      let template = HTomb.Things.Thing.extend.call(this, args);
      template.components = [];
      for (let b in template.Components) {
        if (!HTomb.Things[b]) {
          console.log(b);
        }
        let defaults = HTomb.Things[b];
        let cargs = template.Components[b];
        for (let key in cargs) {
          if (cargs.hasOwnProperty(key)) {
            if (cargs[key]!==defaults[key]) {
              cargs[key] = HTomb.Utils.merge(defaults[key],cargs[key]);
            }
          }
        }
        //!!!so at this point, it doesn't know what it's getting attached to...
        cargs.Template = template;
        let comp = HTomb.Things[b].prespawn(cargs);
        template[comp.name] = comp;
      }
      return template;
    },
    chainPlace: function(x,y,z,options) {
      options = options || {};
      let N = options.n || 12;
      let MIN = options.min || 2;
      let MAX = options.max || 4;
      let TRIES = options.tries || 1000;
      let cavern = options.cavern || null;
      let callback = options.callback || function() {};
      let tries = 0;
      let thing = this.spawn();
      if (thing.validPlace(x,y,z)) {
        thing.place(x,y,z);
      }
      callback(thing);
      let things = [thing];
      let ordered = [thing];
      while(tries<TRIES) {
        if (!thing) {
          console.log("wtf???");
          console.log(things);
        }
        tries+=1;
        things = HTomb.Utils.shuffle(things);
        x = things[0].x;
        y = things[0].y;
        let r = ROT.RNG.getUniform()*2*Math.PI;
        let d = ROT.RNG.getUniform()*(MAX-MIN)+MIN;
        x += Math.round(d*Math.cos(r));
        y += Math.round(d*Math.sin(r));
        if (x<1 || y<1 || x>LEVELW-2 || y>LEVELW-2) {
          continue;
        }
        z = (cavern===null) ? HTomb.Tiles.groundLevel(x,y) : cavern.groundLevels[x][y];
        if (this.validPlace(x,y,z)) {
          thing = this.spawn().place(x,y,z);
          things.push(thing);
          ordered.unshift(thing);
          callback(thing);
        }
        if (things.length>=N) {
          break;
        }
      }
      // returns the last thing placed
      return ordered;
    }
  });

  let Component = Thing.extend({
    template: "Component",
    name: "component",
    entity: null,
    nospawn: false,
    addToEntity: function(ent) {
      HTomb.Debug.logEvent("add",this);
      this.entity = ent;
      ent[this.name] = this;
      if (ent.hasOwnProperty("components")===false) {
        ent.components = [];
      }
      ent.components.push(this);
      ent.components.sort(function(a,b) {
        if (a.name<b.name) {
          return -1;
        } else if (a.name>b.name) {
          return 1;
        } else {
          return 0;
        }
      });
      if (this.onAdd) {
        this.onAdd(this.options);
      }
    },
    detachFromEntity: function() {
      HTomb.Debug.logEvent("detach",this);
      let ent = this.entity;
      if (this.onDetach) {
        this.onDetach();
      }
      ent.components.splice(ent.components.indexOf(this),1);
      this.entity = null;
      delete ent[this.name];
    }
  });

return HTomb;
})(HTomb);
