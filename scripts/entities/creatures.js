// This submodule defines the templates for creature Entities
HTomb = (function(HTomb) {
  "use strict";
  let coord = HTomb.Utils.coord;

  let Entity = HTomb.Things.Entity;
  let Creature = Entity.extend({
    template: "Creature",
    name: "creature",
    // this will eventually create corpses with Material values
    leavesCorpse: true,
    die: function() {
      if (this.x!==null && this.y!==null && this.z!==null) {
        HTomb.Particles.addEmitter(this.x, this.y, this.z, HTomb.Particles.Blood, HTomb.Particles.Spray);
        HTomb.GUI.sensoryEvent(this.describe({capitalized: true, article: "indefinite"}) + " dies.",this.x,this.y,this.z,"red");
        if (this.player) {
          this.player.playerDeath();
        } else {
          if (this.leavesCorpse) {
            let corpse = HTomb.Things.Corpse.spawn({sourceCreature: this});
            corpse.place(this.x, this.y, this.z);
          }
          this.destroy();
        }
      }
    },
    place: function(x,y,z,args) {
      Entity.place.call(this,x,y,z,args);
      let c = coord(x,y,z);
      if (HTomb.World.creatures[c]) {
        HTomb.Debug.pushMessage("Overwrote a creature!");
        let cr = HTomb.World.creatures[c];
        cr.remove();
        cr.despawn();
      }
      HTomb.World.creatures[c] = this;
    },
    remove: function() {
      delete HTomb.World.creatures[coord(this.x, this.y, this.z)];
      Entity.remove.call(this);
    },
    fall: function() {
      var g = HTomb.Tiles.groundLevel(this.x,this.y,this.z);
      if (HTomb.World.creatures[coord(this.x,this.y,g)]) {
        alert("haven't decided how to handle falling creature collisions");
      } else {
        HTomb.GUI.sensoryEvent(this.describe({capitalized: true, article: "indefinite"}) + " falls " + (this.z-g) + " stories!",this.x,this.y,this.z);
        this.place(this.x,this.y,g);
      }
      HTomb.GUI.render();
    },
    despawn: function() {
      if (this.encounter) {
        this.encounter.removeCreature(this);
      }
      Entity.despawn.call(this);
    }
  });

  var b = HTomb.Things;

  Creature.extend({
      template: "Necromancer",
      name: "necromancer",
      symbol: "@",
      fg: "#DD66FF",
      Components: {
        Movement: {swims: true},
        Inventory: {},
        Sight: {},
        Actor: {
          team: "PlayerTeam"
        },
        Equipper: {},
        Master: {tasks: ["DigTask","BuildTask","ConstructTask","DismantleTask","PatrolTask","FurnishTask","Undesignate","HostileTask","RepairTask"]},
        Owner: {},
        SpellCaster: {spells: ["RaiseZombie"]},
        Attacker: {
          damage: {
            level: 1
          },
          accuracy: 1
        },
        Defender: {
          evasion: 1,
          toughness: 1,
        }
      }
  });

  Creature.extend({
    template: "Dryad",
    name: "dryad",
    symbol: "n",
    fg: "#44AA44",
    Components: {
      Actor: {
        team: "AngryNatureTeam"
      },
      Movement: {swims: true},
      Sight: {},
      Attacker: {
        damage: {
          type: "Crushing"
        }
      },
      Defender: {}
    }
  });


  Creature.extend({
    template: "Peasant",
    name: "peasant",
    symbol: "@",
    fg: "brown",
    Components: {
      Actor: {
        goals: ["HuntPlayer"],
        team: "HumanityTeam"
      },
      Movement: {},
      Sight: {},
      Attacker: {
        damage: {
          type: "Crushing"
        }
      },
      Defender: {}
    }
  });

  

  Creature.extend({
    template: "Zombie",
    name: "zombie",
    leavesCorpse: false,
    symbol: "z",
    fg: "#99FF66",
    onSpawn: function(args) {
      args = args || {};
      if (args.sourceCreature) {
        let creature = HTomb.Things[args.sourceCreature];
        this.name = creature.name + " " + this.name;
      }
      return this;
    },
    Components: {
      Actor: {
        goals: ["ServeMaster"]
      },
      Movement: {swims: true},
      Equipper: {},
      Sight: {},
      Worker: {},
      Inventory: {capacity: 2},
      Attacker: {},
      Defender: {
        toughness: 1
      },
      Distinctive: {
        fgRandomRed: 15,
        fgRandomGreen: 15,
        fgRandomBlue: 15
    //         symbol: ["z","\u01B6","\u017A","\u017E",
    // "\u1E91","\u0291","\u017C","\u1E93","\u0225",
    // "\u1E95","\u0290","\u0240","\u1D22","\u0179"],
    // FF5A, A641 A643 1DBB 1DBD 1DBC 1D76 1BA6 1BA7 0291  007A
      },
      Decaying: {}
    }
  });

  var totalGhouls = 0;
  Creature.extend({
    template: "Ghoul",
    name: "ghoul",
    symbol: "z",
    fg: "#FF5522",
    Components: {
      Actor: {
        team: "GhoulTeam",
        goals: ["LongRangeRoam"]
      },
      Movement: {swims: true},
      Sight: {},
      Worker: {},
      Inventory: {capacity: 2},
      Attacker: {
        damage: {
          level: 1
        }
      },
      Defender: {
        toughness: 1
      }
    },
    onDefine: function(args) {
      //HTomb.Events.subscribe(this, "TurnBegin");
    },
    onTurnBegin: function(args) {
      // ghouls only show up at night
      if (HTomb.Time.dailyCycle.hour>=HTomb.Constants.DAWN || HTomb.Time.dailyCycle.hour<=HTomb.Constants.DUSK) {
        return;
      }
      if (HTomb.Utils.dice(1,120)===1 && HTomb.Types.Team.teams.GhoulTeam.members.length<10) {
        let graves = HTomb.Utils.where(HTomb.World.features,function(e) {
          if (e.template==="Tombstone") {
            let x = e.x;
            let y = e.y;
            let z = e.z;
            if (HTomb.World.tasks[HTomb.Utils.coord(x,y,z)]) {
              return false;
            }
            if (HTomb.World.items[HTomb.Utils.coord(x,y,z-1)] && HTomb.World.items[HTomb.Utils.coord(x,y,z-1)].count("Corpse")) {
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

  Creature.extend({
    template: "Bat",
    vermin: true,
    name: "bat",
    symbol: "b",
    fg: "#999999",
    Components: {
      Actor: {},
      Movement: {flies: true, swims: false},
      Sight: {},
      Attacker: {
        damage: {
          level: -1
        }
      },
      Defender: {
        toughness: -1
      }
    }
  });

  Creature.extend({
    template: "Spider",
    name: "spider",
    vermin: true,
    symbol: "s",
    fg: "#BBBBBB",
    Components: {
      Actor: {},
      Movement: {swims: false},
      Attacker: {
        damage: {
          level: -1
        }
      },
      Defender: {
        toughness: -1
      }
    }
  });

  Creature.extend({
    template: "DeathCarp",
    name: "death carp",
    symbol: "p",
    fg: "red",
    Components: {
      Actor: {
        team: "HungryPredatorTeam"
      },
      Movement: {swims: true, walks: false},
      Attacker: {
        damage: {
          level: 2
        },
        accuracy: 1,
      },
      Defender: {}
    }
    // onDefine: function() {
    //   HTomb.Events.subscribe(this, "TurnBegin");
    // },
    // onTurnBegin: function() {
    //   if (HTomb.Utils.dice(1,180)===1 && HTomb.Types.Team.teams.HungryPredatorTeam.members.length<10) {
    //     let fishes = HTomb.Utils.where(HTomb.World.creatures, function(e) {
    //       let x = e.x;
    //       let y = e.y;
    //       let z = e.z;
    //       if (HTomb.World.visible[HTomb.Utils.coord(x,y,z)]) {
    //         return false;
    //       }
    //       return (e.template==="Fish");
    //     });
    //     if (fishes.length>0) {
    //       HTomb.Utils.shuffle(fishes);
    //       let fish = fishes[0];
    //       let x = fish.x;
    //       let y = fish.y;
    //       let z = fish.z;
    //       fish.destroy();
    //       HTomb.Things.DeathCarp().place(x,y,z);
    //       HTomb.GUI.sensoryEvent("A peaceful-looking fish turns out to be a ravenous death carp!",x,y,z,"red");
    //     }
    //   }
    // }
  });

  Creature.extend({
    template: "Fish",
    vermin: true,
    name: "fish",
    symbol: "p",
    fg: "#FF8888",
    Components: {
      Actor: {},
      Movement: {swims: true, walks: false},
      Attacker: {
        damage: {
          level: -1
        }
      },
      Defender: {
        toughness: -1
      }
    }
  });

  return HTomb;
})(HTomb);
