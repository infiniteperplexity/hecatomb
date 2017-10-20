// The lowest-level GUI functionality, interacting with the DOM directly or through ROT.js.
HTomb = (function(HTomb) {
  "use strict";
  // break out constants
  var SCREENW = HTomb.Constants.SCREENW;
  var SCREENH = HTomb.Constants.SCREENH;
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var SCROLLH = HTomb.Constants.SCROLLH;
  var SCROLLW = HTomb.Constants.SCROLLW;
  var MENUW = HTomb.Constants.MENUW;
  var MENUH = HTomb.Constants.MENUH;
  var TOTALW = HTomb.Constants.TOTALW;
  var STATUSH = HTomb.Constants.STATUSH;
  var FONTSIZE = HTomb.Constants.FONTSIZE;
  var UNIBLOCK = HTomb.Constants.UNIBLOCK;
  var EARTHTONE = HTomb.Constants.EARTHTONE;
  var SHADOW = HTomb.Constants.SHADOW;
  var FONTFAMILY = HTomb.Constants.FONTFAMILY;
  var CHARHEIGHT = HTomb.Constants.CHARHEIGHT;
  var CHARWIDTH = HTomb.Constants.CHARWIDTH;
  var TEXTFONT = HTomb.Constants.TEXTFONT;
  var TEXTSIZE = HTomb.Constants.TEXTSIZE;
  var XSKEW = HTomb.Constants.XSKEW;
  var YSKEW = HTomb.Constants.YSKEW;
  var TEXTSPACING = HTomb.Constants.TEXTSPACING;
  var TEXTWIDTH = HTomb.Constants.TEXTWIDTH;
  var coord = HTomb.Utils.coord;
  // set up GUI and display
  var GUI = HTomb.GUI;

  let Panels = GUI.Panels;
  let gameScreen = Panels.gameScreen;
  let menu = Panels.menu;
  let menuDisplay = Panels.menu.display;
  let scroll = Panels.scroll;
  let scrollDisplay = scroll.display;
  let status = Panels.status;
  let overlay = Panels.overlay;
  let overlayDisplay = overlay.display;
  let alert = Panels.alert;
  let alertDisplay = alert.display;

  // Basic rendering of panels
  // Keep track of how many tiles it is offset from 0, 0
  gameScreen.xoffset = 0;
  gameScreen.yoffset = 0;
  // Keep track of which Z level it is on
  gameScreen.z = 1;
  gameScreen.redraw = function(x,y) {
    var z = gameScreen.z;
    var xoffset = gameScreen.xoffset;
    var yoffset = gameScreen.yoffset;
    // Draw every symbol in the right
    var sym = HTomb.Tiles.getSymbol(x,y,z);
    gameScreen.display.draw(this.x0+x-xoffset,this.y0+y-yoffset, sym[0], sym[1], sym[2]);
  };
  gameScreen.render = function() {
    var z = gameScreen.z;
    var xoffset = gameScreen.xoffset;
    var yoffset = gameScreen.yoffset;
    for (var x = xoffset; x < xoffset+SCREENW; x++) {
      for (var y = yoffset; y < yoffset+SCREENH; y++) {
        if (gameScreen.z===undefined) {
          alert("wtf!");
        }
        // Draw every symbol in the right
        var sym = HTomb.Tiles.getSymbol(x,y,z);
        gameScreen.display.draw(this.x0+x-xoffset,this.y0+y-yoffset, sym[0], sym[1], sym[2]);
      }
    }
    gameScreen.renderParticles();
  };
  gameScreen.recenter = function() {
    let p = HTomb.Player.player.delegate;
    gameScreen.center(p.x,p.y,p.z);
  };
  gameScreen.center = function(x,y,z) {
    x = x-Math.floor(SCREENW/2);
    y = y-Math.floor(SCREENH/2);
    z = z || HTomb.Player.player.delegate.z;
    gameScreen.xoffset = x;
    gameScreen.yoffset = y;
    if (z!==undefined) {
      gameScreen.z = z;
    }
  };
  let oldSquares;
  gameScreen.renderParticles = function() {
    let squares = {};
    let p,c,x,y,z;
    // collect the particles
    for (let j=0; j<HTomb.Particles.emitters.length; j++) {
      let emitter = HTomb.Particles.emitters[j];
      for (let i=0; i<emitter.particles.length; i++) {
        p = emitter.particles[i];
        // don't collect particles that aren't on the screen
        x = Math.round(p.x);
        if (x<gameScreen.xoffset || x>=gameScreen.xoffset+SCREENW || x>=LEVELW-1) {
          continue;
        }
        y = Math.round(p.y);
        if (y<gameScreen.yoffset || y>=gameScreen.yoffset+SCREENH || y>=LEVELH-1) {
          continue;
        }
        z = Math.round(p.z);
        // only bother with particles on the same level for now...or maybe within one level?
        //if (z!==gameScreen.z) {
        //  continue;
        //}
        c = coord(x,y,z);
        if (p.alwaysVisible===true || HTomb.World.visible[c]) {
          if (squares[c]===undefined) {
            squares[c] = [];
          }
          squares[c].push(p);
        }
      }
    }
    // process the particles
    for (let s in squares) {
      if (oldSquares[s]) {
        delete oldSquares[s];
      }
      c = HTomb.Utils.decoord(s);
      x = c[0];
      y = c[1];
      z = c[2];
      var particles = squares[s];
      HTomb.Utils.shuffle(particles);
      var ch, fg;
      // if there are ever invisible particles we may need to handle this differently
      fg = HTomb.Tiles.getGlyph(x,y,z)[1];
      fg = ROT.Color.fromString(fg);
      for (var k=0; k<particles.length; k++) {
        var pfg = particles[k].fg;
        pfg[0] = Math.min(255,Math.max(pfg[0],0));
        pfg[1] = Math.min(255,Math.max(pfg[1],0));
        pfg[2] = Math.min(255,Math.max(pfg[2],0));
        //fg = HTomb.Utils.alphaHex(pfg, fg, particles[k].alpha);
        fg = HTomb.Utils.alphaHex(pfg, fg, particles[k].alpha);
      }
      if (particles.length>0) {
        fg[0] = Math.round(fg[0]);
        fg[1] = Math.round(fg[1]);
        fg[2] = Math.round(fg[2]);
        fg = ROT.Color.toHex(fg);
        ch = particles[particles.length-1].symbol;
        gameScreen.drawGlyph(x,y,ch,fg);
      }
    }
    // clean up expired particles
    for (var o in oldSquares) {
      c = HTomb.Utils.decoord(o);
      x = c[0];
      y = c[1];
      gameScreen.refreshTile(x,y);
    }
    oldSquares = squares;
  };

  status.render = function() {
    //black out the entire line with solid blocks
    var cursor = 0;
    let p = (HTomb.Player.player) ? HTomb.Player.player.delegate : HTomb.Player;
    let x = String(p.x);
    let y = String(p.y);
    let z = String(gameScreen.z);
    let day  = String(HTomb.Time.dailyCycle.day);
    let hour = String(HTomb.Time.dailyCycle.hour);
    let minute  = String(HTomb.Time.dailyCycle.minute);
    while(x.length<3) {
      x = "0"+x;
    }
    while(y.length<3) {
      y = "0"+y;
    }
    while(z.length<3) {
      z = "0"+z;
    }
    while(day.length<4) {
      day = "0"+day;
    }
    while(hour.length<2) {
      hour = "0"+hour;
    }
    while(minute.length<2) {
      minute = "0"+minute;
    }
    let txt = "X:"+x+" Y:"+y+" Z:"+z+" "+HTomb.Time.dailyCycle.getPhase().symbol+day+":"+hour+":"+minute;
    if (p.caster) {
      txt = "Sanity:" + p.caster.sanity + "/" + p.caster.getMaxSanity() + " " + txt;
    }
    if (HTomb.Time.isPaused()===true || HTomb.GUI.autopause===true) {
      txt = txt + " %c{yellow}Pause";
    }
    scrollDisplay.drawText(scroll.x0, scroll.y0, txt);
  };
  scroll.bufferMax = 100;
  scroll.buffer = [];
  scroll.bufferIndex = 1;
  scroll.reset = function() {
    scroll.buffer = [];
  }
  scroll.lastLines = 1;
  scroll.render = function() {
    scrollDisplay.clear();
    status.render();
    for (let s=0; s<SCROLLH; s++) {
      if (s+this.bufferIndex>this.buffer.length) {
        break;
      }
      let message = this.buffer[this.buffer.length-s-this.bufferIndex];
      if (s+this.bufferIndex<=scroll.lastLines && message.substr(0,3)!=="%c{") {
        message = "%c{cyan}"+message;
      }
      scrollDisplay.drawText(this.x0,this.y0+s+1,message);
    }
  };
  scroll.scrollUp = function() {
    this.bufferIndex = Math.max(1,this.bufferIndex-1);
    this.render();
  };
  scroll.scrollDown = function() {
    this.bufferIndex = Math.max(1,Math.min(this.bufferIndex+1,this.buffer.length-SCROLLH+2));
    this.render();
  };

  menu.render = function() {
    // compose menu text with proper spacing
    let menuTop = menu.top;
    if (!menuTop || menuTop.length===0) {
      menuTop = GUI.Contexts.active.menuText;
    }
    if (!menuTop || menuTop.length===0) {
      menuTop = menu.defaultTop;
    }
    let menuMiddle = menu.middle;
    if (!menuMiddle|| menuMiddle.length===0) {
      menuMiddle = menu.defaultMiddle;
    }
    let menuBottom = menu.bottom;
    if (!menuBottom || menuBottom.length===0) {
      menuBottom = menu.defaultBottom;
    }
    if (HTomb.Tutorial.enabled) {
      let replace = HTomb.Tutorial.getMenu({
        top: menuTop,
        middle: menuMiddle,
        bottom: menuBottom
      });
      for (let i=0; i<replace.instructions.length; i++) {
        if (replace.instructions[i].substr(0,2)!=="%c") {
          replace.instructions[i] = "%c{lime}"+replace.instructions[i];
        }
      }

      menuTop = replace.controls.concat([" "],replace.instructions);
      menuMiddle = replace.middle;
      menuBottom = replace.bottom;
    }
    let menuText = menuTop;
    menuText = menuText.concat([" ","-".repeat(HTomb.Constants.MENUW-2)]);
    if (menuMiddle.length>0) {
      menuText = menuText.concat([" "], menuMiddle);
    }
    if (menuBottom.length>0) {
      menuText = menuText.concat([" "], menuBottom);
    }
    // handle line breaks
    let c=0;
    let br=null;
    let fgpat = /%c{\w*}/g;
    let bgpat = /%b{\w*}/g;
    // !!!!!More like mishandle line breaks...
    while(c<menuText.length) {
      let txt = menuText[c];
      let fg = fgpat.exec(txt);
      let bg = bgpat.exec(txt);
      let measure = txt.replace(fgpat,"").replace(bgpat,"");
      if (measure.length<MENUW-2) {
        c++;
        continue;
      }
      for (var j=0; j<measure.length; j++) {
        if (measure[j]===" ") {
          br = j;
        }
        if (j>=MENUW-2) {
          var one = measure.substring(0,br);
          var two = measure.substring(br+1);
          if (fg!==null) {
            one = fg[0]+one;
            two = fg[0]+two;
          }
          if (bg!==null) {
            one = bg[0]+one;
            two = bg[0]+two;
          }
          menuText[c] = one;
          menuText.splice(c+1,0,two);
          break;
        }
      }
      c++;
      br = null;
    }
    menuDisplay.clear();
    for (let i=0; i<MENUH; i++) {
      if (menuText[i]) {
        var j = 0;
        if (menuText[i].charAt(0)===" ") {
          for (j=0; j<menuText[i].length; j++) {
            if (menuText[i].charAt(j)!==" ") {
              break;
            }
          }
        }
        if (menuText[i].match("Enter: Enable auto-pause.") && GUI.autopause===true) {
          menuDisplay.drawText(this.x0+j, this.y0+i, menuText[i].replace("Enter: Enable auto-pause.","Enter: Disable auto-pause."));
        } else if (menuText[i].match("PageUp/Down") && window.navigator.platform.substr(0,3)==="Mac") {
          menuDisplay.drawText(this.x0+j, this.y0+i, menuText[i].replace("PageUp/Down","Fn+Up/Down"));
        } else {
          menuDisplay.drawText(this.x0+j, this.y0+i, menuText[i]);
        }
      }
    }
  };

  GUI.reset = function() {
    GUI.Views.parentView();
    if (HTomb.Time.dailyCycle.turn===0) {
      HTomb.Events.publish({type: "Tutorial", beginGame: true});
    }
  };
  // This should probably be an Event, not a GUI method
  GUI.sensoryEvent = function(strng,x,y,z,color) {
    if (HTomb.World.visible[HTomb.Utils.coord(x,y,z)]) {
      GUI.pushMessage(strng, color);
    }
  };

  GUI.pushMessage = function(strng, color) {
    if (Array.isArray(strng)) {
      strng.map(function(e,i,a) {GUI.pushMessage(e,color);});
      return;
    }
    let MAX = HTomb.Constants.SCROLLW-1;
    if (HTomb.Utils.cleanText(strng).length>MAX) {
      let lines = HTomb.Utils.lineBreak(strng,MAX);
      lines.reverse();
      scroll.lastLines = lines.length;
      for (let line of lines) {
        pushMessage(line, color);
      }
      return;  
    }
    scroll.lastLines = 1;
    pushMessage(strng, color);
  }

  function pushMessage(strng, color) {
    if (color!==undefined) {
      strng = "%c{" + color + "}"+strng;
    }
    scroll.bufferIndex = 1;
    scroll.buffer.push(strng);
    //if (scroll.buffer.length>=SCROLLH) {
    //  scroll.bufferIndex = Math.max(0,scroll.buffer.length-SCROLLH+1);
    //}
    if (scroll.buffer.length>scroll.bufferMax) {
      scroll.buffer.shift();
    }
    // Render the message immediatey if the scroll is visible
    //if (GUI.panels.bottom===scroll) {
      scroll.render();
    //}
  };
  HTomb.Debug.pushMessage = function(msg) {
    if (HTomb.Debug.messages===true) {
      HTomb.GUI.pushMessage(msg);
      console.log(msg);
    }
  };

  // Render all four default panels
  GUI.render = function() {
    gameScreen.render();
    scroll.render();
    menu.render();
  };
  //******end defaults

  // ***** Basic right-hand menu stuff *****
  menu.defaultTop = [
    "Esc: System view.",
    "%c{yellow}Avatar mode (Tab: Navigation mode)",
    " ",
    "Move: NumPad/Arrows, ,/.: Up/Down.",
    "(Control+Arrows for diagonal.)",
    "Wait: NumPad 5 / Space.",
    " ",
    "Enter: Enable auto-pause.",
    "+/-: Change speed.",
    " ",
    "Z: Cast spell, J: Assign job.",
    "M: Minions, S: Structures, U: Summary.",
    "G: Pick Up, D: Drop.",
    "I: Inventory, E: Equip/Unequip.",
    " ",
    "PageUp/Down: Scroll messages.",
    "A: Achievements, /: Toggle tutorial."
  ];
  menu.defaultMiddle = [];
  menu.defaultBottom = [];

  menu.refresh = function() {
    menu.render();
  };

  // *** Drawing on the game screen *****
  gameScreen.refreshTile = function(x,y) {
    var xoffset = gameScreen.xoffset || 0;
    var yoffset = gameScreen.yoffset || 0;
    var z = gameScreen.z;
    var sym = HTomb.Tiles.getSymbol(x,y,z);
    gameScreen.display.draw(
      x-xoffset,
      y-yoffset,
      sym[0],
      sym[1],
      sym[2]
    );
  };
  // Draw a character at the appropriate X and Y tile
  gameScreen.drawTile = function(x,y,ch,fg,bg) {
    var xoffset = gameScreen.xoffset || 0;
    var yoffset = gameScreen.yoffset || 0;
    fg = fg || "white"  ;
    bg = bg || "black";
    gameScreen.display.draw(
      x-xoffset,
      y-yoffset,
      ch,
      fg,
      bg
    );
  };

  gameScreen.drawGlyph = function(x,y,ch,fg) {
    var xoffset = gameScreen.xoffset || 0;
    var yoffset = gameScreen.yoffset || 0;
    fg = fg || "white";
    var z = gameScreen.z;
    var bg = HTomb.Tiles.getBackground(x,y,z);
    gameScreen.display.draw(
      x-xoffset,
      y-yoffset,
      ch,
      fg,
      bg
    );
  };

  gameScreen.highlitTiles = {};
  // Change the background color of the appropriate X and Y tile
  gameScreen.highlightTile = function(x,y,bg) {
    var xoffset = gameScreen.xoffset || 0;
    var yoffset = gameScreen.yoffset || 0;
    var z = gameScreen.z;
    var sym = HTomb.Tiles.getGlyph(x,y,z);
    gameScreen.display.draw(
      x-xoffset,
      y-yoffset,
      sym[0],
      sym[1],
      bg
    );
  };

  overlay.update = function(arr) {
    HTomb.Time.stopTime();
    HTomb.Time.stopParticles();
    // we may not want to force the player to reset the GUI...but let's try it out
    overlayDisplay.clear();
    let c=0;
    let br=null;
    //%{\w+}
    while(c<arr.length) {
      let pat = /%c{\w+}/;
      let match = pat.exec(arr[c]);

      let txt = arr[c];
      let index = 0;
      if (match!==null) {
        index = match.index;
        txt = arr[c].replace(match[0],"");
      }
      if (txt.length<TOTALW/TEXTWIDTH-20) {
        c++;
        continue;
      }
      for (let j=0; j<txt.length; j++) {
        if (txt[j]===" ") {
          br = j;
        }
        if (j>=TOTALW/TEXTWIDTH-20) {
          var one = txt.substring(0,br);
          var two = txt.substring(br+1);
          if (match!==null) {
            if (index===0) {
              one = match[0]+one;
            } else {
              one = one.substr(0,index-1) + match + one.substr(index);
            }
            two = match[0]+two;
          }
          arr[c] = one;
          arr.splice(c+1,0,two);
          break;
        }
      }
      c++;
      br = null;
    }
    for (let j=0; j<arr.length; j++) {
      let x=0;
      if (arr[j].charAt(0)===" ") {
        for (x=1; x<arr[j].length; x++) {
          if (arr[j].charAt(x)!==" ") {
            break;
          }
        }
      }
      overlay.currentLines = arr;
      overlayDisplay.drawText(4+x, 3+j, arr[j]);
    }
    overlay.unhide();
  }

  GUI.splash = function(arr) {
    GUI.Contexts.active = GUI.Contexts.new();
    overlay.update(arr);
  };

  GUI.delaySplash = function(arr, n) {
    n = n || 1000;
    GUI.Contexts.locked = true;
    GUI.splash(arr);
    setTimeout(function() {
      GUI.Contexts.locked = false;
    }, n);
  };

  GUI.Contexts.default.clickAlert = closeAlert;


  function closeAlert() {
    if (GUI.Contexts.alert.frozen) {
      return;
    }
    GUI.Contexts.active = GUI.Contexts.alert.saved;
    alert.hide();
    GUI.render();
  }
  GUI.Contexts.alert = GUI.Contexts.new({
    VK_RETURN: closeAlert,
    VK_ESCAPE: closeAlert
  });
  GUI.Contexts.alert.mouseDown = closeAlert;
  GUI.Contexts.alert.clickAt = closeAlert;
  GUI.Contexts.alert.clickTile = closeAlert;
  GUI.Contexts.alert.rightClickTile = closeAlert;
  GUI.Contexts.alert.clickAlert = closeAlert;
  GUI.Contexts.alert.keydown = closeAlert;
  GUI.Contexts.alert.hoverTile = function() {};
  GUI.closeAlert = function() {
    GUI.Contexts.alert.frozen = false;
    closeAlert();
  }
  GUI.alert = function(msg, millis) {
    millis = millis || 1000;
    GUI.Contexts.alert.saved = GUI.Contexts.active;
    GUI.Contexts.alert.frozen = true;
    GUI.Contexts.active = GUI.Contexts.alert;
    HTomb.GUI.autopause = true;
    HTomb.Time.stopTime();
    HTomb.Time.stopParticles();
    alertDisplay.clear();
    alertDisplay.drawText(0,0,"%c{yellow}+" + "=".repeat(28)+"+");
    for (let i=0; i<HTomb.Constants.ALERTHEIGHT-2; i++) {
      alertDisplay.drawText(0,i+1,"%c{yellow}|");
      alertDisplay.drawText(29,i+1,"%c{yellow}|");
    }
    alertDisplay.drawText(0,11,"%c{yellow}+" + "=".repeat(28)+"+");
    let clean = HTomb.Utils.cleanText(msg);
    let hpad = Math.floor((HTomb.Constants.ALERTWIDTH-1-clean.length)/2);
    if (hpad<2) {
      hpad = 2;
    }
    // I have no idea if this formula is any good...
    let vpad = Math.floor((HTomb.Constants.ALERTHEIGHT-4-(clean.length/(HTomb.Constants.ALERTWIDTH)-4))/2);
    if (vpad<2) {
      vpad = 2;
    }
    alertDisplay.drawText(hpad,vpad,msg,HTomb.Constants.ALERTWIDTH-4);
    alert.unhide();
    setTimeout(function() {GUI.Contexts.alert.frozen = false;},millis);
  }



  return HTomb;
})(HTomb);