// This module provides optional debugging functionality
HTomb = (function(HTomb) {
  "use strict";
  var Debug = HTomb.Debug;

  ROT.Display.Rect.cache = true;
  //Debug.explored = true;
  //Debug.visible = true;
  //Debug.mobility = true;
  //Debug.showpaths = true; //not yet implemented
  //Debug.messages = true;
  //Debug.faster = true;
  //Debug.paused = true;\
  //Debug.peaceful = true;
  HTomb.Debug.nextTutorial = function() {
    if (HTomb.Player.master.minions.length===0) {
      HTomb.GUI.splash([
        "(Press Escape or click to exit this screen.)",
        "1) Press Z to cast a spell.",
        "2) Press A to choose 'raise zombie'.",
        "3) Click on a tombstone to cast the spell.",
        "4) Wait for the zombie to dig its way out."
      ]);
    } else {
      HTomb.GUI.splash([
        "(Press Escape or click to exit this screen.)",
        "1) Press J to assign a job.",
        "2) Press A to dig or B to build.",
        "3) Click on two corners of the area you want to dig out or build on.",
        "4) Wait for your zombies to complete their tasks."
      ]);
    }
  }
  return HTomb;
})(HTomb);
