HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var World = HTomb.World;
  var features = World.features;
  var coord = HTomb.Utils.coord;
  var tiles = World.tiles;
  var levels = World.levels;
  // default passability function
  var t;
  function defaultPassable(x,y,z,x0,y0,z0) {
    if (x<0 || x>=LEVELW || y<0 || y>=LEVELH || z<0 || z>=NLEVELS) {
      return false;
    }
    t = tiles[z][x][y];
// t = _fastgrid[z*LEVELH*NLEVELS+y*LEVELH+x];
    // should this be able to handle doors?
    return (t.solid===undefined && t.fallable===undefined);
  }

  // choose between absolute grid distance or euclidean diagonal distance
  var abs = Math.abs;
  function h1(x0,y0,z0,x1,y1,z1) {
    return abs(x1-x0)+abs(y1-y0)+abs(z1-z0);
  }
  function h2(x0,y0,z0,x1,y1,z1) {
    return Math.sqrt((x1-x0)*(x1-x0)+(y1-y0)*(y1-y0)+(z1-z0)*(z1-z0));
  }
  var h = h1;

  var _fastgrid;
  //function aStar(x0,y0,z0,x1,y1,z1,canPass) {

  HTomb.Path.benchmarks = {
    succeeded: [],
    failed: []
  };

  HTomb.Path.failures = {};
  HTomb.Path.successes = {};
  HTomb.Events.subscribe(HTomb.Path,"TurnBegin");
  HTomb.Path.onTurnBegin = function() {
    for (let f in HTomb.Path.failures) {
      HTomb.Path.failures[f][0]-=1;
      if (HTomb.Path.failures[f][0]<=0) {
        delete HTomb.Path.failures[f];
      }
    }
    for (let f in HTomb.Path.successes) {
      HTomb.Path.successes[f][0]-=1;
      if (HTomb.Path.successes[f][0]<=0) {
        delete HTomb.Path.successes[f];
      }
    }
  };
  HTomb.Path.reset = function() {
    for (let i in HTomb.Path.failures) {
      delete HTomb.Path.failures[i];
    }
    for (let i in HTomb.Path.successes) {
      delete HTomb.Path.successes[i];
    }
  };
  HTomb.Path.aStar = function(x0,y0,z0,x1,y1,z1,options) {
    if (x0+y0+z0+x1+y1+z1===undefined || x1===null || y1===null || z1===null || x0===null || y0===null || z0===null) {
      if (options.searcher) {
        console.log("searcher:");
        console.log(options.searcher);
      }
      if (options.searchee) {
        console.log("searchee:");
        console.log(options.searchee);
      }
      console.log("coordinates:");
      console.log([x0,y0,z0,x1,y1,z1]);
      alert("bad path arguments! examine console log for details.");
    }
    options = options || {};
    var useFirst = options.useFirst || false;
    var useLast = (options.useLast===false) ? false : true;
    var canPass = options.canPass || defaultPassable;
    var searcher = options.searcher;
    var searchee = options.searchee;
    var searchTimeout = options.searchTimeout;
    var cacheAfter = options.cacheAfter;
    var cacheTimeout = options.cacheTimeout;
    var maxTries = options.maxTries || LEVELW*LEVELH;
    //wait...do we actually track this in any decent way?  current score?
    var maxLength = options.maxLength || LEVELW*LEVELH;
    if (searcher && searchee && searchTimeout) {
      if (HTomb.Path.failures[searcher.spawnId + "," + searchee.spawnId]) {
        return false;
      }
    }
    if (searcher && searchee && cacheTimeout && cacheAfter!==undefined) {
      let t = HTomb.Path.successes[searcher.spawnId + "," + searchee.spawnId];
      if (t) {
        t = t[1];
        if (HTomb.Path.quickDistance(x0,y0,z0,t.x,t.y,t.z)<2 || HTomb.Path.quickDistance(x0,y0,z0,searchee.x,searchee.y,searchee.z)<2) {
          delete HTomb.Path.successes[searcher.spawnId + "," + searchee.spawnId];
        } else {
          options.searchee = undefined;
          options.cacheAfter = undefined;
          return HTomb.Path.aStar(x0,y0,z0,t.x,t.y,t.z,options);
        }
      }
    }
    let squaresTried = 0;
    //let stats = {
    //  squaresTried: 0,
    //  pathLength: 0,
    //  maxLength: 0
    //};
    if (HTomb.Tiles.isEnclosed(x0,y0,z0,canPass) || HTomb.Tiles.isEnclosed(x1,y1,z1,canPass)) {
      return false;
    }
    // fastest possible lookup
    // random bias should be okay
    var dirs = [
      [ 0, -1],
      [ 1, -1],
      [ 1,  0],
      [ 1,  1],
      [ 0,  1],
      [-1,  1],
      [-1,  0],
      [-1, -1]
    ].randomize();
    var current, next, this_score, h_score, crd;
    var checked = {}, scores = {}, retrace = {}, path = [], pathLength = {};
    // it costs zero to get to the starting square
    scores[coord(x0,y0,z0)] = 0;
    //square that need to be checked
    //three-dimensional coordinate, and estimated (heuristic) distance
    var tocheck = [[x0,y0,z0,this_score+h(x0,y0,z0,x1,y1,z1)]];
    // keep checking until the algorithm finishes
    while (tocheck.length>0) {
      // choose the highest-priority square
      current = tocheck.shift();
      // calculate the fast lookup
      crd = coord(current[0],current[1],current[2]);
      // check if we have found the target square (or maybe distance==1?)
      if (  (current[0]===x1 && current[1]===y1 && current[2]===z1)
            || (useLast===false && HTomb.Utils.arrayInArray([x1,y1,z1],HTomb.Tiles.touchableFrom(current[0],current[1],current[2]))>-1)
        ) {
      // if (current[6]===1) {
        // start with the goal square
        path = [[current[0],current[1],current[2]]];
        // until we get back to the starting square...
        while (current[0]!==x0 || current[1]!==y0 || current[2]!==z0) {
          // retrace the path by one step
          current = retrace[crd];
          // calculate the fast coordinate
          crd = coord(current[0],current[1],current[2]);
          // add the next square to the returnable path
          path.unshift([current[0],current[1],current[2]]);
        }
        // return the complete path
        if (path.length>0 && useFirst===false) {
          path.shift();
        }
        if (searcher && searchee && cacheTimeout && cacheAfter!==undefined && path.length>cacheAfter) {
          let t = path[cacheAfter];
          t = HTomb.Tiles.getTileDummy(t[0],t[1],t[2]);
          cacheTimeout = HTomb.Utils.perturb(cacheTimeout);
          HTomb.Path.successes[searcher.spawnId+","+searchee.spawnId] = [cacheTimeout,t];
        }
        //if (path.length>0 && useLast===false) {
        //  path.pop();
        //}
        return path;
      }
      // we are now checking this square
      checked[crd] = true;
      // loop through neighboring cells
      for (var i=-1; i<8; i++) {
        // -1 is the place where we check for portals
        if (i===-1) {
          // right now cannot handle multiple portals in one square
          if (tiles[current[2]][current[0]][current[1]].zmove!==undefined) {
            next = [current[0],current[1],current[2]+tiles[current[2]][current[0]][current[1]].zmove];
          } else {
            continue;
          }
        } else {
          // grab a neighboring square
          next = [current[0]+dirs[i][0],current[1]+dirs[i][1],current[2]];
        }
        crd = coord(next[0],next[1],next[2]);
        // if this one has been checked already then skip it
        if (checked[crd]) {
          //HTomb.GUI.drawAt(next[0],next[1],"X","purple","black");
          continue;
        }
        squaresTried+=1;
        // otherwise set the score equal to the distance from the starting square
          // this assumes a uniform edge cost of 1
        this_score = scores[coord(current[0],current[1],current[2])]+1;
        // if there is already a better score for this square then skip it
        //if (scores[crd]!==undefined && scores[crd]<=this_score) {
        if (scores[crd]!==undefined && (scores[crd]<=this_score || this_score>=maxLength)) {
          //HTomb.GUI.drawAt(next[0],next[1],"X","yellow","black");
          continue;
        }
        // if the move is not valid then skip it
        if (canPass(next[0],next[1],next[2],current[0],current[1],current[2])===false) {
        //if (canPass(next[0],next[1],next[2])===false) {
          //HTomb.GUI.drawAt(next[0],next[1],"X","red","black");
          continue;
        }
        h_score = this_score + h(next[0],next[1],next[2],x1,y1,z1);
        if (isNaN(h_score)) {
          console.log(this_score);
          console.log(next);
          console.log([x1,y1,z1]);
          alert("scoring failed!");
        }
        //HTomb.GUI.drawAt(next[0],next[1],"X","green","black");
        // now add it to the to-do list unless it already has a better score on there
        for (var j=0; j<tocheck.length; j++) {
          // if this score is better than the one being checked...
          if (h_score<=tocheck[j][3]) {
            // insert it into the priority queue based on estimated distance
            tocheck.splice(j,0,[next[0],next[1],next[2],h_score]);
            // use this as a flag
            h_score = -1;
            break;
          }
        }
        // if it is worse than the worst score on the list, add to the end
        if (h_score>-1) {
          //tocheck.push([next[0],next[1],next[2],this_score+abs(next[0]-x1)+abs(next[1]-y1)+abs(next[2]-z1)]);
          tocheck.push([next[0],next[1],next[2],h_score]);
        }
        // set the parent square in the potential path
        retrace[crd] = [current[0],current[1],current[2]];
        //pathLength[crd] = pathLength[coord(current[0],current[1],current[2])]+1 || 0;
        // save the new best score for this square
        scores[crd] = this_score;
      }
      if (squaresTried>=maxTries) {
        break;
      }
    }
    console.log(searcher);
    console.log(searchee);
    console.log("path failed after " + squaresTried);
    if (searcher && searchee && searchTimeout) {
      let combo = searcher.spawnId+","+searchee.spawnId;
      searchTimeout = HTomb.Utils.perturb(searchTimeout);
      HTomb.Path.failures[combo] = [searchTimeout,squaresTried];
    }
    //for (let len in pathLength) {
    //  stats.maxLength = Math.max(stats.maxLength,pathLength[len]);
    //}
    //HTomb.Path.benchmarks.failed.push(stats);
    return false;
  };


  //bresenham's line drawing algorithm
  HTomb.Path.line = function(x0, y0, x1, y1){
    var path = [];
    var dx = Math.abs(x1-x0);
    var dy = Math.abs(y1-y0);
    var sx = (x0 < x1) ? 1 : -1;
    var sy = (y0 < y1) ? 1 : -1;
    var err = dx-dy;
    while(true){
      path.push([x0,y0]);
      if ((x0==x1) && (y0==y1)) break;
      var e2 = 2*err;
      if (e2 >-dy){ err -= dy; x0  += sx; }
      if (e2 < dx){ err += dx; y0  += sy; }
    }
    return path;
  };

  HTomb.Path.distance = function(x0, y0, x1, y1) {
    var line = HTomb.Path.line(x0,y0,x1,y1);
    // should be length-1?
    return line.length-1;
  };

  HTomb.Path.quickDistance = function(x0,y0,z0,x1,y1,z1) {
    return Math.sqrt((x1-x0)*(x1-x0)+(y1-y0)*(y1-y0)+(z1-z0)+(z1-z0));
  };

  HTomb.Path.closest = function(x,y,z,arr) {
    arr.sort(function(a,b) {
      return HTomb.Path.quickDistance(x,y,z,a.x,a.y,a.z) - HTomb.Path.quickDistance(x,y,z,b.x,b.y,b.z);
    });
    return arr;
  };
  HTomb.Path.closestCallback = function(x,y,z) {
    return function(a,b) {
      return HTomb.Path.quickDistance(x,y,z,a.x,a.y,a.z) - HTomb.Path.quickDistance(x,y,z,b.x,b.y,b.z);
    };
  };

  HTomb.Path.flood = function(x,y, callb) {
    let stack = [[x,y]];
    let checked = [];
    for (let x=0; x<LEVELW; x++) {
      checked.push([]);
    }
    let squares = [];
    while (stack.length>0) {
      let next = stack.pop();
      if (checked[next[0]][next[1]]===true) {
        continue;
      }
      checked[next[0]][next[1]] = true;
      if (callb(next[0],next[1])===true) {
        squares.push(next);
        let vn = HTomb.Path.vonNeumann(next[0],next[1]);
        for (let i=0; i<vn.length; i++) {
          stack.push([vn[i][0],vn[i][1]]);
        }
      }
    }
    return squares;
  };

  HTomb.Path.floodRegions = function(callb) {
    let checked = [];
    for (let x=0; x<LEVELW; x++) {
      checked.push([]);
    }
    let regions = [];
    let tries = 0;
    while (tries<500) {
      let x = Math.floor(Math.random()*(LEVELW-2))+1;
      let y = Math.floor(Math.random()*(LEVELH-2))+1;
      if (callb(x,y) && checked[x][y]!==true) {
        let squares = HTomb.Path.flood(x,y,callb);
        regions.push(squares);
        for (let i=0; i<squares.length; i++) {
          let s = squares[i];
          checked[s[0]][s[1]] = true;
        }
        tries = 0;
      } else {
        tries+=1;
      }
    }
    return regions;
  };

  HTomb.Path.vonNeumann = function(x,y,n,hollow) {
    n = n || 1;
    let coords = [];
    let dirs = ROT.DIRS[4];
    let j1 = 1;
    if (hollow) {
      j1 = n;
    }
    for (let i=0; i<dirs.length; i++) {
      for (let j=j1; j<=n; j++) {
        let x1 = x+j*dirs[i][0]
        let y1 = y+j*dirs[i][1];
        if (x1>0 && x1<LEVELW-1 && y1>0 && y1<LEVELH-1) {
          coords.push([x1,y1]);
        }
      }
    }
    return coords;
  }

  HTomb.Path.moore = function(x,y,n,hollow) {
    n = n || 1;
    let coords = [];
    let k0 = 1;
    if (hollow) {
      k0 = n;
    }
    for (let k=k0; k<=n; k++) {
      for (let i=x-k; i<=x+k; i++) {
        coords.push([i,y+k]);
        coords.push([i,y-k]);
      }
      for (let j=y-k+1; j<=y+k-1; j++) {
        coords.push([x+k,j]);
        coords.push([x-k,j]);
      }
    }
    return coords;
  }

  function initRings() {
    let circles = [];
    for (let i=0; i<LEVELW; i++) {
      let ring = [];
      for (let x=-i; x<=i; x++) {
        for (let y=-i; y<=i; y++) {
          if (Math.round(Math.sqrt(x*x+y*y))===i) {
            ring.push([x,y]);
          }
        }
      }
      circles.push(ring);
    }
    return circles;
  }
  HTomb.Path.concentric = initRings();

  HTomb.Path.DjikstraMap = function() {
    this.grid = [];
    for (let x=0; x<LEVELW-1; x++) {
      this.grid.push([]);
      for (let y=0; y<LEVELH-1; y++) {
        this.grid.push(LEVELH*LEVELW);
      }
    }
  }
return HTomb;
})(HTomb);
