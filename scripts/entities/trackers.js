HTomb = (function(HTomb) {
  "use strict";
  let LEVELW = HTomb.Constants.LEVELW;
  let LEVELH = HTomb.Constants.LEVELH;
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

  Tracker.extend({
    template: "RandomSeed",
    name: "randomseed",
    seed: null,
    state: null,
    getSeed: function() {
      this.seed = ROT.RNG.getSeed();
      this.state = ROT.RNG.getState();
    },
    setSeed: function() {
      ROT.RNG.setSeed(this.seed);
      ROT.RNG.setState(this.state);
    }
  });

  Tracker.extend({
    template: "GrassGrowth",
    name: "grass growth",
    listens: ["TurnBegin"],
    onTurnBegin: function() {
      if (HTomb.Time.dailyCycle.turn%50!==0) {
        return;
      }
      for (let x=1; x<LEVELW-1; x++) {
        for (let y=1; y<LEVELH-1; y++) {
          if (ROT.RNG.getUniform()>=0.1) {
            continue;
          }
          let z = HTomb.Tiles.groundLevel(x,y);
          // don't grow over slopes or features I guess
          if (HTomb.World.tiles[z][x][y]!==HTomb.Tiles.FloorTile || HTomb.World.covers[z][x][y]!==HTomb.Covers.NoCover || HTomb.World.features[coord(x,y,z)]) {
            continue;
          }
          
          if (z===54) {
            if (ROT.RNG.getUniformInt(1,2)===1) {
              var n = HTomb.Tiles.countNeighborsWhere(x,y,z,function(x,y,z) {
                return (HTomb.World.covers[z][x][y]===HTomb.Covers.Grass);
              });
              if (n>0) {
                HTomb.World.covers[z][x][y] = HTomb.Covers.Grass;
              }
            } else {
              HTomb.World.covers[z][x][y] = HTomb.Covers.Snow;
            }
          }
          if (z<54) {
            var n = HTomb.Tiles.countNeighborsWhere(x,y,z,function(x,y,z) {
              return (HTomb.World.covers[z][x][y]===HTomb.Covers.Grass);
            });
            if (n>0) {
              HTomb.World.covers[z][x][y] = HTomb.Covers.Grass;
            }
          } else {
            HTomb.World.covers[z][x][y] = HTomb.Covers.Snow;
          }
        }
      }
    }
  });

  Tracker.extend({
    template: "TimeTracker",
    name: "timetracker",
    turn: 0,
    day: 0
    // this one will probably get a special, direct reference, not an event
  });

  Tracker.extend({
    template: "HumanityTracker",
    name: "humanitytracker",
    listens: ["TurnBegin"],
    onTurnBegin: function() {
      let cycle = HTomb.Time.dailyCycle;
      if (cycle.day===1 && cycle.hour===HTomb.Constants.DAWN && cycle.minute===0) {
        HTomb.Things.PeasantMob.muster(null,null,null,{n: 0});
      }
    }
  });

return HTomb;
})(HTomb);
