const fs = require('fs');
// This submodule handles saving the game
HTomb = (function(HTomb) {
  "use strict";
  let LEVELW = HTomb.Constants.LEVELW;
  let LEVELH = HTomb.Constants.LEVELH;
  let NLEVELS = HTomb.Constants.NLEVELS;
  let coord = HTomb.Utils.coord;
  // Global value for the name of the current game
  HTomb.Save.currentGame = "mySaveGame";

  // synchronously stringify other stuff
  function stringifyOther() {
    let explored = HTomb.Save.stringifyThing(HTomb.World.explored, false);
    let lights = HTomb.Save.stringifyThing(HTomb.World.lights, false);
    let cycle = HTomb.Save.stringifyThing(HTomb.Time.dailyCycle, false);
    let achievements = HTomb.Achievements.list.map(function(e,i,a) {
      return [e.template,e.unlocked];
    });
    achievements = HTomb.Save.stringifyThing(achievements);
    let events = {};
    for (let i=0; i<HTomb.Events.types.length; i++) {
      events[HTomb.Events.types[i]] = HTomb.Events[HTomb.Events.types[i]];
    }
    events.types = HTomb.Events.types;
    events = HTomb.Save.stringifyThing(events);
    let other = '{'.concat(
                '"explored": ', explored, ", ",
                '"lights": ', lights, ", ",
                '"cycle": ', cycle, ", ",
                '"achievements": ', achievements, ", ",
                '"events": ', events,
                '}'
    );
    return other;
  }
  //HTomb.Save.exposeOther = function() {
  //  return stringifyOther();
  //}
  // returns a function that several rows of stringified tiles
  function stringifyTiles(z1,z2) {
    return function() {
      let levels = HTomb.World.tiles.slice(z1,z2+1);
      let tiles = HTomb.Save.stringifyThing(levels, false);
      return tiles;
    };
  };

  function stringifyCovers(z1,z2) {
    return function() {
      let levels = HTomb.World.covers.slice(z1,z2+1);
      let covers = HTomb.Save.stringifyThing(levels, false);
      return covers;
    };
  }

  var killsave = false;
  //!!!Changed to file system!!!
  HTomb.Save.saveGame = function(name) {
    HTomb.Time.lockTime();
    HTomb.GUI.Contexts.locked=true;
    name = name || HTomb.Save.currentGame;
    HTomb.Save.currentGame = name;
    killsave = false;
    for (let i=0; i<HTomb.World.things.length; i++) {
      HTomb.World.things[i].thingId = i;
    }
    let tiles = stringifyTiles(0,63)();
    let covers = stringifyCovers(0,63)();
    let other = stringifyOther();
    let things = [];
    let totalN = HTomb.World.things.length;
    for (let i=0; i<HTomb.World.things.length; i++) {
      let thing = HTomb.World.things[i];
      thing = HTomb.Save.stringifyThing(thing, true);
      things.push(thing);
    }
    things = things.join(",");
    things = "[" + things + "]";
    let json = "{" +
                  '"tiles": ' + tiles + ", " +
                  '"covers": ' + covers + ", " +
                  '"things": ' + things + ", " +
                  '"other": ' + other + "}";
    fs.writeFileSync("./saves/"+name,json);
    HTomb.GUI.splash(["Finished saving "+"'"+name+"'."]);
    for (let i=0; i<HTomb.World.things.length; i++) {
      delete HTomb.World.things[i].thingId;
    }
    HTomb.GUI.Contexts.locked=false;
    HTomb.Time.unlockTime();
  };

  // Custom JSON encoding
  HTomb.Save.stringifyThing = function(obj, topLevel) {
    try {
      let json = JSON.stringify(obj, function(key, val) {
        if (val===undefined) {
          //console.log("why is val undefined?");
          return undefined;
        } else if (val===null) {
          //console.log("could I just do null normally?");
          return null;
        } else if (key==="behaviors") {
          return undefined;
        }
        // if it has special instructions, use those to stringify
        else if (val.stringify) {
          return val.stringify();
          // if it's from the global things table, stringify it normally
        } else if (topLevel===true && val.template!==undefined) {
          topLevel = false;
          let dummy = {};
          let template = HTomb.Things.templates[val.template];
          // "makes" is a tricky one, needs special treatment for now
          for (let p in val) {
            if (p==="template" || p==="makes" || val[p]!==template[p]) {
              dummy[p] = val[p];
            }
          }
          if (dummy.thingId!==undefined) {
            delete dummy.thingId;
          }
          if (dummy.spawnId!==undefined) {
            delete dummy.spawnId;
          }
          return dummy;
        // if it's on the global things table, stringify its ID
        } else if (val.thingId!==undefined) {
          return {tid: val.thingId};
        } else {
          return val;
        }
      });
      return json;
    } catch(e) {
      console.log("messed up stringifying");
      console.log(obj);
      throw e;
    }
  };
  // End code for saving games

  //!!!Changed to file system!!!!
  HTomb.Save.getDir = function(callback) {
    console.time("get request");
    var path = './saves/';
    fs.readdir(path, function(err, items) {
      console.log(items);
      callback(JSON.stringify(items));
      HTomb.Time.unlockTime();
    });
  };

  function restoreThings(json) {
    //length of crashed save was >43 million characters.
    console.log("length of things is " +json.length);
    console.log(json.substr(0,500));
    let tids = [];
    let player = null;
    let itemLists = {};
    let things = JSON.parse(json, function (key, val) {
      if (val===null) {
        return null;
      } else if (key==="items") {
        // this doesn't work right...the thing that gets added is not the real thing
        let items = HTomb.Things.Items(this);
        for (let item of val) {
          itemLists[item.tid] = items;
        }
        return items;
      } else if (val.Type!==undefined) {
        // should not require tracking swaps
        return HTomb.Types.templates[val.Type];
      } else if (val.tid!==undefined) {
        tids.push([this,key,val]);
        return {tid: val.tid};
      } else if (val.template) {
        let template = HTomb.Things.templates[val.template];
        let dummy = Object.create(template);
        for (let p in val) {
          if (p!=="template" || val[p]!==template[p]) {
            dummy[p] = val[p];
          }
        }
        val.swappedWith = dummy;
        return dummy;
      }
      return val;
    });
    // Swap thingIDs for things
    for (let i=0; i<tids.length; i++) {
      let tid = tids[i];
      if (tid[0].swappedWith) {
        tid[0].swappedWith[tid[1]] = things[tid[2].tid];
      } else {
        tid[0][tid[1]] = things[tid[2].tid];
      }
      if (tid[1]==="player") {
        player = things[tid[2].tid];
      }
    }
    for (let tid in itemLists) {
      things[tid].onlist = undefined;
      itemLists[tid].addItem(things[tid]);
    }
    HTomb.Player = player.entity;
    HTomb.Things.templates.Player.delegate = null;
    // Fix ItemContainer references
    while(HTomb.World.things.length>0) {
      let thing = HTomb.World.things.pop();
    }
    fillListFrom(things, HTomb.World.things);
    HTomb.Things.templates.Thing.resetSpawnIds();
    var oldkeys;
    oldkeys = Object.keys(HTomb.World.creatures);
    for (let i=0; i<oldkeys.length; i++) {
      delete HTomb.World.creatures[oldkeys[i]];
    }
    oldkeys = Object.keys(HTomb.World.features);
    for (let i=0; i<oldkeys.length; i++) {
      delete HTomb.World.features[oldkeys[i]];
    }
    oldkeys = Object.keys(HTomb.World.items);
    for (let i=0; i<oldkeys.length; i++) {
      delete HTomb.World.items[oldkeys[i]];
    }
    oldkeys = Object.keys(HTomb.World.tasks);
    for (let i=0; i<oldkeys.length; i++) {
      delete HTomb.World.tasks[oldkeys[i]];
    }
    oldkeys = Object.keys(HTomb.World.blocks);
    for (let i=0; i<oldkeys.length; i++) {
      delete HTomb.World.blocks[oldkeys[i]];
    }
    for (let i=0; i<HTomb.World.encounters.length; i++) {
      delete HTomb.World.encounters[i];
    }
    for (let t = 0; t<things.length; t++) {
      let thing = things[t];
      //load floor item containers into the items list
      let x = thing.x;
      let y = thing.y;
      let z = thing.z;
      if (x===null || y===null || z===null) {
        continue;
      }
      // A lot of these things may need explicit placement
      if (thing.parent==="Creature") {
        HTomb.World.creatures[coord(x,y,z)]=thing;
      }
      if (thing.parent==="Feature") {
        HTomb.World.features[coord(x,y,z)]=thing;
        if (thing.blocking) {
          HTomb.World.blocks[coord(x,y,z)]=thing;
        }
      }
      if (thing.parent==="Task") {
        HTomb.World.tasks[coord(x,y,z)]=thing;
      }
      if (thing.parent==="Encounter") {
        HTomb.World.encounters.push(thing);
      }
      if (thing.parent==="Item") {
        if (!thing.onlist.heldby) {
          thing.onlist = null;
          let pile = HTomb.World.items[coord(x,y,z)] || HTomb.Things.Items();
          pile.addItem(thing);
          HTomb.World.items[coord(x,y,z)] = pile;
        }
      }
    }
    let behaviors = HTomb.Things.templates.Behavior.children.map(function(e,i,a) {return e.name;});
    for (let i=0; i<HTomb.World.things.length; i++) {
      let thing = HTomb.World.things[i];
      for (let key of Object.keys(thing)) {
        if (key in behaviors) {
          if (thing.behaviors===undefined) {
            thing.behaviors = [];
          }
          thing.behaviors.push(thing[key]);
        }
      }
    }
  }

  function restoreTiles(z1,z2) {
    return function(json) {
      let levels = JSON.parse(json, HTomb.Types.parseTile);
      for (let i=0; i<=z2-z1; i++) {
        for (let x=0; x<LEVELW; x++) {
          for (let y=0; y<LEVELH; y++) {
            HTomb.World.tiles[i+z1][x][y] = levels[i][x][y];
          }
        }
      }
    };
  }

  function restoreCovers(z1,z2) {
    return function(json) {
      let covers = JSON.parse(json, HTomb.Types.parseCover);
      for (let i=0; i<=z2-z1; i++) {
        for (let x=0; x<LEVELW; x++) {
          for (let y=0; y<LEVELH; y++) {
            HTomb.World.covers[i+z1][x][y] = covers[i][x][y];
          }
        }
      }

    };
  }

  function restoreOther(json) {
    let other = JSON.parse(json);
    fillGrid3dFrom(other.explored, HTomb.World.explored);
    fillListFrom(other.lights, HTomb.World.lights);
    HTomb.Time.dailyCycle.turn = other.cycle.turn;
    HTomb.Time.dailyCycle.minute = other.cycle.minute;
    HTomb.Time.dailyCycle.hour = other.cycle.hour;
    HTomb.Time.dailyCycle.day = other.cycle.day;
    let saveListeners = [];
    for (let i=0; i<HTomb.Events.types.length; i++) {
      let type = HTomb.Events.types[i];
      for (let j=0; j<HTomb.Events[type].length; j++) {
        let l = HTomb.Events[type][j];
        // everything except for instances Things
        if (l.template===undefined || l.hasOwnProperty("template")) {
          saveListeners.push([l,type]);
        }
      }
    }
    HTomb.Events.reset();
    if (other.events) {
      for (let list in other.events) {
        let list1 = other.events[list];
        let list2 = HTomb.Events[list] = [];
        for (let i=0; i<list1.length; i++) {
          if (list1[i].tid!==undefined) {
            list2.push(list1[i]);
          }
        }
      }
      HTomb.Events.types = other.events.types;
    }
    for (let i=0; i<saveListeners.length; i++) {
      HTomb.Events.subscribe(saveListeners[i][0],saveListeners[i][1]);
    }
    HTomb.Achievements.reset();
    if (other.achievements) {
      for (let i=0; i<other.achievements.length; i++) {
        HTomb.Achievements.list[i].unlocked = other.achievements[i][1];
      }
      HTomb.Achievements.resubscribe();
    }
  };

  // Anything not on the Thing list that contains references to things gets processed here
  function finalSwap() {
    for (let i=0; i<HTomb.World.lights.length; i++) {
      if (HTomb.World.lights[i].tid!==undefined) {
        HTomb.World.lights[i] = HTomb.World.things[HTomb.World.lights[i].tid];
      }
    }
    for (let i=0; i<HTomb.Events.types.length; i++) {
      let type = HTomb.Events.types[i];
      for (let j=0; j<HTomb.Events[type].length; j++) {
        if (HTomb.Events[type][j].tid!==undefined) {
          HTomb.Events[type][j] = HTomb.World.things[HTomb.Events[type][j].tid]
        }
      }
    }
  }

  //!!!Change to file system!!!!
  HTomb.Save.deleteGame = function(name) {
    fs.unlinkSync("./saves/"+name);
    HTomb.Time.unlockTime();
    HTomb.GUI.Contexts.locked=false;
    HTomb.GUI.Views.parentView = HTomb.GUI.Views.startup;
    HTomb.GUI.splash(["'" + name + "' deleted."]);
  };

  //!!!Change to file system!!!!
  HTomb.Save.restoreGame = function(name) {
    HTomb.Time.lockTime();
    HTomb.GUI.Contexts.locked=true;
    //HTomb.GUI.quietUnload = false;
    let content = fs.readFileSync("./saves/"+name);
    let json = JSON.parse(content);
    let things = JSON.stringify(json.things);
    let tiles = JSON.stringify(json.tiles);
    let covers = JSON.stringify(json.covers);
    let other = JSON.stringify(json.other);
    restoreTiles(0,63)(tiles);
    restoreCovers(0,63)(covers);
    restoreThings(things);
    restoreOther(other);
    finalSwap();
    HTomb.Save.currentGame = name;
    HTomb.Path.reset();
    HTomb.Tutorial.finish();
    HTomb.Tutorial.enabled = false;
    HTomb.Types.templates.Team.hostilityMatrix.reset();
    HTomb.World.validate.reset();
    HTomb.World.validate.all();
    HTomb.FOV.resetVisible();
    if (HTomb.Player.sight) {
      HTomb.FOV.findVisible(HTomb.Player.x, HTomb.Player.y, HTomb.Player.z, HTomb.Player.sight.range);
    }
    HTomb.GUI.Panels.gameScreen.center(HTomb.Player.x,HTomb.Player.y);
    HTomb.Time.unlockTime();
    HTomb.Time.stopTime();
    HTomb.GUI.autopause = true;
    HTomb.Time.initialPaused = true;
    HTomb.GUI.Contexts.locked = false;
    HTomb.GUI.Views.parentView = HTomb.GUI.Views.Main.reset;
    HTomb.GUI.Panels.scroll.reset();
    HTomb.GUI.splash(["Game restored."]);
    HTomb.GUI.Panels.gameScreen.recenter();
    if (HTomb.GUI.Views.Main.inSurveyMode) {
      HTomb.GUI.Contexts.survey.saveX = HTomb.GUI.Panels.gameScreen.xoffset;
      HTomb.GUI.Contexts.survey.saveY = HTomb.GUI.Panels.gameScreen.yoffset;
      HTomb.GUI.Contexts.survey.saveZ = HTomb.GUI.Panels.gameScreen.z;
    }
  };

  function fillListFrom(fromList, toList, callb) {
    // default callback is to return self
    callb = callb || function(x) {return x;};

    // if fromList is an array
    if (Array.isArray(fromList) && Array.isArray(toList)) {
      while(toList.length>0) {
        toList.pop();
      }
      for (let i=0; i<fromList.length; i++) {
        toList.push(callb(fromList[i]));
      }
    // if fromList is an associative array
    } else {
      for (let t in toList) {
        //wait...does this make any sense?
        //toList[t] = null;
        // let's try deleting instead
        delete toList[t];
      }
      for (let f in fromList) {
        toList[f] = callb(fromList[f]);
      }
    }
  };

  function fillGrid3dFrom(fromGrid, toGrid, callb) {
  // default callback is to return self
    callb = callb || function(x) {return x;};
    // pull all elements from old grid
    for (let z=0; z<NLEVELS; z++) {
      for (let x=0; x<LEVELW; x++) {
        for (let y=0; y<LEVELH; y++) {
          toGrid[z][x][y] = callb(fromGrid[z][x][y]);
        }
      }
    }
  };
  return HTomb;

})(HTomb);
