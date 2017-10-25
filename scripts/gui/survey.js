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
  let Views = GUI.Views;
  let Main = GUI.Views.Main;

  // ***** Survey mode *********
  Main.surveyMode = function() {
    HTomb.Events.publish({type: "Command", command: "SurveyMode"});
    HTomb.GUI.mouseMovedLast = false;
    Main.inSurveyMode = true;
    Contexts.active = survey;
    survey.saveX = gameScreen.xoffset;
    survey.saveY = gameScreen.yoffset;
    survey.saveZ = gameScreen.z;
    menu.middle = menu.defaultMiddle;
    menu.bottom = menu.defaultBottom;
    let keyCursor = GUI.getKeyCursor();
    GUI.render();
    GUI.Contexts.active.hoverTile(keyCursor[0], keyCursor[1]);
  };
  // Enter survey mode and save the screen's current position
  Main.surveyMove = function(dx,dy,dz) {
    var f = function() {
      HTomb.GUI.mouseMovedLast = false;
      let n = 1;
      HTomb.Events.publish({type: "Command", command: "SurveyMove"});
      if (HTomb.GUI.shiftDown()) {
        n = 8;
      }
      for (let i=0; i<n; i++) {
        if (gameScreen.z+dz < NLEVELS-1 && gameScreen.z+dz > 0) {
          gameScreen.z+=dz;
        }
        if (gameScreen.xoffset+dx < LEVELW-Math.floor(SCREENW/2) && gameScreen.xoffset+dx >= Math.ceil(-SCREENW/2)) {
          gameScreen.xoffset+=dx;
        }
        if (gameScreen.yoffset+dy < LEVELH-Math.floor(SCREENH/2) && gameScreen.yoffset+dy >= Math.ceil(-SCREENH/2)) {
          gameScreen.yoffset+=dy;
        }
      }
      GUI.render();
      let keyCursor = GUI.getKeyCursor();
      GUI.Contexts.active.hoverTile(keyCursor[0], keyCursor[1]);
    };
    // Actually this returns a custom function for each type of movement
    return f;
  };
  // The control context for surveying
  var survey = Contexts.survey = Contexts.new({
    VK_LEFT: Main.surveyMove(-1,0,0),
    VK_RIGHT: Main.surveyMove(+1,0,0),
    VK_UP: Main.surveyMove(0,-1,0),
    VK_DOWN: Main.surveyMove(0,+1,0),
    // bind keyboard movement
    VK_PERIOD: Main.surveyMove(0,0,-1),
    VK_COMMA: Main.surveyMove(0,0,+1),
    VK_NUMPAD7: Main.surveyMove(-1,-1,0),
    VK_NUMPAD8: Main.surveyMove(0,-1,0),
    VK_NUMPAD9: Main.surveyMove(+1,-1,0),
    VK_NUMPAD4: Main.surveyMove(-1,0,0),
    VK_NUMPAD5: Commands.wait,
    VK_NUMPAD6: Main.surveyMove(+1,0,0),
    VK_NUMPAD1: Main.surveyMove(-1,+1,0),
    VK_NUMPAD2: Main.surveyMove(0,+1,0),
    VK_NUMPAD3: Main.surveyMove(+1,+1,0),
    VK_RETURN: HTomb.Time.toggleTime,
    // Exit survey mode and return to the original position
    VK_ESCAPE: function() {Views.systemView();},
    VK_TAB: function() {
      HTomb.GUI.mouseMovedLast = false;
      gameScreen.z = survey.saveZ;
      gameScreen.recenter();
      if (GUI.Contexts.active===GUI.Contexts.survey) {
        Main.inSurveyMode = false;
        GUI.reset();
        //!!!!!Experimental
        //Main.tacticalMode();
      } else {
        gameScreen.render();
        let keyCursor = HTomb.GUI.getKeyCursor();
        GUI.Contexts.active.hoverTile(keyCursor[0],keyCursor[1]);
      }
    },
    VK_J: Commands.showJobs,
    VK_Z: Commands.showSpells,
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
    VK_M: function() {
      HTomb.GUI.Views.creatureView();
    },
    VK_S: function() {
      HTomb.GUI.Views.structureView();
    },
    VK_U: function() {
      HTomb.GUI.Views.summaryView();
    },
    VK_SPACE: function() {
      if (HTomb.GUI.controlDown()) {
        Commands.wait();
        return;
      } else if (HTomb.GUI.mouseMovedLast) {
        let cursor = HTomb.GUI.getMouseCursor();
        HTomb.GUI.Contexts.active.selectTile(cursor[0],cursor[1]);
      } else {
        let keyCursor = GUI.getKeyCursor();
        HTomb.GUI.Contexts.active.selectTile(keyCursor[0],keyCursor[1]);
      }
    },
    VK_A: Main.showAchievements
  });
  survey.contextName = "Survey";

  survey.menuText =
  [ "Esc: System view.",
    "%c{yellow}Navigation mode (Tab: Avatar mode)",
    " ",
    "Move: NumPad/Arrows, ,/.: Up/Down",
    "(Control+Arrows for diagonal.)",
    "Wait: NumPad 5 / Control+Space.",
    " ",
    "Enter: Enable auto-pause.",
    "+/-: Change speed.",
    " ",
    "Z: Cast spell, J: Assign job.",
    "M: Minions, S: Structures, U: Summary.",
    " ",
    "PageUp/Down: Scroll messages.",
    "A: Achievements, /: Toggle tutorial."
  ];

  survey.clickAt = function() {
    Commands.wait();
  };

  return HTomb;
})(HTomb);

