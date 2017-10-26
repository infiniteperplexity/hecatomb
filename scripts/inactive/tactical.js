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
  let Commands = HTomb.Commands;
  let Views = GUI.Views = {};
  let Main = GUI.Views.Main = {};


  Main.tacticalMode = function() {
    HTomb.GUI.autopause = true;
    HTomb.Time.stopTime();
    if (HTomb.Player.notactics) {
      delete HTomb.Player.notactics;
    }
    for (let minion of HTomb.Player.master.minions) {
      if (minion.notactics) {
        delete minion.notactics;
      }
    }
    HTomb.Time.inTacticalMode = true;
    Main.inSurveyMode = false;
    Contexts.active = Contexts.tactical;
    HTomb.GUI.reset();
  };
  Main.exitTactical = function() {
    if (HTomb.Player.notactics) {
      delete HTomb.Player.notactics;
    }
    for (let minion of HTomb.Player.master.minions) {
      if (minion.notactics) {
        delete minion.notactics;
      }
    }
    HTomb.Time.inTacticalMode = false;
    Main.inSurveyMode = false;
    HTomb.Player.player.delegate = HTomb.Player;
    gameScreen.recenter();
    HTomb.GUI.reset();
  };
  let tactical = Contexts.tactical = Contexts.new({
    "VK_TAB": function() {
      Main.exitTactical();
    },
    "VK_U": function() {
      let actor = HTomb.Player.player.delegate;
      actor.notactics = true;
      HTomb.Time.insertActor(actor);
      let allno = true;
      if (HTomb.Player.notactics!==true) {
        allno = false;
      }
      for (let minion of HTomb.Player.master.minions) {
        if (minion.notactics!==true) {
          allno = false;
        }
      }
      if (allno===true) {
        Main.exitTactical();
      }
    },
    "VK_J": function() {
      //use special abilities?
    }
  });
  for (let key in Contexts.main.boundKeys) {
    // link most of the commands in tactical mode to the main mode
    let excepts = [ROT.VK_M, ROT.VK_S,ROT.VK_U,ROT.VK_J,ROT.VK_TAB];
    if (excepts.indexOf(parseInt(key))===-1) {
      tactical.boundKeys[key] = Contexts.main.boundKeys[key];
    }
  }
  tactical.clickAt = function() {
    Commands.wait();
  };
  tactical.clickTile = function(x,y) {
    this.clickAt();
  };
  tactical.rightClickTile = function(x,y) {
    this.clickTile(x,y);
  };
  tactical.contextName = "Tactical";

  tactical.menuText = [
    "Esc: System view.",
    "%c{yellow}Tactical mode (Tab: Avatar mode)",
    " ",
    "Move: NumPad/Arrows, ,/.: Up/Down.",
    "(Control+Arrows for diagonal.)",
    "Wait: NumPad 5 / Space.",
    " ",
    "Enter: Enable auto-pause.",
    "+/-: Change speed.",
    " ",
    "Z: Cast spell, J: Use ability.",
    "U: Release minion from tactics.",
    "G: Pick Up, D: Drop.",
    "I: Inventory, E: Equip/Unequip.",
    " ",
    "PageUp/Down: Scroll messages.",
    "A: Achievements, /: Toggle tutorial."
  ];

  return HTomb;
})(HTomb);

