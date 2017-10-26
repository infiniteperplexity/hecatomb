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
  var ALERTHEIGHT = HTomb.Constants.ALERTHEIGHT;
  var ALERTWIDTH = HTomb.Constants.ALERTWIDTH;
  var coord = HTomb.Utils.coord;

  let GUI = HTomb.GUI;
  /// ***************Handle DOM interaction, directly or through ROT.js************
  var display = new ROT.Display({
    width: SCREENW,
    height: SCREENH,
    fontSize: FONTSIZE,
    fontFamily: FONTFAMILY,
    forceSquareRatio: true
  });
  console.log("Aspect ratio is "+display._backend._spacingX + "x"+display._backend._spacingY+".");
  let canv = display.getContainer();
  console.log("Screen dimensions are "+canv.width + "x" + canv.height+".");

  var scrollDisplay = new ROT.Display({
    width: SCROLLW,
    height: STATUSH+SCROLLH,
    fontSize: TEXTSIZE,
    fontFamily: TEXTFONT,
    spacing: TEXTSPACING
  });
  var menuDisplay = new ROT.Display({
    width: MENUW,
    height: MENUH,
    fontSize: TEXTSIZE,
    fontFamily: TEXTFONT,
    spacing: TEXTSPACING
  });
  //HTomb.GUI.Panels.gameScreen.display._context.canvas.width
  var overlayDisplay = new ROT.Display({
    width: SCREENW*CHARWIDTH/(TEXTWIDTH-TEXTSPACING)+MENUW,
    height: MENUH+1,
    fontSize: TEXTSIZE,
    fontFamily: TEXTFONT,
    spacing: TEXTSPACING
  });
  var alertDisplay = new ROT.Display({
    width: ALERTWIDTH,
    height: ALERTHEIGHT,
    fontSize: TEXTSIZE,
    fontFamily: TEXTFONT,
    spacing: TEXTSPACING
  });

  GUI.domInit = function() {
    var body = document.body;
    var div = document.createElement("div");
    div.id = "main";
    var game = document.createElement("div");
    game.id = "game";
    var scroll = document.createElement("div");
    scroll.id = "scroll";
    var menu = document.createElement("div");
    menu.id = "menu";
    var overlay = document.createElement("div");
    overlay.id = "overlay";
    var alert = document.createElement("div");
    alert.id = "alert";
    body.appendChild(div);
    div.appendChild(game);
    div.appendChild(scroll);
    div.appendChild(menu);
    div.appendChild(overlay);
    div.appendChild(alert);
    game.appendChild(display.getContainer());
    scroll.appendChild(scrollDisplay.getContainer());
    menu.appendChild(menuDisplay.getContainer());
    overlay.appendChild(overlayDisplay.getContainer());
    alert.appendChild(alertDisplay.getContainer());
  };

  // Attach input events
  var controlArrow = null;
  var shiftDown = false;
  var controlDown = false;
  GUI.autopause = false;
  var keydown = function(key) {
    let shortcuts = [
      ROT.VK_F1,
      ROT.VK_F2,
      ROT.VK_F3,
      ROT.VK_F4,
      ROT.VK_F5,
      ROT.VK_F6,
      ROT.VK_F7,
      ROT.VK_F8,
      ROT.VK_F9,
      ROT.VK_F10,
      ROT.VK_F11,
      ROT.VK_F12
    ];
    let arrows = [
      ROT.VK_UP,
      ROT.VK_DOWN,
      ROT.VK_LEFT,
      ROT.VK_RIGHT
    ];
    if (key.altKey || shortcuts.indexOf(key.keyCode)!==-1
        || (key.ctrlKey && arrows.indexOf(key.keyCode)===-1 && key.keyCode!==ROT.VK_CONTROL && key.keyCode!==ROT.VK_SPACE)) {
      return;
    } else {
      key.preventDefault();
    }
    // VK_RETURN is often used to toggle time, and stopping time first breaks that
    if (GUI.Contexts.active!==GUI.Contexts.main && GUI.autopause && key.keyCode!==ROT.VK_RETURN) {
      //experiment with no auto-pause
      HTomb.Time.stopTime();
    }
    if (key.keyCode===ROT.VK_SHIFT) {
      shiftDown = true;
    }
    if (key.keyCode===ROT.VK_CONTROL) {
      controlDown = true;
    }
    if (GUI.Contexts.locked===true) {
      return;
    }
    // Pass the keystroke to the current control context
    var diagonal = null;
    if (key.ctrlKey && arrows.indexOf(key.keyCode)>-1) {
      if (controlArrow===null) {
        controlArrow = key.keyCode;
      } else if (controlArrow===ROT.VK_UP) {
        if (key.keyCode===ROT.VK_LEFT) {
          diagonal = ROT.VK_NUMPAD7;
        } else if (key.keyCode===ROT.VK_RIGHT) {
          diagonal = ROT.VK_NUMPAD9;
        } else {
          controlArrow = key.keyCode;
        }
      } else if (controlArrow===ROT.VK_DOWN) {
        if (key.keyCode===ROT.VK_LEFT) {
          diagonal = ROT.VK_NUMPAD1;
        } else if (key.keyCode===ROT.VK_RIGHT) {
          diagonal = ROT.VK_NUMPAD3;
        } else {
          controlArrow = key.keyCode;
        }
      } else if (controlArrow===ROT.VK_LEFT) {
        if (key.keyCode===ROT.VK_UP) {
          diagonal = ROT.VK_NUMPAD7;
        } else if (key.keyCode===ROT.VK_DOWN) {
          diagonal = ROT.VK_NUMPAD1;
        } else {
          controlArrow = key.keyCode;
        }
      } else if (controlArrow===ROT.VK_RIGHT) {
        if (key.keyCode===ROT.VK_UP) {
          diagonal = ROT.VK_NUMPAD9;
        } else if (key.keyCode===ROT.VK_DOWN) {
          diagonal = ROT.VK_NUMPAD3;
        } else {
          controlArrow = key.keyCode;
        }
      }
      if (diagonal!==null) {
        GUI.Contexts.active.keydown({keyCode: diagonal});
      }
    } else if (key.keyCode===189 || key.keyCode===187) {
      if (key.keyCode===187) {
        GUI.Contexts.active.keydown({keyCode: 61});
      } else if (key.keyCode===189) {
        GUI.Contexts.active.keydown({keyCode: 173});
      }
    } else {
      GUI.Contexts.active.keydown(key);
    }
  };
  function keyup(key) {
    if (key.keyCode===controlArrow) {
      controlArrow=null;
    }
    if (key.keyCode===ROT.VK_SHIFT) {
      shiftDown = false;
    }
    if (key.keyCode===ROT.VK_CONTROL) {
      controlDown = false;
    }
  }
  HTomb.GUI.shiftDown = function() {
    return shiftDown;
  };
  HTomb.GUI.controlDown = function() {
    return controlDown;
  };
  HTomb.GUI.getKeyCursor = function() {
    return [HTomb.GUI.Panels.gameScreen.xoffset+Math.floor(HTomb.Constants.SCREENW/2),HTomb.GUI.Panels.gameScreen.yoffset+Math.floor(HTomb.Constants.SCREENH/2)];
  };

  HTomb.GUI.getMouseCursor = function() {
    var x = lastMouseX;
    var y = lastMouseY;
    var gameScreen = GUI.Panels.gameScreen;
    return [x+gameScreen.xoffset,y+gameScreen.yoffset];
  }
  // this may change a bit if I add click functionality to other canvases
  var mousedown = function(click) {
    click.preventDefault();
    if (GUI.Contexts.locked===true) {
      return;
    }
    // Convert X and Y from pixels to characters
    var x = Math.floor((click.clientX+XSKEW)/CHARWIDTH-1);
    var y = Math.floor((click.clientY+YSKEW)/CHARHEIGHT-1);
    var gameScreen = GUI.Panels.gameScreen;
    if (x+gameScreen.xoffset>=LEVELW || x+gameScreen.xoffset<0 || y+gameScreen.yoffset>=LEVELH || y+gameScreen.yoffset<0) {
      GUI.Contexts.active.mouseOver();
    }
    if (click.button===2) {
      GUI.Contexts.active.rightClickTile(x+gameScreen.xoffset,y+gameScreen.yoffset);
    } else {
      GUI.Contexts.active.clickTile(x+gameScreen.xoffset,y+gameScreen.yoffset);
    }
  };
  var lastMouseX = null;
  var lastMouseY = null;
  GUI.mouseMovedLast = false;
  var mousemove = function(move) {
    GUI.mouseMovedLast = true;
    // Convert X and Y from pixels to characters
    lastMouseX = Math.floor((move.clientX+XSKEW)/CHARWIDTH-1);
    lastMouseY = Math.floor((move.clientY+YSKEW)/CHARHEIGHT-1);
    if (GUI.Contexts.locked===true) {
      return;
    }
    var x = lastMouseX;
    var y = lastMouseY;
      // If the hover is on the game screen, pass the X and Y tile coordinates
    var gameScreen = GUI.Panels.gameScreen;
    GUI.Contexts.mouseX = x;
    GUI.Contexts.mouseY = y;
    if (x+gameScreen.xoffset>=LEVELW || x+gameScreen.xoffset<0 || y+gameScreen.yoffset>=LEVELH || y+gameScreen.yoffset<0) {
      GUI.Contexts.active.mouseOver();
    } else {
      GUI.Contexts.active.mouseTile(x+gameScreen.xoffset,y+gameScreen.yoffset);
    }
  };
  GUI.fakeMouseMove = function() {
    let x = GUI.Contexts.mouseX;
    let y = GUI.Contexts.mouseY;
    let gameScreen = GUI.Panels.gameScreen;
    if (x+gameScreen.xoffset>=LEVELW || x+gameScreen.xoffset<0 || y+gameScreen.yoffset>=LEVELH || y+gameScreen.yoffset<0) {
      GUI.Contexts.active.mouseOver();
    } else {
      GUI.Contexts.active.mouseTile(x+gameScreen.xoffset,y+gameScreen.yoffset);
    }
  };
  // Bind a ROT.js keyboard constant to a function for a particular context
  var bindKey = GUI.bindKey = function(target, key, func) {
    target.boundKeys[ROT[key]] = func;
  };

  GUI.quietUnload = true;
  function handleUnload(e) {
    if (!GUI.quietUnload) {
      let txt = "Warning: You may want to save the game before leaving this page!";
      e.returnValue = txt;
      return txt;
    }
    return null;
  }
  // Set up event listeners
  setTimeout(function() {
    //fix spacing for cross-browser compatibility
    menuDisplay._backend._spacingX = 9;
    scrollDisplay._backend._spacingX = 9;
    overlayDisplay._backend._spacingX = 9;
    window.addEventListener("keydown",keydown);
    window.addEventListener("keyup",keyup);
    window.addEventListener("beforeunload", handleUnload);
    display.getContainer().addEventListener("mousedown",mousedown);
    display.getContainer().addEventListener("mousemove",mousemove);
    window.oncontextmenu = function(e) {if (e && e.stopPropagation) {e.stopPropagation();} return false;};
    menuDisplay.getContainer().addEventListener("mousemove",function() {GUI.Contexts.active.mouseOver();});
    scrollDisplay.getContainer().addEventListener("mousemove",function() {GUI.Contexts.active.mouseOver();});
    ///!!!! Maybe get rid of the next line....
    overlayDisplay.getContainer().addEventListener("mousedown",function() {GUI.Contexts.active.clickOverlay();});
    alertDisplay.getContainer().addEventListener("mousedown",function() {GUI.Contexts.active.clickAlert();});
    console.log("adding event listeners");
  },500);

  //************* Define the basic panels and how they access the DOM *********;
  GUI.Panels = {};
  function Panel(leftx, topy, display, element, active) {
    this.x0 = leftx;
    this.y0 = topy;
    this.display = display;
    this.element = element;
    if (active===false) {
      this.active = active;
    } else {
      this.active = true;
    }

    this.render = function() {};
  }
  Panel.prototype.width = function() {
    return this.display._context.canvas.width;
  }
  Panel.prototype.hide = function() {
    document.getElementById(this.element).style.display = "none";
    this.active = false;
  }
  Panel.prototype.unhide = function() {
    document.getElementById(this.element).style.display = "initial";
    this.active = true;
  }

  GUI.Panels.gameScreen = new Panel(0,0,display);
  GUI.Panels.status = new Panel(1,0,scrollDisplay);
  GUI.Panels.scroll = new Panel(1,STATUSH,scrollDisplay);
  GUI.Panels.menu = new Panel(0,1,menuDisplay);
  GUI.Panels.overlay = new Panel(1,1,overlayDisplay,"overlay",false);
  GUI.Panels.alert = new Panel(0,0,alertDisplay,"alert",false);

  //******* Define the abstract control context *******
  GUI.Contexts = {};
  GUI.Contexts.mouseX = 0;
  GUI.Contexts.mouseY = 0;
  GUI.Contexts.locked = false;

  function Context(bindings) {
    // Pass a map of keystroke / function bindings
    if (bindings===undefined) {
      this.keydown = GUI.reset;
    } else {
      this.boundKeys = {};
      for (var b in bindings) {
        bindKey(this,b,bindings[b]);
      }
    }
  }
  Context.prototype.bindKey = function(k, f) {
    bindKey(this,k,f);
  };
  Context.prototype.keydown = function(key) {
    if (  this.boundKeys[key.keyCode]===undefined) {
      HTomb.Debug.pushMessage("No binding for " + key.keyCode);
    } else {
      this.boundKeys[key.keyCode]();
    }
  };
  Context.prototype.clickOverlay = function() {
    if (GUI.Contexts.locked) {
      return;
    }
    GUI.reset();
  }

  Context.prototype.clone = function() {
    let c = Object.create(this);
    c.boundKeys = {};
    for (let key in this.boundKeys) {
      c.boundKeys[key] = this.boundKeys[key];
    }
    return c;
  };

  GUI.Contexts.new = function(args) {
    return new Context(args);
  }
  // I don't think this line works...
  GUI.Contexts.default = Context.prototype;

  return HTomb;
})(HTomb);
