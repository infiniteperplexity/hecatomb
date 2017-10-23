// This module provides optional debugging functionality
HTomb = (function(HTomb) {
  "use strict";
  var Debug = HTomb.Debug;

  ROT.Display.Rect.cache = true;
  Debug.noingredients = true;
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

  HTomb.Debug.minimap = function() {

  };
  
  return HTomb;
})(HTomb);
