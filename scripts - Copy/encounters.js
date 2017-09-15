HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;

  HTomb.Types.define({
  	template: "Encounter",
  	name: "encounter",
    hostile: true,
    spawn: function() {},
    onDefine: function() {
      if (HTomb.Encounters.table===undefined) {
        HTomb.Encounters.table = [];
      }
      HTomb.Encounters.table.push([this.frequency,this]);
    }
  });
  HTomb.Encounters.check = function() {
    //if (HTomb.Time.dailyCycle.turn===1) {
    //  return true;
    //} else {
    //  return false;
    //}
  };
  HTomb.Encounters.roll = function(callb) {
    //for now we don't use this
    //return;
    callb = callb || function() {return true;};
    var cumulative = 0;
    var table = [];
    for (var k=0; k<HTomb.Encounters.table.length; k++) {
      if (callb(HTomb.Encounters.table[k][1])) {
        if (HTomb.Debug.peaceful && HTomb.Encounters.table[k][1].hostile) {
          continue;
        } else {
          table.push(HTomb.Encounters.table[k]);
        }
      }
    }
    if (table.length===0) {
      return;
    }
    for (var i=0; i<table.length; i++) {
      cumulative+=table[i][0];
    }
    var roll = Math.random()*cumulative;
    cumulative = 0;
    for (var j=0; table.length; j++) {
      cumulative+=table[j][0];
      if (roll<cumulative) {
        table[j][1].spawn();
        break;
      }
    }
  };

  function noCreature(x,y,z) {
    if (HTomb.World.creatures[coord(x,y,z)]===undefined) {
      return true;
    } else {
      return false;
    }
  }
  HTomb.Tiles.getEdgeSquare = function(callb) {
    callb = callb || noCreature;
    var TRIES = 100;
    var tries = 0;
    while (true) {
      tries+=1;
      var x = Math.floor(Math.random()*LEVELW);
      var y = Math.floor(Math.random()*LEVELH);
      var r = Math.random();
      if (r<0.5) {
        if (x>LEVELW/2) {
          x = LEVELW-2;
        } else {
          x = 1;
        }
      } else {
        if (y>LEVELH/2) {
          y = LEVELH-2;
        } else {
          y = 1;
        }
      }
      var z = HTomb.Tiles.groundLevel(x,y);
      if (callb(x,y,z)) {
        console.log("returning " + [x,y,z]);
        return [x,y,z];
      }
      if (tries>=TRIES) {
        alert("failed to find ");
        return;
      }
    }
  };

  HTomb.Types.defineEncounter({
    template: "GhoulTest",
    name: "ghoul test",
    frequency: 1,
    hostile: true,
    spawn: function() {
      var c = HTomb.Tiles.getEdgeSquare();
      var cr = HTomb.Things.Ghoul();
      cr.place(c[0],c[1],c[2]);
      console.log("placed a ghoul");
    }
  });

  HTomb.Types.defineEncounter({
    template: "SpiderTest",
    name: "spidertest",
    // frequency: 2,
    frequency: 0,
    hostile: false,
    spawn: function() {
      var c = HTomb.Tiles.getEdgeSquare();
      var cr = HTomb.Things.Spider();
      cr.place(c[0],c[1],c[2]);
      console.log("placed a spider");
    }
  });




  return HTomb;
})(HTomb);
