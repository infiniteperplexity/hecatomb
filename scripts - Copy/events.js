// The Events submodule, thus far unused, handles events and messaging
HTomb = (function(HTomb) {
  "use strict";

  var Events = HTomb.Events;
  Events.types = [];
  Events.subscribe = function(listener, etype) {
    if (Events[etype] === undefined) {
      Events[etype] = [];
      Events.types.push(etype);
    }
    if (Events[etype].indexOf(listener)===-1) {
      Events[etype].push(listener);
    }
  };
  Events.publish = function(event) {
    if (typeof(event)==="string") {
      event = {type: event};
    }
    if (Events[event.type] === undefined) {
      Events[event.type] = [];
      Events.types.push(event.type);
    }
    var listeners = Events[event.type] || [];
    for (var j=0; j<listeners.length; j++) {
      if (listeners[j]["on"+event.type]) {
        listeners[j]["on"+event.type](event);
      } else {
        console.log([listeners[j],event.type]);
        throw new Error("listener lacked method!");
      }
    }
    HTomb.Tutorial.onEvent(event);
  };
  Events.unsubscribeAll = function(listener) {
    for (var i=0; i<Events.types.length; i++) {
      Events.unsubscribe(listener, Events.types[i]);
    }
  };
  Events.unsubscribe = function(listener, etype) {
    if (Events[etype] === undefined) {
      Events[etype] = [];
      Events.types.push(etype);
    }
    if (Events[etype].indexOf(listener)!==-1) {
      Events[etype].splice(Events[etype].indexOf(listener),1);
    }
  };
  Events.reset = function() {
    let saveListeners = [];
    for (let i=0; i<HTomb.Events.types.length; i++) {
      let type = HTomb.Events.types[i];
      if (HTomb.Events[type]) {
        for (let j=0; j<HTomb.Events[type].length; j++) {
          let l = HTomb.Events[type][j];
          // everything except for instances Things
          if (l.template===undefined || l.hasOwnProperty("template")) {
            saveListeners.push([l,type]);
          }
        }
      }
    }
    for (let i=0; i<this.types.length; i++) {
      if (this[this.types[i]]) {
        delete this[this.types[i]];
      }
    }
    for (let i=0; i<saveListeners.length; i++) {
      HTomb.Events.subscribe(saveListeners[i][0],saveListeners[i][1]);
    }
  };

  return HTomb;
})(HTomb);
