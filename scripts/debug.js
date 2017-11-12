// This module provides optional debugging functionality
HTomb = (function(HTomb) {
  "use strict";
  var Debug = HTomb.Debug;

  //Debug.noingredients = true;
  Debug.nodecay = false;
  //Debug.explored = true;
  //Debug.visible = true;
  //Debug.mobility = true;
  //Debug.showpaths = true; //not yet implemented
  //Debug.messages = true;
  //Debug.faster = true;
  //Debug.paused = true;\
  //Debug.peaceful = true;
  Debug.logEvents = false;
  Debug.maxEvents = 1000;
  Debug.eventLog = [];
  Debug.logVermin = false;
  Debug.logEvent = function(event, thing) {
    if (Debug.logEvents!==true) {
      return;
    }
    if (thing.vermin && Debug.logVermin===false) {
      return;
    }
    let entry = [event, thing.describe({spawnId: true})];
    Debug.eventLog.push(entry);
    if (Debug.eventLog.length>Debug.maxEvents) {
      do {
        Debug.eventLog.shift();
      } while (Debug.eventLog.length>Debug.maxEvents);
    }
  };
  
  HTomb.Debug.minimap = function(options) {
    options = options || {};
    let coord = HTomb.Utils.coord;
    let lookup = {
      63: "#FFFFFF",
      62: "#FFFFFF",
      61: "#FFFFFF",
      60: "#FFFFFF",
      59: "#FFFFFF",
      58: "#FFFFFF",
      57: "#FFFFFF",
      56: "#DDDDFF",
      55: "#CCCCEE",
      54: "#AABBBB",
      53: "#AABB99",
      52: "#889977",
      51: "#667755",
      50: "#445533",
      49: "#334422",
      48: "#8888FF",
      47: "#7777EE",
      46: "#6666DD",
      45: "#5555CC",
      44: "#4444BB",
      43: "#3333AA",
      42: "#222299",
      41: "#111188",
      40: "#000077",
      39: "#000055",
      38: "#000044",
      37: "#000033",
      36: "#000022",
      35: "#000011"
    };
    // so...you'd have the middle 25 columns showing 10 each, and the last two showing...basically nothing.
    // so instead, the middle 23 showing 10 each, the next 2 out showing 12 each.  Fine.
    let scale = 10;
    let cells = 25;
    let padding = 2;
    for (let i=0; i<cells; i++) {
      for (let j=0; j<cells; j++) {
        let zs = [];
        let player = false;
        let unexplored = 0;
        let elevations = 0;
        for (let m=0; m<scale; m++) {
          for (let n=0; n<scale; n++) {
            let x = padding+i*scale+m;
            let y = padding+j*scale+n;
            let z = HTomb.Tiles.groundLevel(x,y);
            let c = HTomb.World.creatures[coord(x,y,z)];
            if (HTomb.Player.x===x && HTomb.Player.y===y) {
              player = true;
            }
            if (!HTomb.World.explored[z][x][y]) {
              unexplored+=1;
            }
            elevations+=z;
          }
        }
        let r = elevations/(scale*scale)-Math.round(elevations/(scale*scale));
        elevations = Math.round(elevations/(scale*scale));
        let sym = [".","black",lookup[elevations]];
        // if (r>0.3) {
        //   sym[0] = "\u02C4";
        // } else if (r<-0.3) {
        //   sym[0] = "\u02C5";
        // }
        unexplored = unexplored/(scale*scale);
        if (unexplored>0.75 && HTomb.Debug.explored!==true) {
          sym[2] = "black";
        }
        if (player) {
          sym[0] = HTomb.Things.Necromancer.symbol;
          sym[1] = HTomb.Things.Necromancer.fg;
        }
        HTomb.GUI.Panels.gameScreen.display.draw(i+1,j+1,sym[0]+"\uFE0E",sym[1],sym[2]);
      }
    }
    for (let i=0; i<HTomb.Constants.SCREENW; i++) {
      for (let j=0; j<HTomb.Constants.SCREENH; j++) {
        if (i===0 || j===0 || i===HTomb.Constants.SCREENW-1 || j===HTomb.Constants.SCREENH-1) {
          HTomb.GUI.Panels.gameScreen.display.draw(i,j,".","black","black");
        }
      }
    }
  };

  HTomb.Debug.minimap = function(options) {
    options = options || {};
    let LEVELW = HTomb.Constants.LEVELW;
    let LEVELH = HTomb.Constants.LEVELH;
    let coord = HTomb.Utils.coord;
    let lookup = {
      63: "#FFFFFF",
      62: "#FFFFFF",
      61: "#FFFFFF",
      60: "#FFFFFF",
      59: "#FFFFFF",
      58: "#FFFFFF",
      57: "#FFFFFF",
      56: "#DDDDFF",
      55: "#CCCCEE",
      54: "#AABBBB",
      53: "#AABB99",
      52: "#889977",
      51: "#667755",
      50: "#445533",
      49: "#334422",
      48: "#8888FF",
      47: "#7777EE",
      46: "#6666DD",
      45: "#5555CC",
      44: "#4444BB",
      43: "#3333AA",
      42: "#222299",
      41: "#111188",
      40: "#000077",
      39: "#000055",
      38: "#000044",
      37: "#000033",
      36: "#000022",
      35: "#000011"
    };
    let scale = 4;
    let cells = 27;
    let middle = parseInt(cells/2);
    let padding = 0;
    let border = (HTomb.Constants.SCREENW-cells)/2;
    // 13 is the middle cell
    let xoffset;
    let yoffset;
    if (HTomb.Player.x<=1+13*scale) {
      xoffset = 0;
    } else if (HTomb.Player.x>=LEVELW-2-13*scale) {
      xoffset = LEVELW-2-27*scale;
    } else {
      xoffset = HTomb.Player.x-13*scale;
    }
    if (HTomb.Player.y<=1+13*scale) {
      yoffset = 0;
    } else if (HTomb.Player.y>=LEVELH-2-13*scale) {
      yoffset = LEVELH-2-scale*27;
    } else {
      yoffset = HTomb.Player.y-13*scale;
    }
    for (let i=0; i<cells; i++) {
      for (let j=0; j<cells; j++) {
        let player = false;
        let unexplored = 0;
        let elevations = 0;
        for (let m=0; m<scale; m++) {
          for (let n=0; n<scale; n++) {
            let x = padding+1+xoffset+i*scale+m;
            let y = padding+1+yoffset+j*scale+n;
            let z = HTomb.Tiles.groundLevel(x,y);
            let c = HTomb.World.creatures[coord(x,y,z)];
            if (HTomb.Player.x===x && HTomb.Player.y===y) {
              player = true;
            }
            if (!HTomb.World.explored[z][x][y]) {
              unexplored+=1;
            }
            elevations+=z;
          }
        }
        let r = elevations/(scale*scale)-Math.round(elevations/(scale*scale));
        elevations = Math.round(elevations/(scale*scale));
        let sym = [".","black",lookup[elevations]];
        // if (r>0.3) {
        //   sym[0] = "\u02C4";
        // } else if (r<-0.3) {
        //   sym[0] = "\u02C5";
        // }
        unexplored = unexplored/(scale*scale);
        if (unexplored>0.75 && HTomb.Debug.explored!==true) {
          sym[2] = "black";
        }
        if (player) {
          sym[0] = HTomb.Things.Necromancer.symbol;
          sym[1] = HTomb.Things.Necromancer.fg;
        }
        HTomb.GUI.Panels.gameScreen.display.draw(border+i,border+j,sym[0]+"\uFE0E",sym[1],sym[2]);
      }
    }
  };
  for (let i=0; i<HTomb.Constants.SCREENW; i++) {
    for (let j=0; j<HTomb.Constants.SCREENH; j++) {
      if (i<border || j<border || i>HTomb.Constants.SCREENW-1-border || j>HTomb.Constants.SCREENH-1-border) {
        HTomb.GUI.Panels.gameScreen.display.draw(i,j,".","black","black");
      }
    }
  }

  return HTomb;
})(HTomb);
