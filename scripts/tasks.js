HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var coord = HTomb.Utils.coord;
  // should we maybe allow a queue of zones???  probably not
  HTomb.Things.defineBehavior({
    template: "Task",
    name: "task",
    longName: "task",
    assigner: null,
    assignee: null,
    makes: null,
    feature: null,
    ingredients: {},
    dormant: 0,
    dormancy: 6,
    beginDescription: function() {
      return "work on " + this.name;
    },
    beginMessage: function() {
      return (this.assignee.describe({capitalized: true, article: "indefinite"}) + " begins " + this.beginDescription()
        + " at " + this.entity.x + ", " + this.entity.y + ", " + this.entity.z + ".");
    },
    onCreate: function(args) {
      HTomb.Events.subscribe(this,"Destroy");
      return this;
    },
    onPlace: function(x,y,z,args) {
      if (this.assigner && this.assigner.master) {
        this.assigner.master.taskList.push(this.entity);
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
      HTomb.World.tasks[coord(x,y,z)] = this.entity;
    },
    designate: function(assigner) {
      HTomb.GUI.selectSquareZone(assigner.z,this.designateSquares,{
        context: this,
        assigner: assigner,
        callback: this.designateTile,
        outline: false,
        bg: this.beginMessage,
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
    validTile: function(x,y,z) {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      if (HTomb.World.tiles[z][x][y]!==HTomb.Tiles.FloorTile) {
        return false;
      }
      let f = HTomb.World.features[coord(x,y,z)];
      if (f===undefined || (f.template==="IncompleteFeature" && f.makes.template===this.makes)) {
        return true;
      }
      else {
        return false;
      }
    },
    designateTile: function(x,y,z,assigner) {
      if (this.validTile(x,y,z)) {
        let t = HTomb.Things[this.template]({assigner: assigner}).place(x,y,z);
        HTomb.Events.publish({type: "Designate", task: t.task});
        return t;
      }
    },
    canAssign: function(cr) {
      let x = this.entity.x;
      let y = this.entity.y;
      let z = this.entity.z;
      if (this.validTile(x,y,z) && HTomb.Tiles.isReachableFrom(x,y,z,cr.x,cr.y,cr.z,{
        searcher: cr,
        searchee: this.entity,
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
    // this is on destruction of a creature, not the task
    onDestroy: function(event) {
      var cr = event.entity;
      if (cr===this.assignee) {
        this.unassign();
      } else if (cr===this.assigner) {
        this.despawn();
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
      this.entity.despawn();
    },
    onRemove: function() {
      let x = this.entity.x;
      let y = this.entity.y;
      let z = this.entity.z;
      delete HTomb.World.tasks[coord(x,y,z)];
    },
    onDespawn: function() {
      var master = this.assigner;
      if (master) {
        var taskList = this.assigner.master.taskList;
        if (taskList.indexOf(this.entity)!==-1) {
          taskList.splice(taskList.indexOf(this.entity),1);
        }
      }
      if (this.assignee) {
        this.assignee.worker.unassign();
      }
    },
    workBegun: function() {
      let x = this.entity.x;
      let y = this.entity.y;
      let z = this.entity.z;
      let f = HTomb.World.features[coord(x,y,z)];
      if (f && f.template==="IncompleteFeature" && f.makes.template===this.makes) {
        return true;
      } else {
        return false;
      }
    },
    beginWork: function() {
      // could handle auto-dismantling here...
      // will this work?  or should we check for ingredients before taking?
      if (this.assignee.inventory.items.hasAll(this.ingredients)!==true) {
        throw new Error("shouldn't reach this due to AI");
      }
      HTomb.GUI.pushMessage(this.beginMessage());
      let items = this.assignee.inventory.items.takeItems(this.ingredients);
      for (let i=0; i<items.length; i++) {
        items[i].despawn();
      }
      let f = HTomb.Things.IncompleteFeature({makes: HTomb.Things[this.makes]()});
      f.place(this.entity.x,this.entity.y,this.entity.z);
      this.assignee.ai.acted = true;
      this.assignee.ai.actionPoints-=16;
    },
    work: function(creature) {
      let x = this.entity.x;
      let y = this.entity.y;
      let z = this.entity.z;
      if (this.validTile(x,y,z)!==true) {
        this.cancel();
      }
      //do I want to make demolishing unowned features the default?
      let f = HTomb.World.features[coord(x,y,z)];
      // we could also handle the dismantling in "beginWork"...
      if (this.workBegun()!==true) {
        this.beginWork(this.assignee);
      } else {
        f.work(this.assignee);
        // be careful not to do this twice...
        this.assignee.ai.acted = true;
        this.assignee.ai.actionPoints-=16;
      }
      if (f && f.finished) {
        this.completeWork(this.assignee);
      }
    },
    completeWork: function(x,y,z) {
      HTomb.Events.publish({type: "Complete", task: this});
      this.entity.despawn();
    },
    ai: function() {
      if (this.assignee.ai.acted===true) {
        return;
      }
      var cr = this.assignee;
      if (this.workBegun()!==true && Object.keys(this.ingredients).length>0) {
        HTomb.Routines.ShoppingList.act(cr.ai);
      }
      if (cr.ai.acted===true) {
        return;
      }
      HTomb.Routines.GoToWork.act(cr.ai);
    }
  });

  HTomb.Things.defineTask({
    template: "DigTask",
    name: "dig",
    longName: "dig corridors/pits/slopes",
    bg: "#884400",
    makes: "Excavation",
    dormancy: 0,
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
      } else if (f && (f.template!=="IncompleteFeature" || this.makes!==f.makes.template)) {
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
  });

  HTomb.Things.defineTask({
    template: "BuildTask",
    name: "build",
    longName: "build walls/floors/slopes",
    bg: "#440088",
    makes: "Construction",
    //ingredients: {Rock: 1},
    validTile: function(x,y,z) {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      //shouldn't be able to build surrounded by emptiness
      var t = HTomb.World.tiles[z][x][y];
      let f = HTomb.World.features[coord(x,y,z)];
      if (t===HTomb.Tiles.VoidTile || t===HTomb.Tiles.WallTile) {
        return false;
      } else if (f && (f.template!=="IncompleteFeature" || this.makes!==f.makes.template)) {
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
    }
  });

  HTomb.Things.defineTask({
    template: "Undesignate",
    name: "undesignate",
    longName: "undesignate tasks",
    validTile: function() {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      return true;
    },
    designate: function(assigner) {
      var deleteTasks = function(x,y,z, assigner) {
        var zn = HTomb.World.tasks[coord(x,y,z)];
        if (zn && zn.task.assigner===assigner) {
          zn.task.cancel();
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

  HTomb.Things.defineTask({
    template: "PatrolTask",
    name: "patrol",
    longName: "patrol an area",
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
      cr.ai.patrol(this.entity.x,this.entity.y,this.entity.z, {
        searcher: cr,
        searchee: this.entity,
        searchTimeout: 10
      });
    }
  });

  HTomb.Things.defineTask({
    template: "ForbidTask",
    name: "forbid",
    longName: "forbid minions from tile",
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

  HTomb.Things.defineTask({
    template: "DismantleTask",
    name: "dismantle",
    longName: "harvest resources/remove fixtures",
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
      HTomb.Things.templates.Task.designateSquares.call(this, squares, options);
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
    work: function(x,y,z) {
      var f = HTomb.World.features[coord(x,y,z)];
      if (f) {
        if (f.feature.integrity===HTomb.Things.templates[f.feature.template].integrity) {
          HTomb.GUI.pushMessage(this.beginMessage());
        }
        f.feature.dismantle(this);
        this.assignee.ai.acted = true;
        this.assignee.ai.actionPoints-=16;
        if (f.isPlaced()===false) {
          this.completeWork(this.assignee);
        }
      } else {
        f = HTomb.World.covers[z][x][y];
        if (f!==HTomb.Covers.NoCover) {
          HTomb.GUI.pushMessage(this.assignee.describe({capitalized: true, article: "indefinite"}) + " removes " + f.name
            + " at " + x + ", " + y + ", " + z + ".");
          HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
          this.assignee.ai.acted = true;
          this.assignee.ai.actionPoints-=16;
          this.completeWork(this.assignee);
        }
      }
    }
  });

  HTomb.Things.defineTask({
    template: "HostileTask",
    name: "hostile",
    longName: "declare hostility",
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
            HTomb.Types.templates[assigner.ai.team].vendettas.push(cr);
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

  HTomb.Things.defineTask({
    template: "EquipTask",
    name: "equip",
    bg: "#882266",
    item: null,
    validTile: function(x,y,z) {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      if (HTomb.World.tiles[z][x][y]===HTomb.Tiles.WallTile || HTomb.World.tiles[z][x][y]===HTomb.Tiles.EmptyTile) {
        return false;
      } else {
        return true;
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
      let x = item.x;
      let y = item.y;
      let z = item.z;
      if (x===cr.x && y===cr.y && z===cr.z) {
        cr.inventory.pickup(item);
        this.completeWork();
      } else {
        cr.ai.target = item;
        let t = cr.ai.target;
        cr.ai.walkToward(t.x,t.y,t.z, {
          searcher: cr,
          searchee: t,
          searchTimeout: 10,
          useLast: true
        });
      }
      cr.ai.acted = true;
    }
  });

  HTomb.Things.defineTask({
    template: "FurnishTask",
    name: "furnish",
    longName: "furnish a fixture",
    bg: "#553300",
    features: ["Door","Throne","ScryingGlass","Torch"],
    designate: function(assigner) {
      var arr = [];
      for (var i=0; i<this.features.length; i++) {
        arr.push(HTomb.Things.templates[this.features[i]]);
      }
      var that = this;
      HTomb.GUI.choosingMenu("Choose a fixture:", arr, function(feature) {
        return function() {
          function createZone(x,y,z) {
            var task = that.designateTile(x,y,z,assigner);
            if (task) {
              task.task.makes = feature.template;
              if (feature.ingredients) {
                task.task.ingredients = HTomb.Utils.clone(feature.ingredients);
              }
              task.name = task.name + " " + HTomb.Things.templates[feature.template].name;
            }
          }
          function myHover(x,y,z) {
            HTomb.GUI.Panels.menu.middle = ["%c{lime}Furnish " + feature.describe({article: "indefinite"}) + " here."];
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
          let hasAll = true;
          if (ings.length>0) {
            g+=" ($: ";
            for (let i=0; i<ings.length; i++) {
              g+=ings[i][1];
              g+=" ";
              g+=HTomb.Things.templates[ings[i][0]].name;
              if (i<ings.length-1) {
                g+=", ";
              } else {
                g+=")";
              }
              if (assigner.master) {
                let has = false;
                for (let j=0; j<assigner.master.ownedItems.length; j++) {
                  if (assigner.master.ownedItems[j].template===ings[i][0]) {
                    has = true;
                  }
                }
                if (has===false) {
                  hasAll = false;
                }
              }
            }
          }
          if (hasAll!==true) {
            g = "%c{gray}"+g;
          }
          return g;
        },
        contextName: "ChooseFixture"
      });
      HTomb.GUI.Panels.menu.middle = ["%c{orange}Choose a fixture before placing it."];
    }
  });

  return HTomb;
})(HTomb);
