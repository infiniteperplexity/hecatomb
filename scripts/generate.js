HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;


  function timeIt(name,callb) {
    console.time(name);
    callb();
    console.timeEnd(name);
  }

  // //************Concrete methods for populating a world****************
  var placement = {
    creatures: {},
    items: {},
    features: {}
  };
  placement.reset = function() {
    this.creatures = {};
    this.items = {};
    this.features = {};
  };
  placement.stack = function(thing,x,y,z) {
    var crd = coord(x,y,z);
    var stack;
    if (thing.feature) {
      stack = this.features[crd] || [];
      stack.push(thing);
      this.features[crd] = stack;
    } else if (thing.item) {
      stack = this.items[crd] || [];
      stack.push(thing);
      this.items[crd] = stack;
    } else if (thing.creature) {
      stack = this.creatures[crd] || [];
      stack.push(thing);
      this.creatures[crd] = stack;
    } else {
      thing.place(x,y,z);
    }
  }
  placement.resolve = function() {
    var crd, stack, d;
    for (crd in this.creatures) {
      if (HTomb.World.creatures[crd]) {
        continue;
      }
      stack = this.creatures[crd];
      if (stack.length>1) {
        HTomb.Utils.shuffle(stack);
      }
      d = HTomb.Utils.decoord(crd);
      stack[0].place(d[0],d[1],d[2]);
    }
    for (crd in this.features) {
      if (HTomb.World.features[crd]) {
        continue;
      }
      stack = this.features[crd];
      if (stack.length>1) {
        HTomb.Utils.shuffle(stack);
      }
      d = HTomb.Utils.decoord(crd);
      stack[0].place(d[0],d[1],d[2]);
    }
    for (crd in this.items) {
      if (HTomb.World.items[crd]) {
        continue;
      }
      stack = this.items[crd];
      if (stack.length>1) {
        HTomb.Utils.shuffle(stack);
      }
      d = HTomb.Utils.decoord(crd);
      stack[0].place(d[0],d[1],d[2]);
    }
    this.reset();
  };

  HTomb.World.generators = {};

  HTomb.World.generators.bestSoFar = function() {
timeIt("elevation", function() {
    assignElevation(50);
}); timeIt("lava", function() {
    placeLava(10);
}); timeIt("water", function() {
    waterTable(48,4);
}); timeIt("slopes", function() {
    addSlopes();
}); timeIt("caverns", function() {
    cavernLevels(3);
}); timeIt("labyrinths", function() {
    labyrinths();
}); timeIt("minerals", function() {
    placeMinerals({template: "IronVein", p: 0.0025});
    placeMinerals({template: "BloodstoneCluster", p: 0.001});
    placeMinerals({template: "GoldVein", p: 0.0025});
    placeMinerals({template: "MoonstoneCluster", p: 0.001});
    placeMinerals({template: "JadeCluster", p: 0.0025});
}); timeIt("grass", function() {
    grassify();
}); timeIt("plants", function() {
    growPlants({template: "Tree", p: 0.05});
    growPlants({template: "Shrub", p: 0.05});
    growPlants({template: "WolfsbanePlant", p: 0.001});
    growPlants({template: "AmanitaPlant", p: 0.001});
    growPlants({template: "MandrakePlant", p: 0.001});
    growPlants({template: "WormwoodPlant", p: 0.001});
    growPlants({template: "BloodwortPlant", p: 0.001});
}); timeIt("graveyards", function() {
    graveyards();
}); timeIt("critters", function() {
    placeCritters();
}); timeIt("resolving", function() {
    placement.resolve();
}); timeIt("no hauling", function() {
    notOwned();
}); timeIt("player", function() {
    placePlayer();
});
  };

  var lowest;
  var highest;
  var elevation;
  function assignElevation(ground) {
    ground = ground || 50;
    //var hscale1 = 128;
    var hscale1 = 256;
    var vscale1 = 3;
    var hscale2 = 64;
    //var hscale2 = 128;
    var vscale2 = 2;
    //var hscale3 = 32;
    var hscale3 = 64;
    var vscale3 = 1;
    var noise = new ROT.Noise.Simplex();
    var grid = [];
    var mx = 0, mn = NLEVELS;
    for (var x=0; x<LEVELW; x++) {
      grid.push([]);
      for (var y=0; y<LEVELH; y++) {
        grid[x][y] = ground;
        grid[x][y]+= noise.get(x/hscale1,y/hscale1)*vscale1;
        grid[x][y]+= noise.get(x/hscale2,y/hscale2)*vscale2;
        grid[x][y]+= noise.get(x/hscale3,y/hscale3)*vscale3;
        grid[x][y] = parseInt(grid[x][y]);
        mx = Math.max(mx,grid[x][y]);
        mn = Math.min(mn,grid[x][y]);
        if (x>0 && x<LEVELW-1 && y>0 && y<LEVELH-1) {
          for (var z=grid[x][y]; z>=0; z--) {
            HTomb.World.tiles[z][x][y] = HTomb.Tiles.WallTile;
          }
          z = grid[x][y]+1;
          HTomb.World.tiles[z][x][y] = HTomb.Tiles.FloorTile;
          HTomb.World.exposed[x][y] = z;
        }
      }
    }
    lowest = mn;
    highest = mx;
    elevation = grid;
    console.log("Highest at " + mx + ", lowest at " + mn);
  }

  HTomb.World.elevation = function(x,y) {
    x = Math.round(x);
    y = Math.round(y);
    if (elevation[x]===undefined) {
      console.log([x,y]);
    }
    return elevation[x][y];
  };

  function addSlopes() {
    var tiles = HTomb.World.tiles;
    var squares;
    var square;
    var slope;
    for (var x=0; x<LEVELW; x++) {
      for (var y=0; y<LEVELH; y++) {
        for (var z=0; z<NLEVELS-1; z++) {
          if (tiles[z][x][y]===HTomb.Tiles.FloorTile && tiles[z+1][x][y]===HTomb.Tiles.EmptyTile) {
            squares = HTomb.Tiles.neighbors(x,y);
            slope = false;
            for (var i=0; i<squares.length; i++) {
              square = squares[i];
              if (tiles[z][square[0]][square[1]]===HTomb.Tiles.WallTile
                  && tiles[z+1][square[0]][square[1]]===HTomb.Tiles.FloorTile) {
                slope = true;
              }
            }
            if (slope===true) {
              //HTomb.Entity.create("UpSlope").place(x,y,z);
              tiles[z][x][y] = HTomb.Tiles.UpSlopeTile;
              tiles[z+1][x][y] = HTomb.Tiles.DownSlopeTile;
            }
          }
        }
      }
    }
  }

  function waterTable(elev, depth) {
    elev = elev || lowest+3;
    depth = depth || 4;
    var rock = new ROT.Map.Cellular(LEVELW,LEVELH);
    rock.randomize(0.45);
    for (var i=0; i<10; i++) {
      rock.create();
    }
    function nonsolids(x,y,z) {return HTomb.World.tiles[z][x][y].solid!==true;}
    for (var x=1; x<LEVELW-1; x++) {
      for (var y=1; y<LEVELH-1; y++) {
        for (var z=elev; z>=lowest; z--) {
          if (z<elev-depth && HTomb.World.tiles[z][x][y]===HTomb.Tiles.WallTile) {
            break;
          } else if (rock._map[x][y]===0 || HTomb.World.tiles[z][x][y]!==HTomb.Tiles.WallTile
              || HTomb.Tiles.countNeighborsWhere(x,y,z,nonsolids)>0) {
            HTomb.World.covers[z][x][y] = HTomb.Covers.Water;
          }
        }
      }
    }
  }
  function placeLava(elev) {
    elev = elev || 10;
    for (var x=1; x<LEVELW-1; x++) {
      for (var y=1; y<LEVELH-1; y++) {
        for (var z=elev; z>0; z--) {
          if (z<elev) {
            HTomb.World.tiles[z][x][y] = HTomb.Tiles.EmptyTile;
          }
          HTomb.World.covers[z][x][y] = HTomb.Covers.Lava;
        }
      }
    }
  }

  function graveyards(options) {
    options = options || {};
    let yardChance = options.p || 0.005;
    let graveChance = 0.5;
    function nonsolids(x,y,z) {return HTomb.World.tiles[z][x][y].solid!==true;}
    let dirs = ROT.DIRS[4];
    for (let i=0; i<LEVELW*LEVELH*yardChance; i++) {
      let x = HTomb.Utils.dice(1,LEVELW-4);
      let y = HTomb.Utils.dice(1,LEVELH-4);
      let z = HTomb.Tiles.groundLevel(x,y);
      if (z<=lowest+4) {
        continue;
      }
      for (let j=0; j<dirs.length; j++) {
        if (HTomb.Tiles.countNeighborsWhere(x+dirs[j][0],y+dirs[j][1],z-1,nonsolids)>0) {
          continue;
        } else if (Math.random()<graveChance) {
          placement.stack(HTomb.Things.Tombstone(),x+dirs[j][0],y+dirs[j][1],z);
          HTomb.World.covers[z-1][x][y] = HTomb.Covers.NoCover;
        }
      }
    }
  }

/*
  function graveyards(options) {
    options = options || {};
    var p = options.p || 0.01;
    var n = options.n || 3;
    var born = options.born || [0,0.1,0.2,0.3,0.5,0.3,0.2,0];
    var survive = options.survive || [0.7,0.8,0.8,0.8,0.6,0.4,0.2,0.1];
    var cells = new HTomb.Cells({
      born: born,
      survive: survive
    });
    cells.randomize(p);
    cells.iterate(n);
    function fallables(x,y,z) {return HTomb.World.tiles[z][x][y].fallable;}
    cells.apply(function(x,y,val) {
      if (val) {
        var z = HTomb.Tiles.groundLevel(x,y);
        if (z>lowest+4) {
          if (HTomb.Tiles.countNeighborsWhere(x,y,z,fallables)===0
              && HTomb.World.covers[z][x][y]===HTomb.Covers.NoCover
              && HTomb.World.covers[z-1][x][y]===HTomb.Covers.NoCover) {
            var grave = HTomb.Things["Tombstone"]();
            placement.stack(grave,x,y,z);
          }
        }
      }
    });
  }*/

  function graveyards2(options) {
    options = options || {};
    let gen = new GreuterGenerator();
    for (let i=0; i<40; i++) {
      let x = HTomb.Utils.dice(1,LEVELW-2);
      let y = HTomb.Utils.dice(1,LEVELH-2);
      gen.spawn(x,y);
    }
    gen.resolve();
  }

  function voronoi() {
    let routes = HTomb.Path.citygen();
    for (let i=0; i<routes.length; i++) {
      let route = routes[i];
      for (let j=0; j<route.length-1; j++) {
        let x0 = route[j][0];
        let y0 = route[j][1];
        let x1 = route[j+1][0];
        let y1 = route[j+1][1];
        let line = HTomb.Path.line(x0,y0,x1,y1);
        for (let k=0; k<line.length; k++) {
          let x = line[k][0];
          let y = line[k][1];
          let z = HTomb.Tiles.groundLevel(x,y);
          HTomb.Things.Bloodstone().place(x,y,z);
        }
      }
    }
  };


  function cavernLevels(n) {
    n = n || 4;
    n = parseInt(ROT.RNG.getNormal(n,1));
    var used = [];
    for (var k=0; k<n; k++) {
      var placed = false;
      var tries = 0;
      var max = 50;
      while (placed===false && tries<max) {
        var z = parseInt(Math.random()*40)+11;
        if (used.indexOf(z)!==-1) {
          tries+=1;
          continue;
        }
        placed = true;
        var z = parseInt(Math.random()*30)+11;
        used.push(z);
        used.push(z+1);
        used.push(z-1);
        var caves = new ROT.Map.Cellular(LEVELW-2,LEVELH-2,{connected: true});
        caves.randomize(0.5);
        for (var i=0; i<6; i++) {
          caves.create();
        }
        console.log("cavern level at " + z);
        caves.create(function(x,y,val) {
          if (val) {
            HTomb.World.tiles[z][x+1][y+1] = HTomb.Tiles.FloorTile;
            HTomb.World.validate.dirtify(x+1,y+1,z);
          }
        });
      }
    }
    HTomb.World.validate.clean();
  }
  function labyrinths(n) {
    n = n || 12;
    n = parseInt(ROT.RNG.getNormal(n,n/4));
    for (var k=0; k<n; k++) {
      var width = parseInt(Math.random()*8)+8;
      var height = parseInt(Math.random()*8)+8;
      var placed = false;
      var tries = 0;
      var max = 50;
      while (placed===false && tries<max) {
        var x = parseInt(Math.random()*(LEVELW-20))+10;
        var y = parseInt(Math.random()*(LEVELH-20))+10;
        var z = parseInt(Math.random()*(lowest-8))+11;
        placed = true;
        outerLoop:
        for (var i=x; i<x+width; i++) {
          for (var j=y; j<y+height; j++) {
            if (HTomb.World.tiles[z][i][j]!==HTomb.Tiles.WallTile) {
              placed = false;
              break outerLoop;
            }
          }
        }
        if (placed===true) {
          var maze = new ROT.Map.EllerMaze(width,height);
          maze.create(function(x0,y0,val) {
            if (val===0) {
              HTomb.World.tiles[z][x+x0][y+y0] = HTomb.Tiles.FloorTile;
              HTomb.World.validate.dirtify(x+x0,y+y0,z);
            }
          });
        }
        tries = tries+1;
      }
    }
    HTomb.World.validate.clean();
  }

  function placeMinerals(options) {
    options = options || {};
    var template = options.template || "IronVein";
    let nodeChance = options.p || 0.001;
    let bottom = 15;
    let oreChance = 0.5;
    function nonsolids(x,y,z) {return HTomb.World.tiles[z][x][y].solid!==true;}
    for (let z=bottom; z<highest; z++) {
      for (let i=0; i<LEVELW*LEVELH*nodeChance; i++) {
        let dx = HTomb.Utils.dice(1,LEVELW-4);
        let dy = HTomb.Utils.dice(1,LEVELH-4);
        for (let x=dx-1; x<=dx+1; x++) {
          for (let y=dy-1; y<=dy+1; y++) {
            if (HTomb.Tiles.countNeighborsWhere(x,y,z,nonsolids)>0) {
              continue;
            } else if (Math.random()<oreChance) {
              HTomb.World.covers[z][x][y] = HTomb.Covers[template];
            }
          }
        }
      }
    }
  };

  function growPlants(options) {
    options = options || {};
    var template = options.template || "Shrub";
    var p = options.p || 0.01;
    var n = options.n || 3;
    var born = options.born || [0,0.1,0.2,0.3,0.5,0.5,0.8,0.8];
    var survive = options.survive || [0.9,0.8,0.8,0.7,0.7,0.2,0.2,0.2];
    var cells = new HTomb.Cells({
      born: born,
      survive: survive
    });
    cells.randomize(p);
    cells.iterate(n);
    cells.apply(function(x,y,val) {
      if (val) {
        var z = HTomb.Tiles.groundLevel(x,y);
        var t = HTomb.World.covers[z][x][y];
        var plant;
        if (t!==HTomb.Covers.NoCover && t.liquid) {
          if (Math.random()<0.5) {
            plant = HTomb.Things.Seaweed();
            placement.stack(plant,x,y,z);
          }
        } else {
          plant = HTomb.Things[template]();
          placement.stack(plant,x,y,z);
        }
      }
    });
  }

  function grassify() {
    var tiles = HTomb.World.tiles;
    var squares;
    var square;
    var slope;
    for (var x=0; x<LEVELW; x++) {
      for (var y=0; y<LEVELH; y++) {
        var z = HTomb.Tiles.groundLevel(x,y);
        //if (tiles[z][x][y]===HTomb.Tiles.FloorTile && HTomb.World.covers[z][x][y]===HTomb.Covers.NoCover) {
        if (HTomb.World.covers[z][x][y]===HTomb.Covers.NoCover) {
          HTomb.World.covers[z][x][y] = HTomb.Covers.Grass;
        }
      }
    }
  }
  function notOwned() {
    for (var fe in HTomb.World.features) {
      HTomb.World.features[fe].owner = null;
    }
    for (var it in HTomb.World.items) {
      var items = HTomb.World.items[it];
      items.forEach(function(e,a,i) {e.item.owner=null});
    }
  }
  function placePlayer() {
    var placed = false;
    let padding = 25;
    // place the player near some graves
    let graves = HTomb.Utils.where(HTomb.World.features, function(v,k,o) {
      if (v.template!=="Tombstone") {
        return false;
      }
      let c = HTomb.Utils.decoord(k);
      let x = c[0];
      let y = c[1];
      let z = c[2];
      // look for graves that are not near the edge
      if (x<padding || x>LEVELW-padding || y<padding || y>LEVELH-padding) {
        return false;
      }
      // make sure there are at least two graves close together
      let n = HTomb.Tiles.countNeighborsWhere(x,y,z,function(x1,y1,z1) {
        let f = HTomb.World.features[coord(x1,y1,z1)];
        if (f && f.template==="Tombstone") {
          return true;
        }
        return false;
      });
      if (n>=1) {
        return true;
      }
      return false;
    });
    if (graves.length===0) {
      alert("no valid starting locations!");
      return;
    }
    while (placed===false) {
      HTomb.Utils.shuffle(graves);
      let grave = graves[0];
      let xdiff = HTomb.Utils.dice(2,6)-7;
      let ydiff = HTomb.Utils.dice(2,6)-7;
      let x = grave.x+xdiff;
      let y = grave.y+ydiff;
      if (x<=0 || y<=0 || x>=LEVELW-1 || y>=LEVELH-1) {
        continue;
      }
      let z = HTomb.Tiles.groundLevel(x,y);
      // do not displace another creature
      if (HTomb.World.creatures[coord(x,y,z)]) {
        continue;
      }
      // do not place under water
      if (HTomb.World.covers[z][x][y].liquid) {
        continue;
      }
      // do not place on a different Z level from the graves
      if (z!==grave.z) {
        continue;
      }
      // do not place directly on top of a tombstone
      let f = HTomb.World.features[coord(x,y,z)];
      if (f && f.template==="Tombstone") {
        continue;
      }
      var p = HTomb.Things.Necromancer();
      HTomb.Things.Player().addToEntity(p);
      p.place(x,y,z);
      if (p.sight) {
        HTomb.FOV.findVisible(p.x, p.y, p.z, p.sight.range);
      }
      placed = true;
    }
  }

  function placeCritters(p) {
    p = p || 0.01;
    var landCritters = ["Bat","Spider"];
    var waterCritters = ["Fish"];
    var template;
    for (var x=1; x<LEVELW-1; x++) {
      for (var y=1; y<LEVELH-1; y++) {
        if (Math.random()<p) {
          var z = HTomb.Tiles.groundLevel(x,y);
          var t = HTomb.World.covers[z][x][y]
          if (t.liquid) {
            template = HTomb.Utils.shuffle(waterCritters)[0];
          } else {
            template = HTomb.Utils.shuffle(landCritters)[0];
          }
          var critter = HTomb.Things[template]();
          placement.stack(critter,x,y,z);
        }
      }
    }
  }

  function GreuterGenerator(table) {
  	// convex vertices that can spawn new shapes
  	this.vertices = [];
  	// all squares visited by this Greuter generation process
  	this.used = {};
  	// list of shapes generated by this Greuter process
  	this.shapes = [];
  	// frequency table of Greuter shapes
  	this.table = table;
  }
  GreuterGenerator.prototype.getVertex = function() {
  	HTomb.Utils.shuffle(this.vertices);
  	return this.vertices.pop();
  }
  GreuterGenerator.prototype.spawn = function(x,y,n,padding) {
  	// not sure how to propagate this
  	padding = padding || 10;
  	// choose how many shapes to spawn
  	n = n || HTomb.Utils.dice(2,3)-1;
    let that = this;
  	let _recurse = function(x,y) {
  		// pick a new shape from the frequency table
  		//let shape = new (this.table.roll())(this);
      let shape = new GreuterShape(that);
  		// generate the shape centered around x,y
  		shape.generate(x,y,that);
  		// decrement and re-run
  		if (n>0) {
  			n-=1;
  			// grab a random vertex
  			let v = that.getVertex();
  			_recurse(v[0],v[1]);
  		}
  	};
  	// kick off recursive generation
  	_recurse(x,y);
  	// reset the vertices list
  	this.vertices = [];
  };
  GreuterGenerator.prototype.resolve = function() {
  	this.resolved = {};
  	this.shapes.sort(function(a,b) {
  		if (a.zindex<b.zindex) {
  			return +1;
  		} else if (a.zindex>b.zindex) {
  			return -1;
  		} else {
  			return 0;
  		}
  	});
  	for (let i=0; i<this.shapes.length; i++) {
  		this.shapes[i].resolve(this);
  	}
  }

  function GreuterShape(gen) {
  	gen.shapes.push(this);
  	this.id = gen.shapes.length;
  }
  GreuterShape.prototype.generate = function(x0,y0,gen) {
    let w = (HTomb.Utils.dice(1,3)+1)*2;
    let h = (HTomb.Utils.dice(1,3)+1)*2;
  	this.zindex = 0;
  	this.graves = [];
  	this.walls = [];
  	for (let x=0; x<w; x++) {
  		for (let y=0; y<h; y++) {
  			let x1 = x0+x-w/2;
  			let y1 = y0+y-h/2;
        if (x1>0 && x1<LEVELW-1 && y1>0 && y1<LEVELH-1) {
          let z = HTomb.Tiles.groundLevel(x1,y1);
  			  //if (x1%2===0) {
  					this.graves.push([x1,y1,z]);
          //}
          if ((x===0 || x===w-1) && (y===0 || y===h-1)) {
    				if (gen.used[coord(x1,y1,z)]===undefined) {
    					gen.vertices.push([x1,y1]);
    				}
    			}
    			gen.used[coord(x1,y1)] = this.id;
  			}
  		}
  	}
  };
  GreuterShape.prototype.resolve = function(gen) {
  	for (let i=0; i<this.graves.length; i++) {
  		let g = this.graves[i];
  		if (gen.resolved[coord(g[0],g[1],g[2])]===undefined) {
  			//HTomb.Things.Tombstone().place(g[0],g[1],g[2]);
        HTomb.Things.Bloodstone().place(g[0],g[1],g[2]);
  			gen.resolved[coord(g[0],g[1],g[2])] = this.id;
  		}
  	}
  	for (let i=0; i<this.walls.length; i++) {
  		let w = this.walls[i];
  		if (gen.resolved[coord(w[0],w[1],w[2])]===undefined) {
  			HTomb.World.tiles[w[2]][w[0]][w[1]] = HTomb.Tiles.UpSlopeTile;
  			HTomb.World.tiles[w[2]+1][w[0]][w[1]] = HTomb.Tiles.DownSlopeTile;
  			gen.resolved[coord(w[0],w[1],w[2])] = this.id;
  		}
  	}
  };



  // faster to track this as globally rather than in grass



  return HTomb;
})(HTomb);
