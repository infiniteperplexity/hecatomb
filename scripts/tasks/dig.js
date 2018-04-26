HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var coord = HTomb.Utils.coord;

  let Task = HTomb.Things.Task;

  HTomb.Things.Task.extend({
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
      let earth = HTomb.World.covers[z][x][y];
      let t = HTomb.World.tiles[z][x][y];
      if (t===HTomb.Tiles.FloorTile || t===HTomb.Tiles.DownSlopeTile) {
        earth = HTomb.World.covers[z-1][x][y];
      }
      let hardness;
      if (earth.mineral) {
        hardness = earth.mineral.hardness;
      } else {
        hardness = 0;
      }
      let labor = 0;
      // actually now we could change how this works...
      if (cr.worker) {
        labor = cr.worker.getLabor();
      }
      // if this minion can't do it...
      if (labor-hardness<=0) {
        for (let minion of this.assigner.master.minions) {
          labor = Math.max(labor, minion.worker.getLabor());
        }
        // ...if no minion can, cancel the task
        if (labor-hardness<=0) {
          this.cancel();
        }
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
      let earth = HTomb.World.covers[z][x][y];
      if (t===HTomb.Tiles.FloorTile || t===HTomb.Tiles.DownSlopeTile) {
        earth = HTomb.World.covers[z-1][x][y];
      }
      let hardness;
      if (earth.mineral) {
        hardness = earth.mineral.hardness;
      } else {
        hardness = 0;
      }
      let labor = 0;
      // no assigner yet at this point?
      for (let minion of HTomb.Player.master.minions) {
        labor = Math.max(labor, minion.worker.getLabor());
      }
      if (labor-hardness<=0) {
        if (this.isPlaced()) {
          this.cancel();
        }
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
      let earth = HTomb.World.covers[z][x][y];
      if (HTomb.World.tiles[z][x][y]===HTomb.Tiles.DownSlopeTile || HTomb.World.Tiles===HTomb.Tiles.FloorTile) {
        earth = HTomb.World.covers[z-1][x][y];
      }
      if (earth.mineral) {
        f.effort = earth.mineral.hardness;
      } else {
        f.effort = 0;
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
      // this should unforbid items...
      // If there is a slope below, dig out the floor
      if (tiles[z-1][x][y]===UpSlopeTile && HTomb.World.explored[z-1][x][y] && (t===WallTile || t===FloorTile)) {
        tiles[z][x][y] = DownSlopeTile;
        HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
      // If it's a wall, dig a tunnel
      } else if (t===WallTile) {
        tiles[z][x][y] = FloorTile;
        if (c.mineral) {
          c.mineral.mine(x,y,z);
        } else {
          HTomb.Things.Mineral.mine(x,y,z);
        }
      } else if (t===FloorTile) {
        // If it's a floor with a wall underneath dig a trench
        if (tiles[z-1][x][y]===WallTile) {
          tiles[z][x][y] = DownSlopeTile;
          tiles[z-1][x][y] = UpSlopeTile;
          HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
          c = HTomb.World.covers[z-1][x][y];
          if (c.mineral) {
            c.mineral.mine(x,y,z-1);
          } else {
            HTomb.Things.Mineral.mine(x,y,z-1);
          }
          z-=1;
        // Otherwise just remove the floor
        } else {
          tiles[z][x][y] = EmptyTile;
          HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
        }
      // If it's a down slope tile, remove the slopes
      } else if (t===DownSlopeTile) {
        tiles[z][x][y] = EmptyTile;
        tiles[z-1][x][y] = FloorTile;
        HTomb.World.covers[z-1][x][y] = HTomb.Covers.NoCover;
      // if it's an upward slope, remove the slope
      } else if (t===UpSlopeTile) {
        tiles[z][x][y] = FloorTile;
        HTomb.World.covers[z][x][y] = HTomb.Covers.NoCover;
        if (tiles[z+1][x][y]===DownSlopeTile) {
          tiles[z+1][x][y] = EmptyTile;
          z-=1;
        }
      } else if (t===EmptyTile) {
        // this shouldn't happen
      }
      // This is buried stuff you dug up, I think? not the mined materials?
      let items = HTomb.World.items[coord(x,y,z)] || HTomb.Things.Items();
      for (let item of items) {
        item.owned = true;
      }
      HTomb.World.validate.cleanNeighbors(x,y,z);
      HTomb.World.validate.breach(x,y,z);
      let f = HTomb.World.features[coord(x,y,this.z)];
      // this fails sometimes?
      if (f) {
        f.remove();
        f.despawn();
      }
      //!!!Experimental...hope this doesn't slow things down
      HTomb.Path.reset();
    }
  });

  HTomb.Things.Feature.extend({
    template: "Excavation",
    name: "excavation",
    Components: {
      Fixture: {
        labor: 10,
        effort: 0,
        incompleteSymbol: "\u2717",
        incompleteFg: HTomb.Constants.BELOWFG
      }
    }
  });

  return HTomb;
})(HTomb);


