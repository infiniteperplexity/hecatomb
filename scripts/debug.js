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
    let w = window.open();
    let lookup = {
      63: "#FFFFFF",
      62: "#FFFFFF",
      61: "#FFFFFF",
      60: "#FFFFFF",
      59: "#FFFFFF",
      58: "#FFFFFF",
      57: "#FFFFFF",
      56: "#EEEEFF",
      55: "#DDDDEE",
      54: "#BBCCCC",
      53: "#99AA99",
      52: "#889988",
      51: "#778877",
      50: "#667766",
      49: "#556655",
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
    let SIZE = 22;
    let PAD = 6;
    let N = 11;
    let loopText = [];
    let scale = 2;
    let setupTxt =
      'let c = document.createElement("canvas"); c.height = '
      + 256*scale + "; c.width = "
      + 256*scale + '; let ctx = c.getContext("2d"); document.body.appendChild(c);'
    ;
    for (let x=1; x<255; x++) {
      for (let y=1; y<255; y++) {
        let z = HTomb.Tiles.groundLevel(x,y);
        let joiner = [scale*x,scale*y,scale*x+scale,scale*y+scale];
        let fill = lookup[z];
        let f = HTomb.World.features[coord(x,y,z)];
        if (f) {
          if (f.template==="Tree") {
            fill = "#005500";
          } else if (f.template==="Shrub") {
            fill = "#008800";
          } else if (f.template==="Boulder") {
            fill = "#555544";
          } else if (HTomb.World.covers[z][x][y]!==HTomb.Covers.Water) {
            fill = f.fg;
          }
        }
        let c = HTomb.World.creatures[coord(x,y,z)];
        if (c && c.template==="Necromancer") {
          fill = "#FF0088";
        }
        if (options.ruled===true && (x%SIZE===PAD+1 || y%SIZE===PAD+1)) {
          fill="red";
        }

        let txt = "ctx.fillStyle = '" + fill + "'; ctx.fillRect("+joiner.join()+");";
        loopText.push(txt);
      }
    }
    loopText = loopText.join(" ");
    w.eval(setupTxt + loopText);
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
      56: "#EEEEFF",
      55: "#DDDDEE",
      54: "#BBCCCC",
      53: "#99AA99",
      52: "#889988",
      51: "#778877",
      50: "#667766",
      49: "#556655",
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
        let fills = [];
        let player = false;
        for (let m=0; m<scale; m++) {
          for (let n=0; n<scale; n++) {
            let x = padding+i*scale+m;
            let y = padding+j*scale+n;
            let z = HTomb.Tiles.groundLevel(x,y);
            let fill = lookup[z];
            let f = HTomb.World.features[coord(x,y,z)];
            if (f) {
              if (f.template==="Tree") {
                fill = "#005500";
              } else if (f.template==="Shrub") {
                fill = "#008800";
              } else if (f.template==="Boulder") {
                fill = "#555544";
              } else if (HTomb.World.covers[z][x][y]!==HTomb.Covers.Water) {
                fill = f.fg;
              }
            }
            let c = HTomb.World.creatures[coord(x,y,z)];
            if (c===HTomb.Player) {
              player = true;
            }
            fills.push(ROT.Color.fromString(fill));
          }
        }
        let tally = [0,0,0];
        for (let fill of fills) {
          for (let i=0; i<fill.length; i++) {
            tally[i]+=fill[i];
          }
        }
        for (let i=0; i<tally.length; i++) {
          tally[i] = parseInt(tally[i]/fills.length);
        }
        let sym = [".","black",ROT.Color.toHex(tally)];
        if (player) {
          sym[0] = HTomb.Things.Necromancer.symbol;
          sym[1] = HTomb.Things.Necromancer.fg;
        }
        HTomb.GUI.Panels.gameScreen.display.draw(i,j,sym[0],sym[1],sym[2]);
      }
    }
  };
  
  return HTomb;
})(HTomb);
