HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var coord = HTomb.Utils.coord;

  let Task = HTomb.Things.Task;
  
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
    priority: 3,
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
    act: function() {
      var cr = this.assignee;
      cr.actor.patrol(this.x,this.y,this.z, {
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
    wounds: 8,
    labor: null,
    validTile: function(x,y,z) {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      if (HTomb.World.features[coord(x,y,z)] || (!HTomb.World.covers[z][x][y].liquid && HTomb.World.covers[z][x][y]!==HTomb.Covers.NoCover)) {
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
    begin: function() {
      let f = HTomb.World.features[coord(this.x, this.y, this.z)];
      if (f) {
        this.work();
      } else if (HTomb.World.covers[z][x][y]!==HTomb.Covers.NoCover) {
        HTomb.GUI.pushMessage(this.assignee.describe({capitalized: true, article: "indefinite"}) + " removes " + f.name
          + " at " + x + ", " + y + ", " + z + ".");
        HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
        this.assignee.actor.acted = true;
        this.assignee.actor.actionPoints-=16;
        this.complete();
      }
    },
    begun: function() {
      return (this.labor!==null);
    },
    done: function() {
      let f = HTomb.World.features[coord(this.x, this.y, this.z)];
      if (!f) {
        return true;
      } else {
        return false;
      }
    },
    work: function() {
      let f = HTomb.World.features[coord(this.x, this.y, this.z)];
      let toughness = (f && f.defender) ? f.defender.toughness : 5;
      if (this.labor<=0 || this.labor===null) {
        this.labor = Math.ceil(2*toughness/3);
      }
      this.labor-=this.assignee.worker.getLabor();
      if (this.labor<=0) {
        if (f.defender) {
          f.defender.wounds.level+=1;
          f.defender.wounds.type = "Dismantle";
          if (f.defender.wounds.level>=8 && f.harvestable) {
            f.harvestable.harvest();
          }
          f.defender.tallyWounds();
        } else {
          this.wounds-=1;
          if (this.wounds<=0 && f.harvestable) {
            f.harvestable.harvest();
            f.destroy();
          }
        }
      }
      this.assignee.actor.acted = true;
      this.assignee.actor.actionPoints-=16;
    },
    finish: function() {
      //odd that we do not put harvesting here...
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
        if (cr && cr.actor && cr.actor.isHostile(assigner)===false) {
          if (cr.actor.team!=="PlayerTeam" || confirm("Really declare hostility to " + cr.describe({article: "definite"}) + "?")) {
            HTomb.Particles.addEmitter(cr.x, cr.y, cr.z, HTomb.Particles.Anger);
            HTomb.Types[assigner.actor.team].vendettas.push(cr);
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
    act: function() {
      let cr = this.assignee;
      if (cr.actor.acted) {
        return;
      }
      HTomb.Behaviors.FetchItem.act(cr.actor, {
        task: this,
        item: this.item,
      });
      if (cr.actor.acted) {
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
    features: ["Ramp","Door","Torch","SpearTrap"],
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
              if (HTomb.Debug.noingredients) {
                task.ingredients = {};
              } else {
                task.ingredients = feature.fixture.ingredients;
              }
              task.name = task.name + " " + HTomb.Things[feature.template].name;
            }
          }
          function myHover(x,y,z) {
            HTomb.GUI.Panels.menu.middle = ["%c{lime}Furnish " + feature.describe({article: "indefinite"}) + " here.",
            "","%c{lime}"+feature.fixture.tooltip];
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
          for (let ing in feature.fixture.ingredients) {
            ings.push([ing, feature.fixture.ingredients[ing]]);
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
          if (assigner && assigner.master && assigner.owner.ownsAllIngredients(feature.fixture.ingredients)!==true) {
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

  Task.extend({
    template: "ZombieEmergeTask",
    name: "emerge",
    bg: "#884400",
    //beginDescription: function() {
    //  return "digging up from its grave";
    //},
    validTile: function() {
      // this thing is going to be special...it should keep respawning if thwarted
      return true;
    },
    workOnTask: function(x,y,z) {
      let f = HTomb.World.features[HTomb.Utils.coord(x,y,z)];
      // There is a special case of digging upward under a tombstone...
      if (f && f.template==="Tombstone") {
        if (f.integrity===null || f.integrity===undefined) {
          f.integrity=10;
        }
        if (f.integrity===10) {
          HTomb.GUI.pushMessage(this.assignee.describe({capitalized: true, article: "indefinite"}) + " begins digging toward the surface.");
        }
        f.integrity-=1;
        this.assignee.actor.acted = true;
        this.assignee.actor.actionPoints-=16;
        if (f.integrity<=0) {
          f.explode(this.assigner);
          var cr = this.assignee;
          HTomb.GUI.sensoryEvent(cr.describe({capitalized: true, article: "indefinite"}) + " bursts forth from the ground!",x,y,z);
          HTomb.World.tiles[z][x][y] = HTomb.Tiles.DownSlopeTile;
          let c = HTomb.World.covers[z][x][y];
          if (c.mine) {
            c.mine(x,y,z,this.assigner);
          }
          this.complete();
          HTomb.World.validate.cleanNeighbors(x,y,z);
        }
      }
    }
  });

  Task.extend({
    template: "ClaimTask",
    name: "claim task",
    description: "claim unclaimed items",
    validTile: function() {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      return true;
    },
    designate: function(assigner) {
      var claimItems = function(x,y,z, assigner) {
        let items = HTomb.World.items[coord(x,y,z)] || HTomb.Things.Items();
        for (let item of items) {
          item.owned = true;
        }
      };
      function myHover() {
        HTomb.GUI.Panels.menu.middle = ["%c{lime}Claim all unclaimed items in this area."];
      }
      HTomb.GUI.selectSquareZone(assigner.z,this.designateSquares,{
        context: this,
        assigner: assigner,
        callback: claimItems,
        hover: myHover,
        contextName: "Designate"+this.template
      });
    }
  });


  return HTomb;
})(HTomb);


