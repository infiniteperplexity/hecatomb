HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var coord = HTomb.Utils.coord;

  //refactoring dummy
  HTomb.Things.defineTask = function(args) {};


  let Entity = HTomb.Things.templates.Entity;

  let Task = Entity.extend({
  //let task = Object.create(HTomb.Things.templates.Entity);
  //task.super = HTomb.Things.templates.Entity;
  //task = Object.assign(task, {
    template: "Task",
    name: "task",
    // description is the name for the job assignment menu
    description: "a generic task",
    // assigner and assignee are the most fundamental task behavior
    assigner: null,
    assignee: null,
    // many tasks have ingredients and/or make a configurable thing
    ingredients: {},
    makes: null,
    // tasks sometimes go dormant when assignment fails
    dormant: 0,
    dormancy: 6,
    priority: 0,
    // placeholder...
    behaviors: [],
    blurb: function() {
      return (this.assignee.describe({capitalized: true, article: "indefinite"}) + " begins work on " + this.describe({atCoordinates: true}) + ".");
    },
    // Entity-placement behavior
    place: function(x,y,z,args) {
      Entity.place.call(this,x,y,z,args);
      if (this.assigner && this.assigner.master) {
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
    // eventually we need to refactor this to override thing.spawn
    onCreate: function() {
      Entity.onCreate.call(this);
      HTomb.Events.subcribe(this, "Destroy");
      return this;
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
        this.assignee.worker.unassign();
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
        searchTimeout: 10
      }) && cr.inventory.canFindAll(this.ingredients)) {
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
        cr.worker.onAssign(this);
      }
    },
    unassign: function() {
      var cr = this.assignee;
      if (cr.worker===undefined) {
        HTomb.Debug.pushMessage("Problem unassigning task");
      } else {
        this.assignee = null;
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
    designateTile: function(x,y,z,assigner) {
      if (this.validTile(x,y,z)) {
        let t = HTomb.Things[this.template]({assigner: assigner}).place(x,y,z);
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
    // Doing actual work...the thorny bits...
    //this will get renamed "actor" or "action", I think, or "act"
    ai: function() {
      if (this.assignee.ai.acted===true) {
        return;
      }
      var cr = this.assignee;
      if (this.begun()!==true && Object.keys(this.ingredients).length>0) {
        HTomb.Routines.ShoppingList.act(cr.ai);
      }
      if (cr.ai.acted===true) {
        return;
      }
      HTomb.Routines.GoToWork.act(cr.ai);
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
      let f = HTomb.Things.IncompleteFeature({makes: this.makes});
      f.place(this.x,this.y,this.z);
    },
    work: function() {
      let x = this.x;
      let y = this.y;
      let z = this.z;
      let f = HTomb.World.features[coord(x,y,z)];
      let labor = this.assignee.worker.getLabor();
      labor = Math.max(0, labor-f.effort);
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
      HTomb.Things[f.makes].place(x,y,z);
      f.despawn();
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
      } else if (t===HTomb.Tiles.EmptyTile && (tb===HTomb.Tiles.EmptyTile || tb===HTomb.Tiles.FloorTile)) {;
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
      };
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
        let t = HTomb.Things[this.template]({assigner: assigner}).place(x,y,z);
        HTomb.Events.publish({type: "Designate", task: t.task});
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
        // Otherwise just remove the floor
        } else {
          tiles[z][x][y] = EmptyTile;
        }
      // If it's a down slope tile, remove the slopes
      } else if (t===DownSlopeTile) {
        tiles[z][x][y] = EmptyTile;
        tiles[z-1][x][y] = FloorTile;
      // if it's an upward slope, remove the slope
      } else if (t===UpSlopeTile) {
        tiles[z][x][y] = FloorTile;
        if (tiles[z+1][x][y]===DownSlopeTile) {
          tiles[z+1][x][y] = EmptyTile;
        }
      } else if (t===EmptyTile) {
        // this shouldn't happen
      }
      // Eventually this might get folded into mining...
      HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
      if (Math.random()<0.25) {
        var rock = HTomb.Things.Rock();
        rock.item.n = 1;
        if (tiles[z][x][y]===DownSlopeTile) {
          let item = rock.place(x,y,z-1);
          item.item.setOwner(HTomb.Player);
        } else {
          let item = rock.place(x,y,z);
          item.item.setOwner(HTomb.Player);
        }
      }
      HTomb.World.validate.cleanNeighbors(x,y,z);
      let f = HTomb.World.features[coord(x,y,z)];
      f.remove();
      f.despawn();
    }
  });

  Task.extend({
    template: "BuildTask",
    name: "build",
    description: "build walls/floors/slopes",
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
        if (tile===HTomb.Tiles.UpSlopeTile) {
          tallest = Math.max(tallest,1);
        } else if (tile===HTomb.Tiles.FloorTile) {
          tallest = Math.max(tallest,0);
        }
      }
      if (tallest===1) {
        squares = squares.filter(function(e,i,a) {
          return (HTomb.World.tiles[e[2]][e[0]][e[1]]===HTomb.Tiles.UpSlopeTile);
        });
      } else if (tallest===0) {
        squares = squares.filter(function(e,i,a) {
          return (HTomb.World.tiles[e[2]][e[0]][e[1]]===HTomb.Tiles.FloorTile);
        });
      }
      HTomb.Things.templates.Task.designateSquares.call(this, squares, options);
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
            menu.middle = ["%c{lime}Building here will construct an upward slope (can be upgraded into a wall.)"];
          } else {
            menu.middle = ["%c{orange}Can't build on this tile."];
          }
          return;
        }
        var tallest = -2;
        for (var j=0; j<squares.length; j++) {
          var s = squares[j];
          let tile = HTomb.World.tiles[s[2]][s[0]][s[1]];
          if (tile===HTomb.Tiles.UpSlopeTile) {
            tallest = Math.max(tallest,1);
          } else if (tile===HTomb.Tiles.FloorTile) {
            tallest = Math.max(tallest,0);
          } else if (tile===HTomb.Tiles.DownSlopeTile || tile===HTomb.Tiles.EmptyTile) {
            tallest = Math.max(tallest,-1);
          }
        }
        if (tallest===1) {
          menu.middle = ["%c{lime}Construct new walls in this area."];
        } else if (tallest===0) {
          menu.middle = ["%c{lime}Construct new slopes in this area."];
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
      if (t===FloorTile) {
        tiles[z][x][y] = UpSlopeTile;
        if (tiles[z+1][x][y]===EmptyTile) {
          tiles[z+1][x][y] = DownSlopeTile;
        }
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
    }
  });


  HTomb.Things.defineFeature({
    template: "IncompleteFeature",
    name: "incomplete feature",
    symbol: "\u25AB",
    fg: "#BB9922",
    makes: null,
    finished: false,
    labor: 5,
    effort: 0,
    onCreate: function(args) {
      if (args.makes) {
        let makes = HTomb.Things.templates[args.makes];
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

  HTomb.Things.defineFeature({
    template: "Excavation",
    name: "excavation",
    labor: 10,
    effort: 0,
    incompleteSymbol: "\u2717",
    incompleteFg: HTomb.Constants.BELOWFG
  });

  HTomb.Things.defineFeature({
    template: "Construction",
    name: "construction",
    incompleteSymbol: "\u2692",
    labor: 15,
    incompleteFg: HTomb.Constants.WALLFG
  });


  return HTomb;
})(HTomb);


