HTomb = (function(HTomb) {
  "use strict";
  let coord = HTomb.Utils.coord;

  let Task = HTomb.Things.Task;

  Task.extend({
    template: "ConstructTask",
    name: "construct",
    description: "create a structure",
    bg: "#553300",
    makes: null,
    labor: null,
    structures: ["GuardPost","Workshop","Stockpile","BlackMarket","Sanctum"],
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
    designate: function(assigner) {
      var arr = [];
      for (var i=0; i<this.structures.length; i++) {
        //should be pushing the entity, not the structure?
        arr.push(HTomb.Things[this.structures[i]]);
      }
      var that = this;
      HTomb.GUI.choosingMenu("Choose a structure:", arr, function(structure) {
        function placeBox(squares, options) {
          let failed = false;
          let struc = null;
          let mid = Math.floor(squares.length/2);
          for (let i=0; i<squares.length; i++) {
            let crd = squares[i];
            let f = HTomb.World.features[coord(crd[0],crd[1],crd[2])];
            if (HTomb.World.tiles[crd[2]][crd[0]][crd[1]]!==HTomb.Tiles.FloorTile) {
              failed = true;
            // a completed, partial version of the same structure
          } else if (f && f.template===structure.template+"Feature") {
              struc = f.structure;
              if (struc.isPlaced()===true || struc.structure.x!==squares[mid][0] || struc.structure.y!==squares[mid][1]) {
                failed = true;
              }
            // an incomplete version of the same structure
          } else if (f && (f.template!=="IncompleteFeature" || !f.makes || f.makes!==structure.template+"Feature")) {
              failed = true;
            }
          }
          if (failed===true) {
            HTomb.GUI.pushMessage("Can't build there.");
            return;
          }
          let w;
          if (struc!==null) {
            w = struc;
          } else {
            w = HTomb.Things[structure.template].spawn();
            w.owner = assigner;
            w.squares = squares;
            w.x = squares[mid][0];
            w.y = squares[mid][1];
            w.z = squares[mid][2];
          }
          for (let i=0; i<squares.length; i++) {
            let crd = squares[i];
            // this might get weird if you try overlapping a farm with anothern farm...
            if (HTomb.World.features[coord(crd[0],crd[1],crd[2])] && HTomb.World.features[coord(crd[0],crd[1],crd[2])].template===w.template+"Feature") {
              continue;
            }
            this.makes = w.template+"Feature";
            let task = this.designateTile(crd[0],crd[1],crd[2],assigner);
            if (task) {
              //Bugs have occurred here...I may have fixed them...
              task.structure = w;
              task.labor = w.labor;
              task.makes = w.template+"Feature";
              if (w.height!==null && w.width!==null) {
                if (HTomb.Debug.noingredients) {
                  task.ingredients = {};
                } else {
                  if (w.ingredients[i]===undefined) {
                    task.ingredients = {};
                  } else {
                    task.ingredients = HTomb.Utils.clone(w.ingredients[i]);
                  }
                }
              }
              task.position = i;
              task.name = task.name + " " + w.name;
            }
          }
        }
        function myHover(x, y, z, squares) {
          //hacky as all get-out
          let menu = HTomb.GUI.Panels.menu;
          // this may not be used any more because it was for varied-size structures
          if (Array.isArray(x)===false && squares===undefined) {
            if (HTomb.World.explored[z][x][y]!==true) {
              menu.middle = ["%c{orange}Unexplored tile."];
              return;
            }
            if (that.validTile(x,y,z)!==true) {
              menu.middle = ["%c{orange}Cannot build structure here."];
              return;
            }
            menu.middle = ["%c{lime}Build structure starting here."];
            return;
          } else {
            if (Array.isArray(x)) {
              squares = x;
            }
            let valid = true;
            for (var j=0; j<squares.length; j++) {
              let s = squares[j];
              //!! this went out of range?
              if (HTomb.World.explored[s[2]][s[0]][s[1]]!==true || that.validTile(s[0],s[1],s[2])!==true) {
                valid = false;
              }
            }
            if (valid===false) {
              menu.middle = ["%c{orange}Cannot build structure here."];
              return;
            } else {
              menu.middle = ["%c{lime}Build structure here.","","%c{lime}"+structure.tooltip];
            }
          }
        }
        if (structure.height!==null && structure.width!==null) {
          return function() {
            HTomb.GUI.selectBox(structure.width, structure.height, assigner.z,that.designateBox,{
              assigner: assigner,
              context: that,
              bg: that.bg,
              callback: placeBox,
              hover: myHover,
              contextName: "Designate"+that.template
            });
          };
        } else {
          return function() {
            HTomb.GUI.selectSquareZone(assigner.z,that.designateBox,{
              assigner: assigner,
              context: that,
              bg: that.bg,
              callback: placeBox,
              hover: myHover,
              contextName: "Designate"+that.template
            });
          };
        }
      },
      {
        format: function(structure) {
          let g = structure.describe();
          let ings = structure.totalIngredients();
          if (Object.keys(ings).length>0) {
            g+=" ";
            g+=HTomb.Utils.listIngredients(ings);
            if (!assigner || !assigner.master) {
              return g;
            }
            if (assigner.owner.ownsAllIngredients(ings)!==true) {
              g = "%c{gray}"+g;
            }
            return g;
          } else {
            return g;
          }
        },
        contextName: "ChooseStructure"
      });
    },
    canAssign: function(cr) {
      if (Task.canAssign.call(this, cr)!==true) {
        return false;
      }
      // more intuitive if ingredient-needing features are built first
      let ings = this.structure.neededIngredients();
      // what on earth???
      if (Object.keys(ings).length>0 && Object.keys(this.ingredients).length===0) {
        return false;
      }
      return true;
    },
    begin: function() {
      Task.begin.call(this);
      let i = this.position;
      let f = HTomb.World.features[coord(this.x, this.y, this.z)];
      f.fg = this.structure.fgs[i] || this.structure.fg;
      HTomb.World.covers[this.z][this.x][this.y] = HTomb.Covers.NoCover;
    },
    finish: function() {
      Task.finish.call(this);
      let x = this.x;
      let y = this.y;
      let z = this.z;
      let f = HTomb.World.features[coord(x,y,z)];
      let p = this.position;
      f.fg = this.structure.fgs[p] || this.structure.fg;
      f.symbol = this.structure.symbols[p] || this.structure.symbol;
      f.structure = this.structure;
      this.structure.features[p] = f;
      for (let i=0; i<this.structure.squares.length; i++) {
        if (!this.structure.features[i]) {
          break;
        }
        if (i===this.structure.squares.length-1) {
          let w = this.structure;
          w.place(w.x, w.y, w.z);
          HTomb.GUI.pushMessage(this.assignee.describe({capitalized: true, article: "indefinite"}) + " finishes building " + w.describe({article: "indefinite", atCoordinates: true}) + ".");
        }
      }
    },
    designateBox: function(squares, options) {
      options = options || {};
      var assigner = options.assigner;
      var callb = options.callback;
      callb.call(options.context,squares,assigner);
    }
  });

return HTomb;
})(HTomb);