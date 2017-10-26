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
  var coord = HTomb.Utils.coord;
  // set up GUI and display
  var GUI = HTomb.GUI;
  var gameScreen = GUI.Panels.gameScreen;
  var overlay = GUI.Panels.overlay;
  var menu = GUI.Panels.menu;
  var scroll = GUI.Panels.scroll;

  let Contexts = GUI.Contexts;
  let survey = Contexts.survey;

// **** Selection and targeting methods
  GUI.selectSquareZone = function(z, callb, options) {
    options = options || {};
    let hover = options.hover || function(x, y, z, sq) {};
    //var context = Object.create(survey);
    var context = survey.clone();
    if (options.contextName) {
      context.contextName = options.contextName;
    }
    GUI.bindKey(context, "VK_ESCAPE", GUI.reset);
    context.menuText = [
      "%c{orange}**Esc: Cancel.**",
      "%c{yellow}Select first corner with keys or mouse.",
      " ",
      "Move: NumPad/Arrows, ,/.: Up/Down.",
      "(Control+Arrows for diagonal.)",
      "Wait: NumPad 5 / Control+Space.",
      " ",
      "Click / Space: Select.",
      "Enter: Toggle Pause."
    ];
    if (options.message) {
      context.menuText.unshift("");
      context.menuText.unshift(options.message);
    }
    Contexts.active = context;
    survey.saveX = gameScreen.xoffset;
    survey.saveY = gameScreen.yoffset;
    survey.saveZ = gameScreen.z;
    context.hoverTile = function(x,y) {
      Contexts.default.hoverTile(x,y);
      hover(x, y, gameScreen.z);
    };
    context.selectTile = function (x,y) {
      context.menuText[1] = "%c{yellow}Select second corner with keys or mouse.";
      var context2 = survey.clone();
      GUI.bindKey(context2, "VK_ESCAPE", GUI.reset);
      Contexts.active = context2;
      if (options.contextName) {
        context2.contextName = options.contextName;
      }
      context2.menuText = context.menuText;
      menu.refresh();
      context2.selectTile = secondSquare(x,y);
      context2.hoverTile = drawSquareBox(x,y);
      if (HTomb.GUI.mouseMovedLast===false) {
        let keyCursor2 = GUI.getKeyCursor();
        context2.hoverTile(keyCursor2[0],keyCursor2[1]);
      }
    };
    let keyCursor = GUI.getKeyCursor();
    context.hoverTile(keyCursor[0],keyCursor[1]);
    var drawSquareBox = function(x0,y0) {
      var bg = options.bg || "#550000";
      return function(x1,y1) {
        gameScreen.render();
        var xs = [];
        var ys = [];
        for (var i=0; i<=Math.abs(x1-x0); i++) {
          xs[i] = x0+i*Math.sign(x1-x0);
        }
        for (var j=0; j<=Math.abs(y1-y0); j++) {
          ys[j] = y0+j*Math.sign(y1-y0);
        }
        var squares = [];
        for (var x=0; x<xs.length; x++) {
          for (var y=0; y<ys.length; y++) {
            if (options.outline===true) {
              if (xs[x]===x0 || xs[x]===x1 || ys[y]===y0 || ys[y]===y1) {
                squares.push([xs[x],ys[y],gameScreen.z]);
              }
            } else {
              squares.push([xs[x],ys[y],gameScreen.z]);
            }
          }
        }
        for (var k =0; k<squares.length; k++) {
          var coord = squares[k];
          gameScreen.highlightTile(coord[0],coord[1],bg);
        }
        hover(x1, y1, gameScreen.z, squares);
        menu.bottom = gameScreen.examineSquare(x1,y1,gameScreen.z);
        menu.refresh();
      };
    };
    var secondSquare = function(x0,y0) {
      return function(x1,y1) {
        var xs = [];
        var ys = [];
        for (var i=0; i<=Math.abs(x1-x0); i++) {
            xs[i] = x0+i*Math.sign(x1-x0);
          }

        for (var j=0; j<=Math.abs(y1-y0); j++) {
          ys[j] = y0+j*Math.sign(y1-y0);
        }
        var squares = [];
        for (var x=0; x<xs.length; x++) {
          for (var y=0; y<ys.length; y++) {
            // If options.outline = true, use only the outline
            if (options.outline===true) {
              if (xs[x]===x0 || xs[x]===x1 || ys[y]===y0 || ys[y]===y1) {
                squares.push([xs[x],ys[y],gameScreen.z]);
              }
            } else {
              squares.push([xs[x],ys[y],gameScreen.z]);
            }
          }
        }
        // Invoke the callback function on the squares selected
        callb(squares, options);
        if (options.reset!==false) {
          GUI.reset();
        }
      };
    };
  };

  GUI.selectBox = function(width, height, z, callb, options) {
    options = options || {};
    let hover = options.hover || function(sq) {};
    var gameScreen = GUI.Panels.gameScreen;
    //var context = Object.create(survey);
    var context = survey.clone();
    if (options.contextName) {
      context.contextName = options.contextName;
    }
    GUI.bindKey(context, "VK_ESCAPE", GUI.reset);
    context.menuText = [
      "%c{orange}**Esc: Cancel**.",
      "%c{yellow}Select a box with keys or mouse.",
      " ",
      "Move: NumPad/Arrows, ,/.: Up/Down.",
      "(Control+Arrows for diagonal.)",
      "Wait: NumPad 5 / Control+Space.",
      " ",
      "Click / Space: Select.",
      "Enter: Toggle Pause."
    ];

    context.hoverTile = function(x0,y0) {
      var bg = options.bg || "#550000";
      gameScreen.render();
      var squares = [];
      for (var x=0; x<width; x++) {
        for (var y=0; y<height; y++) {
          squares.push([x0+x,y0+y,gameScreen.z]);
        }
      }
      for (var k =0; k<squares.length; k++) {
        var coord = squares[k];
        gameScreen.highlightTile(coord[0],coord[1],bg);
      }
      // maybe give the coordinates here?
      menu.bottom = [];
      hover(squares);
      menu.refresh();
    };
    Contexts.active = context;
    let keyCursor = GUI.getKeyCursor();
    context.hoverTile(keyCursor[0],keyCursor[1]);
    if (options.message) {
      context.menuText.unshift("");
      context.menuText.unshift(options.message);
    }
    menu.refresh();
    survey.saveX = gameScreen.xoffset;
    survey.saveY = gameScreen.yoffset;
    survey.saveZ = gameScreen.z;
    context.selectTile = function(x0,y0) {
      var squares = [];
      for (var y=0; y<height; y++) {
        for (var x=0; x<width; x++) {
          squares.push([x0+x,y0+y,gameScreen.z]);
        }
      }
      callb(squares,options);
      GUI.reset();
    };
  };

  GUI.choosingMenu = function(header, items, action, options) {
    options = options || {};
    let format = options.format;
    HTomb.Time.stopTime();
    var alpha = "abcdefghijklmnopqrstuvwxyz";
    var contrls = {};
    var choices = [
      "%c{orange}**Esc: Cancel**.",
      "%c{yellow}"+header
    ];
    let context = survey.clone();
    if (options.contextName) {
      context.contextName = options.contextName;
    }
    GUI.bindKey(context, "VK_ESCAPE", GUI.reset);
    for (var i=0; i<items.length; i++) {
      var desc;
      if (format) {
        desc = format(items[i]);
      } else if (items[i].describe) {
        desc = items[i].describe();
      } else {
        desc = items[i];
      }
      var choice = items[i];
      // Bind a callback function and its closure to each keystroke
      let func = action(choice);
      GUI.bindKey(context, "VK_" + alpha[i].toUpperCase(), function() {
        func();
        menu.refresh();
      });
      choices.push(alpha[i]+") " + desc);
    }
    // context.hoverTile = Contexts.main.hoverTile;
    // let keyCursor = GUI.getKeyCursor();
    // context.hoverTile(keyCursor[0],keyCursor[1]);
    Contexts.active = context;
    Contexts.active.menuText = choices;
    menu.refresh();
  };

  // Select a single square with the mouse
  GUI.selectSquare = function(z, callb, options) {
    options = options || {};
    let hover = options.hover || function(x, y, z) {};
    var context = survey.clone();
    if (options.contextName) {
      context.contextName = options.contextName;
    }
    GUI.bindKey(context, "VK_ESCAPE", GUI.reset);

    context.menuText = [
      "%c{orange}**Esc: Cancel.**",
      "%c{yellow}Select a square with keys or mouse.",
      " ",
      "Move: NumPad/Arrows, ,/.: Up/Down.",
      "(Control+Arrows for diagonal.)",
      "Wait: NumPad 5 / Control+Space.",
      " ",
      "Click / Space: Select.",
      "Enter: Toggle Pause."
    ];
    Contexts.active = context;
    if (options.message) {
      context.menuText.unshift("");
      context.menuText.unshift(options.message);
    }
    survey.saveX = gameScreen.xoffset;
    survey.saveY = gameScreen.yoffset;
    survey.saveZ = gameScreen.z;
    context.selectTile = function(x,y) {
      callb(x,y,gameScreen.z,options);
      GUI.reset();
    };
    context.hoverTile = function(x,y) {
      Contexts.default.hoverTile(x,y);
      hover(x, y, gameScreen.z);
    }
    let keyCursor = GUI.getKeyCursor();
    context.hoverTile(keyCursor[0],keyCursor[1]);
    if (options.line!==undefined) {
      var x0 = options.line.x || HTomb.Player.player.delegate.x;
      var y0 = options.line.y || HTomb.Player.player.delegate.y;
      var bg = options.line.bg || "#550000";
      context.hoverTile = function(x,y) {
        gameScreen.render();
        var line = HTomb.Path.line(x0,y0,x,y);
        for (var i in line) {
          var sq = line[i];
          gameScreen.highlightSquare(sq[0],sq[1],bg);
        }
      };
    }
  };
  return HTomb;
})(HTomb);

