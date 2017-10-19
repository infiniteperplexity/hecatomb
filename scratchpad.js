

// the initial seed
Math.seed = 6;
 
// in order to work 'Math.seed' must NOT be undefined,
// so in any case, you HAVE to provide a Math.seed
Math.seededRandom = 

HTomb.Utils.seed = 6;

Events.publish = function(event) {
  if (typeof(event)==="string") {
    event = {type: event};
  }
  if (Events[event.type] === undefined) {
    Events[event.type] = [];
    Events.types.push(event.type);
  }
  var listeners = Events[event.type] || [];
  let event;
  let passed;
  for (var j=0; j<listeners.length; j++) {
    if (listeners[j]["on"+event.type]) {
      passed = listeners[j]["on"+event.type](event);
      if (passed) {
        event = passed;
      }
    } else {
      console.log([listeners[j],event.type]);
      throw new Error("listener lacked method!");
    }
  }
  HTomb.Tutorial.onEvent(event);
};




