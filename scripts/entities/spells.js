HTomb = (function(HTomb) {
  "use strict";
  var coord = HTomb.Utils.coord;

  let Entity = HTomb.Things.Entity;

  let Spell = Entity.extend({
    template: "Spell",
    name: "spell",
    caster: null,
    cost: 10,
    getCost: function() {
      return this.cost;
    },
    spend: function() {
      this.caster.sanity-=this.getCost();
    },
  });



  HTomb.Debug.testParticles = function(args, modifiers) {
    for (let arg in args) {
      HTomb.Things.ParticleTest.args[arg] = args[arg];
    }
    for (let arg in modifiers) {
      HTomb.Things.ParticleTest.args[arg] = modifiers[arg];
    }
  };
  HTomb.Debug.resetParticles = function() {
    HTomb.Things.ParticleTest.args = {};
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
          that.spend();
          HTomb.Particles.addEmitter(c.x,c.y,c.z,HTomb.Particles.Acid,{alwaysVisible: true});
          HTomb.Particles.addEmitter(x,y,z,HTomb.Particles.Acid,{alwaysVisible: true});
          HTomb.GUI.sensoryEvent(c.describe({capitalized: true, article: "indefinite"}) + " casts an acid bolt at " + cr.describe({article: "indefinite"})+".",c.x,c.y,c.z,"orange");
          if (cr.body) {
            cr.body.endure(that.attack);
          }
          c.actor.acted = true;
          c.actor.actionPoints-=16;
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
    Components: {
      Researchable: {
        ingredients: {Ectoplasm: 1}
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
          that.spend();
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
          c.actor.acted = true;
          c.actor.actionPoints-=16;
        } else if (cr) {
          HTomb.Events.publish({type: "Cast", spell: that, x: x, y: y, z: z});
          that.spend();
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
          c.actor.acted = true;
          c.actor.actionPoints-=16;
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
    template: "CondenseEctoplasm",
    name: "condense ectoplasm",
    cost: 20,
    Components: {
      Researchable: {}
    },
    cast: function() {
      let caster = this.caster;
      var c = caster.entity;
      let that = this;  
      function castBolt(x,y,z) {
        if (HTomb.World.tiles[z][x][y]===true || (HTomb.Debug.explored===false && HTomb.World.explored[z][x][y]!==true)) { 
          HTomb.GUI.pushMessage("Can't cast the spell there.");
        } else {
          that.spend();
          HTomb.Particles.addEmitter(c.x,c.y,c.z,HTomb.Particles.SpellCast,{fg: "cyan", alwaysVisible: true});
          HTomb.Particles.addEmitter(x,y,z,HTomb.Particles.SpellTarget,{fg: "cyan", alwaysVisible: true});
          HTomb.Things.Ectoplasm.spawn().place(x,y,z); 
        }
      }
      function myHover(x, y, z) {
        if (HTomb.World.explored[z][x][y]!==true) {
          HTomb.GUI.Panels.menu.middle = ["%c{orange}Unexplored tile."];
          return;
        } else if (HTomb.World.tiles[z][x][y].solid===true) {
          HTomb.GUI.Panels.menu.middle = ["%c{orange}Cannot condense ectoplasm there."];
          return;
        } else {
          HTomb.GUI.Panels.menu.middle = ["%c{lime}Condense ectoplasm here."];
        }
      }
      HTomb.GUI.selectSquare(
        c.z,
        castBolt,
        {hover: myHover}
      );
    }
  });

  // castable only at night? or can only teleport from darkness to darkness?
  Spell.extend({
    template: "StepIntoShadow",
    name: "step into shadow",
    cost: 6,
    range: 6,
    Components: {
      Researchable: {
        ingredients: {
          Ectoplasm: 1
        }
      }
    },
    cast: function() {
      let caster = this.caster;
      let c = caster.entity;
      let x = c.x;
      let y = c.y;
      let z = c.z;
      let s = HTomb.Tiles.getRandomWithinRange(c.x,c.y,c.z,this.range);
      HTomb.Particles.addEmitter(x,y,z,HTomb.Particles.SpellCast,{alwaysVisible: true});
      this.spend();
      if (!s) {
        HTomb.GUI.sensoryEvent("Spell fizzled!",c.x,c.y,c.z);
        HTomb.GUI.reset();
        return;
      }
      c.movement.stepTo(s[0],s[1],s[2]);            
      HTomb.Particles.addEmitter(s[0],s[1],s[2],HTomb.Particles.SpellTarget,{alwaysVisible: true});   
      HTomb.GUI.sensoryEvent(c.describe({article: "indefinite", capitalized: true}) + " disappears into a shadow and emerges elsewhere.",c.x,c.y,c.z);
      HTomb.GUI.Panels.gameScreen.recenter();
      HTomb.GUI.reset();
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
            that.spend();
            corpse.despawn();
            if (sourceCreature) {
              zombie = HTomb.Things.Zombie.spawn({sourceCreature: sourceCreature});
            } else {
              zombie = HTomb.Things.Zombie.spawn();
            }
            zombie.place(x,y,z);
            HTomb.Things.Minion.spawn().addToEntity(zombie);
            caster.entity.master.addMinion(zombie);
            zombie.actor.acted = true;
            zombie.actor.actionPoints-=16;
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
            that.spend();
            corpse.despawn();
            if (HTomb.World.tiles[z-1][x][y]===HTomb.Tiles.WallTile) {
              HTomb.World.tiles[z-1][x][y]=HTomb.Tiles.UpSlopeTile;
            }
            if (sourceCreature) {
              zombie = HTomb.Things.Zombie.spawn({sourceCreature: sourceCreature});
            } else {
              zombie = HTomb.Things.Zombie.spawn();
            }
            zombie.place(x,y,z-1);
            HTomb.Things.Minion.spawn().addToEntity(zombie);
            caster.entity.master.addMinion(zombie);
            let task = HTomb.Things.ZombieEmergeTask.spawn({assigner: caster.entity}).place(x,y,z);
            task.assignTo(zombie);
            zombie.actor.acted = true;
            zombie.actor.actionPoints-=16;
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

  return HTomb;
})(HTomb);
