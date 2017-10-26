// The lowest-level GUI functionality, interacting with the DOM directly or through ROT.js.
HTomb = (function(HTomb) {
  const remote = (this.require) ? require('electron').remote : null;
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
  var Views = GUI.Views;
  let menu = GUI.Panels.menu;

  // ******* Summary View ***************
  Views.summaryView = function() {
    HTomb.Time.stopTime();
    Views.Summary.summaryIndex = 0;
    GUI.Contexts.active = GUI.Contexts.summary;
    GUI.Contexts.summary.menuText = GUI.Views.Summary.summaryText();
    menu.bottom = menu.defaultBottom;
    menu.render();
  }
  GUI.Contexts.summary = GUI.Contexts.new({
    VK_ESCAPE: HTomb.GUI.reset,
    VK_UP: function() {
      Views.Summary.scrollUp();
    },
    VK_DOWN: function() {
      Views.Summary.scrollDown();
    },
    VK_RETURN: function() {
      HTomb.Time.toggleTime();
    },
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
      HTomb.GUI.pushMessage("Speed set to " + HTomb.Time.getSpeed() + ".");
      if (HTomb.Time.getSpeed()!==oldSpeed) {
        HTomb.GUI.pushMessage("Speed set to " + HTomb.Time.getSpeed() + ".");
      }
    },
    VK_SPACE: function() {
      HTomb.Commands.wait();
    },
    VK_PAGE_UP: function() {HTomb.GUI.Panels.scroll.scrollUp();},
    VK_PAGE_DOWN: function() {HTomb.GUI.Panels.scroll.scrollDown();},
  });
  GUI.Contexts.summary.mouseTile = function() {};
  Views.Summary = {};
  Views.Summary.summaryIndex = 0;
  Views.Summary.summaryText = function() {
    var text = [
      "%c{orange}**Esc: Done.**",
      "Wait: NumPad 5 / Control+Space.",
      "Click / Space: Select.",
      "Enter: Toggle Pause.",
      " ",
      "%c{yellow}Summary:",
      "Up/Down: Scroll text."
    ];
    text.push(" ");
    var s;
    text.push("Minions:");
    // none of these should delegate
    for (let i=0; i<HTomb.Player.master.minions.length; i++) {
      var cr = HTomb.Player.master.minions[i];
      s = "  "+cr.describe({atCoordinates: true})+".";
      text.push(s);
    }
    text.push(" ");
    text.push("Structures:");
    for (let k=0; k<HTomb.Player.master.structures.length; k++) {
      let w = HTomb.Player.master.structures[k];
      s = "  "+w.describe({atCoordinates: true})+".";
      text.push(s);
    }
    text.push(" ");
    text.push("Unassigned Tasks:");
    for (let k=0; k<HTomb.Player.master.taskList.length; k++) {
      var task = HTomb.Player.master.taskList[k];
      if (task.assignee===null) {
        s = "  "+task.describe({atCoordinates: true}) + ".";
        text.push(s);
      }
    }
    return text;
  };
  Views.Summary.scrollUp = function() {
    Views.Summary.summaryIndex = Math.max(0, Views.Summary.summaryIndex-1);
    GUI.Contexts.summary.menuText = GUI.Views.Summary.summaryText().splice(Views.Summary.summaryIndex,MENUH);
    menu.render();
  };
  Views.Summary.scrollDown = function() {
    Views.Summary.summaryIndex = Math.min(Views.Summary.summaryIndex+1, Math.max(0,GUI.Views.Summary.summaryText().length-MENUH));
    GUI.Contexts.summary.menuText = GUI.Views.Summary.summaryText().splice(Views.Summary.summaryIndex,MENUH);
    menu.render();
  };

  // ***************** Structure (or Structure?) view **********
  Views.structureView = function(w) {
    if (w && w.isPlaced()===false) {
      HTomb.GUI.reset();
      return;
    }
    HTomb.Time.stopTime();
    Views.Structures.structureCursor = -1;
    if (Views.Structures.selectedStructure) {
      Views.Structures.selectedStructure.unhighlight();
    }
    if (Views.Creature.selectedCreature) {
      Views.Creature.selectedCreature.unhighlight();
    }
    HTomb.Events.unsubscribe(Views.Creature,"PlayerActive");
    HTomb.Events.subscribe(Views.Structures, "PlayerActive");
    w = w || HTomb.Player.master.structures[0] || null;
    Views.Structures.selectedStructure = w;
    if (w===null) {
      GUI.Contexts.active = GUI.Contexts.structures;
      GUI.Contexts.structures.menuText = ["%c{orange}**Esc: Done.**",
      "Wait: NumPad 5 / Control+Space.",
      "Click / Space: Select.",
      "Enter: Toggle Pause.",
      " ",
      "%c{orange}You have no current structures."];
      menu.bottom = menu.defaultBottom;
      menu.render();
      return;
    }
    GUI.Contexts.active = GUI.Contexts.structures;
    let alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    for (let i=0; i<alphabet.length; i++) {
      GUI.bindKey(GUI.Contexts.structures,"VK_"+alphabet[i], function() {
        Views.Structures.selectedStructure.choiceCommand(i);
        Views.Structures.displayStructureInfo(Views.Structures.selectedStructure);
      });
    }
    Views.Structures.displayStructureInfo(w);
  };

  GUI.Contexts.structures = GUI.Contexts.new({
    VK_ESCAPE: function() {
      if (Views.Structures.selectedStructure) {
        Views.Structures.selectedStructure.unhighlight();
      }
      HTomb.Events.unsubscribe(Views.Structures,"PlayerActive");
      HTomb.GUI.reset();
    },
    VK_UP: function() {Views.Structures.structureUp();},
    VK_DOWN: function() {Views.Structures.structureDown();},
    VK_TAB: function() {Views.Structures.nextStructure();},
    VK_LEFT: function() {Views.Structures.structureLeft();},
    VK_RIGHT: function() {Views.Structures.structureRight();},
    VK_CLOSE_BRACKET: function() {Views.Structures.structureMore();},
    VK_OPEN_BRACKET: function() {Views.Structures.structureLess();},
    VK_BACK_SPACE: function() {Views.Structures.structureCancel();},
    VK_DELETE: function() {Views.Structures.structureCancel();},
    VK_RETURN: function() {
      HTomb.Time.toggleTime();
    },
    VK_SPACE: function() {
      HTomb.Commands.wait();
    },
    VK_PAGE_UP: function() {HTomb.GUI.Panels.scroll.scrollUp();},
    VK_PAGE_DOWN: function() {HTomb.GUI.Panels.scroll.scrollDown();},
  });
  GUI.Contexts.structures.mouseTile = function() {};
  Views.Structures = {};
  Views.Structures.selectedStructure = null;
  //done!
  Views.Structures.displayStructureInfo = function(w) {
    w.highlight("#557722");
    HTomb.GUI.Views.Main.zoomIfNotVisible(w.x,w.y,w.z);
    GUI.Contexts.structures.menuText = w.structureText();
    menu.bottom = menu.defaultBottom;
    menu.render();
    GUI.Contexts.active = GUI.Contexts.structures;
  };
  Views.Structures.structureCancel = function() {
    Views.Structures.selectedStructure.cancelCommand();
    Views.Structures.displayStructureInfo(Views.Structures.selectedStructure);
  };
  Views.Structures.structureDown = function() {
    Views.Structures.selectedStructure.downCommand();
    Views.Structures.displayStructureInfo(Views.Structures.selectedStructure);
  };
  Views.Structures.structureUp = function() {
    Views.Structures.selectedStructure.upCommand();
    Views.Structures.displayStructureInfo(Views.Structures.selectedStructure);
  };
  Views.Structures.structureRight = function() {
    Views.Structures.selectedStructure.rightCommand();
    Views.Structures.displayStructureInfo(Views.Structures.selectedStructure);
  };
  Views.Structures.structureLeft = function() {
    Views.Structures.selectedStructure.leftCommand();
    Views.Structures.displayStructureInfo(Views.Structures.selectedStructure);
  };
  Views.Structures.structureMore = function() {
    Views.Structures.selectedStructure.moreCommand();
    Views.Structures.displayStructureInfo(Views.Structures.selectedStructure);
  };
  Views.Structures.structureLess = function() {
    Views.Structures.selectedStructure.lessCommand();
    Views.Structures.displayStructureInfo(Views.Structures.selectedStructure);
  };
  Views.Structures.nextStructure = function() {
    Views.Structures.structureCursor = -1;
    if (Views.Structures.selectedStructure) {
      Views.Structures.selectedStructure.unhighlight();
    }
    var p = HTomb.Player;
    if (Views.Structures.selectedStructure===null && p.master.structures.length>0) {
      p = p.master.structures[0];
    } else if (p.master.structures.indexOf(Views.Structures.selectedStructure)===-1) {
      Views.Structures.selectedStructure = null;
      HTomb.GUI.reset();
      return;
    } else {
      let i = p.master.structures.indexOf(Views.Structures.selectedStructure);
      if (i===p.master.structures.length-1) {
        i = 0;
      } else {
        i+=1;
      }
      p = p.master.structures[i];
    }
    Views.Structures.selectedStructure = p;
    Views.Structures.displayStructureInfo(p);
  };
  Views.Structures.onPlayerActive = function() {
    if (Views.Structures.selectedStructure) {
      Views.Structures.displayStructureInfo(Views.Structures.selectedStructure);
    }
  };
  Views.Structures.previousStructure = function() {
    Views.Structures.structureCursor = -1;
    if (Views.Structures.selectedStructure) {
      Views.Structures.selectedStructure.unhighlight();
    }
    var p = HTomb.Player;
    if (Views.Structures.selectedStructure===null && p.master.structures.length>0) {
      let w = p.master.structures[p.master.structures.length-1];
    } else if (p.master.structures.indexOf(Views.Structures.selectedStructure)===-1) {
      Views.Structures.selectedStructure = null;
      HTomb.GUI.reset();
      return;
    } else {
      var i = p.master.structures.indexOf(Views.Structures.selectedStructure);
      if (i===0) {
        i = p.master.structures.length-1;;
      } else {
        i-=1;
      }
      let w = p.master.structures[i];
    }
    Views.Structures.selectedStructure = w;
    Views.Structures.displayStructureInfo(w);
  };

  // *********** Creature view ****************
  Views.creatureView = function(c) {
    if (c && c.isPlaced()===false) {
      HTomb.GUI.reset();
      return;
    }
    HTomb.Time.stopTime();
    if (Views.Structures.selectedStructure) {
      Views.Structures.selectedStructure.unhighlight();
    }
    HTomb.Events.unsubscribe(Views.Structures, "PlayerActive");
    HTomb.Events.subscribe(Views.Creature, "PlayerActive");
    c = c || HTomb.Player;
    if (Views.Creature.selectedCreature) {
      Views.Creature.selectedCreature.unhighlight();
    }
    Views.Creature.selectedCreature = c;
    Views.Creature.displayCreatureInfo(c);
  };
  GUI.Contexts.creatures = GUI.Contexts.new({
    VK_ESCAPE: function() {
      HTomb.Events.unsubscribe(Views.Creature,"PlayerActive");
      if (Views.Creature.selectedCreature) {
        Views.Creature.selectedCreature.unhighlight();
      }
      HTomb.GUI.reset();
    },
    VK_TAB: function() {Views.Creature.nextMinion();},
    VK_RETURN: function() {
      HTomb.Time.toggleTime();
    },
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
      HTomb.Time.startTime();
    },
    VK_SPACE: function() {
      HTomb.Commands.wait();
    },
    VK_PAGE_UP: function() {HTomb.GUI.Panels.scroll.scrollUp();},
    VK_PAGE_DOWN: function() {HTomb.GUI.Panels.scroll.scrollDown();},
  });
  GUI.Contexts.creatures.mouseTile = function() {};
  GUI.Contexts.creatures.clickAt = function() {
    HTomb.Time.toggleTime();
  };
  GUI.Contexts.creatures.rightClickTile = function(x, y) {
    this.clickTile(x,y);
  }
  Views.Creature = {};
  Views.Creature.selectedCreature = null;
  Views.Creature.displayCreatureInfo = function(c) {
    c.highlight("#557722");
    GUI.Contexts.active = GUI.Contexts.creatures;
    if (c===HTomb.Player || HTomb.Player.master.minions.indexOf(c)!==-1 || HTomb.World.visible[HTomb.Utils.coord(c.x, c.y, c.z)]) {
      HTomb.GUI.Views.Main.zoomIfNotVisible(c.x,c.y,c.z);
      GUI.Contexts.creatures.menuText = Views.Creature.creatureDetails(c);
    } else {
      let txt = [
        "%c{orange}**Esc: Done.**",
        "Wait: NumPad 5 / Control+Space.",
        "Click / Space: Select.",
        "Enter: Toggle Pause.",
        " ",
        "%c{yellow}Creature: "+c.describe({capitalized: true})+" at ??, ??, ??.",
        "Tab: View player and minions.",
        " "
      ];
      GUI.Contexts.creatures.menuText = txt;
    }
    menu.bottom = menu.defaultBottom;
    menu.render();
  };
  // may move this to Creature Behavior?
  Views.Creature.creatureDetails = function(c) {
    let txt = [
      "%c{orange}**Esc: Done.**",
      "Wait: NumPad 5 / Control+Space.",
      "Click / Space: Select.",
      "Enter: Toggle Pause.",
      " ",
      "%c{yellow}Creature: "+c.describe({capitalized: true, atCoordinates: true}),
      "Tab: Next minion.",
      " "
    ];
    if (c===HTomb.Player) {
      txt[6] = "Tab: View minions.";
    } else if (HTomb.Player.master && HTomb.Player.master.minions.indexOf(c)===-1) {
      txt[6] = "Tab: View player and minions.";
    } else if (HTomb.Player.master && c===HTomb.Player.master.minions[HTomb.Player.master.minions.length-1]) {
      txt[6] = "Tab: View player.";
    }
    if (c.actor && c.actor.target) {
      let b = c.actor.target;
      txt.push("Focus: " + b.describe({atCoordinates: true}) +".");
      txt.push(" ");
    }
    if (c.worker) {
      let b = c.worker;
      if (b.task) {
        let s = "Task: " + b.task.describe({atCoordinates: true}) + ".";
        txt.push(s);
      }
      txt.push(" ");
    }
    if (c.inventory && c.inventory.items.length>0) {
      let b = c.inventory.items;
      txt.push("Carrying: ");
      let s = "  ";
      for (let i=0; i<b.length; i++) {
        s+=b.expose(i).describe({article: "indefinite"});
        txt.push(s);
        s = "  ";
      }
      txt.push(" ");
    }
    if (c.defender) {
      let wounds = c.defender.wounds.level;
      let type = null;
      if (c.defender.wounds.type) {
        type = HTomb.Types[c.defender.wounds.type].name;
      }
      txt.push("Health: ");
      if (wounds===0) {
        txt.push(" No wounds.");
      } else if (wounds<=3) {
        txt.push(" Mild " + type + " wounds.");
      } else if (wounds<=5) {
        txt.push(" Moderate " + type + " wounds.");
      } else if (wounds<=6) {
        txt.push(" Severe " + type + " wounds.");
      } else {
        txt.push(" Critical " + type + " wounds.");
      }

    }
    return txt;
  };
  Views.Creature.onPlayerActive = function() {
  //Views.Creature.onTurnEnd = function() {
    if (Views.Creature.selectedCreature) {
      Views.Creature.displayCreatureInfo(Views.Creature.selectedCreature);
    }
  };
  Views.Creature.nextMinion = function() {
    if (Views.Creature.selectedCreature) {
      Views.Creature.selectedCreature.unhighlight();
    }
    var p = HTomb.Player;
    if ((Views.Creature.selectedCreature===null || Views.Creature.selectedCreature===p) && p.master.minions.length>0) {
      p = p.master.minions[0];

    } else if (p.master.minions.indexOf(Views.Creature.selectedCreature)===-1) {
      p = HTomb.Player;
    } else {
      var i = p.master.minions.indexOf(Views.Creature.selectedCreature);
      if (i===p.master.minions.length-1) {
        i = -1;
        p = HTomb.Player;
      } else {
        i+=1;
        p = p.master.minions[i];
      }
    }
    Views.Creature.selectedCreature = p;
    this.displayCreatureInfo(p);
  };
  Views.Creature.previousMinion = function() {
    if (Views.Creature.selectedCreature) {
      Views.Creature.selectedCreature.unhighlight();
    }
    var p = HTomb.Player;
    if (Views.Creature.selectedCreature===null && p.master.minions.length>0) {
      p = p.master.minions[p.master.minions.length-1];
    } else if (p.master.minions.indexOf(Views.Creature.selectedCreature)===-1) {
      p = HTomb.Player;
    } else {
      var i = p.master.minions.indexOf(Views.Creature.selectedCreature);
      if (i===0) {
        i = p.master.minions.length-1;
      } else {
        i-=1;
      }
      p = p.master.minions[i];
    }
    Views.Creature.selectedCreature = p;
    this.displayCreatureInfo(p);
  };

  return HTomb;
})(HTomb);