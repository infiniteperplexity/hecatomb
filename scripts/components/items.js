// ****** This module implements Components, which are the basic units of functionality for creatures, items, and features
HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;

  let Component = HTomb.Things.Component;
// The Inventory bomponent allows a creature to carry things
  Component.extend({
    template: "Inventory",
    name: "inventory",
    capacity: 10,
    onSpawn: function() {
      this.items = HTomb.Things.Items(this);
    },
    pickup: function(item) {
      var e = this.entity;
      if (this.entity===HTomb.Player || (this.entity.minion && this.entity.minion.master===HTomb.Player)) {
        item.owned = true;
      }
      item.remove();
      HTomb.GUI.sensoryEvent(this.entity.describe({capitalized: true, article: "indefinite"}) + " picks up " + item.describe({article: "indefinite"})+".",e.x,e.y,e.z);
      this.items.addItem(item);
      this.entity.actor.acted = true;
      this.entity.actor.actionPoints-=16;
    },
    pickupOne: function(i_or_t) {
      var e = this.entity;
      var items = HTomb.World.items[coord(e.x,e.y,e.z)] || HTomb.Things.Items();
      var item = items.take(i_or_t);
      if (item) {
        this.pickup(item);
      }
    },
    pickupSome: function(i_or_t,n) {
      var e = this.entity;
      var items = HTomb.World.items[coord(e.x,e.y,e.z)] || HTomb.Things.Items();
      var item = items.take(i_or_t,n);
      if (item) {
        this.pickup(item);
      }
    },
    drop: function(item) {
      var e = this.entity;
      this.items.removeItem(item);
      item.place(e.x,e.y,e.z);
      HTomb.GUI.sensoryEvent(this.entity.describe({capitalized: true, article: "indefinite"}) + " drops " + item.describe({article: "indefinite"})+".",e.x,e.y,e.z);
      this.entity.actor.acted = true;
      this.entity.actor.actionPoints-=16;
    },
    push: function(item) {
      this.addItem(item);
    },
    unshift: function(item) {
      this.addItem(item);
    },
    addItem: function(item) {
      this.items.addItem(item);
    },
    count: function(arg) {
      return this.items.count(arg);
    },
    contains: function(item) {
      return this.items.contains(item);
    },
    getItem: function(arg) {
      return this.items.getItem(arg);
    },
    take: function(arg,n) {
      return this.items.take(arg,n);
    },
    takeItems: function(ings) {
      return this.items.takeItems(ings);
    },
    removeItem: function(item) {
      return this.items.removeItem(item);
    },
    hasAll: function(ingredients) {
      return this.items.hasAll(ingredients);
    },
    asIngredients: function() {
      return this.items.asIngredients();
    },
    // !!!! should there be an "onEntityDestroyed"?
    onDespawn: function() {
      //should probably drop all the items, right?
      if (this.entity.isPlaced()) {
        for (let item of this.items) {
          this.drop(item);
        }
      }
    }
  });

  Component.extend({
    template: "Equipment",
    name: "equipment",
    slot: null,
    labor: 1,
    onEquip: function(equipper) {},
    onUnequip: function(equipper) {}
  });

  Component.extend({
    template: "Equipper",
    name: "equipper",
    items: null,
    slots: {
      MainHand: null,
      OffHand: null    
    },
    onSpawn: function(args) {
      this.items = HTomb.Things.Items(this);
      this.slots = HTomb.Utils.copy(this.slots);
      return this;
    },
    unequipItem: function(slot_or_item) {
      let item;
      if (typeof(slot_or_item)==="string") {
        item = this.slots[slot];
      } else {
        item = slot_or_item;
      }
      if (item) {
        this.items.remove(item);
        item.equipment.onUnequip(this);
        if (this.entity.inventory) {
          this.entity.inventory.add(item);
        } else {
          let e = this.entity;
          item.place(e.x,e.y,e.z);
        }
        this.entity.slot = null;
      }
      let x = this.entity.x;
      let y = this.entity.y;
      let z = this.entity.z;
      HTomb.GUI.sensoryEvent(this.entity.describe({capitalized: true, article: "indefinite"}) + " unequips " + item.describe({article: "indefinite"})+".",x,y,z);
      this.entity.actor.actionPoints-=16;
      this.entity.acted = true;
      return item;
    },
    // should this always use action points?
    equipItem: function(item) {
      let e = item.equipment;
      if (e) {
        let slot = e.slot;
        if (this.slots[slot]!==undefined) {
          if (this.slots[slot]!==null) {
            this.unequipItem(slot);
          }
          this.items.push(item);
          e.onEquip(this);
          this.slots[slot] = item;
        }
      }
      let x = this.entity.x;
      let y = this.entity.y;
      let z = this.entity.z;
      HTomb.GUI.sensoryEvent(this.entity.describe({capitalized: true, article: "indefinite"}) + " equips " + item.describe({article: "indefinite"})+".",x,y,z);
      this.entity.actor.actionPoints-=16;
      this.entity.acted = true;
    }
  });

  return HTomb;
})(HTomb);
