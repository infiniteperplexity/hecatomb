HTomb = (function(HTomb) {
  "use strict";
  var coord = HTomb.Utils.coord;

  let Entity = HTomb.Things.Entity;
  let Component = HTomb.Things.Component;

  let Lore = Entity.extend({
    template: "Lore",
    name: "lore",
    turns: 48,
    researcher: null,
    ingredients: {},
    fulfilled: false,
    structure: null,
    prerequisites: [],
    complete: function() {
      if (this.spell) {
        HTomb.GUI.alert("You have completed research on the spell '" + this.describe()+".'");
        let spells = this.assigner.caster.spells.map(function(s) {return s.template;});
        if (spells.indexOf(this.template)===-1) {
          // is this the way to go?  Or should I move it over?
          let spell = this.spawn({caster: this.assigner.caster});
          this.assigner.caster.addSpell(spell);
        }
      } else {
        HTomb.GUI.alert("You have completed research on '" + this.describe()+".'");
      }
      if (this.assigner.researcher.researched.indexOf(this.template)===-1) {
        this.assigner.researcher.researched.push(this.template);
      }
    },
    use: function(args) {
    },
    researchable: function() {
      let researched = this.researcher.researcher.researched;
      for (let p of prequisites) {
        if (!researched.includes(p)) {
          return false;
        }
      }
      return true;
    },
    useable: function() {
      let structures = this.researcher.owner.structures;
      for (let s of structures) {
        if (s.template===this.structure) {
          return true;
        }
      }
      return false;
    },
    cancel: function() {
      this.despawn();
    }
  });

  let Tools = Component.extend({
    template: "Tools",
    name: "tools",
    // I could do this as a bonus or a flat number
    labor: 0
  });

  Lore.extend({
    template: "FlintTools",
    name: "flint tools",
    turns: 48,
    ingredients: {Flint: 3, WoodPlank: 3},
    structure: "Workshop",
    Components: {
      Tools: {labor: 2}
    }
  });

  // this could be nospawn...
  let Spell = Component.extend({
    template: "Spell",
    name: "spell",
    cost: 10,
    range: 6,
    caster: null,
    withinRange: function(x,y,z) {
      let c = this.caster.entity;
      if (HTomb.Path.quickDistance(c.x, c.y, c.z, x, y, z)>this.range) {
        return false;
      } else {
        return true;
      }
    },
    getCost: function() {
      if (this.entity.getCost) {
        return this.entity.getCost();
      }
      return this.cost;
    },
    spend: function() {
      this.caster.sanity-=this.getCost();
    },
    cast: function(args) {
      this.entity.use(args);
    }
  });

  Lore.extend({
    template: "RaiseZombie",
    name: "raise zombie",
    Components: {
      Spell: {range: 10}
    },
    getCost: function() {
      let cost = [10,15,20,25,30,35,40];
      let c = this.spell.caster.entity;
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
    use: function() {
      var c = this.spell.caster.entity;
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
            that.spell.spend();
            corpse.despawn();
            if (sourceCreature) {
              zombie = HTomb.Things.Zombie.spawn({sourceCreature: sourceCreature});
            } else {
              zombie = HTomb.Things.Zombie.spawn();
            }
            zombie.place(x,y,z);
            HTomb.Things.Minion.spawn().addToEntity(zombie);
            c.master.addMinion(zombie);
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
            that.spell.spend();
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
            c.master.addMinion(zombie);
            let task = HTomb.Things.ZombieEmergeTask.spawn({assigner: c}).place(x,y,z);
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
        } else if (that.spell.withinRange(x,y,z)===false) {
          HTomb.GUI.Panels.menu.middle = ["%c{orange}Select a tile within range."];
        } else if (that.validTile(x,y,z)) {
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
      } else if (this.spell.withinRange(x,y,z)===false) {
        return false;
      }
      if (/*HTomb.World.features[coord(x,y,z)] && HTomb.World.features[coord(x,y,z)].template==="Tombstone" && */HTomb.World.items[coord(x,y,z-1)] && HTomb.World.items[coord(x,y,z-1)].count("Corpse")) {
        return true;
      }
      if (HTomb.World.items[coord(x,y,z)] && HTomb.World.items[coord(x,y,z)].count("Corpse")) {
        return true;
      }
      return false;
    }
  });

  Lore.extend({
    template: "PoundOfFlesh",
    name: "pound of flesh",
    ingredients: {Ectoplasm: 1},         //ingredients: {Flesh: 1, Bone: 1}
    turns: 48,
    Components: {
      Spell: {cost: 10}
    },
    // should standardize validTile for spells?
    use: function() {
      let c = this.spell.caster.entity;
      let that = this;
      function castBolt(x,y,z) {
        let cr = HTomb.World.creatures[HTomb.Utils.coord(x,y,z)]
        if (cr && cr.isA("Zombie")) {
          HTomb.Events.publish({type: "Cast", spell: that, x: x, y: y, z: z});
          that.spell.spend();
          HTomb.Particles.addEmitter(c.x,c.y,c.z,HTomb.Particles.SpellCast,{alwaysVisible: true});
          HTomb.Particles.addEmitter(x,y,z,HTomb.Particles.SpellTarget,{alwaysVisible: true});
          HTomb.GUI.sensoryEvent(c.describe({capitalized: true, article: "indefinite"}) + " siphons flesh to " + cr.describe({article: "indefinite"})+".",c.x,c.y,c.z,"orange");
          if (c.defender) {
            c.defender.wounds.level+=2;
            if (c.defender.wounds.type===null) {
              c.defender.wounds.type = "Decay";
            }
            c.defender.tallyWounds();
            if (cr.defender) {
              cr.defender.wounds.level-=2;
              cr.defender.tallyWounds();
            }
          }
          c.actor.acted = true;
          c.actor.actionPoints-=16;
        } else if (cr) {
          HTomb.Events.publish({type: "Cast", spell: that, x: x, y: y, z: z});
          that.spell.spend();
          HTomb.Particles.addEmitter(c.x,c.y,c.z,HTomb.Particles.SpellTarget,{alwaysVisible: true});
          HTomb.Particles.addEmitter(x,y,z,HTomb.Particles.SpellCast,{alwaysVisible: true});
          HTomb.GUI.sensoryEvent(c.describe({capitalized: true, article: "indefinite"}) + " siphons flesh from " + cr.describe({article: "indefinite"})+".",c.x,c.y,c.z,"orange");
          // should this be an attack or not?  just a direct drain?
          if (cr.defender) {
           cr.defender.wounds.level+=2;
            if (cr.defender.wounds.type===null) {
              cr.defender.wounds.type = "Decay";
            }
            cr.defender.tallyWounds();
            if (c.defender) {
              c.defender.wounds.level-=2;
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
        if (cr && cr.isA("Zombie")) {
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

  Lore.extend({
    template: "CondenseEctoplasm",
    name: "condense ectoplasm",
    cost: 20,
    turns: 48,
    Components: {
      Spell: {cost: 20, range: 6}
    },
    use: function() {
      let c = this.spell.caster.entity;
      let that = this;  
      function castBolt(x,y,z) {
        if (HTomb.World.tiles[z][x][y]===true || (HTomb.Debug.explored===false && HTomb.World.explored[z][x][y]!==true)) { 
          HTomb.GUI.pushMessage("Can't cast the spell there.");
        } else {
          that.spell.spend();
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
  Lore.extend({
    template: "StepIntoShadow",
    name: "step into shadow",
    ingredients: {
      Ectoplasm: 1
    },
    Components: {
      Spell: {cost: 6, range: 6}
    },
    use: function() {
      let c = this.spell.caster.entity;
      let x = c.x;
      let y = c.y;
      let z = c.z;
      let s = HTomb.Tiles.getRandomWithinRange(c.x,c.y,c.z,this.spell.range);
      HTomb.Particles.addEmitter(x,y,z,HTomb.Particles.SpellCast,{alwaysVisible: true});
      this.spell.spend();
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


  return HTomb;
})(HTomb);
