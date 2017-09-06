// The lowest-level GUI functionality, interacting with the DOM directly or through ROT.js.
HTomb = (function(HTomb) {
  const remote = require('electron').remote;
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
  var Views = GUI.Views;
  let menu = GUI.Panels.menu;

  Views.manual = function() {
    HTomb.Time.stopTime();
    let url = window.location.href;
    let pat = /[^/]+$/;
    let match = pat.exec(url);
    let feedback = "manual.html"
    if (match===null) {
      window.open(url+feedback);
    } else {
      window.open(url.replace(match,feedback));
    };
  }
  Views.feedback = function() {
    HTomb.Time.stopTime();
    let url = window.location.href;
    let pat = /[^/]+$/;
    let match = pat.exec(url);
    let feedback = "feedback.html"
    if (match===null) {
      window.open(url+feedback);
    } else {
      window.open(url.replace(match,feedback));
    }
  };

  // ***** Code for various "frozen" views
  GUI.Contexts.frozen = GUI.Contexts.new({});
  GUI.Contexts.frozen.clickAt = function() {};
  GUI.Contexts.frozen.clickTile = function() {};
  GUI.Contexts.frozen.rightClickTile = function() {};
  GUI.Contexts.frozen.mouseTile = function() {};
  GUI.Contexts.frozen.clickOverlay = function() {};
  Views.progressView = function(arr) {
    HTomb.Time.stopTime();
    GUI.Contexts.active = GUI.Contexts.frozen;
    GUI.Panels.overlay.update(arr);
  };

  // ****** Start-up screen *******
  let introAnimation = null;
  Views.startup = function() {
    HTomb.Time.stopTime();
    GUI.quietUnload = true;
    GUI.Contexts.active = GUI.Contexts.startup;
    HTomb.Intro.setup();
    introAnimation = setInterval(introTick,250);
  };
  function introTick() {
    let txt = [
      "Welcome to Hecatomb!",
      "N) New game.",
      "R) Restore game.",
      //"F) Submit feedback or bug report.",
      "M) Read the manual.",
      "Q) Quit."
      //,
      //"%c{yellow}!!!Warning: During playtest, all players can see, save over, and restore all other players' saved games."
    ];
    GUI.Panels.overlay.update(txt);
    let yoffset = txt.length+4;
    let xoffset = 5;
    let tiles = HTomb.Intro.getTiles();
    let display= GUI.Panels.overlay.display;
    for (let y=0; y<tiles.length; y++) {
      for (let x=0; x<tiles[y].length; x++) {
        let t = tiles[y][x];
        if (HTomb.Fonts.textLookup[t[0]]===undefined) {
          if (HTomb.Fonts.charFound(t[0])) {
            HTomb.Fonts.textLookup[t[0]] = t[0];
          } else {
            HTomb.Fonts.textLookup[t[0]] = HTomb.Fonts.getBackup(t[0]);
          }
        }
        t[0] = HTomb.Fonts.textLookup[t[0]];
        display.draw(x+xoffset,y+yoffset,t[0],t[1],t[2]);
      }
    }
    HTomb.Intro.tick();
  }
  GUI.Contexts.startup = GUI.Contexts.new({
    VK_N: function() {
      clearInterval(introAnimation);
      HTomb.World.newGame();
    },
    VK_R: function() {
      clearInterval(introAnimation);
      Views.parentView = Views.startup;
      Views.System.restore();
    },
    VK_Q: function() {Views.System.quit();},
  //  VK_F: function() {Views.feedback();},
    VK_M: function() {Views.manual();}
  });
  GUI.Contexts.startup.clickOverlay = function() {};


  Views.promptView = function(prompt, callback) {
    GUI.Contexts.active = GUI.Contexts.textInput;
    GUI.Contexts.textInput.prompt = prompt;
    GUI.Contexts.textInput.callback = callback;
    GUI.Panels.overlay.update([
      GUI.Contexts.textInput.prompt,
      GUI.Contexts.textInput.currentText+"...",
      "Esc) Cancel.     Enter) Okay."
    ]);
  }
  let textInputControls = {
    VK_ESCAPE: function() {
      GUI.Contexts.textInput.currentText = "";
      HTomb.GUI.reset();
    }
  };
  let alpha = "abcdefghijklmnopqrstuvwxyz";
  for (let i=0; i<alpha.length; i++) {
    textInputControls["VK_"+alpha[i].toUpperCase()] = function() {
      if (GUI.Contexts.textInput.currentText.length>=25) {
        return;
      }
      if (HTomb.GUI.shiftDown()) {
        GUI.Contexts.textInput.currentText+=(alpha[i].toUpperCase());
      } else {
        GUI.Contexts.textInput.currentText+=alpha[i];
      }
      GUI.Panels.overlay.update([
        GUI.Contexts.textInput.prompt,
        GUI.Contexts.textInput.currentText+"...",
        "Esc) Cancel.     Enter) Okay."
      ]);
    }
  }
  let numeric = "0123456789";
  for (let i=0; i<numeric.length; i++) {
    textInputControls["VK_"+numeric[i]] = function() {
      if (GUI.Contexts.textInput.currentText.length>=25) {
        return;
      }
      GUI.Contexts.textInput.currentText+=numeric[i];
      GUI.Panels.overlay.update([
        GUI.Contexts.textInput.prompt,
        GUI.Contexts.textInput.currentText+"...",
        "Esc) Cancel.     Enter) Okay."
      ]);
    }
  }
  textInputControls["VK_SPACE"] = function() {
    if (GUI.Contexts.textInput.currentText.length>=25) {
      return;
    }
    GUI.Contexts.textInput.currentText+="_";
    GUI.Panels.overlay.update([
      GUI.Contexts.textInput.prompt,
      GUI.Contexts.textInput.currentText+"...",
      "Esc) Cancel.     Enter) Okay."
    ]);
  }
  textInputControls["VK_UNDERSCORE"] = textInputControls["VK_SPACE"];
  textInputControls["VK_HYPHEN_MINUS"] = textInputControls["VK_SPACE"];
  textInputControls["VK_BACK_SPACE"] = function() {
    GUI.Contexts.textInput.currentText = GUI.Contexts.textInput.currentText.slice(0, -1);
    GUI.Panels.overlay.update([
      GUI.Contexts.textInput.prompt,
      GUI.Contexts.textInput.currentText+"...",
      "Esc) Cancel.     Enter) Okay."
    ]);
  }
  textInputControls["VK_RETURN"] = function() {
    GUI.Contexts.textInput.callback(GUI.Contexts.textInput.currentText);
    GUI.Contexts.textInput.currentText = "";
  }
  textInputControls["VK_ENTER"] = textInputControls["VK_RETURN"];
  GUI.Contexts.textInput = GUI.Contexts.new(textInputControls);
  GUI.Contexts.textInput.currentText = "";
  // ******* System View *********
  Views.systemView = function() {
    HTomb.Time.stopTime();
    GUI.Views.parentView = GUI.Views.Main.reset;
    GUI.Contexts.active = GUI.Contexts.system;
    GUI.Panels.overlay.update([
      "Esc) Back to game.",
      "S) Save game ('" + HTomb.Save.currentGame +"').",
      "A) Save game as...",
      "R) Restore game.",
      "D) Delete current game('" + HTomb.Save.currentGame +"').",
      "N) New game.",
      "Q) Quit game.",
      "M) Read the manual."
      //,
      ///"%c{yellow}!!!Warning: During playtest, all players can see, save over, and restore all other players' saved games."
    ]);
  };
  GUI.Contexts.system = GUI.Contexts.new({
    VK_ESCAPE: HTomb.GUI.reset,
    VK_A: function() {Views.System.saveAs();},
    VK_S: function() {Views.System.save();},
    VK_R: function() {HTomb.GUI.Views.parentView = HTomb.GUI.Views.systemView; Views.System.restore();},
    VK_Q: function() {Views.System.quit();},
    VK_D: function() {Views.System.delete();},
    VK_M: function() {Views.manual();},
    VK_N: function() {HTomb.World.newGame();}
  });
  Views.System = {};
  Views.System.save = function() {
    // Uses the current or default save game name
    HTomb.GUI.Views.progressView(["Saving game..."]);
    setTimeout(HTomb.Save.saveGame,500);
  };
  Views.System.delete = function() {
    if (confirm("Really delete game?")) {
      HTomb.GUI.Views.progressView(["Deleting game..."])
      setTimeout(HTomb.Save.deleteGame,500,HTomb.Save.currentGame);
    } else {
      Views.systemView();
    }
  };

  Views.System.saveAs = function() {
    HTomb.Time.stopTime();
    HTomb.GUI.Views.parentView = HTomb.GUI.Views.systemView;
    HTomb.Save.getDir(function(arg) {
      let saves = [];
      if (arg!==" ") {
        saves = JSON.parse(arg);
      }
      var alpha = "abcdefghijklmnopqrstuvwxyz";
      var controls = {VK_ESCAPE: GUI.reset};
      for (let i=0; i<saves.length; i++) {
        controls["VK_"+alpha[i].toUpperCase()] = function() {
            let fragment = saves[i];
            return function() {
              if (confirm("Really overwrite save file?")) {
                Views.progressView(["Saving game..."]);
                setTimeout(HTomb.Save.saveGame,500,fragment);
              } else {
                return;
              }
            }
        }();
        saves[i] = alpha[i]+") " + saves[i];
      }
      saves.unshift("Choose a save file to overwrite:");
      saves.push(alpha[saves.length-1]+") ...new save...");
      controls["VK_"+alpha[saves.length-2].toUpperCase()] = function() {
        GUI.Views.promptView("Name your saved game:",function(entered) {
          console.log("trying to save!");
          entered = entered.replace(/[.,\/#!$%\^&\*;:{}=\-`~()]/g,"");
          if (entered==="") {
            HTomb.GUI.reset();
          } else {
            HTomb.GUI.Views.progressView(["Saving game..."]);
            setTimeout(HTomb.Save.saveGame, 500, entered);
          }
        });
      };
      //saves.push("%c{yellow}!!!Warning: During playtest, all players can see, save over, and restore all other players' saved games.");
      GUI.Contexts.active = GUI.Contexts.new(controls);
      GUI.Panels.overlay.update(saves);
    });
  };
  Views.System.restore = function() {
    HTomb.Time.stopTime();
    HTomb.Save.getDir(function(arg) {
      let saves = [];
      if (arg==="[]") {
        //HTomb.GUI.splash(["No saved games exist on the server."]);
        HTomb.GUI.splash(["No saved games exist."]);
        return;
      } else {
        saves = JSON.parse(arg);
      }
      var alpha = "abcdefghijklmnopqrstuvwxyz";
      var controls = {VK_ESCAPE: GUI.reset};
      for (let i=0; i<saves.length; i++) {
        controls["VK_"+alpha[i].toUpperCase()] = function() {
            let fragment = saves[i];
            return function() {
              GUI.Views.progressView(["Restoring game..."]);
              setTimeout(HTomb.Save.restoreGame, 500, fragment);
              //HTomb.Save.restoreGame(fragment);
            }
        }();
        saves[i] = alpha[i]+") " + saves[i];
      }
      saves.unshift("Choose a save file to restore:");
      //saves.push("%c{yellow}!!!Warning: During playtest, all players can see, save over, and restore all other players' saved games.");
      GUI.Contexts.active = GUI.Contexts.new(controls);
      GUI.Panels.overlay.update(saves);
    });
  };
  Views.System.quit = function() {
    HTomb.Time.stopTime();
    if (confirm("Really quit?")) {
      //Views.startup();
      remote.getCurrentWindow().close();
    }
  };

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
      if (task.task.assignee===null) {
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
    HTomb.Time.stopTime();
    Views.Structures.structureCursor = -1;
    if (Views.Structures.selectedStructure) {
      Views.Structures.selectedStructure.structure.unhighlight();
    }
    if (Views.Creature.selectedCreature) {
      Views.Creature.selectedCreature.unhighlight();
    }
    HTomb.Events.unsubscribe(Views.Creature,"PlayerActive");
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
        Views.Structures.selectedStructure.structure.choiceCommand(i);
        Views.Structures.displayStructureInfo(Views.Structures.selectedStructure);
      });
    }
    Views.Structures.displayStructureInfo(w);
  };

  GUI.Contexts.structures = GUI.Contexts.new({
    VK_ESCAPE: function() {
      if (Views.Structures.selectedStructure) {
        Views.Structures.selectedStructure.structure.unhighlight();
      }
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
    w.structure.highlight("#557722");
    HTomb.GUI.Views.Main.zoomIfNotVisible(w.x,w.y,w.z);
    GUI.Contexts.structures.menuText = w.structure.detailsText();
    menu.bottom = menu.defaultBottom;
    menu.render();
    GUI.Contexts.active = GUI.Contexts.structures;
  };
  Views.Structures.structureCancel = function() {
    Views.Structures.selectedStructure.structure.cancelCommand();
    Views.Structures.displayStructureInfo(Views.Structures.selectedStructure);
  };
  Views.Structures.structureDown = function() {
    Views.Structures.selectedStructure.structure.downCommand();
    Views.Structures.displayStructureInfo(Views.Structures.selectedStructure);
  };
  Views.Structures.structureUp = function() {
    Views.Structures.selectedStructure.structure.upCommand();
    Views.Structures.displayStructureInfo(Views.Structures.selectedStructure);
  };
  Views.Structures.structureRight = function() {
    Views.Structures.selectedStructure.structure.rightCommand();
    Views.Structures.displayStructureInfo(Views.Structures.selectedStructure);
  };
  Views.Structures.structureLeft = function() {
    Views.Structures.selectedStructure.structure.leftCommand();
    Views.Structures.displayStructureInfo(Views.Structures.selectedStructure);
  };
  Views.Structures.structureMore = function() {
    Views.Structures.selectedStructure.structure.moreCommand();
    Views.Structures.displayStructureInfo(Views.Structures.selectedStructure);
  };
  Views.Structures.structureLess = function() {
    Views.Structures.selectedStructure.structure.lessCommand();
    Views.Structures.displayStructureInfo(Views.Structures.selectedStructure);
  };
  Views.Structures.nextStructure = function() {
    Views.Structures.structureCursor = -1;
    if (Views.Structures.selectedStructure) {
      Views.Structures.selectedStructure.structure.unhighlight();
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
  Views.Structures.previousStructure = function() {
    Views.Structures.structureCursor = -1;
    if (Views.Structures.selectedStructure) {
      Views.Structures.selectedStructure.structure.unhighlight();
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
    HTomb.Time.stopTime();
    if (Views.Structures.selectedStructure) {
      Views.Structures.selectedStructure.structure.unhighlight();
    }
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
    if (c.ai && c.ai.target) {
      let b = c.ai.target;
      txt.push("Focus: " + b.describe({atCoordinates: true}) +".");
      txt.push(" ");
    }
    if (c.worker) {
      let b = c.worker;
      if (b.task) {
        let s = "Task: " + b.task.entity.describe({atCoordinates: true}) + ".";
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
    if (c.body && c.body.materials) {
      let b = c.body.materials;
      txt.push("Body: ");
      let s = "  ";
      for (let i in b) {
        s+=HTomb.Materials[i].describe() + " (" + b[i].has + " out of " + b[i].max + ")";
        txt.push(s);
        s = "  ";
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
