// The lowest-level GUI functionality, interacting with the DOM directly or through ROT.js.
HTomb = (function(HTomb) {
  const remote = (this.require) ? require('electron').remote : null;
  "use strict";
  // break out constants
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;

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
      (remote) ? "R) Restore game." : "%c{gray}R) Restore game.",
      //"F) Submit feedback or bug report.",
      "M) Read the manual.",
      (remote) ? "Q) Quit." : "%c{gray}Q) Quit."
    ];
    if (!remote) {
      txt.push("%c{yellow}Note: Saved games are currently disabled for online playtesting.");
    }
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
      (remote) ? "S) Save game ('" + HTomb.Save.currentGame +"')." : "%c{gray}S) Save game.",
      (remote) ? "A) Save game as..." : "%c{gray}A) Save game as...",
      (remote) ? "R) Restore game." : "%c{gray}R) Restore game.",
      (remote) ? "D) Delete current game('" + HTomb.Save.currentGame +"')." : "%c{gray}D) Delete game.",
      "N) New game.",
      (remote) ? "Q) Quit game." : "%c{gray}Q) Quit game.",
      "M) Read the manual."
    ]);
  };
  GUI.Contexts.system = GUI.Contexts.new({
    VK_ESCAPE: HTomb.GUI.reset,
    VK_A: function() {Views.System.saveAs();},
    VK_S: function() {Views.System.save();},
    VK_R: function() {
      if (!remote) {
        return;
      }
      HTomb.GUI.Views.parentView = HTomb.GUI.Views.systemView;
      Views.System.restore();
    },
    VK_Q: function() {Views.System.quit();},
    VK_D: function() {Views.System.delete();},
    VK_M: function() {Views.manual();},
    VK_N: function() {HTomb.World.newGame();}
  });
  Views.System = {};
  Views.System.save = function() {
    if (!remote) {
      return;
    }
    // Uses the current or default save game name
    HTomb.GUI.Views.progressView(["Saving game..."]);
    setTimeout(HTomb.Save.saveGame,500);
  };
  Views.System.delete = function() {
    if (!remote) {
      return;
    }
    if (confirm("Really delete game?")) {
      HTomb.GUI.Views.progressView(["Deleting game..."])
      setTimeout(HTomb.Save.deleteGame,500,HTomb.Save.currentGame);
    } else {
      Views.systemView();
    }
  };

  Views.System.saveAs = function() {
    if (!remote) {
      return;
    }
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
    if (!remote) {
      return;
    }
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
    if (!remote) {
      return;
    }
    HTomb.Time.stopTime();
    if (confirm("Really quit?")) {
      //Views.startup();
      remote.getCurrentWindow().close();
    }
  };

  return HTomb;
})(HTomb);
