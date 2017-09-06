HTomb = (function(HTomb) {
  	"use strict";
  	let LEVELW = HTomb.Constants.LEVELW;
	let LEVELH = HTomb.Constants.LEVELH;
  	let coord = HTomb.Utils.coord;

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

  HTomb.Things.defineBehavior({
    template: "Worker",
    name: "worker",
    labor: 1,
    task: null,
    allowedTasks: ["DigTask","BuildTask","PatrolTask","FurnishTask","HaulTask","ConstructTask","ProduceTask","DismantleTask","HarvestFarmTask","ConvergeTask"],
    onAssign: function(tsk) {
      this.task = tsk;
      HTomb.Debug.pushMessage(this.entity.describe({capitalized: true, article: "indefinite"}) + " was assigned " + tsk.describe());
    },
    unassign: function() {
      if (this.task===null) {
        return;
      }
      HTomb.Debug.pushMessage(this.entity.describe({capitalized: true, article: "indefinite"}) + " was unassigned from " + this.task.describe());
      this.task = null;
    },
    getLabor: function() {
      let e = this.entity.equipper;
      if (e && e.slots.MainHand && e.slots.MainHand.equipment.labor>this.labor) {
        return e.slots.MainHand.equipment.labor;
      }
      return this.labor;
    }
  });

  HTomb.Things.defineTask({
    template: "DigTask",
    name: "dig",
    longName: "dig corridors/pits/slopes",
    bg: "#884400",
    makes: "Excavation",
    dormancy: 0,
    canAssign: function(cr) {
      let x = this.entity.x;
      let y = this.entity.y;
      let z = this.entity.z;
      let tf = HTomb.Things.templates.Task.canAssign.call(this, cr);
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
    beginWork: function(assignee) {
      HTomb.Things.templates.Task.beginWork.call(this,assignee);
      let x = this.entity.x;
      let y = this.entity.y;
      let z = this.entity.z;
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
    }
  });




	return HTomb;
})(HTomb);
