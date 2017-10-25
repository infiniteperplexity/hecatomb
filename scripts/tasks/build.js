HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var coord = HTomb.Utils.coord;

  let Task = HTomb.Things.Task;

  HTomb.Things.Task.extend({
    template: "BuildTask",
    name: "build",
    description: "build walls or floors",
    //description: "build walls or floors ($: 1 rock)",
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
        if (tile===HTomb.Tiles.UpSlopeTile || tile===HTomb.Tiles.FloorTile) {
          tallest = Math.max(tallest,1);
        }
      }
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
          if (tile===HTomb.Tiles.UpSlopeTile || tile===HTomb.Tiles.FloorTile) {
            tallest = Math.max(tallest,1);
          } else if (tile===HTomb.Tiles.DownSlopeTile || tile===HTomb.Tiles.EmptyTile) {
            tallest = Math.max(tallest,-1);
          }
        }
        if (tallest===1) {
          menu.middle = ["%c{lime}Construct new walls in this area."];
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
      // If it's a floor, build a wall
      if (t===FloorTile) {
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
      HTomb.Path.reset();
    }
  });

  HTomb.Things.Feature.extend({
    template: "Construction",
    name: "construction",
    Components: {
      Fixture: {
        incompleteSymbol: "\u2692",
        labor: 15,
        incompleteFg: HTomb.Constants.WALLFG
      }
    }
  });

  return HTomb;
})(HTomb);


