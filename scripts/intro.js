HTomb = (function(HTomb) {
  "use strict";
  // alright

  let width = 64;
  let height = 30;
  let z = 1;
  let nzombies = 6;
  let zombies = null;
  let necro = null;
  let throne = null;
  HTomb.Intro = {};
  HTomb.Intro.setup = function() {
    HTomb.World.reset();
    zombies = [];
    for (let x=1; x<width+1; x++) {
      for (let y=1; y<height+1; y++) {
        HTomb.World.tiles[z][x][y] = HTomb.Tiles.WallTile;
      }
    }
    let roomWidth = 9;
    let roomHeight = 7;
    for (let x=0; x<roomWidth; x++) {
      for (let y=0; y<roomHeight; y++) {
        HTomb.World.tiles[z][Math.ceil(x+width/2-roomWidth/2)][Math.ceil(y+height/2-roomHeight/2)] = HTomb.Tiles.FloorTile;
      }
    }
    necro = HTomb.Things.Necromancer();
    // only works well if height and width are odd...let's leave that for now
    let maze = new ROT.Map.EllerMaze(width-1,height-1);
    maze.create(function(x,y,val) {
      if (val===0 && HTomb.World.tiles[z][x+1][y+1]===HTomb.Tiles.WallTile) {
        HTomb.Things.DigTask({assigner: necro}).place(x+1,y+1,z);
      }
      //if (val===0) {
      //  HTomb.World.tiles[z][x+1][y+1] = HTomb.Tiles.FloorTile;
      //}
    });
    necro.place(Math.floor(width/2),Math.floor(height/2),z);
    HTomb.necroTest = necro;
    throne = HTomb.Things.Throne();
    throne.place(Math.floor(width/2),Math.floor(height/2),z);
    HTomb.Things.FakeTorch().place(throne.x+3, throne.y-1, z);
    HTomb.Things.FakeTorch().place(throne.x-3, throne.y-1, z);
    HTomb.Things.Pentagram().place(throne.x, throne.y+2, z);
    for (let i=0; i<nzombies; i++) {
      let zombie = HTomb.Things.Zombie();
      HTomb.Things.Minion().addToEntity(zombie);
      necro.master.addMinion(zombie);
      zombies.push(zombie);
      let xyz = HTomb.Tiles.randomEmptyNeighbor(necro.x,necro.y,necro.z);
      zombie.place(xyz[0],xyz[1],xyz[2]);
    }
    let ores = ["GoldOre","Bloodstone","Moonstone","Jade","IronOre","Corpse"];
    for (let i=0; i<100; i++) {
      let x = HTomb.Utils.dice(1,width);
      let y = HTomb.Utils.dice(1,height);
      if (HTomb.World.tiles[z][x][y]===HTomb.Tiles.WallTile) {
        let template = ores[HTomb.Utils.dice(1,ores.length)-1];
        HTomb.Things[template]().place(x,y,z);
      }
    }
  };
  HTomb.Intro.getTiles = function() {
    let grid = [];
    for (let y=1; y<height; y++) {
      let line = [];
      for (let x=1; x<width; x++) {
        let t = HTomb.World.tiles[z][x][y];
        let crd = HTomb.Utils.coord(x,y,z);
        let c = HTomb.World.creatures[crd];
        let f = HTomb.World.features[crd];
        let items = HTomb.World.items[crd]
        if (c) {
          line.push([c.symbol, c.fg, "black"]);
        } else if (f) {
          line.push([f.symbol, f.fg || "yellow","black"]);
        } else if (items && HTomb.World.tiles[z][x][y]===HTomb.Tiles.FloorTile) {
          let item = items.head();
          line.push([item.symbol, item.fg || "gray", "black"]);
        } else {
          line.push([t.symbol, t.fg, "black"]);
        }
      }
      grid.push(line);
    }
    return grid;
  };
  HTomb.Intro.tick = function() {
    if (necro.master.taskList.length===0) {
      HTomb.Intro.setup();
    }
    if (HTomb.Utils.dice(1,4)===1) {
      necro.ai.patrol(throne.x, throne.y, throne.z, {min: 1, max: 3});
    }
    let tasks = HTomb.Utils.shuffle(necro.master.taskList);
    HTomb.Utils.shuffle(zombies);
    for (let i=0; i<zombies.length; i++) {
      let zombie = zombies[i];
      if (zombie.worker.task===null) {
        for (let j=0; j<tasks.length; j++) {
          let task = tasks[j];
          if (task.assignee===null && zombie.worker.task===null && task.canAssign(zombie)) {
            task.assignTo(zombie);
          }
        }
      }
      zombie.ai.acted = false;
      zombie.ai.act();
    }
  };

  return HTomb;
})(HTomb);
