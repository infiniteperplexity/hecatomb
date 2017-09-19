// This submodule defines the templates for creature Entities
HTomb = (function(HTomb) {
  "use strict";
  let coord = HTomb.Utils.coord;

  let Entity = HTomb.Things.templates.Entity;
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
            let corpse = HTomb.Things.Corpse({sourceCreature: this});
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
    }
  });

  var b = HTomb.Things;

  Creature.extend({
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
        Equipper: {},
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

  Creature.extend({
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

  Creature.extend({
    template: "Zombie",
    name: "zombie",
    leavesCorpse: false,
    symbol: "z",
    fg: "#99FF66",
    onCreate: function(args) {
      //ugh...
      //Creature.onCreate.call(this,args);
      args = args || {};
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
      Equipper: {},
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
  Creature.extend({
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

  Creature.extend({
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

  Creature.extend({
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

  Creature.extend({
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
