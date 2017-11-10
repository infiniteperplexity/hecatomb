// ****** This module implements Components, which are the basic units of functionality for creatures, items, and features
HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;

  let Component = HTomb.Things.Component;


  let Damage = HTomb.Types.Damage;

  Component.extend({
    template: "Attacker",
    name: "attacker",
    damage: {
      type: "Slashing",
      level: 0,
    },
    accuracy: 0,
    checkTerrain(v,r) {
      let e = this.entity;
      // higher ground
      if (e.z>v.z) {
        r+=1;
      }
      return r;
    },
    attack: function(victim) {
      let e = this.entity;
      // publish an event, giving listeners a chance to modify it
      let event = {
        type: "Attack",
        attacker: e,
        target: victim,
        modifiers: {
          accuracy: 0,
          evasion: 0,
          toughness: 0,
          damage: {
            type: null,
            level: 0
          },
          armor: {
            material: null,
            level: 0
          }
        }
      };
      event = HTomb.Events.publish(event);
      e = event.attacker;
      victim = event.target;
      let modifiers = event.modifiers;
      let evade = (victim.defender) ? victim.defender.evasion - victim.defender.wounds.level : -10;
      evade += modifiers.evasion;
      let roll = ROT.RNG.getUniformInt(1,20);
      if (e.entity && e.entity.equipper
            && e.entity.equipper.slots.MainHand
            && e.entity.equipper.slots.MainHand.accuracy) {
        roll += e.entity.equipper.slots.MainHand.accuracy;
      }
      roll = this.checkTerrain(victim, roll);
      if (roll+this.accuracy + modifiers.evasion >= 11 + evade) {
        (victim.defender) ? victim.defender.defend(event) : {};       
      } else {
        HTomb.GUI.sensoryEvent(this.entity.describe({capitalized: true, article: "indefinite"}) + " misses " + victim.describe({article: "indefinite"})+".",this.entity.x,this.entity.y,this.entity.z,"yellow");
      }
      if (this.entity.actor) {
        this.entity.actor.acted = true;
        this.entity.actor.actionPoints-=16;
      }
    }
  });

  Component.extend({
    template: "Defender",
    name: "defender", 
    material: "Flesh",
    evasion: 0,
    armor: {
      material: "Flesh",
      level: 0
    },
    toughness: 0,
    wounds: {
      type: null,
      level: 0 // 2 is mild, 4 is moderate, 6 is severe, 8 is dead
    },
    onSpawn: function() {
      this.wounds = HTomb.Utils.copy(HTomb.Things.Defender.wounds);
    },
    tallyWounds: function() {
      if (this.wounds.level<=0) {
        this.wounds.level = 0;
        this.wounds.type = null;
      }
      if (this.wounds.level>=8) {
        if (this.entity.die) {
          this.entity.die();
        } else {
          this.entity.destroy();
          HTomb.GUI.sensoryEvent(this.entity.describe({article: "indefinite", capitalized: true}) +" is destroyed.",x,y,z,"#FFBB00");
        }
      }
    },
    endure: function(roll, event) {
      let attack = event.attacker.attacker;
      let modifiers = event.modifiers;
      let atype = modifiers.damage.type || attack.damage.type;
      let alevel = attack.damage.level + modifiers.damage.level;
      let armor = {
        material: modifiers.armor.type || this.armor.material,
        level: this.armor.level + modifiers.armor.level
      };
      if (attack.entity && attack.entity.equipper
            && attack.entity.equipper.slots.MainHand
            && attack.entity.equipper.slots.MainHand.damage) {
        let d = attack.entity.equipper.slots.MainHand;
        if (d.type) {
          atype = modifiers.damage.type || d.type;
        }
        if (d.level) {
          alevel = d.level + modifiers.damage.level;
        }
      }
      let modifier = HTomb.Types.Damage.table[atype][this.material];
      let penetrate = HTomb.Types.Damage.table[atype][armor.material];
      let total = roll;
      console.log("Roll: " + total);
      total += alevel;
      total += modifier;
      total += this.wounds.level;
      if (attack.entity.defender) {
        total -= attack.entity.defender.wounds.level;
      }
      // armor can never leave you worse off
      total -= Math.max(0, armor.level-penetrate);
      total -= (this.toughness + modifiers.toughness);
      console.log("Total: " + total);
      let x = this.entity.x;
      let y = this.entity.y;
      let z = this.entity.z;
      let attacker = attack.entity.describe({capitalized: true, article: "indefinite"});
      let defender = this.entity.describe({article: "indefinite"});
      let type = HTomb.Types[atype].name;
      if (total>=20) {
        HTomb.GUI.sensoryEvent(attacker + " deals critical " + type + " damage to " + defender + ".",x,y,z,"red");
        this.wounds.level = 8;
        this.wounds.type = atype;
      } else if (total>=17) {
        HTomb.GUI.sensoryEvent(attacker + " deals severe " + type + " damage to " + defender + ".",x,y,z,"#FFBB00");
        if (this.wounds.level<6) {
          this.wounds.level = 6;
        } else {
          this.wounds.level = 8;
        }
        this.wounds.type = atype;
      } else if (total>=14) {
        HTomb.GUI.sensoryEvent(attacker + " deals " + type + " damage to " + defender + ".",x,y,z,"orange");
        if (this.wounds.level<4) {
          this.wounds.level = 4;
          this.wounds.type = atype;
        } else if (this.wounds.level===4) {
          this.wounds.level+=2;
          this.wounds.type = atype;
        } else {
          this.wounds.level+=2;
          if (!type) {
            this.wounds.type = atype;
          }
        }
      } else if (total>=8) {
        HTomb.GUI.sensoryEvent(attacker + " deals mild " + type + " damage to " + defender + ".",x,y,z,"#FFBB00");
        if (this.wounds.level<2) {
          this.wounds.level = 2;
          this.wounds.type = atype;
        // cannot die of a mild wound
        } else if (this.wounds.level<7) {
          this.wounds.level+=1;
          if (!atype) {
            this.wounds.type = atype;
          }
        }
      } else {
        HTomb.GUI.sensoryEvent(attacker + " hits " + defender +" but deals no damage.",x,y,z,"yellow");
      }
      console.log("Wounds: ",this.wounds.level);
      this.tallyWounds();
    },
    defend: function(event) {
      let roll = ROT.RNG.getUniformInt(1,20);
      this.endure(roll, event);      
    },
    onDescribe: function(options) {
      let pre = options.pre || [];
      if (this.entity.parent==="Feature") {
        let wounds = this.wounds.level;
        if (wounds<=0) {
          return options;
        } else if (wounds<=3) {
          pre.push("mildly damaged");
        } else if (wounds<=5) {
          pre.push("damaged");
        } else if (wounds<=6) {
          pre.push("severely damaged");
        } else {
          pre.push("totaled");
        }
        options.pre = pre;
        options.startsWithVowel = false;
        return options;
      }
      return options;
    }
  });

  return HTomb;
})(HTomb);
