HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var coord = HTomb.Utils.coord;

  let Entity = HTomb.Things.Entity;

  let Task = Entity.extend({
    template: "Task",
    name: "task",
    // description is the name for the job assignment menu
    description: "a generic task",
    // assigner and assignee are the most fundamental task behavior
    assigner: null,
    assignee: null,
    // many tasks have ingredients and/or make a configurable thing
    ingredients: {},
    // do I need to manually create this for instances?
    claimedItems: [],
    makes: null,
    // tasks sometimes go dormant when assignment fails
    dormant: 0,
    dormancy: 6,
    priority: 0,
    workRange: 1,
    // placeholder...
    behaviors: [],
    blurb: function() {
      return (this.assignee.describe({capitalized: true, article: "indefinite"}) + " begins work on " + this.describe({atCoordinates: true}) + ".");
    },
    // Entity-placement behavior
    place: function(x,y,z,args) {
      Entity.place.call(this,x,y,z,args);
      if (this.assigner && this.assigner.master && this.assigner.master.taskList.indexOf(this)===-1) {
        this.assigner.master.taskList.push(this);
      }
      let t = HTomb.World.tasks[coord(x,y,z)];
      if (t) {
        // not sure how I plan on handling such conflicts...
        if (false) {
          throw new Error("Unhandled task placement conflict!");
        }
        t.remove();
        t.despawn();
      }
      //HTomb.World.tasks[coord(x,y,z)] = this.entity;
      HTomb.World.tasks[coord(x,y,z)] = this;
      return this;
    },
    remove: function() { 
        let x = this.x;
        let y = this.y;
        let z = this.z;
        delete HTomb.World.tasks[coord(x,y,z)];
        Entity.remove.call(this);
    },
    spawn: function(args) {
      let o = Entity.spawn.call(this,args);
      HTomb.Events.subscribe(o, "Destroy");
      return o;
    },
    claim: function(item, n) {
      n = n || 1;
      if (this.hasOwnProperty("claimedItems")===false) {
        this.claimedItems = [];
      }
      let idx = this.claimedItems.indexOf(item);
      if (idx===-1) {
        this.claimedItems.push([item, n]);
      } else {
        this.claimedItems[idx][1]+=n;
      }
      item.claimed+=n;
    },
    claimIngredients: function(args) {
      if (HTomb.Debug.noingredients) {
        return;
      }
      args = args || {};
      let ingredients = args.ingredients || this.ingredients || {};
      let cr = this.assignee;
      // assume we have already checked availability
      for (let ingredient in ingredients) {
        let n = this.ingredients[ingredient];
        let items = [];
        for (let item of this.assigner.master.ownedItems()) {
          if (item.template===ingredient && item.isOnGround() 
                && HTomb.Tiles.isReachableFrom(item.x,item.y,item.z,cr.x,cr.y,cr.z,{
                  canPass: cr.movement.boundMove(),
                  searcher: cr,
                  searchee: item,
                  searchTimeout: 10
                })) {
            items.push(item);
          }
        }
        items = HTomb.Path.closest(cr.x,cr.y,cr.z,items);
        for (let item of items) {
          if (n<=0) {
            break;
          }
          if (item.n-item.claimed>=n) {
            this.claim(item,n);
            n = 0;
          } else {
            this.claim(item, item.n-item.claimed)
            n-=(item.n-item.claimed);
          }
        }
      }
    },
    hasClaimed: function(item) {
      for (let tuple of this.claimedItems) {
        if (tuple[0]===item) {
          return true;
        }
      }
      return false;
    },
    unclaim: function(item) {
      if (this.hasClaimed(item)===false) {
        return;
      }
      for (let i=0; i<this.claimedItems.length; i++) {
        let tuple = this.claimedItems[i];
        if (tuple[0]===item) {
          item.claimed-=tuple[1];
          this.claimedItems.splice(i,1);
          return;
        }
      }
    },
    unclaimItems: function() {
      while (this.claimedItems.length>0) {
        // this will often go below zero when items have been picked up
        let tuple = this.claimedItems.pop();
        tuple[0].claimed-=tuple[1];
        if (tuple[0].claimed<0) {
            console.log("claims went below zero: " + tuple[0].describe());
          }
      }
    },
    onDestroy: function(event) {
    let cr = event.entity;
      if (cr===this.assignee) {
        this.unassign();
      } else if (cr===this.assigner) {
        this.despawn();
      }
    },
    despawn: function() {
      this.unclaimItems();
      let master = this.assigner;
        if (master) {
          let taskList = this.assigner.master.taskList;
          ///!!!! First thing I need to refactor...
          // if (taskList.indexOf(this.entity)!==-1) {
              //taskList.splice(taskList.indexOf(this.entity),1);
            if (taskList.indexOf(this)!==-1) {
              taskList.splice(taskList.indexOf(this),1);
            }
        }
      if (this.assignee) {
        this.unassign();
      }
      Entity.despawn.call(this);
    },
    // assignment-related methods
    canAssign: function(cr) {
      let x = this.x;
      let y = this.y;
      let z = this.z;
      if (this.validTile(x,y,z) && HTomb.Tiles.isReachableFrom(x,y,z,cr.x,cr.y,cr.z,{
        searcher: cr,
        searchee: this,
        canPass: cr.movement.boundMove(),
        searchTimeout: 10
      }) && HTomb.Tiles.canFindAll(cr.x, cr.y, cr.z, this.ingredients, {
        searcher: cr,
        respectClaims: (this.assigner===HTomb.Player) ? true : false,
        ownedOnly: (this.assigner===HTomb.Player) ? true : false
      })) {
        return true;
      } else {
        return false;
      }
    },
    assignTo: function(cr) {
      if (cr.minion===undefined) {
        HTomb.Debug.pushMessage("Problem assigning task");
      } else {
        this.assignee = cr;
        this.claimIngredients();
        cr.worker.onAssign(this);
      }
    },
    unassign: function() {
      var cr = this.assignee;
      if (!cr) {
        this.assignee = null;
        return;
      }
      if (cr.worker===undefined) {
        HTomb.Debug.pushMessage("Problem unassigning task");
      } else {
        this.assignee = null;
        this.unclaimItems();
        cr.worker.unassign();
      }
    },
    cancel: function() {
      this.despawn();
    },
    // default methods for designating tasks
    designate: function(assigner) {
      HTomb.GUI.selectSquareZone(assigner.z,this.designateSquares,{
          context: this,
          assigner: assigner,
          callback: this.designateTile,
          outline: false,
          bg: this.bg,
          contextName: "Designate"+this.template
      });
    },
    designateSquare: function(x,y,z, options) {
      options = options || {};
      var assigner = options.assigner;
      var callb = options.callback;
      callb.call(options.context,x,y,z,assigner);
    },
    designateSquares: function(squares, options) {
      options = options || {};
      var assigner = options.assigner;
      var callb = options.callback;
      for (var i=0; i<squares.length; i++) {
        var crd = squares[i];
        callb.call(options.context,crd[0],crd[1],crd[2],assigner);
      }
    },
    // wait what's the difference between designateTile and designateSquare?
    designateTile: function(x,y,z,assigner) {
      if (this.validTile(x,y,z)) {
        let t = HTomb.Things[this.template].spawn({assigner: assigner}).place(x,y,z);
        HTomb.Events.publish({type: "Designate", task: this});
        //HTomb.Events.publish({type: "Designate", task: t.task});
        return t;
      }
    },
    validTile: function(x,y,z) {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      if (HTomb.World.tiles[z][x][y]!==HTomb.Tiles.FloorTile) {
        return false;
      }
      let f = HTomb.World.features[coord(x,y,z)];
      if (f===undefined || (f.template==="IncompleteFeature" && f.makes===this.makes)) {
        return true;
      }
      else {
        return false;
      }
    },
    //this will get renamed "actor" or "action", I think, or "act"
    ai: function() {
      let cr = this.assignee;
      if (cr.ai.acted) {
        return;
      }
      // if it still needs items, fetch one
      HTomb.Routines.FetchItems.act(cr.ai, {
        task: this,
        ingredients: this.ingredients
      });
      if (cr.ai.acted) {
        return;
      }
      // if this got unassigned
      if (this.assignee==null) {
        return;
      }
      // otherwise, try to work
      if (this.x===cr.x && this.y===cr.y && this.z===cr.z) {
        // can always work from range 0
        this.workOnTask(this.x,this.y,this.z);
      } else if (this.workRange>=1 && HTomb.Tiles.isTouchableFrom(this.x,this.y,this.z,cr.x,cr.y,cr.z)) {
        this.workOnTask(this.x,this.y,this.z);
      // otherwise, walk toward
      } else {
        cr.ai.walkToward(this.x,this.y,this.z, {
          searcher: cr,
          searchee: this,
          searchTimeout: 10
        });
      }
    },
    onFetchFail: function() {
      this.unassign();
    },
    // "workOnTask" is basically a switch statement based on the progress of work
    workOnTask: function() {
      let x = this.x;
      let y = this.y;
      let z = this.z;
      //cancel the task if something weird happened to the tile
      if (this.validTile(x,y,z)!==true) {
        this.cancel();
      }
      //do I want to make demolishing unowned features the default?
      // maybe do that with "subsidiary tasks"
      if (this.begun()!==true) {
        this.expend();
        this.begin();
      }
      this.work();
      let f = HTomb.World.features[coord(x,y,z)];
      if (f && f.labor<=0) {
        this.finish();
        this.complete();
      }
    },
    expend: function() {
      if (HTomb.Debug.noingredients) {
        return;
      }
      if (this.assignee.inventory.items.hasAll(this.ingredients)!==true) {
        throw new Error("shouldn't reach this due to AI");
      }
      HTomb.GUI.pushMessage(this.blurb());
      let items = this.assignee.inventory.items.takeItems(this.ingredients);
      for (let i=0; i<items.length; i++) {
        items[i].despawn();
      }
    },
    begin: function() {
      let f = HTomb.Things.IncompleteFeature.spawn({makes: this.makes});
      f.place(this.x,this.y,this.z);
    },
    work: function() {
      let x = this.x;
      let y = this.y;
      let z = this.z;
      let f = HTomb.World.features[coord(x,y,z)];
      let labor = this.assignee.worker.getLabor();
      if (f.effort===undefined || f.effort===null) {
        Math.max(labor,0);
      } else {
        labor = Math.max(0, labor-f.effort);
      }
      f.labor-=labor;
      this.assignee.ai.acted = true;
      this.assignee.ai.actionPoints-=16;
    },
    finish: function() {
      let x = this.x;
      let y = this.y;
      let z = this.z;
      let f = HTomb.World.features[coord(x,y,z)];
      f.remove();
      let newf = HTomb.Things[f.makes].spawn();
      newf.owner = this.assigner;
      newf.place(x,y,z);
      if (newf.solid) {
        HTomb.Path.reset();
      }
      f.despawn();
      ///!!!! Very ad hoc solution...not sure whether I like it
      if (newf.placeholder) {
        newf.despawn();
      }
    },
    begun: function() {
      let x = this.x;
      let y = this.y;
      let z = this.z;
      let f = HTomb.World.features[coord(x,y,z)];
      if (f && f.template==="IncompleteFeature" && f.makes===this.makes) {
        return true;
      } else {
        return false;
      }
    },
    complete: function(x,y,z) {
      HTomb.Events.publish({type: "Complete", task: this});
      this.despawn();
    }
  });

  Task.extend({
    template: "DigTask",
    name: "dig",
    description: "dig corridors/pits/slopes",
    bg: "#884400",
    makes: "Excavation",
    dormancy: 0,
    canAssign: function(cr) {
      let x = this.x;
      let y = this.y;
      let z = this.z;
      let tf = Task.canAssign.call(this, cr);
      if (tf===false) {
        return false;
      }
      let soil = HTomb.World.covers[z][x][y];
      let t = HTomb.World.tiles[z][x][y];
      if (t===HTomb.Tiles.FloorTile || t===HTomb.Tiles.DownSlopeTile) {
        soil = HTomb.World.covers[z-1][x][y];
      }
      let hardness;
      if (soil.hardness===undefined) {
        hardness = 0;
      } else {
        hardness = soil.hardness;
      }
      let labor = 0;
      if (cr.worker) {
        labor = cr.worker.getLabor();
      }
      if (labor-hardness<=0) {
        return false;
      }
      return true;
    },
    validTile: function(x,y,z) {
      if (HTomb.World.explored[z][x][y]!==true) {
        return true;
      }
      var t = HTomb.World.tiles[z][x][y];
      var tb = HTomb.World.tiles[z-1][x][y];
      // this is the special part for DigTask
      let f = HTomb.World.features[coord(x,y,z)];
      if (t===HTomb.Tiles.VoidTile) {
        return false;
      } else if (f && (f.template!=="IncompleteFeature" || this.makes!==f.makes)) {
        return false;
      } else if (t===HTomb.Tiles.FloorTile && tb===HTomb.Tiles.VoidTile) {
        return false;
      } else if (t===HTomb.Tiles.EmptyTile && (tb===HTomb.Tiles.EmptyTile || tb===HTomb.Tiles.FloorTile)) {
        return false;
      }
      let soil = HTomb.World.covers[z][x][y];
      if (t===HTomb.Tiles.FloorTile || t===HTomb.Tiles.DownSlopeTile) {
        soil = HTomb.World.covers[z-1][x][y];
      }
      let hardness;
      if (soil.hardness===undefined) {
        hardness = 0;
      } else {
        hardness = soil.hardness;
      }
      let labor = 0;
      // no assigner yet at this point?
      for (let minion of HTomb.Player.master.minions) {
        labor = Math.max(labor, minion.worker.getLabor());
      }
      if (labor-hardness<=0) {
        return false;
      }
      return true;
    },
    // experiment with a filter to dig only one level at a time
    designateSquares: function(squares, options) {
      var tallest = -1;
      for (var j=0; j<squares.length; j++) {
        var s = squares[j];
        let tile = HTomb.World.tiles[s[2]][s[0]][s[1]];
        if (tile===HTomb.Tiles.WallTile) {
          tallest = Math.max(tallest,1);
        } else if (tile===HTomb.Tiles.UpSlopeTile) {
          tallest = Math.max(tallest,1);
        } else if (tile===HTomb.Tiles.FloorTile) {
          tallest = Math.max(tallest,0);
        }
      }
      if (tallest===1) {
        squares = squares.filter(function(e,i,a) {
          return (HTomb.World.tiles[e[2]][e[0]][e[1]]===HTomb.Tiles.UpSlopeTile
                  || HTomb.World.tiles[e[2]][e[0]][e[1]]===HTomb.Tiles.WallTile);
        });
      } else if (tallest===0) {
        squares = squares.filter(function(e,i,a) {
          return (HTomb.World.tiles[e[2]][e[0]][e[1]]===HTomb.Tiles.FloorTile);
        });
      }
      Task.designateSquares.call(this, squares, options);
    },
    designate: function(assigner) {
      let menu = HTomb.GUI.Panels.menu;
      let that = this;
      function myHover(x, y, z, squares) {
        if (squares===undefined) {
          if (HTomb.World.explored[z][x][y]!==true) {
            menu.middle = ["%c{orange}Unexplored tile."];
            return;
          }
          if (that.validTile(x,y,z)!==true) {
            menu.middle = ["%c{orange}Cannot dig here."];
            return;
          }
          let tile = HTomb.World.tiles[z][x][y];
          if (tile===HTomb.Tiles.DownSlopeTile) {
            menu.middle = ["%c{lime}Digging here will excavate the slope on the level below."];
          } else if (tile===HTomb.Tiles.UpSlopeTile) {
            menu.middle = ["%c{lime}Digging here will remove the slope."];
          } else if (tile===HTomb.Tiles.FloorTile) {
            menu.middle = ["%c{lime}Digging here will excavate a slope to a lower level."];
          } else if (tile===HTomb.Tiles.WallTile) {
            menu.middle = ["%c{lime}Digging here from the side makes a roofed tunnel; digging from an upward slope below makes a downward slope."];
          } else {
            menu.middle = ["%c{orange}Cannot dig here."];
          }
          return;
        }
        var tallest = -2;
        for (var j=0; j<squares.length; j++) {
          var s = squares[j];
          let tile = HTomb.World.tiles[s[2]][s[0]][s[1]];
          if (tile===HTomb.Tiles.WallTile) {
            tallest = Math.max(tallest,1);
          } else if (tile===HTomb.Tiles.UpSlopeTile) {
            tallest = Math.max(tallest,1);
          } else if (tile===HTomb.Tiles.FloorTile) {
            tallest = Math.max(tallest,0);
          } else if (tile===HTomb.Tiles.DownSlopeTile) {
            tallest = Math.max(tallest,-1);
          }
        }
        if (tallest===1) {
          menu.middle = ["%c{lime}Dig tunnels and level slopes in this area."];
        } else if (tallest===0) {
          menu.middle = ["%c{lime}Dig downward slopes in this area."];
        } else if (tallest===-1) {
          menu.middle = ["%c{lime}Level downward slopes below this area."];
        } else {
          menu.middle = ["%c{orange}Can't dig in this area."];
        }
      }
      HTomb.GUI.selectSquareZone(assigner.z,this.designateSquares,{
        context: this,
        assigner: assigner,
        callback: this.designateTile,
        bg: this.bg,
        hover: myHover,
        contextName: "Designate"+this.template
      });
    },
    designateTile: function(x,y,z,assigner) {
      if (this.validTile(x,y,z) || HTomb.World.explored[z][x][y]!==true) {
        let t = HTomb.Things[this.template].spawn({assigner: assigner}).place(x,y,z);
        HTomb.Events.publish({type: "Designate", task: this});
        return t;
      }
    },
    begin: function() {
      Task.begin.call(this);
      let x = this.x;
      let y = this.y;
      let z = this.z;
      let f = HTomb.World.features[coord(x,y,z)];
      let soil = HTomb.World.covers[z][x][y];
      if (HTomb.World.tiles[z][x][y]===HTomb.Tiles.DownSlopeTile || HTomb.World.Tiles===HTomb.Tiles.FloorTile) {
        soil = HTomb.World.covers[z-1][x][y];
      }
      if (soil.hardness===undefined) {
        f.effort = 0;
      } else {
        f.effort = soil.hardness;
      } 
    },
    finish: function() {
      let x = this.x;
      let y = this.y;
      let z = this.z;
      var tiles = HTomb.World.tiles;
      var EmptyTile = HTomb.Tiles.EmptyTile;
      var FloorTile = HTomb.Tiles.FloorTile;
      var WallTile = HTomb.Tiles.WallTile;
      var UpSlopeTile = HTomb.Tiles.UpSlopeTile;
      var DownSlopeTile = HTomb.Tiles.DownSlopeTile;
      var t = tiles[z][x][y];
      let c = HTomb.World.covers[z][x][y];
      let downOne = false;
      // this should unforbid items...
      // If there is a slope below, dig out the floor
      if (tiles[z-1][x][y]===UpSlopeTile && HTomb.World.explored[z-1][x][y] && (t===WallTile || t===FloorTile)) {
        tiles[z][x][y] = DownSlopeTile;
      // If it's a wall, dig a tunnel
      } else if (t===WallTile) {
        tiles[z][x][y] = FloorTile;
        if (c.mine) {
          c.mine(x,y,z);
        }
      } else if (t===FloorTile) {
        // If it's a floor with a wall underneath dig a trench
        if (tiles[z-1][x][y]===WallTile) {
          tiles[z][x][y] = DownSlopeTile;
          tiles[z-1][x][y] = UpSlopeTile;
          c = HTomb.World.covers[z-1][x][y];
          if (c.mine) {
            c.mine(x,y,z-1);
          }
          downOne = true;
        // Otherwise just remove the floor
        } else {
          tiles[z][x][y] = EmptyTile;
        }
      // If it's a down slope tile, remove the slopes
      } else if (t===DownSlopeTile) {
        tiles[z][x][y] = EmptyTile;
        tiles[z-1][x][y] = FloorTile;
        downOne = true;
      // if it's an upward slope, remove the slope
      } else if (t===UpSlopeTile) {
        tiles[z][x][y] = FloorTile;
        if (tiles[z+1][x][y]===DownSlopeTile) {
          tiles[z+1][x][y] = EmptyTile;
          downOne = true;
        }
      } else if (t===EmptyTile) {
        // this shouldn't happen
      }
      // Eventually this might get folded into mining...
      HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
      if (Math.random()<0.25) {
        var rock = HTomb.Things.Rock.spawn();
        rock.n = 1;
        if (tiles[z][x][y]===DownSlopeTile) {
          let item = rock.place(x,y,z-1);
          item.owned = true;
        } else {
          let item = rock.place(x,y,z);
          item.owned = true;
        }
      }
      let items = HTomb.World.items[coord(x,y,z)] || HTomb.Things.Items();
      if (downOne) {
        items = HTomb.World.items[coord(x,y,z-1)] || HTomb.Things.Items();
      }
      for (let item of items) {
        item.owned = true;
      }
      HTomb.World.validate.cleanNeighbors(x,y,z);
      let f = HTomb.World.features[coord(x,y,z)];
      f.remove();
      f.despawn();
      //!!!Experimental...hope this doesn't slow things down
      HTomb.Path.reset();
    }
  });

  Task.extend({
    template: "BuildTask",
    name: "build",
    //description: "build walls/floors/slopes",
    description: "build walls or floors",
    bg: "#440088",
    makes: "Construction",
    ingredients: {Rock: 1},
    validTile: function(x,y,z) {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      //shouldn't be able to build surrounded by emptiness
      var t = HTomb.World.tiles[z][x][y];
      let f = HTomb.World.features[coord(x,y,z)];
      if (t===HTomb.Tiles.VoidTile || t===HTomb.Tiles.WallTile) {
        return false;
      } else if (f && (f.template!=="IncompleteFeature" || this.makes!==f.makes)) {
        return false;
      } else {
        return true;
      }
    },
    designateSquares: function(squares, options) {
      var tallest = -1;
      for (var j=0; j<squares.length; j++) {
        var s = squares[j];
        let tile = HTomb.World.tiles[s[2]][s[0]][s[1]];
        // if (tile===HTomb.Tiles.UpSlopeTile) {
        //   tallest = Math.max(tallest,1);
        // } else if (tile===HTomb.Tiles.FloorTile) {
        //   tallest = Math.max(tallest,0);
        // }
        if (tile===HTomb.Tiles.UpSlopeTile || tile===HTomb.Tiles.FloorTile) {
          tallest = Math.max(tallest,1);
        }
      }
      // if (tallest===1) {
      //   squares = squares.filter(function(e,i,a) {
      //     return (HTomb.World.tiles[e[2]][e[0]][e[1]]===HTomb.Tiles.UpSlopeTile);
      //   });
      // } else if (tallest===0) {
      //   squares = squares.filter(function(e,i,a) {
      //     return (HTomb.World.tiles[e[2]][e[0]][e[1]]===HTomb.Tiles.FloorTile);
      //   });
      // }
      if (tallest===1) {
        squares = squares.filter(function(e,i,a) {
          let t = HTomb.World.tiles[e[2]][e[0]][e[1]];
          return (t===HTomb.Tiles.UpSlopeTile || t===HTomb.Tiles.FloorTile);
        });
      }
      HTomb.Things.Task.designateSquares.call(this, squares, options);
    },
    designate: function(assigner) {
      let menu = HTomb.GUI.Panels.menu;
      let that = this;
      function myHover(x, y, z, squares) {
        if (squares===undefined) {
          if (HTomb.World.explored[z][x][y]!==true) {
            menu.middle = ["%c{orange}Unexplored tile."];
            return;
          }
          if (that.validTile(x,y,z)!==true) {
            menu.middle = ["%c{orange}Cannot build here."];
            return;
          }
          let tile = HTomb.World.tiles[z][x][y];
          if (tile===HTomb.Tiles.EmptyTile || tile===HTomb.Tiles.DownSlopeTile) {
            menu.middle = ["%c{lime}Building here will construct a floor over empty space."];
          } else if (tile===HTomb.Tiles.UpSlopeTile) {
            menu.middle = ["%c{lime}Building here will convert this slope into a wall."];
          } else if (tile===HTomb.Tiles.FloorTile) {
            // menu.middle = ["%c{lime}Building here will construct an upward slope (can be upgraded into a wall.)"];
            menu.middle = ["%c{lime}Building here will construct a wall."];
          } else {
            menu.middle = ["%c{orange}Can't build on this tile."];
          }
          return;
        }
        var tallest = -2;
        for (var j=0; j<squares.length; j++) {
          var s = squares[j];
          let tile = HTomb.World.tiles[s[2]][s[0]][s[1]];
          // if (tile===HTomb.Tiles.UpSlopeTile) {
          if (tile===HTomb.Tiles.UpSlopeTile || tile===HTomb.Tiles.FloorTile) {
            tallest = Math.max(tallest,1);
          // } else if (tile===HTomb.Tiles.FloorTile) {
          //   tallest = Math.max(tallest,0);
          } else if (tile===HTomb.Tiles.DownSlopeTile || tile===HTomb.Tiles.EmptyTile) {
            tallest = Math.max(tallest,-1);
          }
        }
        if (tallest===1) {
          menu.middle = ["%c{lime}Construct new walls in this area."];
        // } else if (tallest===0) {
        //   menu.middle = ["%c{lime}Construct new slopes in this area."];
        } else if (tallest===-1) {
          menu.middle = ["%c{lime}Construct new floors in this area."];
        } else {
          menu.middle = ["%c{orange}Can't build in this area."];
        }
      };
      HTomb.GUI.selectSquareZone(assigner.z,this.designateSquares,{
        context: this,
        assigner: assigner,
        callback: this.designateTile,
        outline: true,
        bg: this.bg,
        hover: myHover,
        contextName: "Designate"+this.template
      });
    },
    finish: function() {
      let x = this.x;
      let y = this.y;
      let z = this.z;
      var tiles = HTomb.World.tiles;
      var EmptyTile = HTomb.Tiles.EmptyTile;
      var FloorTile = HTomb.Tiles.FloorTile;
      var WallTile = HTomb.Tiles.WallTile;
      var UpSlopeTile = HTomb.Tiles.UpSlopeTile;
      var DownSlopeTile = HTomb.Tiles.DownSlopeTile;
      var t = tiles[z][x][y];
      HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
      // If it's a floor, build a slope
      // !!!Now build a wall instead
      if (t===FloorTile) {
        // tiles[z][x][y] = UpSlopeTile;
        // if (tiles[z+1][x][y]===EmptyTile) {
        //   tiles[z+1][x][y] = DownSlopeTile;
        // }
        tiles[z][x][y] = WallTile;
      // If it's a slope, make it into a wall
    } else if (t===UpSlopeTile) {
        tiles[z][x][y] = WallTile;
        if (tiles[z+1][x][y] === DownSlopeTile) {
          tiles[z+1][x][y] = FloorTile;
        }
      // If it's empty, add a floor
      } else if (t===DownSlopeTile || t===EmptyTile) {
        tiles[z][x][y] = FloorTile;
      }
      HTomb.World.validate.cleanNeighbors(x,y,z);
      let f = HTomb.World.features[coord(x,y,z)];
      f.remove();
      f.despawn();
      //!!!Experimental...hope this doesn't slow things down
      HTomb.Path.reset();
    }
  });

  let Feature = HTomb.Things.Feature;


  Feature.extend({
    template: "IncompleteFeature",
    name: "incomplete feature",
    symbol: "\u25AB",
    fg: "#BB9922",
    makes: null,
    finished: false,
    labor: 5,
    effort: 0,
    onSpawn: function(args) {
      if (args.makes) {
        let makes = HTomb.Things[args.makes];
        this.makes = args.makes;
        this.labor = makes.labor || this.labor;
        this.effort = makes.effort || this.effort;
        this.symbol = makes.incompleteSymbol || this.symbol;
        this.fg = makes.incompleteFg || makes.fg || this.fg;
        this.name = "incomplete "+ makes.name;
      }
      return this;
    }
  });

  Feature.extend({
    template: "Excavation",
    name: "excavation",
    labor: 10,
    effort: 0,
    incompleteSymbol: "\u2717",
    incompleteFg: HTomb.Constants.BELOWFG
  });

  Feature.extend({
    template: "Construction",
    name: "construction",
    incompleteSymbol: "\u2692",
    labor: 15,
    incompleteFg: HTomb.Constants.WALLFG
  });

  Task.extend({
    template: "Undesignate",
    name: "undesignate",
    description: "undesignate tasks",
    validTile: function() {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      return true;
    },
    designate: function(assigner) {
      var deleteTasks = function(x,y,z, assigner) {
        var task = HTomb.World.tasks[coord(x,y,z)];
        if (task && task.assigner===assigner) {
          task.cancel();
        }
      };
      function myHover() {
        HTomb.GUI.Panels.menu.middle = ["%c{lime}Remove all designations in this area."];
      }
      HTomb.GUI.selectSquareZone(assigner.z,this.designateSquares,{
        context: this,
        assigner: assigner,
        callback: deleteTasks,
        hover: myHover,
        contextName: "Designate"+this.template
      });
    }
  });

  Task.extend({
    template: "PatrolTask",
    name: "patrol",
    description: "patrol an area",
    bg: "#880088",
    validTile: function(x,y,z) {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      return true;
    },
    designate: function(assigner) {
      function myHover() {
        HTomb.GUI.Panels.menu.middle = ["%c{lime}Assign a minion to patrol this square."];
      }
      HTomb.GUI.selectSquare(assigner.z,this.designateSquare,{
        assigner: assigner,
        context: this,
        callback: this.designateTile,
        hover: myHover,
        contextName: "Designate"+this.template
      });
    },
    ai: function() {
      var cr = this.assignee;
      cr.ai.patrol(this.x,this.y,this.z, {
        searcher: cr,
        searchee: this,
        searchTimeout: 10
      });
    }
  });

  Task.extend({
    template: "ForbidTask",
    name: "forbid",
    description: "forbid minions from tile",
    bg: "#880000",
    validTile: function(x,y,z) {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      return true;
    },
    designate: function(assigner) {
      function myHover() {
        HTomb.GUI.Panels.menu.middle = ["%c{lime}Forbid minions from entering this square."];
      }
      HTomb.GUI.selectSquare(assigner.z,this.designateSquare,{
        assigner: assigner,
        context: this,
        callback: this.designateTile,
        hover: myHover,
        contextName: "Designate"+this.template
      });
    },
    // this task will never be assigned...
    canAssign: function() {
      return false;
    }
  });

  Task.extend({
    template: "DismantleTask",
    name: "dismantle",
    description: "harvest resources/remove fixtures",
    bg: "#446600",
    validTile: function(x,y,z) {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      if (HTomb.World.features[coord(x,y,z)] || (HTomb.World.covers[z][x][y].liquid!==true && HTomb.World.covers[z][x][y]!==HTomb.Covers.NoCover)) {
        return true;
      } else {
        return false;
      }
    },
    // filter depending on whether we are removing features or covers
    designateSquares: function(squares, options) {
      let anyf = false;
      for (let j=0; j<squares.length; j++) {
        let s = squares[j];
        if (HTomb.World.features[coord(s[0],s[1],s[2])]) {
          anyf = true;
        }
      }
      if (anyf===true) {
        squares = squares.filter(function(e,i,a) {
          return (HTomb.World.features[coord(e[0],e[1],e[2])]);
        });
      }
      Task.designateSquares.call(this, squares, options);
    },
    designate: function(assigner) {
      let menu = HTomb.GUI.Panels.menu;
      function myHover(x, y, z, squares) {
        if (squares===undefined) {
          let feature = HTomb.World.features[coord(x,y,z)];
          let cover = HTomb.World.covers[z][x][y];
          if (feature) {
            menu.middle = ["%c{lime}Harvest or dismantle "+feature.describe({article: "indefinite"})+"."];
          } else if (cover!==HTomb.Covers.NoCover) {
            menu.middle = ["%c{lime}Remove "+cover.describe()+"."];
          } else {
            menu.middle = ["%c{orange}Nothing to remove here."];
          }
        } else {
          let anyf = false;
          for (let j=0; j<squares.length; j++) {
            let s = squares[j];
            if (HTomb.World.features[coord(s[0],s[1],s[2])]!==undefined) {
              anyf = true;
            }
          }
          if (anyf===true) {
            menu.middle = ["%c{lime}Harvest or dismantle features in this area."];
          } else {
            menu.middle = ["%c{lime}Remove covers in this area."];
          }
        }
      }
      HTomb.GUI.selectSquareZone(assigner.z,this.designateSquares,{
        context: this,
        assigner: assigner,
        callback: this.designateTile,
        bg: this.bg,
        hover: myHover,
        contextName: "Designate"+this.template
      });
    },
    workOnTask: function(x,y,z) {
      var f = HTomb.World.features[coord(x,y,z)];
      if (f) {
        if (f.integrity===HTomb.Things[f.template].integrity) {
          HTomb.GUI.pushMessage(this.blurb());
        }
        f.dismantle(this);
        this.assignee.ai.acted = true;
        this.assignee.ai.actionPoints-=16;
        if (f.isPlaced()===false) {
          this.complete(this.assignee);
        }
      } else {
        f = HTomb.World.covers[z][x][y];
        if (f!==HTomb.Covers.NoCover) {
          HTomb.GUI.pushMessage(this.assignee.describe({capitalized: true, article: "indefinite"}) + " removes " + f.name
            + " at " + x + ", " + y + ", " + z + ".");
          HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
          this.assignee.ai.acted = true;
          this.assignee.ai.actionPoints-=16;
          this.complete(this.assignee);
        }
      }
    }
  });

  Task.extend({
    template: "HostileTask",
    name: "hostile",
    description: "declare hostility",
    bg: "#880000",
    validTile: function() {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      return true;
    },
    designate: function(assigner) {
      let declareHostility = function(x,y,z, assigner) {
        let cr = HTomb.World.creatures[coord(x,y,z)];
        if (cr && cr.ai && cr.ai.isHostile(assigner)===false) {
          if (cr.ai.team!=="PlayerTeam" || confirm("Really declare hostility to " + cr.describe({article: "definite"}) + "?")) {
            HTomb.Particles.addEmitter(cr.x, cr.y, cr.z, HTomb.Particles.Anger);
            HTomb.Types[assigner.ai.team].vendettas.push(cr);
          }
        }
      };
      function myHover(x,y,z) {
        let cr = HTomb.World.creatures[coord(x,y,z)];
        if (cr) {
          HTomb.GUI.Panels.menu.middle = ["%c{red}Declare hostility to " + cr.describe({article: "definite"}) + "."];
        } else {
          HTomb.GUI.Panels.menu.middle = ["%c{orange}Nothing to declare hostility to here."];
        }
      }
      HTomb.GUI.selectSquare(assigner.z,this.designateSquare,{
        context: this,
        assigner: assigner,
        callback: declareHostility,
        hover: myHover,
        contextName: "Designate"+this.template
      });
    }
  });

  Task.extend({
    template: "EquipTask",
    name: "equip",
    bg: "#882266",
    item: null,
    validTile: function(x,y,z) {
      // no longer needs to be placed...
      return true;
    },
    canAssign: function(cr) {
      let i = this.item;
      return HTomb.Tiles.isReachableFrom(i.x,i.y,i.z,cr.x, cr.y, cr.z, {
        searcher: cr,
        searchee: i,
        canMove: cr.movement.boundMove(),
        searchTimeout: 10
      });
    },
    ai: function() {
      let cr = this.assignee;
      if (cr.ai.acted) {
        return;
      }
      HTomb.Routines.FetchItem.act(cr.ai, {
        task: this,
        item: this.item,
      });
      if (cr.ai.acted) {
        return;
      }
      if (cr.inventory.items.contains(this.item)) {
        cr.equipper.equipItem(this.item);
        this.complete();
      }; 
    }
  });

  Task.extend({
    template: "FurnishTask",
    name: "furnish",
    description: "furnish a fixture",
    bg: "#553300",
    features: ["Ramp","Door",/*"Throne",*/"Torch","SpearTrap"],
    designate: function(assigner) {
      var arr = [];
      for (var i=0; i<this.features.length; i++) {
        arr.push(HTomb.Things[this.features[i]]);
      }
      var that = this;
      HTomb.GUI.choosingMenu("Choose a fixture:", arr, function(feature) {
        return function() {
          function createZone(x,y,z) {
            var task = that.designateTile(x,y,z,assigner);
            if (task) {
              task.makes = feature.template;
              if (feature.ingredients && !HTomb.Debug.noingredients) {
                task.ingredients = HTomb.Utils.clone(feature.ingredients);
              }
              task.name = task.name + " " + HTomb.Things[feature.template].name;
            }
          }
          function myHover(x,y,z) {
            HTomb.GUI.Panels.menu.middle = ["%c{lime}Furnish " + feature.describe({article: "indefinite"}) + " here.",
            "","%c{lime}"+feature.tooltip];
          }
          HTomb.GUI.selectSquare(assigner.z,that.designateSquare,{
            assigner: assigner,
            context: that,
            callback: createZone,
            hover: myHover,
            contextName: "Designate"+that.template
          });
        };
      },
      {
        format: function(feature) {
          let g = feature.describe();
          let ings = [];
          for (let ing in feature.ingredients) {
            ings.push([ing, feature.ingredients[ing]]);
          }
          if (ings.length>0) {
            g+=" ($: ";
            for (let i=0; i<ings.length; i++) {
              g+=ings[i][1];
              g+=" ";
              g+=HTomb.Things[ings[i][0]].name;
              if (i<ings.length-1) {
                g+=", ";
              } else {
                g+=")";
              }
            }
          }
          if (assigner && assigner.master && assigner.master.ownsAllIngredients(feature.ingredients)!==true) {
            g = "%c{gray}"+g;
          }
          return g;
        },
        contextName: "ChooseFixture"
      });
      HTomb.GUI.Panels.menu.middle = ["%c{orange}Choose a fixture before placing it."];
    }
  });

  Task.extend({
    template: "RepairTask",
    name: "repair",
    description: "repair a fixture or rearm a trap",
    bg: "#553300",
    ingredients: {},
    expended: false,
    validTile: function(x,y,z) {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      let f = HTomb.World.features[coord(x,y,z)];
      if (f && f.defender && f.defender.wounds.level>0) {
        return true;
      } else if (f && f.trap && f.trap.sprung) {
        return true;
      }
      return false;
    },
    designate: function(assigner) {
      let menu = HTomb.GUI.Panels.menu;
      function myHover(x, y, z) {
        let f = HTomb.World.features[coord(x,y,z)];
        if (f && f.defender && f.defender.wounds.level>0) {
          menu.middle = ["%c{lime}Repair "+f.describe({article: "indefinite"})+"."];
        } else if (f && f.trap && f.trap.sprung) {
          menu.middle = ["%c{lime}Rearm "+f.describe({article: "indefinite"})+"."];
        } else {
          menu.middle = ["%c{orange}Nothing to repair or rearm here."];
        }
      }
      let that = this;
      function createZone(x,y,z) {
        let task = that.designateTile(x,y,z,assigner);
        if (task) {
          let f = HTomb.World.features[coord(x,y,z)];
          if (f && f.trap && f.trap.sprung) {
            if (f.trap.rearmCost) {
              task.ingredients = f.trap.rearmCost;
            } else {
              task.ingredients = f.ingredients || {};
            }
          } else if (f && f.defender && f.defender.wounds.level>0) {
            if (f.repairCost) {
              task.ingredients = f.repairCost;
            } else {
              task.ingredients = f.ingredients || {};
            }
          }
        }
      }
      HTomb.GUI.selectSquare(assigner.z,that.designateSquare,{
        assigner: assigner,
        context: that,
        callback: createZone,
        hover: myHover,
        contextName: "Designate"+that.template
      });
    },
    begin: function() {
      this.expended = true;
    },
    begun: function() {
      return this.expended;
    },
    // wait a second...how do we decide how much labor is needed?
    finish: function() {
      let x = this.x;
      let y = this.y;
      let z = this.z;
      let f = HTomb.World.features[coord(x,y,z)];
      if (f && f.trap && f.trap.sprung) {
        f.trap.rearm();
      } else if (f && f.defender) {
        f.defender.wounds.level = 0;
        f.defender.wounds.type = null;
      }
    }
  });


  return HTomb;
})(HTomb);


