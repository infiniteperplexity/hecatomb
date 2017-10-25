// The lowest-level GUI functionality, interacting with the DOM directly or through ROT.js.
HTomb = (function(HTomb) {
  "use strict";
  // break out constants
  var SCREENW = HTomb.Constants.SCREENW;
  var SCREENH = HTomb.Constants.SCREENH;
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;

  var coord = HTomb.Utils.coord;
  // set up GUI and display
  var GUI = HTomb.GUI;
  var gameScreen = GUI.Panels.gameScreen;
  var overlay = GUI.Panels.overlay;
  var menu = GUI.Panels.menu;
  var scroll = GUI.Panels.scroll;

  let Contexts = GUI.Contexts;
  let Commands = HTomb.Commands;
  let Views = GUI.Views = {};
  let Main = GUI.Views.Main = {};

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

  Main.inSurveyMode = false;
  Main.reset = function() {
    if (HTomb.GUI.autopause===false && HTomb.Time.initialPaused!==true) {
      HTomb.Time.startTime();
    }
    if (overlay.active) {
      overlay.hide();
    }
    if (Main.inSurveyMode===true) {
      HTomb.Events.publish({type: "Command", command: "SurveyMode"});
      Main.surveyMode();
      return;
    } else if (HTomb.Time.inTacticalMode===true) {
      HTomb.Events.publish({type: "Command", command: "TacticalMode"});
      GUI.Contexts.active = GUI.Contexts.tactical;
      menu.refresh();
      let p = HTomb.Player.player.delegate;
      if (gameScreen.xoffset>p.x || gameScreen.yoffset>p.y || gameScreen.xoffset<=p.x-SCREENW || gameScreen.yoffset<=p.y-SCREENW) {
        gameScreen.recenter();
      }
      GUI.render();   
      return;
    } else {
      HTomb.Events.publish({type: "Command", command: "MainMode"});
    }
    GUI.Contexts.active = GUI.Contexts.main;
    menu.middle = menu.defaultMiddle;
    menu.bottom = menu.defaultBottom;
    menu.refresh();
    // This can be a bit annoying at times...
    // should this delegate?
    let p = HTomb.Player.player.delegate;
    //if (!GUI.getKeyCursor || gameScreen.xoffset>p.x || gameScreen.yoffset>p.y || gameScreen.xoffset<=p.x-SCREENW || gameScreen.yoffset<=p.y-SCREENW) {
    if (gameScreen.xoffset>p.x || gameScreen.yoffset>p.y || gameScreen.xoffset<=p.x-SCREENW || gameScreen.yoffset<=p.y-SCREENW) {
      gameScreen.recenter();
    }
    GUI.render();
  };
  // **** Set default controls
  // By default, dragging the mouse outside the game screen resets the game screen
  // This clears out highlighted tiles from hovering, for example
  var oldCursor = null;
  Contexts.default.mouseOver = function() {
    if (oldCursor!==null) {
      gameScreen.refreshTile(oldCursor[0],oldCursor[1]);
    }
    oldCursor = null;
  };

  function highlightTile(x,y) {
    if (oldCursor!==null) {
      gameScreen.refreshTile(oldCursor[0],oldCursor[1]);
    }
    gameScreen.highlightTile(x,y,"#00FFFF");
    oldCursor = [x,y];
  }

  Contexts.default.mouseTile = function(x,y) {
    this.hoverTile(x,y);
  };
  Contexts.default.hoverTile = function(x,y) {
    highlightTile(x,y);
    menu.bottom = examineSquare(x,y,gameScreen.z);
    menu.refresh();
  };

  // For now default click action is nothing
  Contexts.default.clickAt = function() {};

  Contexts.default.selectTile = function(x,y) {
    console.log(x + ", " + y + ", " + gameScreen.z);
    // If we clicked on a creature, go to creature view
    let c = HTomb.World.creatures[coord(x,y,gameScreen.z)];
    let visible = HTomb.World.visible[coord(x,y,gameScreen.z)];
    if (c && visible) {
      GUI.Views.creatureView(c);
      return;
    }
    // If we clicked on a workshop, go to workshop view
    let f = HTomb.World.features[coord(x,y,gameScreen.z)];
    if (f && f.structure && f.structure.isPlaced() && HTomb.World.creatures[coord(x,y,gameScreen.z)]===undefined) {
      GUI.Views.structureView(f.structure);
      return;
    }
    // Otherwise fall back on a default
    this.clickAt();
  };

  Contexts.default.clickTile = function(x,y) {
    this.selectTile(x,y);
  };

  Contexts.default.rightClickTile = function(x,y) {
    this.clickTile(x,y);
  };

  // ***** I'm not sure how to categorize this one yet...
  function examineSquare(x,y,z) {
    var square = HTomb.Tiles.getSquare(x,y,z);
    var below = HTomb.Tiles.getSquare(x,y,z-1);
    var above = HTomb.Tiles.getSquare(x,y,z+1);
    let mainColor = "%c{LightCyan}";
    let otherColor = "%c{Gainsboro}";
    let text = [];
    var next;
    text.push(mainColor + "Coord: " + square.x +"," + square.y + "," + square.z);
    if(square.explored || HTomb.Debug.explored) {
      next = mainColor + "Terrain: "+square.terrain.name;
      if (square.terrain===HTomb.Tiles.FloorTile && below.cover.liquid) {
        if (below.cover===HTomb.Covers.Water) {
          next = next + " (muddy)";
        } else if (below.cover===HTomb.Covers.Lava) {
          next = next + " (warm)";
        }
      }
      text.push(next);
      next = mainColor + "Creature: ";
      if (square.creature && (square.visible || HTomb.Debug.visible)) {
        next+=square.creature.describe({article: "indefinite"});
        text.push(next);
      }
      next = mainColor + "Items: ";
      if (square.items) {
        for (let i=0; i<square.items.length; i++) {
          next+=square.items[i].describe({article: "indefinite"});
          text.push(next);
          next = "       "+mainColor;
        }
      }
      next = mainColor + "Feature: ";
      if (square.feature) {
        next+=square.feature.describe({article: "indefinite"});
        text.push(next);
      }
      next = mainColor + "Task: ";
      if (square.task) {
        next+=square.task.describe();
        text.push(next);
      }
      next = mainColor + "Cover: ";
      if (square.cover!==HTomb.Covers.NoCover) {
        next+=square.cover.describe();
        text.push(next);
      } else if (square.terrain.zview===-1 && below.cover.liquid) {
        next+=below.cover.describe();
        next+=" (surface)";
        text.push(next);
      }
      next = mainColor + "Lighting: ";
      if (square.visible || HTomb.Debug.visible) {
        next+=Math.round(HTomb.World.lit[z][x][y]);
        text.push(next);
      }
      text.push(" ");
    }
    if (square.exploredAbove || HTomb.Debug.explored) {
      next = otherColor + "Above: "+above.terrain.name;
      text.push(next);
      next = otherColor + "Creature: ";
      if (above.creature && (square.visibleAbove || HTomb.Debug.visible)) {
        next+=above.creature.describe({article: "indefinite"});
        text.push(next);
      }
      next = otherColor + "Items: ";
      if (above.items) {
        for (let i=0; i<above.items.length; i++) {
          next+=above.items[i].describe({article: "indefinite"});
          text.push(next);
          next = otherColor+"       ";
        }
      }
      next = otherColor + "Feature: ";
      if (above.feature) {
        next+=above.feature.describe({article: "indefinite"});
        text.push(next);
      }
      next = otherColor + "Task: ";
      if (above.task) {
        next+=above.task.describe();
        text.push(next);
      }
      next = otherColor + "Cover: ";
      if (above.cover!==HTomb.Covers.NoCover) {
        next+=above.cover.describe();
        text.push(next);
      }
      next = otherColor + "Lighting: ";
      if (square.visibleAbove || HTomb.Debug.visible) {
        next+=Math.round(HTomb.World.lit[z+1][x][y]);
        text.push(next);
      }
      text.push(" ");
    }
    if (square.exploredBelow || HTomb.Debug.explored) {
      next = otherColor + "Below: "+below.terrain.name;
      text.push(next);
      next = otherColor + "Creature: ";
      if (below.creature && (square.visibleBelow || HTomb.Debug.visible)) {
        next+=below.creature.describe({article: "indefinite"});
        text.push(next);
      }
      next = otherColor + "Items: ";
      if (below.items) {
        for (let i=0; i<below.items.length; i++) {
          next+=below.items[i].describe({article: "indefinite"});
          text.push(next);
          next = otherColor+"       ";
        }
      }
      next = otherColor + "Feature: ";
      if (below.feature) {
        next+=below.feature.describe({article: "indefinite"});
        text.push(next);
      }
      next = otherColor + "Task: ";
      if (below.task) {
        next+=below.task.describe();
        text.push(next);
      }
      next = otherColor + "Cover: ";
      if (below.cover!==HTomb.Covers.NoCover) {
        next+=below.cover.describe();
        text.push(next);
      }
      next = otherColor + "Lighting: ";
      if (square.visibleBelow || HTomb.Debug.visible) {
        next+=Math.round(HTomb.World.lit[z][x][y]);
        text.push(next);
      }
    }
    return text;
  }
  gameScreen.examineSquare = examineSquare;
  // ***** Main control context ******
  // These are the default controls
  var main = Contexts.main = Contexts.new({
    // bind number pad movement
    VK_LEFT: Commands.tryMoveWest,
    VK_RIGHT: Commands.tryMoveEast,
    VK_UP: Commands.tryMoveNorth,
    VK_DOWN: Commands.tryMoveSouth,
    // bind keyboard movement
    //VK_A: Commands.act,
    VK_NUMPAD7: Commands.tryMoveNorthWest,
    VK_NUMPAD8: Commands.tryMoveNorth,
    VK_NUMPAD9: Commands.tryMoveNorthEast,
    VK_NUMPAD4: Commands.tryMoveWest,
    VK_NUMPAD5: Commands.wait,
    VK_NUMPAD6: Commands.tryMoveEast,
    VK_NUMPAD1: Commands.tryMoveSouthWest,
    VK_NUMPAD2: Commands.tryMoveSouth,
    VK_NUMPAD3: Commands.tryMoveSouthEast,
    VK_PERIOD: Commands.tryMoveDown,
    VK_COMMA: Commands.tryMoveUp,
    VK_G: Commands.pickup,
    VK_D: Commands.drop,
    VK_I: Commands.inventory,
    VK_E: Commands.equip,
    VK_J: Commands.showJobs,
    VK_Z: Commands.showSpells,
    VK_SLASH: function() {
      if (HTomb.Tutorial.enabled) {
        if (HTomb.Tutorial.active >= HTomb.Tutorial.tutorials.length-2 || confirm("Really disable tutorials?")) {
          HTomb.Tutorial.enabled = false;
          HTomb.Events.publish({type: "Command", command: "DisableTutorial"});
          if (HTomb.Time.dailyCycle.turn===0) {
            HTomb.GUI.autopause = false;
          }
        }
      } else {
        HTomb.Tutorial.enabled = true;
      }
      HTomb.GUI.Panels.menu.refresh();
    },
    VK_BACK_SPACE: function() {
      //if (HTomb.Tutorial.enabled) {
      //  HTomb.Tutorial.rewind();
      //}
    },
    VK_DELETE: function() {
      //if (HTomb.Tutorial.enabled) {
      //  HTomb.Tutorial.rewind();
      //}
    },
    VK_TAB: function() {Main.surveyMode();},
    VK_SPACE: function() {
        Commands.wait();
    },
    VK_RETURN: function() {
      HTomb.Time.toggleTime();
      if (HTomb.GUI.autopause===false) {
        HTomb.Events.publish({type: "Command", command: "UnPause"});
      }
    },
    VK_ESCAPE: function() {Views.systemView();},
    VK_HYPHEN_MINUS: function() {
      let oldSpeed = HTomb.Time.getSpeed();
      HTomb.Time.slowDown();
      if (HTomb.Time.getSpeed()!==oldSpeed) {
        HTomb.GUI.pushMessage("Speed set to " + HTomb.Time.getSpeed() + ".");
      }
    },
    VK_EQUALS: function() {
      let oldSpeed = HTomb.Time.getSpeed();
      HTomb.Time.speedUp();
      if (HTomb.Time.getSpeed()!==oldSpeed) {
        HTomb.GUI.pushMessage("Speed set to " + HTomb.Time.getSpeed() + ".");
      }
    },
    VK_PAGE_UP: function() {scroll.scrollUp();},
    VK_PAGE_DOWN: function() {scroll.scrollDown();},
    VK_M: function() {
      HTomb.GUI.Views.creatureView();
    },
    VK_S: function() {
      HTomb.GUI.Views.structureView();
    },
    VK_U: function() {
      HTomb.GUI.Views.summaryView();
    },
    VK_A: function() {
      Main.showAchievements();
    }
  });
  main.contextName = "Main";

  Main.showAchievements = function() {
    HTomb.Events.publish({type: "Command", command: "ShowAchievements"});
    HTomb.Time.stopTime();
    let txt = [
      "%c{orange}**Esc: Go back.**",
      " ",
      "%c{yellow}Achievements:",
      " "
    ];
    for (let i=0; i<HTomb.Achievements.list.length; i++) {
      let a = HTomb.Achievements.list[i];
      let s = "";
      if (!a.unlocked) {
        s+="%c{gray}";
      } else {
        s+="%c{white}";
      }
      s+=a.name;
      s+=": ";
      s+=a.description;
      txt.push(s);
    }
    HTomb.GUI.splash(txt);
  };

  Main.zoomIfNotVisible = function(x,y,z) {
    if (x >= gameScreen.xoffset+SCREENW-2) {
      gameScreen.xoffset = Math.min(Math.max(0,x-parseInt(SCREENW/2)+1),LEVELW-SCREENW);
    } else if (x <= gameScreen.xoffset) {
      gameScreen.xoffset = Math.min(Math.max(0,x-1-parseInt(SCREENW/2)),LEVELW-SCREENW);;
    }
    if (y >= gameScreen.yoffset+SCREENH-2) {
      gameScreen.yoffset = Math.min(Math.max(0,y-parseInt(SCREENH/2)+1),LEVELH-SCREENH);;
    } else if (y <= gameScreen.yoffset) {
      gameScreen.yoffset = Math.min(Math.max(0,y-1-parseInt(SCREENH/2)),LEVELH-SCREENH);;
    }
    gameScreen.z = z;
    let keyCursor = GUI.getKeyCursor();
    if (keyCursor) {
      GUI.Contexts.mouseX = keyCursor[0];
      GUI.Contexts.mouseY = keyCursor[1];
    }
    gameScreen.render();
  };

  return HTomb;
})(HTomb);

