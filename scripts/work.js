HTomb = (function(HTomb) {
  	"use strict";
  	let LEVELW = HTomb.Constants.LEVELW;
	let LEVELH = HTomb.Constants.LEVELH;
  	let coord = HTomb.Utils.coord;

	//
	let task = Object.create(HTomb.Things.templates.Entity);
	task.template = "Task";
	task.name = "task";
	// description is the name for the job assignment menu
	task.description = "a generic task";
	// assigner and assignee are the most fundamental task behavior
	task.assigner = null;
	task.assignee = null;
	// many tasks have ingredients and/or make a configurable thing
	task.ingredients = {};
	task.makes = null;
	// tasks sometimes go dormant when assignment fails
	task.dormant = 0;
	task.dormancy = 6;
	task.priority = 0;
	// Entity-placement behavior
	task.onPlace = function(x,y,z,args) {
      if (this.assigner && this.assigner.master) {
        //this.assigner.master.taskList.push(this.entity);
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
    };
    task.onRemove = function() {
      	let x = this.entity.x;
      	let y = this.entity.y;
      	let z = this.entity.z;
      	delete HTomb.World.tasks[coord(x,y,z)];
    };

    // Cleanup-type events
	task.onCreate = function() {
		HTomb.Events.subcribe(this, "Destroy");
	};
	task.onDestroy = function(event) {
		let  cr = event.entity;
	    if (cr===this.assignee) {
	    	this.unassign();
	    } else if (cr===this.assigner) {
	        this.despawn();
	    }
	};
    task.onDespawn = function() {
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
    };
    // assignment-related methods
    task.canAssign = function(cr) {
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
    };
    task.assignTo = function(cr) {
      if (cr.minion===undefined) {
        HTomb.Debug.pushMessage("Problem assigning task");
      } else {
        this.assignee = cr;
        cr.worker.onAssign(this);
      }
    };
    this.unassign = function() {
      var cr = this.assignee;
      if (cr.worker===undefined) {
        HTomb.Debug.pushMessage("Problem unassigning task");
      } else {
        this.assignee = null;
        cr.worker.unassign();
      }
    };
    this.cancel = function() {
      this.despawn();
    };
    // default methods for designating tasks
    task.designate = function(assigner) {
    	HTomb.GUI.selectSquareZone(assigner.z,this.designateSquares,{
        	context: this,
        	assigner: assigner,
        	callback: this.designateTile,
        	outline: false,
        	bg: this.bg,
        	contextName: "Designate"+this.template
      });
    };
    task.designateSquare = function(x,y,z, options) {
      options = options || {};
      var assigner = options.assigner;
      var callb = options.callback;
      callb.call(options.context,x,y,z,assigner);
    };
    task.designateSquares = function(squares, options) {
      options = options || {};
      var assigner = options.assigner;
      var callb = options.callback;
      for (var i=0; i<squares.length; i++) {
        var crd = squares[i];
        callb.call(options.context,crd[0],crd[1],crd[2],assigner);
      }
    };
    task.designateTile = function(x,y,z,assigner) {
      if (this.validTile(x,y,z)) {
        let t = HTomb.Things[this.template]({assigner: assigner}).place(x,y,z);
        HTomb.Events.publish({type: "Designate", task: this});
        //HTomb.Events.publish({type: "Designate", task: t.task});
        return t;
      }
    }
    task.validTile = function(x,y,z) {
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
    };
    // Doing actual work...the thorny bits...
    //this will get renamed "actor" or "action", I think, or "act"
    task.ai = function() {
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
    };
    task.workBegun = function() {
      let x = this.x;
      let y = this.y;
      let z = this.z;
      let f = HTomb.World.features[coord(x,y,z)];
      if (f && f.template==="IncompleteFeature" && f.makes.template===this.makes) {
        return true;
      } else {
        return false;
      }
    };
    task.beginWork = function() {
      // could handle auto-dismantling here...
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
    };
    task.work: function(creature) {
      let x = this.x;
      let y = this.y;
      let z = this.z;
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
    };
    task.completeWork = function(x,y,z) {
      HTomb.Events.publish({type: "Complete", task: this});
      this.despawn();
    };

HTomb.Things.defineBehavior({
    template: "Master",
    name: "master",
    minions: null,
    taskList: null,
    workshops: null,
    tasks: null,
    ownedItems: null,
    onCreate: function(options) {
      options = options || {};
      this.tasks = options.tasks || [];
      this.minions = [];
      this.taskList = [];
      this.structures = [];
      this.ownedItems = [];
      HTomb.Events.subscribe(this, "Destroy");
      return this;
    },
    onDestroy: function(event) {
      if (this.minions.indexOf(event.entity)>-1) {
        HTomb.GUI.sensoryEvent(this.entity.describe({capitalized:true, article:"indefinite"}) + " mourns the death of " + event.entity.describe({article: "indefinite"})+".",this.entity.x,this.entity.y,this.entity.z);
        this.minions.splice(this.minions.indexOf(event.entity),1);
      }
    },
    addMinion: function(cr) {
      this.minions.push(cr);
      cr.minion.setMaster(this.entity);
    },
    removeMinion: function(cr) {
      this.minions.splice(this.minions.indexOf(cr,1));
    },
    addWorkshop: function(w) {
      this.workshops.push(w);
    },
    removeWorkshop: function(w) {
      this.workshops.splice(this.workshops.indexOf(w,1));
    },
    designate: function(tsk) {
      tsk.designate(this.entity);
    },
    assignTasks: function() {
      let priorities = {
        DigTask: 1,
        BuildTask: 1,
        ConstructTask: 1,
        DismantleTask: 1,
        FurnishTask: 1,
        ProduceTask: 1,
        HaulTask: 2,
        PatrolTask: 3
      };
      HTomb.Utils.shuffle(this.taskList);
      //count down dormant tasks
      for (let i=0; i<this.taskList.length; i++) {
        let task = this.taskList[i].task;
        if (task.dormant>0) {
          console.log("reducing dormancy");
          task.dormant-=1;
        }
      }
      let failed = [];
      for (let i=0; i<this.minions.length; i++) {
        let minion = this.minions[i];
        if (minion.worker===undefined) {
          continue;
        } else if (minion.worker.task!==null) {
          continue;
        } else {
          // look for labor tools
          let labor = minion.worker.labor;
          if (minion.equipper && minion.equipper.slots.MainHand && minion.equipper.slots.MainHand.equipment.labor>labor) {
            labor = minion.equipper.slots.MainHand.equipment.labor;
          }
          let invenTools = this.ownedItems.filter(function(e,i,a) {
            return (e.equipment && e.equipment.labor>labor && minion.inventory && minion.inventory.items.items.indexOf(e)!==-1);
          });
          let groundTools = this.ownedItems.filter(function(e,i,a) {
            return (e.equipment && e.equipment.labor>labor && e.item.isOnGround());
          });
          // sort by labor value
          let comp = function(a,b) {
            if (a.equipment.labor>b.equipment.labor) {
              return 1;
            } else if (a.equipment.labor<b.equipment.labor) {
              return -1;
            } else {
              return 0;
            }
          };
          invenTools.sort(comp);
          groundTools.sort(comp);
          // equip best ones
          if (invenTools.length>0) {
            minion.equipper.equipItem(invenTools[0]);
            this.entity.ai.acted = true;
            this.entity.ai.actionPoints-=16;
            continue;
          } else if (groundTools.length>0) {
            let task = HTomb.Things.EquipTask();
            let item = groundTools[0];
            task.task.item = item;
            task.task.assigner = this.entity;
            task.name = "equip " + item.describe();
            task.place(item.x, item.y, item.z);
            task.task.assignTo(minion);
            continue;
          }
          let MAXPRIORITY = 3;
          for (let j=0; j<=MAXPRIORITY; j++) {
            let tasks = this.taskList.filter(function(e,i,a) {return (priorities[e.task.template]===j && !e.task.dormant && e.task.assignee===null)});
            // for hauling tasks, this gives misleading results...but I could fix that
            tasks = HTomb.Path.closest(minion.x, minion.y, minion.z,tasks);
            for (let k=0; k<tasks.length; k++) {
              let task = tasks[k].task;
              if (minion.worker.allowedTasks.indexOf(task.template)!==-1 && task.canAssign(minion)) {
                task.assignTo(minion);
                //very ad hoc
                let j = MAXPRIORITY+1;
                break;
              } else if (failed.indexOf(task)===-1) {
                failed.push(task);
              }
            }
          }
        }
      }
      for (let i=0; i<failed.length; i++) {
        let task = failed[i];
        task.dormant = HTomb.Utils.perturb(task.dormancy);
      }
    },
    listTasks: function() {
      var tasks = [];
      for (var i=0; i<this.tasks.length; i++) {
        tasks.push(HTomb.Things.templates[this.tasks[i]]);
      }
      return tasks;
    },
    ownsAllIngredients: function(ingredients) {
      let owned = {};
      for (let i=0; i<this.ownedItems.length; i++) {
        let temp = this.ownedItems[i].template
        if (ingredients[temp]>0) {
          owned[temp] = owned[temp] || 0;
          let n = this.ownedItems[i].n || 1;
          owned[temp]+=n;
        }
      }
      for (let ing in ingredients) {
        if (!owned[ing] || owned[ing]<ingredients[ing]) {
          return false;
        }
      }
      return true;
    }
  });

HTomb.Things.defineFeature({
    template: "Excavation",
    name: "excavation",
    labor: 10,
    incompleteSymbol: "\u2717",
    incompleteFg: HTomb.Constants.BELOWFG,
    onPlace: function(x,y,z) {
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
      this.despawn();
    }
  });


  HTomb.Things.defineFeature({
    template: "Construction",
    name: "construction",
    incompleteSymbol: "\u2692",
    labor: 15,
    incompleteFg: HTomb.Constants.WALLFG,
    onPlace: function(x,y,z) {
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
      this.despawn();
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
      this.makes = args.makes;
      this.labor = this.makes.labor || this.labor;
      this.effort = this.makes.effort || this.effort;
      this.symbol = this.makes.incompleteSymbol || this.symbol;
      this.fg = this.makes.incompleteFg || this.makes.fg || this.fg;
      this.name = "incomplete "+this.makes.name;
      return this;
    },
    work: function(assignee) {
      let labor = assignee.worker.getLabor();
      labor = Math.max(0, labor-this.effort);
      // need to account for work axes somehow
      this.labor-=labor;
      //deal with hardness here?
      if (this.labor<=0) {
        this.finish();
      }
    },
    finish: function() {
      var x = this.x;
      var y = this.y;
      var z = this.z;
      // need to swap over the stack, if necessary...
      this.finished = true;
      this.remove();
      this.makes.place(x,y,z);
      this.despawn();
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

  Commands.showJobs = function() {
    // This one alone should *not* delegate
    HTomb.Events.publish({type: "Command", command: "ShowJobs"});
    GUI.choosingMenu("Choose a task:", HTomb.Player.master.listTasks(),
      function(task) {
        return function() {
          HTomb.Events.publish({type: "Command", command: "ChooseJob", task: task})
          HTomb.Player.master.designate(task);
          //HTomb.Time.resumeActors();
        };
      },
      {
        format: function(task) {
          let name = task.longName;
          return (name.substr(0,1).toUpperCase() + name.substr(1)+".");
        },
        contextName: "ShowJobs"
      }
    );
  };




	return HTomb;
})(HTomb);
