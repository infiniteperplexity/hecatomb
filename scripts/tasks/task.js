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
    priority: 1,
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
        for (let item of this.assigner.owner.ownedItems()) {
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
    act: function() {
      let cr = this.assignee;
      if (cr.actor.acted) {
        return;
      }
      // if it still needs items, fetch one
      HTomb.Behaviors.FetchItems.act(cr.actor, {
        task: this,
        ingredients: this.ingredients
      });
      if (cr.actor.acted) {
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
        cr.actor.walkToward(this.x,this.y,this.z, {
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
        return;
      }
      //do I want to make demolishing unowned features the default?
      // maybe do that with "subsidiary tasks"
      if (this.begun()!==true) {
        this.expend();
        this.begin();
      }
      this.work();
      if (this.done()) {
        this.finish();
        this.complete();
      }
    },
    done: function() {
      let f = HTomb.World.features[coord(this.x,this.y,this.z)];
      if (f && f.labor<=0) {
        return true;
      } else {
        return false;
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
      this.assignee.actor.acted = true;
      this.assignee.actor.actionPoints-=16;
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


  HTomb.Things.Feature.extend({
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
        this.labor = makes.fixture.labor || this.labor;
        this.effort = makes.fixture.effort || this.effort;
        this.symbol = makes.fixture.incompleteSymbol || this.symbol;
        this.fg = makes.fixture.incompleteFg || makes.fg || this.fg;
      }
      return this;
    },
    onDescribe: function(args) {
      args.name = "incomplete " + HTomb.Things[this.makes].name;
      return args;
    }
  });

  return HTomb;
})(HTomb);


