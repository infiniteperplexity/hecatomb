
  HTomb.Things.defineStructure({
    template: "Storage",
    height: 3,
    width: 3,
    dormant: 0,
    dormancy: 10,
    stores: function(item) {return false;},
    onTurnBegin: function() {
      // this probably shouldn't happen every turn...maybe have a countdown?
      if (this.dormant>0) {
        this.dormant-=1;
        return;
      }
      this.dormant = this.dormancy;
      let items = this.owner.master.ownedItems;
      for (let i=0; i<items; i++) {
        if (!item.item.isOnGround()) {
          continue;
        } else if (this.stores(item)===false) {
          continue;
        } else if (HTomb.World.tasks[coord(item.x,item.y,item.z)]!==undefined) {
          continue;
        } else {
          let f = HTomb.World.features[coord(item.x,item.y,item.z)];
          if (f.feature.structure && f.feature.structure.template===this.template) {
            continue;
          } else {
            z = HTomb.Things.HaulTask({
              assigner: this.owner,
              stores: this.stores,
              name: "haul " + item.describe()
            });
          z.place(item.x,item.y,item.z);
          }
        }
      }
    }
  });

  HTomb.Things.defineTask({
    template: "HaulTask",
    name: "haul",
    bg: "#555544",
    hauls: function(item) {return false;},
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
      if (this.validTile(x,y,z) && HTomb.Tiles.isReachableFrom(x,y,z,cr.x,cr.y,cr.z, {
        searcher: cr,
        searchee: this.entity,
        searchTimeout: 10
      }) && this.getSomeValidItem(cr)) {
        return true;
      } else {
        return false;
      }
    },
    getSomeValidItem: function(cr) {
      // right now we ignore the creature argument
      let pile = HTomb.World.items[coord(this.x,this.y,this.z)] || HTomb.Things.Container();
      let that = this;
      let items = this.assigner.master.ownedItems.filter(function(item) {
        if (that.hauls(item)!==true) {
          return false;
        } else if (pile.canFit(item)<1) {
          return false;
        } else if (item.item.isOnGround()!==true) {
          return false;
        }
        let task = HTomb.World.tasks[coord(item.x,item.y,item.z)];
        if (task && task.task.template==="HaulTask") {
          return false;
        }
        if (HTomb.Tiles.isReachableFrom(item.x, item.y, item.z, that.entity.x, that.entity.y, that.entity.z, {
          searcher: cr,
          searchee: item,
          searchTimeout: 10
        })) {
          return true;
        } else {
          return false;
        }
      });
      if (items.length>0) {
        return HTomb.Path.closest(cr.x,cr.y,cr.z,items)[0];
      } else {
        return null;
      }
    },
    ai: function() {
      let cr = this.assignee;
      // this could be either the task square or the item
      let t = cr.ai.target;
      if (cr.movement) {
        var x = this.entity.x;
        var y = this.entity.y;
        var z = this.entity.z;
        var path = HTomb.Path.aStar(cr.x,cr.y,cr.z,x,y,z,{
          canPass: HTomb.Utils.bind(cr.movement,"canMove"),
          searcher: cr,
          searchee: t,
          searchTimeout: 10
        });
        if (path===false) {
          this.unassign();
          cr.ai.walkRandom();
        } else {
          let carrying = false;
          for (let j=0; j<cr.inventory.items.length; j++) {
            let item = cr.inventory.items.expose(j);
            if (this.hauls(item)) {
              carrying=true;
              // if you're standing on the square and have a valid item, drop it
              if (cr.x===x && cr.y===y && cr.z===z) {
                cr.inventory.drop(item);
                cr.ai.target = null;
                this.unassign();
                HTomb.Events.publish({type: "Complete", task: this});
              } else {
                // otherwise if you have a valid item
                cr.ai.walkToward(x,y,z, {
                  searcher: cr,
                  searchee: item,
                  searchTimeout: 10
                });
              }
              break;
            }
          }
          if (carrying===false) {
            // if not carrying any valid item...hold on...shouldn't we keep a valid target if we already have one?
            if (!t || !t.item || !this.hauls(t) || !t.isPlaced()) {
              cr.ai.target = this.getSomeValidItem(cr);
            }
            // should maybe use fetch with an option to avoid things in hoards?
            if (cr.ai.target===null) {
              this.unassign();
              cr.ai.walkRandom();
            } else if (cr.x===cr.ai.target.x && cr.y===cr.ai.target.y && cr.z===cr.ai.target.z) {
              let pile = HTomb.World.items[coord(this.x,this.y,this.z)] || HTomb.Things.Container();
              let item = cr.ai.target;
              let n = Math.min(pile.canFit(item),item.item.n);
              if (n===0) {
                this.unassign();
                cr.ai.walkRandom();
              } else {
                cr.inventory.pickupSome(item.template,n);
                cr.ai.target = null;
              }
            } else {
              cr.ai.walkToward(cr.ai.target.x,cr.ai.target.y,cr.ai.target.z, {
                searcher: cr,
                searchee: cr.ai.target,
                searchTimeout: 10
              });
            }
          }
        }
      }
      cr.ai.acted = true;
    }
  });