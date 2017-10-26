HTomb = (function(HTomb) {
  "use strict";
  var coord = HTomb.Utils.coord;
  
  let Thing = HTomb.Things.Thing;
  // Define a generic entity that occupies a tile space
  let Entity = Thing.extend({
    template: "Entity",
    name: "entity",
    x: null,
    y: null,
    z: null,
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
        this.onPlace(x,y,z);
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
