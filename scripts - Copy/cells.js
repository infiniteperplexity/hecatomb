HTomb = (function(HTomb) {
  "use strict";

  HTomb.Cells = function(options) {
    options = options || {};
    this.width = options.width || HTomb.Constants.LEVELW;
    this.height = options.height || HTomb.Constants.LEVELH;
    this.born = options.born || [0,0,0,0,0,1,1,1,1];
    this.survive = options.survive || [0,0,0,1,1,1,1,1];
    this.borders = options.borders || false;
    this.neighborhood =  options.neighborhood || ROT.DIRS[8];
    this.map = [];
    this.mask = [];
    for (var x=0; x<this.width; x++) {
      this.map.push([]);
      this.mask.push([]);
      for (var y=0; y<this.height; y++) {
        if (x===0 || y===0 || x===this.width-1 || y===this.height-1) {
          this.mask[x][y] = this.borders;
        } else {
          this.mask[x][y] = null;
        }
      }
    }
  };
  HTomb.Cells.prototype.randomize = function(p) {
    p = p || 0.5;
    for (var x=0; x<this.width; x++) {
      for (var y=0; y<this.height; y++) {
        this.map[x][y] = (this.mask[x][y]===null) ? (Math.random()<p) : this.mask[x][y];
      }
    }
  };
  HTomb.Cells.prototype.initialize = function(callb) {
    for (var x=0; x<this.width; x++) {
      for (var y=0; y<this.height; y++) {
        this.map[x][y] = (this.mask[x][y]===null) ? callb(x,y) : this.mask[x][y];
      }
    }
  };
  HTomb.Cells.prototype.setMask = function(callb) {
    for (var x=1; x<this.width-1; x++) {
      for (var y=1; y<this.height-1; y++) {
        this.mask[x][y] = callb(x,y);
      }
    }
  };
  HTomb.Cells.prototype.iterate = function(n) {
    n = n || 1;
    var x, y;
    for (var i=0; i<n; i++) {
      var next = [];
      for (x=0; x<this.width; x++) {
        next.push([]);
        for (y=0; y<this.height; y++) {
          var tally = 0;
          if (this.mask[x][y]===null) {
            for (var j=0; j<this.neighborhood.length; j++) {
              var dx = x+this.neighborhood[j][0];
              var dy = y+this.neighborhood[j][1];
              if (this.map[dx][dy]===true) {
                tally+=1;
              }
            }
          }
          if (this.mask[x][y]!==null) {
            next[x][y] = this.mask[x][y];
          } else if (this.map[x][y]===true) {
            next[x][y] = (Math.random()<this.survive[tally]) ? true : false;
          } else {
            next[x][y] = (Math.random()<this.born[tally]) ? true : false;
          }
        }
      }
      for (x=0; x<this.width; x++) {
        for (y=0; y<this.height; y++) {
          this.map[x][y] = next[x][y];
        }
      }
    }
  };
  HTomb.Cells.prototype.apply = function(callb) {
    for (var x=0; x<this.width; x++) {
      for (var y=0; y<this.height; y++) {
        if (this.mask[x][y]===null) {
          callb(x, y, this.map[x][y]);
        }
      }
    }
  };


return HTomb;
})(HTomb);
