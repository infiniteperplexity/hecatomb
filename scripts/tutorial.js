HTomb = (function(HTomb) {
  "use strict";


  /* specify...so, if we roll back to the way things used to be, what exceptions do we need?
    - Skip to the past the first three zombie tasks if you're already waiting for the zombie.
    - Skip past




  */
  HTomb.Tutorial = {
    enabled: true,
    //enabled: false,
    active: 0,
    tutorials: [],
    templates: {},
    finish: function() {
      this.active = this.tutorials.length-1;
    },
    rewind: function() {
      while (this.active>0) {
        this.active-=1;
        if (this.tutorials[this.active].norepeat!==true && this.tutorials[this.active].rollback===null) {
          this.tutorials[this.active].tracking = {};
          break;
        }
      }
      console.log("Rewinding to tutorial: "+this.tutorials[this.active].rollback);
      HTomb.GUI.Panels.menu.render();
    },
    reset: function() {
      this.active = 0;
      for (let i=0; i<this.tutorials.length; i++) {
        this.tutorials[i].completed = false;
        this.tutorials[i].tracking = {};
      }
    },
    onEvent: function(event) {
      if (this.tutorials[this.active].listens.indexOf(event.type)!==-1) {
        let completed = this.tutorials[this.active].trigger(event);
        if (completed) {
          console.log("Completed tutorial: "+this.tutorials[this.active].template);
          this.tutorials[this.active].completed = true;
          this.tutorials[this.active].onComplete();
          if (this.active<this.tutorials.length-1) {
            this.active+=1;
            while (this.tutorials[this.active].norepeat===true) {
              this.active+=1;
            }
            this.tutorials[this.active].tracking = {};
            console.log("Beginning tutorial: "+this.tutorials[this.active].template);
            this.tutorials[this.active].onBegin();
            return;
          }
        }
      }
      if (event.type==="Command" && (event.command==="MainMode" || event.command==="SurveyMode") && this.tutorials[this.active].rollback) {
       console.log("Rolling back tutorial: "+this.tutorials[this.active].rollback);
       this.goto(this.tutorials[this.active].rollback);
       return;
     }
      let skip = this.templates[this.tutorials[this.active].skip];
      if (!skip) {
        return;
      }
      let index = this.tutorials.indexOf(skip);
      if (skip.listens.indexOf(event.type)!==-1 && skip.trigger(event)) {
        do {
          console.log("Skipping tutorial: "+this.tutorials[this.active].template);
          this.tutorials[this.active].onComplete();
          if (this.active<this.tutorials.length-1) {
            this.active+=1;
            this.tutorials[this.active].tracking = {};
            console.log("Beginning tutorial: "+this.tutorials[this.active].template);
            this.tutorials[this.active].onBegin();
          }
        } while (this.active<=index);
      }

    },
    getMenu: function(menu) {
      let active = this.tutorials[this.active];
      let obj = {
        controls: HTomb.Utils.copy(menu.top),
        instructions: null,
        middle: HTomb.Utils.copy(menu.middle),
        bottom: HTomb.Utils.copy(menu.bottom)
      };
      let context = HTomb.GUI.Contexts.active;
      if (active.contexts==="All" || active.contexts.indexOf(context.contextName)!==-1) {
        if (active.controls!==null) {
          if (typeof(active.controls)==="function") {
            let txt = obj.controls;
            txt = active.controls(txt);
            obj.controls = txt;
          } else {
            obj.controls = active.controls;
          }
        }
        if (typeof(active.instructions)==="function") {
          obj.instructions = active.instructions();
        } else {
          obj.instructions = active.instructions;
        }
        if (active.middle!==null) {
          obj.middle = active.middle;
        }
        if (active.bottom!==null) {
          obj.bottom = active.bottom;
        }
        return obj;
      } else {
        if (context.contextName==="Survey") {
          obj.instructions = ["%c{orange}%b{DarkGreen}You have strayed from the tutorial.  Press Tab to get back on track or ? to hide tutorial messages."];
        } else if (active.backupInstructions) {
          obj.instructions = active.backupInstructions;
        } else if (context.contextName!=="Main") {
          obj.instructions = ["%c{orange}%b{DarkGreen}You have strayed from the tutorial.  Press Escape to get back on track or ? to hide tutorial messages."];
        } else {
          obj.instructions = ["If you see this, something has gone wrong..."];
        }
        return obj;
      }
    },
    goto: function(arg) {
      if (typeof(arg)==="number") {
        this.active = arg;
      } else if (typeof(arg)==="string") {
        let template = this.templates[arg];
        this.active = this.tutorials.indexOf(template);
      }
      while(this.tutorials[this.active].norepeat===true) {
        this.active+=1;
      }
      this.tutorials[this.active].tracking = {};
      console.log("Beginning tutorial: "+this.tutorials[this.active].template);
      this.tutorials[this.active].onBegin();
    }
  };

  function Tutorial(args) {
    args = args || {};
    this.template = args.template;
    HTomb.Tutorial.tutorials.push(this);
    HTomb.Tutorial.templates[this.template] = this;
    this.contexts = args.contexts || ["Main"];
    this.controls = args.controls || null;
    this.instructions = args.instructions || null;
    this.backupInstructions = args.backupInstructions || null;
    this.completed = false;
    this.norepeat = false;
    this.skip = args.skip || null;
    this.rollback = args.rollback || null;
    this.middle = args.middle || null;
    this.bottom = args.bottom || null;
    this.listens = args.listens || [];
    this.onBegin = args.onBegin || function() {};
    this.onComplete = args.onComplete || function() {};
    this.trigger = args.trigger || function () {return false};
    this.tracking = {};
  }

  new Tutorial({
    template: "WelcomeAndMovementStepOne",
    name: "welcome and movement",
    controls: [
      "Esc: System view.",
      " ",
      "%c{cyan}Move: NumPad/Arrows.",
      "(Control+Arrows for diagonal.)",
      " ",
      "/: Toggle tutorial."
    ],
    instructions: [
      "%c{yellow}Welcome to the Hecatomb in-game tutorial.  Follow the instructions on this panel to proceed through the tutorial, or press ? to turn off these messages and play without the tutorial.",
      " ",
      "%c{white}You walk amongst the tombstones of a hillside graveyard, searching for the site upon which you will build your mighty fortress.",
      " ",
      "%c{lime}This symbol is you: %c{magenta}@",
      " ",
      "%c{cyan}%b{DarkRed}Try walking around using the numeric keypad.  If your keyboard has no keypad, use the arrow keys.  One turn will pass for each step you take."
    ],
    listens: ["Command"],
    skip: "RaisingAZombieStepOne",
    onBegin: function() {
      if (HTomb.Tutorial.enabled) {
        //HTomb.Time.slowDown();
        //HTomb.Time.slowDown();
        HTomb.GUI.autopause = true;
      }
    },
    trigger: function(event) {
      if (!this.tracking.moves) {
        this.tracking.moves = 0;
      }
      if (event.command==="Move") {
        this.tracking.moves+=1;
      }
      return (this.tracking.moves>=5);
    }
  });

  new Tutorial({
    template: "WelcomeAndMovementStepTwo",
    name: "welcome and movement",
    controls: [
      "Esc: System view.",
      " ",
      "%c{cyan}Move: NumPad/Arrows.",
      "(Control+Arrows for diagonal.)",
      " ",
      "/: Toggle tutorial."
      //,"Backspace / Delete: Previous tutorial."
    ],
    instructions: [
      "%c{white}You walk amongst the tombstones of a hillside graveyard, searching for the site upon which you will build your mighty fortress.",
      " ",
      '- Green areas with " are grass.',
      " ",
      "- Dim green areas are also grass, one elevation level below you.",
      " ",
      "- Gray areas with # are walls, but they may have walkable floors one level above you.",
      " ",
      "- Other symbols (\u2663, \u2660, \u2698) may be",
      "trees or plants.",
      " ",
      "- Letters such as 's' or 'b' are wild animals, mostly harmless for now.",
      " ",
      "%c{cyan}%b{DarkRed}Try walking around using the numeric keypad.  If your keyboard has no keypad, use the arrow keys.  One turn will pass for each step you take."
    ],
    listens: ["Command"],
    skip: "RaisingAZombieStepOne",
    //onBegin: function() {
      //if (HTomb.Tutorial.enabled) {
      //  HTomb.GUI.autopause = true;
      //}
    //},
    trigger: function(event) {
      if (!this.tracking.moves) {
        this.tracking.moves = 0;
      }
      if (event.command==="Move") {
        this.tracking.moves+=1;
      }
      return (this.tracking.moves>=10);
    }
  });

  new Tutorial({
    template: "ClimbingSlopes",
    name: "climbing slopes",
    controls: [
      "Esc: System view.",
      " ",
      "Move: NumPad/Arrows, %c{cyan},/.: Up/Down.",
      "(Control+Arrows for diagonal.)",
      " ",
      "/: Toggle tutorial."
      //,"Backspace / Delete: Previous tutorial."
    ],
    instructions: [
      "%c{white}You scramble up and down the slopes for a better view of the area.",
      " ",
      "- The \u02C4 and \u02C5 symbols are slopes.",
      " ",
      "- The game world is a 3D grid of tiles.  Slopes provide access to different elevation levels.",
      " ",
      "- You can climb up or down a slope by standing on it and pressing . (<) or , (>).",
      " ",
      "- If you try to walk sideways off a cliff or into a wall, you will automatically climb a slope instead if possible.",
      " ",
      "- When you climb up or down, colors change with your relative elevation.",
      " ",
      "%c{cyan}%b{DarkRed}Try climbing up and down a few slopes."
    ],
    listens: ["Command"],
    skip: "RaisingAZombieStepOne",
    onBegin: function() {
      if (HTomb.Player.master.minions.length>=1) {
        let m = HTomb.Player.master.minions[0];
        if (m.worker.task && m.worker.task.template==="ZombieEmergeTask") {
          HTomb.Tutorial.templates.RaisingAZombieStepThree.onComplete();
          HTomb.Tutorial.goto("AchievementsAndScrolling");
        } else {
          HTomb.Tutorial.templates.RaisingAZombieStepThree.onComplete();
          HTomb.Tutorial.templates.WaitingForTheZombie.onComplete();
          HTomb.Tutorial.goto("UnpausingAndChangingSpeeds");
        }
      }
    },
    trigger: function(event) {
      if (this.tracking.ups===undefined && this.tracking.downs===undefined) {
        this.tracking.ups = 0;
        this.tracking.downs = 0;
      }
      if (event.command==="Move" && event.dir==='U') {
        this.tracking.ups+=1;
      } else if (event.command==="Move" && event.dir==='D') {
        this.tracking.downs+=1;
      }
      return (this.tracking.ups+this.tracking.downs>=3);
    }
  });

  new Tutorial({
    template: "RaisingAZombieStepOne",
    name: "raising a zombie",
    controls: [
      "Esc: System view.",
      " ",
      "Move: NumPad/Arrows, ,/.: Up/Down.",
      "(Control+Arrows for diagonal.)",
      " ",
      "%c{cyan}Z: Cast spell.",
      " ",
      "/: Toggle tutorial."
      //,"Backspace / Delete: Previous tutorial."
    ],
    instructions: [
      "%c{white}Enough of this pointless wandering - it is time to summon an undead servant.",
      " ",
      "Near where you started, there should be some symbols like this: \u2670. These are tombstones.  If you want to know what a symbol represents, hover over it with the mouse and look at the bottom half of the right panel.",
      " ",
      "%c{cyan}%b{DarkRed}Find a tombstone - you don't have to stand right next to it.  Then press Z to view a list of spells you can cast, and press A to choose 'raise zombie.'"
    ],
    listens: ["Command"],
    skip: "WaitingForTheZombie",
    trigger: function(event) {
      return (event.command==="ShowSpells");
    }
  });

  new Tutorial({
    template: "RaisingAZombieStepTwo",
    name: "raising a zombie",
    contexts: ["ShowSpells"],
    controls: function(txt) {
      txt[2] = "%c{cyan}" + txt[2];
      return txt;
    },
    instructions: HTomb.Tutorial.templates.RaisingAZombieStepOne.instructions,
    backupInstructions: ["%c{cyan}%b{DarkRed}Find a tombstone - you don't have to stand right next to it.  Then press Z to view a list of spells you can cast, and press A to choose 'raise zombie.'"],
    listens: ["Command"],
    skip: "WaitingForTheZombie",
    rollback: "RaisingAZombieStepOne",
    trigger: function(event) {
      return (event.command==="ChooseSpell" && event.spell.template==="RaiseZombie");
    }
  });


  new Tutorial({
    template: "RaisingAZombieStepThree",
    name: "raising a zombie",
    contexts: ["CastRaiseZombie"],
    controls: [
      "%c{orange}**Esc: Cancel.**",
      "%c{yellow}Select a square with keys or mouse.",
      " ",
      "Move: NumPad/Arrows, ,/.: Up/Down.",
      "(Control+Arrows for diagonal.)",
      " ",
      "%c{cyan}Click / Space: Select.",
    ],
    instructions: [
      "%c{cyan}%b{DarkRed}Select a tombstone, either by using the mouse, or by navigating with the direction keys and pressing space to select.  Make sure the tombstone is on the current elevation level.",
      " ",
      "Notice that the bottom portion of this panel gives you information about the square you are hovering over - whether it's a valid target for your spell, what the terrain is like, and so on."
    ],
    backupInstructions: HTomb.Tutorial.templates.RaisingAZombieStepTwo.backupInstructions,
    listens: ["Cast"],
    skip: "WaitingForTheZombie",
    rollback: "RaisingAZombieStepOne",
    trigger: function(event) {
      return (event.spell.template==="RaiseZombie");
    },
    onComplete: function() {
      HTomb.Tutorial.templates.RaisingAZombieStepOne.norepeat = true;
      HTomb.Tutorial.templates.RaisingAZombieStepTwo.norepeat = true;
      HTomb.Tutorial.templates.RaisingAZombieStepThree.norepeat = true;
    }
  });

  new Tutorial({
    template: "AchievementsAndScrolling",
    name: "achievements and scrolling",
    controls: [
      "Esc: System view.",
      " ",
      "Move: NumPad/Arrows, ,/.: Up/Down.",
      "(Control+Arrows for diagonal.)",
      " ",
      "Enter: Enable auto-pause.",
      "+/-: Change speed.",
      " ",
      "Z: Cast spell.",
      " ",
      "%c{cyan}PageUp/Down: Scroll messages.",
      "%c{cyan}A: Achievements, %c{}/: Toggle tutorial."
      //,"Backspace / Delete: Previous tutorial."
    ],
    instructions: [
      "%c{white}Forbidden runes swirl around you as you call forth a corpse from its grave.",
      " ",
      "You just earned an achievement, as noted on the message bar below the play area.  You can scroll messages up and down using the PageUp and PageDown keys (on a Mac, Fn+Arrows.)",
      " ",
      "%c{cyan}%b{DarkRed}Press 'A' to view the achievements screen."
    ],
    listens: ["Command"],
    skip: "WaitingForTheZombie",
    onBegin: function() {
      if (HTomb.Player.master.minions.length>=1) {
        let m = HTomb.Player.master.minions[0];
        if (!m.worker.task || m.worker.task.template!=="ZombieEmergeTask") {
          this.onComplete();
          HTomb.Tutorial.goto("UnpausingAndChangingSpeeds");
        }
      }
    },
    trigger: function(event) {
      if (this.tracking.achievements===undefined) {
        this.tracking.achievements = false;
      }
      if (this.tracking.reset===undefined) {
        this.tracking.reset = false;
      }
      if (event.command==="ShowAchievements") {
        this.tracking.achievements = true;
      }
      if (this.tracking.achievements===true && event.command==="MainMode") {
        this.tracking.reset = true;
      }
      return (this.tracking.achievements===true && this.tracking.reset===true);
    }
  });

  new Tutorial({
    template: "WaitingForTheZombie",
    name: "waiting for the zombie",
    controls: [
      "Esc: System view.",
      " ",
      "Move: NumPad/Arrows, ,/.: Up/Down.",
      "(Control+Arrows for diagonal.)",
      "%c{cyan}Wait: NumPad 5 / Space.",
      " ",
      "Z: Cast spell.",
      " ",
      "PageUp/Down: Scroll messages.",
      "A: Achievements, /: Toggle tutorial."
      //,"Backspace / Delete: Previous tutorial."
    ],
    instructions: [
      "%c{white}You wait, smiling grimly as your undead thrall claws its way out of its grave.",
      " ",
      "The orange background around the tombstone indicates that there is a task assigned in that square.",
      " ",
      '%c{cyan}%b{DarkRed}Press 5 on the numeric keypad several times, to pass turns ("wait") until your zombie emerges.  If you have no numeric keypad, press Space to wait.'
    ],
    listens: ["Complete"],
    skip: "AssignAJob",
    trigger: function(event) {
      return (event.task && event.task.template==="ZombieEmergeTask");
    },
    onComplete: function() {
      // the first three should already be true, but...
      HTomb.Tutorial.templates.RaisingAZombieStepOne.norepeat = true;
      HTomb.Tutorial.templates.RaisingAZombieStepTwo.norepeat = true;
      HTomb.Tutorial.templates.RaisingAZombieStepThree.norepeat = true;
      HTomb.Tutorial.templates.WaitingForTheZombie.norepeat = true;
    }
  });

  new Tutorial({
    template: "UnpausingAndChangingSpeeds",
    name: "unpausing and changing speeds",
    controls: [
      "Esc: System view.",
      " ",
      "Move: NumPad/Arrows, ,/.: Up/Down.",
      "(Control+Arrows for diagonal.)",
      "Wait: NumPad 5 / Space.",
      " ",
      "%c{cyan}Enter: Enable auto-pause.",
      "%c{cyan}+/-: Change speed.",
      " ",
      "Z: Cast spell.",
      " ",
      "PageUp/Down: Scroll messages.",
      "A: Achievements, /: Toggle tutorial."
      //,"Backspace / Delete: Previous tutorial."
    ],
    instructions: [
      "%c{white}Your minion bursts forth from the ground!",
      " ",
      "The word 'Pause' above the right-hand side of the message bar.  The game is currently auto-paused - one turn will pass for each action you take.  If you turn auto-pause off, turns will pass in realtime even if you take no actions.  You can press + or - to make time pass faster or slower.",
      " ",
      "%c{cyan}%b{DarkRed}Press Enter / Return to turn off auto-pause, then wait for several turns to pass.",
      " ",
      "Your zombie will wander a short distance from you.  If it seems to disappear, it probably went up or down a slope."
    ],
    listens: ["TurnBegin","Command"],
    skip: "AssignAJob",
    trigger: function(event) {
      if (event.type==="Command" && event.command==="UnPause") {
        this.tracking.unpaused = true;
      }
      if (!this.tracking.turns) {
        this.tracking.turns = 0;
      }
      this.tracking.turns+=1;
      return (this.tracking.turns>=15 && this.tracking.unpaused);
    }
  });

  new Tutorial({
    template: "AssignAJob",
    name: "assign a job",
    controls: [
      "Esc: System view.",
      " ",
      "Move: NumPad/Arrows, ,/.: Up/Down.",
      "(Control+Arrows for diagonal.)",
      "Wait: NumPad 5 / Space.",
      " ",
      "Enter: Enable auto-pause.",
      "+/-: Change speed.",
      " ",
      "Z: Cast spell, %c{cyan}J: Assign job.",
      " ",
      "PageUp/Down: Scroll messages.",
      "A: Achievements, /: Toggle tutorial."
      //,"Backspace / Delete: Previous tutorial."
    ],
    instructions: [
      "%c{white}You close your eyes and concentrate, formulating a task for your unthinking slave.",
      " ",
      "%c{cyan}%b{DarkRed}Press J to assign a job, and then press A to make your zombie dig.  You can assign a job from any distance.",
      " ",
      "(Remember, if auto-pause is turned off, you can turn it back on by pressing Enter / Return.)"
    ],
    listens: ["Command"],
    skip: "WaitingForDig",
    trigger: function(event) {
      return (event.command==="ShowJobs");
    }
  });

  new Tutorial({
    template: "ChooseAJob.",
    name: "choose a job",
    contexts: ["ShowJobs"],
    controls: function(txt) {
      txt[2] = "%c{cyan}" + txt[2];
      return txt;
    },
    instructions: HTomb.Tutorial.templates.AssignAJob.instructions,
    backupInstructions: HTomb.Tutorial.templates.AssignAJob.instructions,
    listens: ["Command"],
    skip: "WaitingForDig",
    rollback: "AssignAJob",
    trigger: function(event) {
      return (event.command==="ChooseJob" && event.task && event.task.template==="DigTask");
    }
  });

  new Tutorial({
    template: "DesignateTilesForDigging",
    name: "designate tiles for digging",
    contexts: ["DesignateDigTask"],
    controls: null,
    instructions: [
      "%c{cyan}%b{DarkRed}Using the mouse or keyboard, select two corners of a rectangular area for your zombie to dig.",
      " ",
      "What 'dig' means is contextual, depending on the terrain you select:",
      " ",
      "- Digging on the floor will make a pit.",
      " ",
      "- Digging in a wall will make a tunnel.",
      " ",
      "- Digging on a slope levels the slope.",
      " ",
      "Look below this panel for a hint about   what digging does in the highlighted square."
    ],
    backupInstructions: HTomb.Tutorial.templates.AssignAJob.instructions,
    listens: ["Designate"],
    skip: "WaitingForDig",
    rollback: "AssignAJob",
    trigger: function(event) {
      return (event.task && event.task.template==="DigTask");
    }
  });

  new Tutorial({
    template: "WaitingForDig",
    name: "waiting for zombie to dig",
    controls: [
      "Esc: System view.",
      " ",
      "Move: NumPad/Arrows, ,/.: Up/Down.",
      "(Control+Arrows for diagonal.)",
      "%c{cyan}Wait: NumPad 5 / Space.",
      " ",
      "Enter: Enable auto-pause.",
      "+/-: Change speed.",
      " ",
      "Z: Cast spell, J: Assign job.",
      " ",
      "PageUp/Down: Scroll messages.",
      "A: Achievements, /: Toggle tutorial."
      //,"Backspace / Delete: Previous tutorial."
    ],
    instructions: [
      "%c{white}The zombie shuffles dutifully to complete its task.",
      " ",
      "%c{cyan}%b{DarkRed}Now wait (pass turns) while your zombie digs.",
      " ",
      "There is a chance that you will unlock one or more additional achievements, depending on where your zombie digs and what it finds.  Note that some types of soil or stone may be too hard to dig through, at least until your zombies equip better tools."
    ],
    skip: "WaitForSecondZombie",
    listens: ["Complete"],
    trigger: function(event) {
      // special handling for the one tutorial you're allowed to do out of order
      if (event.task && event.task.template==="ZombieEmergeTask" && HTomb.Player.master.minions.length>=2) {
          HTomb.Tutorial.templates.RaiseASecondZombie.norepeat = true;
          HTomb.Tutorial.templates.WaitForSecondZombie.norepeat = true;
          HTomb.Tutorial.goto("NavigationModeStepOne");
          return false;
      }
      return (event.task && event.task.template==="DigTask");
    }
  });

  new Tutorial({
    template: "RaiseASecondZombie",
    name: "raise a second zombie",
    contexts: ["Main","ShowSpells","CastRaiseZombie"],
    controls: function(txt) {
      let context = HTomb.GUI.Contexts.active.contextName;
      if (context==="Main") {
        return [
          "Esc: System view.",
          " ",
          "Move: NumPad/Arrows, ,/.: Up/Down.",
          "(Control+Arrows for diagonal.)",
          "%c{cyan}Wait: NumPad 5 / Space.",
          " ",
          "Enter: Enable auto-pause.",
          "+/-: Change speed.",
          " ",
          "%c{cyan}Z: Cast spell, %c{}J: Assign job.",
          " ",
          "PageUp/Down: Scroll messages.",
          "A: Achievements, /: Toggle tutorial."
          //,"Backspace / Delete: Previous tutorial."
        ];
      } else if (context==="ShowSpells") {
        txt[2] = "%c{cyan}"+txt[2];
        return txt;
      } else {
        return txt;
      }
    },
    instructions: [
      "%c{white}This decaying wretch is but the beginning - soon, you will command an undead horde.",
      " ",
      "Every zombie under your control raises the cost of the 'raise zombie' spell in entropy points.  Your current entropy points are listed above the left-hand side of the message bar.",
      " ",
      "%c{cyan}%b{DarkRed}Wait (pass turns) until you have 15 entropy points, then raise a second zombie and wait for it to emerge.  Press Z to cast a spell, press A to choose 'raise zombie', and then select a tombstone."
    ],
    listens: ["Cast"],
    skip: "WaitForSecondZombie",
    onBegin: function() {
      if (HTomb.Player.master.minions.length>=2) {
        //skip to something else?
        this.nopeat = true;
        HTomb.Tutorial.templates.WaitForSecondZombie.norepeat = true;
        HTomb.Tutorial.goto("NavigationModeStepOne");
      }
    },
    trigger: function(event) {
      return (event.spell.template==="RaiseZombie");
    },
    onComplete: function() {
      this.norepeat = true;
    }
  });

  new Tutorial({
    template: "WaitForSecondZombie",
    name: "raise a second zombie",
    controls: [
      "Esc: System view.",
      " ",
      "Move: NumPad/Arrows, ,/.: Up/Down.",
      "(Control+Arrows for diagonal.)",
      "%c{cyan}Wait: NumPad 5 / Space.",
      " ",
      "Enter: Enable auto-pause.",
      "+/-: Change speed.",
      " ",
      "Z: Cast spell, %c{}J: Assign job.",
      " ",
      "PageUp/Down: Scroll messages.",
      "A: Achievements, /: Toggle tutorial."
      //,"Backspace / Delete: Previous tutorial."
    ],
    instructions: [
      "%c{white}This decaying wretch is but the beginning - soon, you will command an undead horde.",
      " ",
      "Every zombie under your control raises the cost of the 'raise zombie' spell in entropy points.  Your current entropy points are listed above the left-hand side of the message bar.",
      " ",
      "%c{cyan}%b{DarkRed}Wait (pass turns) until your zombie emerges."
    ],
    listens: ["Complete"],
    skip: "WaitingForHarvest",
    trigger: function(event) {
      return (event.task && event.task.template==="ZombieEmergeTask" && HTomb.Player.master.minions.length>=2);
    },
    onComplete: function() {
      HTomb.Tutorial.templates.RaiseASecondZombie.norepeat = true;
      this.norepeat = true;
    }
  });

  new Tutorial({
    template: "NavigationModeStepOne",
    name: "navigation mode",
    controls: [
      "Esc: System view.",
      "%c{cyan}Avatar mode (Tab: Navigation mode)",
      " ",
      "Move: NumPad/Arrows, ,/.: Up/Down.",
      "(Control+Arrows for diagonal.)",
      "Wait: NumPad 5 / Space.",
      " ",
      "Enter: Enable auto-pause.",
      "+/-: Change speed.",
      " ",
      "Z: Cast spell, J: Assign job.",
      " ",
      "PageUp/Down: Scroll messages.",
      "A: Achievements, /: Toggle tutorial."
      //,"Backspace / Delete: Previous tutorial."
    ],
    instructions: [
      "%c{white}These mindless servants shall be your hands, eyes, and ears.",
      " ",
      "Once you have several zombies, there is less need for your necromancer to walk around.  You may wish to spend most of your time in 'Navigation Mode', moving the viewing window independently while your necromancer meditates on a throne or conducts research in a laboratory.",
      " ",
      "%c{cyan}%b{DarkRed}Press Tab to enter Navigation Mode.",
    ],
    listens: ["Command"],
    skip: "WaitingForHarvest",
    trigger: function(event) {
      return (event.command==="SurveyMode");
    }
  });

  new Tutorial({
    template: "NavigationModeStepTwo",
    name: "navigation mode",
    contexts: ["Survey"],
    controls: [
      "Esc: System view.",
      "%c{cyan}Navigation mode (Tab: Avatar mode)",
      " ",
      "%c{cyan}Move: NumPad/Arrows, ,/.: Up/Down",
      "%c{cyan}(Control+Arrows for diagonal.)",
      "%c{cyan}Wait: NumPad 5 / Control+Space.",
      " ",
      "Enter: Enable auto-pause.",
      "+/-: Change speed.",
      " ",
      "Z: Cast spell, J: Assign job.",
      " ",
      "PageUp/Down: Scroll messages.",
      "A: Achievements, /: Toggle tutorial."
      //,"Backspace / Delete: Previous tutorial."
    ],
    instructions: [
      "Now you are in navigation mode.",
      " ",
      "Move the screen around using the keypad or arrows.  Hold Shift to move multiple spaces at a time.  Also try pressing . or , to move the view up or down a level.  To wait in Navigation Mode, press 5 on the keypad, or Control+Space.",
      " ",
      "%c{cyan}%b{DarkRed}Press Tab to return to 'Avatar Mode' and recenter the screen, putting you in direct control of the necromancer."
    ],
    listens: ["Command"],
    skip: "WaitingForHarvest",
    trigger: function(event) {
      return (event.command==="MainMode");
    }
  });


  new Tutorial({
    template: "HarvestResourcesStepOne",
    name: "harvest resources",
    contexts: ["Main","Survey"],
    controls: function(txt) {
      let i = 0;
      txt = HTomb.Utils.copy(txt);
      do {
        if (txt[i]==="Z: Cast spell, J: Assign job.") {
          txt[i] = "Z: Cast spell, %c{cyan}J: Assign job.";
        } else if (txt[i]==="G: Pick Up, D: Drop, I: Inventory." || txt[i]==="M: Minions, S: Structures, U: Summary." || txt[i]==="F: Submit Feedback.") {
          txt.splice(i,1);
          i--;
        }
        i++;
      } while (i<txt.length);
      return txt;
    },
    instructions: [
      "%c{white}The boulders of these hills will form the bones of your fortress, and the trees shall fuel its fires.",
      " ",
      "%c{cyan}%b{DarkRed}Press J to assign a job, and then press D to harvest."
    ],
    listens: ["Command"],
    skip: "WaitingForHarvest",
    trigger: function(event) {
      return (event.command==="ShowJobs");
    }
  });

  new Tutorial({
    template: "HarvestResourcesStepTwo.",
    name: "harvest resources",
    contexts: ["ShowJobs"],
    controls: function(txt) {
      txt[5] = "%c{cyan}" + txt[5];
      return txt;
    },
    instructions: HTomb.Tutorial.templates.HarvestResourcesStepOne.instructions,
    listens: ["Command"],
    skip: "WaitingForHarvest",
    rollback: "HarvestResourcesStepOne",
    trigger: function(event) {
      return (event.command==="ChooseJob" && event.task && event.task.template==="DismantleTask");
    }
  });

  new Tutorial({
    template: "HarvestResourcesStepThree.",
    name: "harvest resources",
    contexts: ["DesignateDismantleTask"],
    controls: null,
    instructions: [
      "The green \u2663 and \u2660 symbols on the map are trees.",
      " ",
      "%c{cyan}%b{DarkRed}Using the mouse or keyboard, select two corners of a rectangular area that includes some trees that are on your current elevation level.  Then wait for your zombies to harvest some wood."
    ],
    listens: ["Designate"],
    skip: "WaitingForHarvest",
    rollback: "HarvestResourcesStepOne",
    trigger: function(event) {
      return (event.task && event.task.template==="DismantleTask");
    }
  });

  new Tutorial({
    template: "WaitingForHarvest",
    name: "harvest resources",
    contexts: ["Main","Survey"],
    controls: function(txt) {
      let i = 0;
      txt = HTomb.Utils.copy(txt);
      do {
        if (txt[i]==="Wait: NumPad 5 / Space." || txt[i]==="Wait: NumPad 5 / Control+Space.") {
          txt[i] = "%c{cyan}"+txt[i];
        } else if (txt[i]==="G: Pick Up, D: Drop, I: Inventory." || txt[i]==="M: Minions, S: Structures, U: Summary." || txt[i]==="F: Submit Feedback.") {
          txt.splice(i,1);
          i--;
        }
        i++;
      } while (i<txt.length);
      return txt;
    },
    instructions: [
      "%c{cyan}%b{DarkRed}Wait for your zombies to harvest some wood."
    ],
    listens: ["Complete"],
    trigger: function(event) {
      // This logic attempts to catch whether someone harvested the wrong thing
      if (event.task && event.task.template==="DismantleTask") {
        if (HTomb.Player.master.ownsAllIngredients({WoodPlank: 1})) {
          console.log(HTomb.Player.master.taskList.length);
          return true;
        } else if (HTomb.Player.master.taskList.length<=1) {
          HTomb.Tutorial.goto("HarvestResourcesStepOne");
          return false;
        }
      }
    }
  });

  new Tutorial({
    template: "PickingUpItems",
    name: "picking up items",
    contexts: ["Main","Survey","ChooseItemToPickup"],
    controls: function(txt) {
      let context = HTomb.GUI.Contexts.active.contextName;
      if (context==="Main") {
        return [
          "Esc: System view.",
          "%c{yellow}Avatar mode (Tab: Navigation mode)",
          " ",
          "Move: NumPad/Arrows, ,/.: Up/Down.",
          "(Control+Arrows for diagonal.)",
          "Wait: NumPad 5 / Space.",
          " ",
          "Enter: Enable auto-pause.",
          "+/-: Change speed.",
          " ",
          "Z: Cast spell, J: Assign job.",
          "%c{cyan}G: Pick Up, %c{}D: Drop, I: Inventory.",
          " ",
          "PageUp/Down: Scroll messages.",
          "A: Achievements, ?: Tutorial."
          //,"Backspace / Delete: Previous tutorial."
        ];
      } else {
        return txt;
      }
    },
    instructions: function() {
      let context = HTomb.GUI.Contexts.active.contextName;
      let txt = [
        "%c{white}Your zombies chop down a tree.  You approach to examine their handiwork.",
        " ",
        "Once you build some workshops, your zombies can use this wood to craft items.  Also, you can press G, D, or I to have your necromancer pick up, drop, and examine items.",
        " "
      ];
      if (context==="Main") {
        txt.push("%c{cyan}%b{DarkRed}Walk until you are standing on the wooden plank, then try picking it up.");
      } else if (context==="Survey") {
        txt.push("%c{cyan}%b{DarkRed}Press Tab to directly control the necromancer.");
      } else {
        txt.push("%c{cyan}%b{DarkRed}Press a letter key to choose an item.");
      }
      return txt;
    },
    listens: ["Command"],
    trigger: function(event) {
      return (event.command==="PickUp" && event.item.template==="WoodPlank");
    }
  });

  new Tutorial({
    template: "DroppingItems",
    name: "dropping items",
    contexts: ["Main","ChooseItemToDrop"],
    controls: function(txt) {
      if (HTomb.GUI.Contexts.active.contextName==="ChooseItemToDrop") {
        return txt;
      } else {
        return [
          "Esc: System view.",
          "%c{yellow}Avatar mode (Tab: Navigation mode)",
          " ",
          "Move: NumPad/Arrows, ,/.: Up/Down.",
          "(Control+Arrows for diagonal.)",
          "Wait: NumPad 5 / Space.",
          " ",
          "Enter: Enable auto-pause.",
          "+/-: Change speed.",
          " ",
          "Z: Cast spell, J: Assign job.",
          "G: Pick Up, %c{cyan}D: Drop, I: Inventory.",
          " ",
          "PageUp/Down: Scroll messages.",
          "A: Achievements, ?: Tutorial."
          //,"Backspace / Delete: Previous tutorial."
        ];
      }
    },
    instructions: [
      "%c{white}You pick up a wooden plank.  From crude materials such as these, your servants will fashion buildings, tools...and weapons.",
      " ",
      "For the most part, you won't carry around many items in this game.  The wood you harvested, for example, is more useful lying on the ground where your zombies can get to it.  Once you build some workshops, the zombies can use the wood planks to create items or furnishings.",
      " ",
      "%c{cyan}%b{DarkRed}Try dropping an item now."
    ],
    listens: ["Command"],
    trigger: function(event) {
      return (event.command==="Drop");
    }
  });


  new Tutorial({
    template: "EndOfTutorial",
    name: "end of tutorial",
    contexts: ["Main","Survey"],
    controls: function(txt) {
      txt = HTomb.Utils.copy(txt);
      for (let i=0; i<txt.length; i++) {
        if (txt[i]==="M: Minions, S: Structures, U: Summary." || txt[i]==="F: Submit Feedback.") {
          txt[i] = "%c{cyan}"+txt[i];
        } else if (txt[i]==="A: Achievements, ?: Tutorial.") {
          txt[i] = "A: Achievements, %c{cyan}?: Tutorial.";
        }
      }
      //txt.push("Backspace / Delete: Previous tutorial.");
      return txt;
    },
    instructions: [
      "%c{white}Cruel laughter wells in your throat.  Your fortress will cast a shadow of menace over all the land.  The undead under your command will become a legion, a multitude, an army.  And then all who have wronged you will pay!",
      " ",
      "Congratulations, you finished the in-game tutorial.  Experiment with different tasks and commands.  See if you can unlock all the achievements in the demo.",
      " ",
      "%c{cyan}%b{DarkRed}Press ? to dismiss these messages."
    ],
    listens: ["Command"],
    trigger: function(event) {
      return (event.command==="DisableTutorial");
    }
  });

  new Tutorial({
    template: "EndOfTutorial",
    name: "end of tutorial",
    contexts: "All",
    controls: null,
    instructions: [
      "You have finished the in-game tutorial.  Enjoy the rest of the demo!",
      " ",
      "%c{cyan}Press ? to dismiss these messages."
    ],
    listens: [],
    trigger: function(event) {
      return false;
    }
  });


  return HTomb;
})(HTomb);
