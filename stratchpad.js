
  HTomb.Things.defineStructure({
    template: "Storage",
    height: 3,
    width: 3,
    dormant: 0,
    dormancy: 10,
    tasks: null,
    stores: function(item) {return false;},
    onCreate: function() {
      this.tasks = [];
      for (let i=0; i<this.width*this.height; i++) {
        this.tasks.push(null);
      }
      return this;
    },
    onTurnBegin: function() {
      // this probably shouldn't happen every turn...maybe have a countdown?
      if (this.dormant>0) {
        this.dormant-=1;
        return;
      }
      this.dormant = this.dormancy;
      let items = this.owner.master.ownedItems;
      for (let i=0; i<items; i++) {
        //if ever we run out of task space, break the loop
        if (this.tasks.indexOf(null)===-1) {
          return;
        }
        let item = items[i];
        let f = HTomb.World.features[coord(item.x,item.y,item.z)];
        if (!item.item.isOnGround()) {
          continue;
        } else if (this.stores(item)===false) {
          continue;
        } else if (HTomb.World.tasks[coord(item.x,item.y,item.z)]!==undefined) {
          continue;
        } else if (HTomb.Tiles.isReachableFrom(this.x, this.y, this.z, item.x, item.y, item.z)===false) {
          continue;
        } else if (f.feature.structure && f.feature.structure.template===this.template) {
          continue;
        } else {
          let slots = [];
          for (let j=0; j<this.tasks.length; j++) {
            if (this.tasks[j]===null) {
              slots.push(j);
            }
          }
          HTomb.Utils.shuffle(slots[0]);
          f = this.features[j];
          z = HTomb.Things.HaulTask({
             assigner: this.owner,
             item: item,
             storage: this,
             feature: f,
             name: "haul " + item.describe()
           });
          z.place(item.x,item.y,item.z);
          this.tasks[slots[0]] = z;
        }
      }
    }
  });

  HTomb.Things.defineTask({
    template: "HaulTask",
    name: "haul",
    bg: "#555544",
    item: null,
    feature: null,
    validTile: function(x,y,z) {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      if (HTomb.World.tiles[z][x][y]===HTomb.Tiles.FloorTile) {
        return true;
      } else {
        return false;
      }
    },
    canAssign: function(cr) {
      let x = this.entity.x;
      let y = this.entity.y;
      let z = this.entity.z;
      // should this check whether the item is still here?
      if (this.validTile(x,y,z) && HTomb.Tiles.isReachableFrom(x,y,z,cr.x,cr.y,cr.z, {
        searcher: cr,
        searchee: this.entity,
        searchTimeout: 10
      }) && this.item.x===x && this.item.y===y && this.item.z===z) {
        return true;
      } else {
        return false;
      }
    },
    ai: function() {
      let cr = this.assignee;
      let item = this.item;
      let feature = this.feature;
      // this could be either the task square or the item
      let x = this.entity.x;
      let y = this.entity.y;
      let z = this.entity.z;
      if (x===cr.x && y===cr.y && z===cr.z) {
        if (cr.inventory.items.items.indexOf(this.item)) {
          cr.inventory.drop(this.item);
          this.complete();
          cr.ai.acted = true;
          return;
        } else if (item.x===cr.x && item.y===cr.y && item.z===cr.z) {
          // move it to the building;
          this.place()
          cr.inventory.pickup(item);
          cr.ai.acted = true;
           eturn;
        } else {
          console.log("something went wrong with hauling!");
        }
      }
      if (cr.inventory.items.items.indexOf(this.item)) {
        cr.ai.target = feature;
      } else if (item.item.isOnGround())  {
        cr.ai.target = item;
      } else {
        this.cancel();
        return;
      }
      let t = cr.ai.target;
      cr.ai.walkToward(x,y,z, {
        searcher: cr,
        searchee: t,
        searchTimeout: 10
      });
      cr.ai.acted = true;
    },
    onDespawn: function() {
      let tasks = this.storage.tasks;
      tasks.splice(tasks.indexOf[this],1,null);
    }
  });