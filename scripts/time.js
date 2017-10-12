HTomb = (function(HTomb) {
  "use strict";

  var Time = HTomb.Time;
  HTomb.Time.initialPaused = true;
  var timePassing = null;
  var speeds = ["1/4","1/2","3/4","1/1","3/2","2/1","4/1","8/1"];
  var speed = speeds.indexOf("1/1");
  HTomb.Time.inTacticalMode = false;

  var timeLocked = false;
  HTomb.Time.speedUp = function(spd) {
    speed = Math.min(speed+1,speeds.length-1);
    if (timePassing) {
      HTomb.Time.stopTime();
      HTomb.Time.startTime();
    }
  };
  HTomb.Time.slowDown = function(spd) {
    speed = Math.max(0,speed-1);
    if (timePassing) {
      HTomb.Time.stopTime();
      HTomb.Time.startTime();
    }
  };
  HTomb.Time.getSpeed = function() {
    return speeds[speed];
  };

  // remove pause and hold conditions
  HTomb.Time.lockTime = function() {
    HTomb.Time.stopTime();
    timeLocked = true;
  };
  HTomb.Time.timeIsLocked = function() {
    return timeLocked;
  }
  HTomb.Time.unlockTime = function() {
    timeLocked = false;
  };
  HTomb.Time.startTime = function() {
    HTomb.Time.initialPaused = false;
    if (timeLocked===true) {
      return;
    }
    let split = speeds[speed].split("/");
    clearInterval(timePassing);
    timePassing = setInterval(HTomb.Time.passTime,1000*split[1]/split[0]);
    if (HTomb.Time.initialPaused===false) {
      HTomb.GUI.Panels.scroll.render();
    }
  };
  HTomb.Time.stopTime = function() {
    clearInterval(timePassing);
    timePassing = null;
    if (HTomb.Time.initialPaused===false) {
      HTomb.GUI.Panels.scroll.render();
    }
  };

  // this needs to work correctly in all conditions
  HTomb.Time.toggleTime = function() {
    if (HTomb.Time.initialPaused===true && HTomb.GUI.autopause===false) {
      HTomb.GUI.autopause = true;
      HTomb.Time.initialPaused = false;
    } else if (timePassing===null || HTomb.GUI.autopause===true) {
      HTomb.GUI.autopause = false;
      HTomb.Time.startTime();
    } else {
      HTomb.GUI.autopause = true;
      HTomb.Time.stopTime();
    }
    HTomb.GUI.Panels.menu.refresh();
  };

  HTomb.Time.passTime = function() {
    if (HTomb.GUI.Contexts.locked===false) {
      HTomb.Time.autoWait();
    }
  };

  HTomb.Time.autoWait = function() {
    HTomb.Player.player.delegate.ai.acted = true;
    HTomb.Player.player.delegate.ai.actionPoints-=16;
    HTomb.Time.resumeActors(null, true);
    if (HTomb.GUI.mouseMovedLast || HTomb.GUI.Contexts.active===HTomb.GUI.Contexts.main) {
      let gameScreen = HTomb.GUI.Panels.gameScreen;
      let x = HTomb.GUI.Contexts.mouseX;
      let y = HTomb.GUI.Contexts.mouseY;
      if (x+gameScreen.xoffset>=HTomb.Constants.LEVELW || x+gameScreen.xoffset<0 || y+gameScreen.yoffset>=HTomb.Constants.LEVELH || y+gameScreen.yoffset<0) {
        HTomb.GUI.Contexts.active.mouseOver();
      } else {
        HTomb.GUI.Contexts.active.mouseTile(x+gameScreen.xoffset,y+gameScreen.yoffset);
      }
    } else {
      let keyCursor = HTomb.GUI.getKeyCursor();
      HTomb.GUI.Contexts.active.hoverTile(keyCursor[0], keyCursor[1]);
    }
  };


  var particleTime;
  var particleSpeed = 50;
  HTomb.Time.startParticles = function() {
    if (particleTime===undefined) {
      particleTime = setInterval(function() {
        HTomb.Particles.update(particleSpeed);
        HTomb.GUI.Panels.gameScreen.renderParticles();
      },particleSpeed);
    }
  };
  HTomb.Time.isPaused = function() {
    return (timePassing===null);
  };
  HTomb.Time.stopParticles = function() {
    clearInterval(particleTime);
    particleTime = undefined;
  };

  //// ****Handle turns and actions with recursive breaking ******************
  // Actors that still need to be checked
  let queue = [];
  // Actors that have already been checked
  let deck = [];
  // Recursive function for interruptable actions
  function nextActor() {
    if (queue.length===0) {
      if (deck.length===0) {
        // If the queue and deck are both exhausted, halt recursion
        HTomb.Events.publish({type: "TurnEnd"});
        HTomb.Time.dailyCycle.onTurnEnd();
        HTomb.Time.turn();
        return;
      } else {
        // If the queue is exhausted but the deck is not, reverse the deck onto the queue
        queue = deck.reverse();
        deck = [];
      }
    }
    // Take the next actor off the queue
    let actor = queue.pop();

    if (HTomb.Time.inTacticalMode && (actor===HTomb.Player || HTomb.Player.master.minions.indexOf(actor)!==-1) && actor.notactics!==true) {
      if (!HTomb.Tiles.isEnclosed(actor.x, actor.y, actor.z)) {
        HTomb.Player.player.delegate = actor;
      }
    }
    // Eventually we will do this in a more complex way to allow for round-robin combat mode
    if (actor===HTomb.Player.player.delegate && actor.ai.actionPoints>0) {
      HTomb.Events.publish({type: "PlayerActive"});
      // When we hit the player, halt recursion and update visibility
      HTomb.Player.player.visibility();
      if (HTomb.GUI.Contexts.active===HTomb.GUI.Contexts.main || HTomb.GUI.Contexts.active===HTomb.GUI.Contexts.tactical) {
        // maybe should center on active actor?
        HTomb.GUI.Panels.gameScreen.recenter();
      }
      HTomb.GUI.Panels.menu.render();
      HTomb.GUI.render();
      actor.ai.acted = false;
      return;
    } else {
      // If the actor can't act, skip it--
      if (actor.ai.actionPoints>0 && actor.isPlaced()) {
        // Act
        actor.ai.acted = false;
        let points = actor.ai.actionPoints;
        actor.ai.act();
        if (points===actor.ai.actionPoints) {
          console.log("Danger of infinite recursion from " + actor);
        }
        // If the actor can still act, put it on deck
        if (actor.ai.actionPoints>0 && actor.isPlaced()) {
          deck.push(actor);
        }
      }
    }
    // Activate recursion
    nextActor();
  }
  // Expose a method to resume queue recursion
  HTomb.Time.resumeActors = function(actor, nodelay) {
    HTomb.Time.initialPaused = false;
    if (nodelay!==true) {
      HTomb.GUI.Contexts.locked = true;
    }
    actor = actor || HTomb.Player.player.delegate;
    if (actor.ai.actionPoints>0 && actor.isPlaced()) {
      deck.push(actor);
    }
    let split = speeds[speeds.length-1].split("/");
    let maxSpeed = 1000*0.5*split[1]/split[0];
    if (nodelay!==true) {
      setTimeout(function() {
        HTomb.GUI.Contexts.locked = false;
      },maxSpeed);
    }
    split = speeds[speed].split("/");
    clearInterval(timePassing);
    if (HTomb.GUI.autopause===false) {
      timePassing = setInterval(HTomb.Time.passTime,1000*split[1]/split[0]);
    }
    nextActor();
  };
  // Process a turn of play
  HTomb.Time.turn = function() {
    // 1) Check to make sure time is unlocked
    if (timeLocked===true) {
      return;
    }
    HTomb.Time.startParticles();
    // 2) Handle TurnBegin listeners
    HTomb.Time.dailyCycle.onTurnBegin();
    HTomb.Events.publish({type: "TurnBegin"});
    for (let thing of HTomb.World.things) {
      if (thing.template===1) {
        console.log("What's wrong with this???");
        console.log(thing);
      }
    }



    // 3) Assign tasks
    HTomb.Player.master.assignTasks();
    // 4) Deal with actors
    queue = [];
    deck = [];
    for (let c in HTomb.World.creatures) {
      let cr = HTomb.World.creatures[c];
      if (cr.ai) {
        cr.ai.regainPoints();
        queue.push(cr);
      }
    }
    // Sort according to priority
    queue.sort(function(a,b) {
      if (a.ai.actionPoints < b.ai.actionPoints) {
        return -1;
      } else if (a.ai.actionPoints > b.ai.actionPoints) {
        return 1;
      } else if (a===HTomb.Player) {
        return -1;
      } else if (b===HTomb.Player) {
        return 1;
      } else if (a.spawnId < b.spawnId) {
        return -1;
      } else if (a.spawnId > b.spawnId) {
        return 1;
      } else {
        return 0;
      }
    });
    // Begin recursive traversal of the queue
    nextActor();
  };

  HTomb.Time.insertActor = function(actor) {
    //maybe we need an AI for the necro?
    if (actor.ai && actor.ai.actionPoints>0 && actor.isPlaced()) {
      // Act
      actor.ai.acted = false;
      let points = actor.ai.actionPoints;
      actor.ai.act();
      if (points===actor.ai.actionPoints) {
        console.log("Danger of infinite recursion from " + actor);
      }
      // If the actor can still act, put it on deck
      if (actor.ai.actionPoints>0 && actor.isPlaced()) {
        deck.push(actor);
      }
    }
    nextActor();
  };


  // **** Handle daily cycle
  HTomb.Constants.STARTHOUR = 8;
  //HTomb.Constants.STARTHOUR = 16;
  HTomb.Constants.DAWN = 6;
  HTomb.Constants.DUSK = 17;
  HTomb.Constants.MONTH = 12;

  HTomb.Time.dailyCycle = {
    hour: HTomb.Constants.STARTHOUR,
    minute: 0,
    day: 0,
    turn: 0,
    reset: function() {
      this.hour = HTomb.Constants.STARTHOUR;
      this.minute = 0;
      this.day = 0;
      this.turn = 0;
    },
    onTurnEnd: function() {
      this.turn+=1;
      this.minute+=1;
      if (this.minute>=60) {
        this.minute = 0;
        this.hour+=1;
        if (this.hour>=24) {
          this.hour = 0;
          this.day+=1;
        }
      }
    },
    onTurnBegin: function() {
      if (this.minute===0) {
        if (this.hour===this.times.dawn) {
          HTomb.GUI.pushMessage("The sun is coming up.");
          HTomb.World.validate.lighting();
        } else if (this.hour===this.times.dusk) {
          HTomb.GUI.pushMessage("Night is falling.");
          HTomb.World.validate.lighting();
        }
      }
      if ((this.hour>=this.times.dawn && this.hour<this.times.dawn+2)
        || (this.hour>=this.times.dusk && this.hour<this.times.dusk+2)) {
        if (this.turn%5===0) {
          HTomb.World.validate.lighting(undefined,HTomb.World.validate.lowestExposed);
        }
      }
    },
    //sunlight: {symbol: "\u263C"},
    sunlight: {symbol: "\u2600"},
    waning: {symbol: "\u263E", light: 32},
    twilight: {symbol: "\u25D2"},
    fullMoon: {symbol: "\u26AA", light: 64},
    waxing: {symbol: "\u263D", light: 32},
    newMoon: {symbol: "\u25CF", light: 1},
    times: {
      dawn: HTomb.Constants.DAWN,
      dusk: HTomb.Constants.DUSK,
      waxing: 2,
      fullMoon: 5,
      waning: 8,
      newMoon: 11,
      order: ["waxing","fullMoon","waning","newMoon"]
    },
    getPhase: function() {
      if (this.hour<this.times.dusk && this.hour>=this.times.dawn+1) {
        return this.sunlight;
      } else if (this.hour<this.times.dusk+1 && this.hour>=this.times.dawn) {
        return this.twilight;
      } else {
        return this.getMoon();
      }
      console.log(["how did we reach this?",this.day,this.tally]);
    },
    getMoon: function() {
      var phase = this.day%HTomb.Constants.MONTH;
      var tally = 0;
      for (var i=0; i<this.times.order.length; i++) {
        tally+=this.times[this.times.order[i]];
        if (phase<=tally) {
          return this[this.times.order[i]];
        }
      }
    },
    lightLevel: function() {
      var dawn = HTomb.Constants.DAWN;
      var dusk = HTomb.Constants.DUSK;
      var darkest = 64;
      var light, moonlight;
      if (this.hour < dawn || this.hour >= dusk+1) {
        moonlight = this.getMoon().light;
        return Math.round(darkest+moonlight);
      } else if (this.hour < dawn+1) {
        moonlight = this.getMoon().light;
        light = Math.min(255,(this.minute/60)*(255-darkest)+darkest+moonlight);
        return Math.round(light);
      } else if (this.hour >= dusk) {
        moonlight = this.getMoon().light;
        light = Math.min(255,((60-this.minute)/60)*(255-darkest)+darkest+moonlight);
        return Math.round(light);
      } else {
        return 255;
      }
    }
  };

  return HTomb;
})(HTomb);
