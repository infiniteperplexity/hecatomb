// This submodule defines the templates for creature Entities
HTomb = (function(HTomb) {
  "use strict";

  var b = HTomb.Things;

  HTomb.Things.defineCreature({
      template: "Necromancer",
      name: "necromancer",
      symbol: "@",
      fg: "#DD66FF",
      Behaviors: {
        Movement: {swims: true},
        Inventory: {},
        Sight: {},
        AI: {
          team: "PlayerTeam"
        },
        Master: {tasks: ["DigTask","BuildTask","ConstructTask","DismantleTask","PatrolTask","FurnishTask","Undesignate","HostileTask"]},
        SpellCaster: {spells: ["RaiseZombie"]},
        Body: {
          materials: {
            Flesh: 25,
            Bone: 25
          }
        },
        Combat: {
          accuracy: 1,
          evasion: 2,
          damage: {
            Slashing: [1,4]
          }
        }
      }
  });

  HTomb.Things.defineCreature({
    template: "Dryad",
    name: "dryad",
    symbol: "n",
    fg: "#44AA44",
    onDefine: function(args) {
      HTomb.Events.subscribe(this, "Destroy");
    },
    onDestroy: function(event) {
      // no dryads in the extremely early game
      if (HTomb.Time.dailyCycle.turn<500 && HTomb.Player.master.structures.length===0 && HTomb.Player.master.ownsAllIngredients({WoodPlank: 5})===false) {
        return;
      }
      let t = event.entity;
      if (t.template==="Tree") {
        let x = t.x;
        let y = t.y;
        let z = t.z;
        if (HTomb.Utils.dice(1,25)===1) {
          let trees = HTomb.Utils.where(HTomb.World.features,
            function(e) {
              let d = HTomb.Path.quickDistance(e.x,e.y,e.z,x,y,z);
              return (e.template==="Tree" && d>=5 && d<=9);
            }
          );
          if (trees.length>0) {
            let tree = HTomb.Path.closest(x,y,z,trees)[0];
            let dryad = HTomb.Things.Dryad();
            dryad.place(tree.x,tree.y,tree.z);
            HTomb.Particles.addEmitter(tree.x,tree.y,tree.z,HTomb.Particles.SpellTarget, HTomb.Particles.DryadEffect);
            HTomb.GUI.sensoryEvent("An angry dryad emerges from a nearby tree!",tree.x,tree.y,tree.z,"red");
          }
        }
      }
    },
    Behaviors: {
      AI: {
        team: "AngryNatureTeam"
      },
      Movement: {swims: true},
      Sight: {},
      Combat: {
        accuracy: 0,
        damage: {
          Crushing: [1,4]
        }
      },
      Body: {
        materials: {
          Wood: 25,
          Flesh: 25,
          Bone: 25
        }
      }
    }
  });

  HTomb.Things.defineCreature({
    template: "Zombie",
    name: "zombie",
    leavesCorpse: false,
    symbol: "z",
    fg: "#99FF66",
    onCreate: function(args) {
      if (args.sourceCreature) {
        let creature = HTomb.Things.templates[args.sourceCreature];
        this.name = creature.name + " " + this.name;
      }
      return this;
    },
    Behaviors: {
      AI: {
        goals: ["ServeMaster"]
      },
      Movement: {swims: true},
      Sight: {},
      Worker: {},
      Inventory: {capacity: 2},
      Combat: {
        accuracy: 1,
        damage: {
          Slashing: [1,3],
          Crushing: [1,3]
        }
      },
      Body: {
        materials: {
          Flesh: {
            max: 25,
            needs: 1
          },
          Bone: 25
        }
      }
    }
  });

  var totalGhouls = 0;
  HTomb.Things.defineCreature({
    template: "Ghoul",
    name: "ghoul",
    symbol: "z",
    fg: "#FF5522",
    Behaviors: {
      AI: {
        team: "GhoulTeam",
        goals: ["LongRangeRoam"]
      },
      Movement: {swims: true},
      Sight: {},
      Worker: {},
      Inventory: {capacity: 2},
      Combat: {
        accuracy: 0,
        damage: {
          Slashing: [1,6]
        }
      },
      Body: {
        materials: {
          Flesh: 10,
          Bone: 10
        }
      }
    },
    onDefine: function(args) {
      HTomb.Events.subscribe(this, "TurnBegin");
    },
    onTurnBegin: function(args) {
      // ghouls only show up at night
      if (HTomb.Time.dailyCycle.hour>=HTomb.Constants.DAWN || HTomb.Time.dailyCycle.hour<=HTomb.Constants.DUSK) {
        return;
      }
      if (HTomb.Utils.dice(1,120)===1 && HTomb.Types.templates.Team.teams.GhoulTeam.members.length<10) {
        let graves = HTomb.Utils.where(HTomb.World.features,function(e) {
          if (e.template==="Tombstone") {
            let x = e.x;
            let y = e.y;
            let z = e.z;
            if (HTomb.World.tasks[HTomb.Utils.coord(x,y,z)]) {
              return false;
            }
            if (HTomb.World.items[HTomb.Utils.coord(x,y,z-1)] && HTomb.World.items[HTomb.Utils.coord(x,y,z-1)].containsAny("Corpse")) {
              return true;
            }
          }
          return false;
        });
        HTomb.Utils.shuffle(graves);
        let grave = graves[0];
        let x = grave.x;
        let y = grave.y;
        let z = grave.z;
        HTomb.Particles.addEmitter(x,y,z,HTomb.Particles.SpellTarget,{fg: "red"});
        grave.explode();
        HTomb.World.tiles[z-1][x][y] = HTomb.Tiles.UpSlopeTile;
        HTomb.World.tiles[z][x][y] = HTomb.Tiles.DownSlopeTile;
        let ghoul = HTomb.Things.Ghoul().place(x,y,z-1);
        HTomb.GUI.sensoryEvent("A ravenous ghoul bursts forth from its grave!",x,y,z,"red");
      }
    }
  });

  HTomb.Things.defineCreature({
    template: "Bat",
    name: "bat",
    symbol: "b",
    fg: "#999999",
    Behaviors: {
      AI: {},
      Movement: {flies: true, swims: false},
      Sight: {},
      Combat: {},
      Body: {
        materials: {
          Flesh: 5,
          Bone: 2
        }
      }
    }
  });

  HTomb.Things.defineCreature({
    template: "Spider",
    name: "spider",
    symbol: "s",
    fg: "#BBBBBB",
    Behaviors: {
      AI: {},
      Movement: {swims: false},
      Combat: {},
      Body: {
        materials: {
          Flesh: 5,
          Bone: 2
        }
      }
    }
  });

  HTomb.Things.defineCreature({
    template: "DeathCarp",
    name: "death carp",
    symbol: "p",
    fg: "red",
    Behaviors: {
      AI: {
        team: "HungryPredatorTeam"
      },
      Movement: {swims: true, walks: false},
      Combat: {
        accuracy: 1,
        damage: {
          Slashing: [1,8]
        }
      },
      Body: {
        materials: {
          Flesh: 10,
          Bone: 10,
          Blood: 10
        }
      }
    },
    onDefine: function() {
      HTomb.Events.subscribe(this, "TurnBegin");
    },
    onTurnBegin: function() {
      if (HTomb.Utils.dice(1,180)===1 && HTomb.Types.templates.Team.teams.HungryPredatorTeam.members.length<10) {
        let fishes = HTomb.Utils.where(HTomb.World.creatures, function(e) {
          let x = e.x;
          let y = e.y;
          let z = e.z;
          if (HTomb.World.visible[HTomb.Utils.coord(x,y,z)]) {
            return false;
          }
          return (e.template==="Fish");
        });
        if (fishes.length>0) {
          HTomb.Utils.shuffle(fishes);
          let fish = fishes[0];
          let x = fish.x;
          let y = fish.y;
          let z = fish.z;
          fish.destroy();
          HTomb.Things.DeathCarp().place(x,y,z);
          HTomb.GUI.sensoryEvent("A peaceful-looking fish turns out to be a ravenous death carp!",x,y,z,"red");
        }
      }

    }
  });

  HTomb.Things.defineCreature({
    template: "Fish",
    name: "fish",
    symbol: "p",
    fg: "#FF8888",
    Behaviors: {
      AI: {},
      Movement: {swims: true, walks: false},
      Combat: {},
      Body: {
        materials: {
          Flesh: 5,
          Bone: 2
        }
      }
    }
  });

  return HTomb;
})(HTomb);
