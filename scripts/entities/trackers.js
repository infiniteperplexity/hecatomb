HTomb = (function(HTomb) {
  "use strict";
  var coord = HTomb.Utils.coord;

  let Thing = HTomb.Things.Thing;

  let Tracker = Thing.extend({
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
      let o = Thing.spawn.call(this, args);
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
      Thing.despawn.call(this,args);
    },
    extend: function(args) {
      let t = Thing.extend.call(this, args);
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

return HTomb;
})(HTomb);
