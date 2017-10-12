HTomb = (function(HTomb) {
  "use strict";
  var coord = HTomb.Utils.coord;

  let Entity = HTomb.Things.templates.Entity;

  let Spell = Entity.extend({
    template: "Spell",
    name: "spell",
    getCost: function() {
      return 10;
    },
    spendEntropy: function() {
      this.caster.entropy-=this.getCost();
    },
  });

  HTomb.Things.templates.Behavior.extend({
    template: "Researchable",
    name: "researchable",
    //time: 480,
    time: 120,
    cost: {},
    finish: function(args) {
      let r = args.researcher;
      if (this.entity.parent==="Spell") {
        HTomb.GUI.alert("You have completed research on the spell '" + this.entity.describe()+".'");
        this.entity.caster = args.researcher.caster;
        let spell = this.entity;
        if (this.entity.caster.spells.indexOf(spell.template)===-1) {
          this.entity.caster.spells.push(this.entity);
        } else {
          this.entity.despawn();
        }
      }
      
    }
  });


  HTomb.Debug.testParticles = function(args, modifiers) {
    for (let arg in args) {
      HTomb.Things.templates.ParticleTest.args[arg] = args[arg];
    }
    for (let arg in modifiers) {
      HTomb.Things.templates.ParticleTest.args[arg] = modifiers[arg];
    }
  };
  HTomb.Debug.resetParticles = function() {
    HTomb.Things.templates.ParticleTest.args = {};
  }
  Spell.extend({
    template: "ParticleTest",
    name: "particle test",
    args: {},
    getCost: function() {
      return 0;
    },
    cast: function() {
      var c = this.caster.entity;
      HTomb.Particles.addEmitter(c.x,c.y,c.z,this.args);
    }
  });


  Spell.extend({
    template: "AcidBolt",
    name: "acid bolt",
    attack: {
      damage: {
        Acid: [1,10,1]
      }
    },
    cast: function() {
      let caster = this.caster;
      var c = caster.entity;
      let that = this;
      function castBolt(x,y,z) {
        let cr = HTomb.World.creatures[HTomb.Utils.coord(x,y,z)]
        if (cr) {
          HTomb.Events.publish({type: "Cast", spell: that, x: x, y: y, z: z});
          that.spendEntropy();
          HTomb.Particles.addEmitter(c.x,c.y,c.z,HTomb.Particles.Acid,{alwaysVisible: true});
          HTomb.Particles.addEmitter(x,y,z,HTomb.Particles.Acid,{alwaysVisible: true});
          HTomb.GUI.sensoryEvent(c.describe({capitalized: true, article: "indefinite"}) + " casts an acid bolt at " + cr.describe({article: "indefinite"})+".",c.x,c.y,c.z,"orange");
          if (cr.body) {
            cr.body.endure(that.attack);
          }
          c.ai.acted = true;
          c.ai.actionPoints-=16;
        } else {
          HTomb.GUI.pushMessage("Can't cast the spell there.");
        }
      }
      function myHover(x, y, z) {
        if (HTomb.World.explored[z][x][y]!==true) {
          HTomb.GUI.Panels.menu.middle = ["%c{orange}Unexplored tile."];
          return;
        }
        let cr = HTomb.World.creatures[HTomb.Utils.coord(x,y,z)]
        if (cr) {
          HTomb.GUI.Panels.menu.middle = ["%c{lime}Fire an acid bolt at " + cr.describe({article: "indefinite"})];
        } else {
          HTomb.GUI.Panels.menu.middle = ["%c{orange}Select a creature to target."];
        }
      }
      HTomb.GUI.selectSquare(
        c.z,
        castBolt,
        {hover: myHover}
      );
    }
  });

  Spell.extend({
    template: "PoundOfFlesh",
    name: "pound of flesh",
    Behaviors: {
      Researchable: {
        ingredients: {}
        //ingredients: {Flesh: 1, Bone: 1}
      }
    },
    cast: function() {
      let caster = this.caster;
      var c = caster.entity;
      let that = this;
      function castBolt(x,y,z) {
        let cr = HTomb.World.creatures[HTomb.Utils.coord(x,y,z)]
        if (cr && cr.template==="Zombie") {
          HTomb.Events.publish({type: "Cast", spell: that, x: x, y: y, z: z});
          that.spendEntropy();
          HTomb.Particles.addEmitter(c.x,c.y,c.z,HTomb.Particles.SpellCast,{alwaysVisible: true});
          HTomb.Particles.addEmitter(x,y,z,HTomb.Particles.SpellTarget,{alwaysVisible: true});
          HTomb.GUI.sensoryEvent(c.describe({capitalized: true, article: "indefinite"}) + " siphons flesh to " + cr.describe({article: "indefinite"})+".",c.x,c.y,c.z,"orange");
          if (c.defender) {
            c.defender.wounds.level+=1;
            if (c.defender.wounds.type===null) {
              c.defender.wounds.type = "Wither";
            }
            c.defender.tallyWounds();
            if (cr.defender) {
              cr.defender.wounds.level-=1;
              cr.defender.tallyWounds();
            }
          }
          c.ai.acted = true;
          c.ai.actionPoints-=16;
        } else if (cr) {
          HTomb.Events.publish({type: "Cast", spell: that, x: x, y: y, z: z});
          that.spendEntropy();
          HTomb.Particles.addEmitter(c.x,c.y,c.z,HTomb.Particles.SpellTarget,{alwaysVisible: true});
          HTomb.Particles.addEmitter(x,y,z,HTomb.Particles.SpellCast,{alwaysVisible: true});
          HTomb.GUI.sensoryEvent(c.describe({capitalized: true, article: "indefinite"}) + " siphons flesh from " + cr.describe({article: "indefinite"})+".",c.x,c.y,c.z,"orange");
          // should this be an attack or not?  just a direct drain?
          if (cr.defender) {
           cr.defender.wounds.level+=1;
            if (cr.defender.wounds.type===null) {
              cr.defender.wounds.type = "Wither";
            }
            cr.defender.tallyWounds();
            if (c.defender) {
              c.defender.wounds.level-=1;
              c.defender.tallyWounds();
            }
          }
          c.ai.acted = true;
          c.ai.actionPoints-=16;
        } else {
          HTomb.GUI.pushMessage("Can't cast the spell there.");
        }
      }
      function myHover(x, y, z) {
        if (HTomb.World.explored[z][x][y]!==true) {
          HTomb.GUI.Panels.menu.middle = ["%c{orange}Unexplored tile."];
          return;
        }
        let cr = HTomb.World.creatures[HTomb.Utils.coord(x,y,z)]
        if (cr && cr.template==="Zombie") {
          HTomb.GUI.Panels.menu.middle = ["%c{lime}Sacrifice your flesh to heal " + cr.describe({article: "indefinite"})+"."];
        } else if (cr) {
          HTomb.GUI.Panels.menu.middle = ["%c{lime}Siphon flesh from this creature to heal your wounds."];
        } else {
          HTomb.GUI.Panels.menu.middle = ["%c{orange}Select a zombie to heal or enemy to damage."];
        }
      }
      HTomb.GUI.selectSquare(
        c.z,
        castBolt,
        {hover: myHover}
      );
    }
  });

  Spell.extend({
    template: "RaiseZombie",
    name: "raise zombie",
    getCost: function() {
      let cost = [10,15,20,25,30,35,40];
      let c = this.caster.entity;
      if (c.master===undefined) {
        return cost[0];
      }
      else if (c.master.minions.length<cost.length-1) {
        return cost[c.master.minions.length];
      }
      else {
        return cost[cost.length-1];
      }
    },
    cast: function() {
      let caster = this.caster;
      var c = caster.entity;
      var that = this;
      var items, zombie, i;
      function raiseZombie(x,y,z) {
        if (that.validTile(x,y,z)) {
          HTomb.Particles.addEmitter(c.x,c.y,c.z,HTomb.Particles.SpellCast,{alwaysVisible: true});
          HTomb.Particles.addEmitter(x,y,z,HTomb.Particles.SpellTarget,{alwaysVisible: true});
          // cast directly on a corpse
          items = HTomb.World.items[coord(x,y,z)] || HTomb.Things.Items();
          if (items.count("Corpse")) {
            HTomb.Events.publish({type: "Cast", spell: that, x: x, y: y, z: z});
            let corpse = items.take("Corpse");
            let sourceCreature = corpse.sourceCreature;
            that.spendEntropy();
            corpse.despawn();
            if (sourceCreature) {
              zombie = HTomb.Things.Zombie({sourceCreature: sourceCreature});
            } else {
              zombie = HTomb.Things.Zombie();
            }
            zombie.place(x,y,z);
            HTomb.Things.Minion().addToEntity(zombie);
            caster.entity.master.addMinion(zombie);
            zombie.ai.acted = true;
            zombie.ai.actionPoints-=16;
            HTomb.GUI.sensoryEvent("The corpse stirs and rises...",x,y,z);
            HTomb.Time.resumeActors();
            return;
          }
          // if it's a tombstone
          items = HTomb.World.items[coord(x,y,z-1)] || HTomb.Thigns.Items();
          if (items.count("Corpse")) {
            HTomb.Events.publish({type: "Cast", spell: that, x: x, y: y, z: z-1});
            let corpse = items.take("Corpse");
            let sourceCreature = corpse.sourceCreature;
            that.spendEntropy();
            corpse.despawn();
            if (HTomb.World.tiles[z-1][x][y]===HTomb.Tiles.WallTile) {
              HTomb.World.tiles[z-1][x][y]=HTomb.Tiles.UpSlopeTile;
            }
            if (sourceCreature) {
              zombie = HTomb.Things.Zombie({sourceCreature: sourceCreature});
            } else {
              zombie = HTomb.Things.Zombie();
            }
            zombie.place(x,y,z-1);
            HTomb.Things.Minion().addToEntity(zombie);
            caster.entity.master.addMinion(zombie);
            let task = HTomb.Things.ZombieEmergeTask({assigner: caster.entity}).place(x,y,z);
            task.assignTo(zombie);
            zombie.ai.acted = true;
            zombie.ai.actionPoints-=16;
            HTomb.GUI.sensoryEvent("You hear an ominous stirring below the earth...",x,y,z);
            HTomb.Time.resumeActors();
            return;
          }
        } else {
          HTomb.GUI.pushMessage("Can't cast the spell there.");
        }
      }
      function myHover(x, y, z) {
        if (HTomb.World.explored[z][x][y]!==true) {
          HTomb.GUI.Panels.menu.middle = ["%c{orange}Unexplored tile."];
          return;
        }
        if (that.validTile(x,y,z)) {
          HTomb.GUI.Panels.menu.middle = ["%c{lime}Raise a zombie here."];
        } else {
          HTomb.GUI.Panels.menu.middle = ["%c{orange}Select a tile with a tombstone or corpse."];
        }
      }
      HTomb.GUI.selectSquare(c.z,raiseZombie,{
        hover: myHover,
        contextName: "CastRaiseZombie"
      });
    },
    validTile: function(x,y,z) {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      if (HTomb.World.features[coord(x,y,z)] && HTomb.World.features[coord(x,y,z)].template==="Tombstone" && HTomb.World.items[coord(x,y,z-1)] && HTomb.World.items[coord(x,y,z-1)].count("Corpse")) {
        return true;
      }
      if (HTomb.World.items[coord(x,y,z)] && HTomb.World.items[coord(x,y,z)].count("Corpse")) {
        return true;
      }
      return false;
    }
  });

  HTomb.Things.templates.Task.extend({
  //HTomb.Things.defineTask({
    template: "ZombieEmergeTask",
    name: "emerge",
    bg: "#884400",
    //beginDescription: function() {
    //  return "digging up from its grave";
    //},
    validTile: function() {
      // this thing is going to be special...it should keep respawning if thwarted
      return true;
    },
    workOnTask: function(x,y,z) {
      let f = HTomb.World.features[HTomb.Utils.coord(x,y,z)];
      // There is a special case of digging upward under a tombstone...
      if (f && f.template==="Tombstone") {
        if (f.integrity===null || f.integrity===undefined) {
          f.integrity=10;
        }
        if (f.integrity===10) {
          HTomb.GUI.pushMessage(this.assignee.describe({capitalized: true, article: "indefinite"}) + " begins digging toward the surface.");
        }
        f.integrity-=1;
        this.assignee.ai.acted = true;
        this.assignee.ai.actionPoints-=16;
        if (f.integrity<=0) {
          f.explode(this.assigner);
          var cr = this.assignee;
          HTomb.GUI.sensoryEvent(cr.describe({capitalized: true, article: "indefinite"}) + " bursts forth from the ground!",x,y,z);
          HTomb.World.tiles[z][x][y] = HTomb.Tiles.DownSlopeTile;
          let c = HTomb.World.covers[z][x][y];
          if (c.mine) {
            c.mine(x,y,z,this.assigner);
          }
          this.complete();
          HTomb.World.validate.cleanNeighbors(x,y,z);
        }
      }
    }
  });

  return HTomb;
})(HTomb);
