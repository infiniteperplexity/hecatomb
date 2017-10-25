// ****** This module implements Components, which are the basic units of functionality for creatures, items, and features
HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var NLEVELS = HTomb.Constants.NLEVELS;
  var coord = HTomb.Utils.coord;

  let Component = HTomb.Things.Component;
  // The Movement bomponent allows the creature to move
  Component.extend({
    template: "Movement",
    name: "movement",
    // flags for different kinds of movement
    walks: true,
    climbs: true,
    displaceCreature: function(cr,x,y,z) {
      //x y z are optional arguments
      x = (x!==undefined) ? x : this.entity.x;
      y = (y!==undefined) ? y : this.entity.y;
      z = (z!==undefined) ? z : this.entity.z;
      let x1 = cr.x;
      let y1 = cr.y;
      let z1 = cr.z;
      cr.remove();
      this.stepTo(x1,y1,z1);
      if (cr.movement) {
        cr.movement.stepTo(x,y,z);
      } else {
        cr.place(x,y,z);
      }
    },
    stepTo: function(x,y,z) {
      this.displaced = null;
      this.entity.place(x,y,z);
      let cost = 16;
      if (HTomb.World.items[coord(x,y,z)] && !this.flies) {
        cost = 25;
      }
      if (this.entity.actor) {
        this.entity.actor.acted = true;
        this.entity.actor.actionPoints-=cost;
      }
      HTomb.Events.publish({type: "Step", creature: this.entity});
    },
    // If the square is crossable and unoccupied
    canPass: function(x,y,z) {
      if (this.canMove(x,y,z)===false) {
        return false;
      }
      var square = HTomb.Tiles.getSquare(x,y,z);
      if (square.creature) {
        return false;
      }
      return true;
    },
    boundMove: function() {
      return this.canMove.bind(this);
    },
    // If the square is crossable for this creature
    canMove: function(x,y,z,x0,y0,z0) {
      if (x<0 || x>=LEVELW || y<0 || y>=LEVELH || z<0 || z>=NLEVELS) {
        return false;
      }
      var c = coord(x,y,z);
      ////Passability independent of position
      // can't go through solid terrain
      var terrain = HTomb.World.tiles[z][x][y];
      if (terrain.solid===true && this.phases!==true) {
        return false;
      }
      // can't walk over a pit
      if (terrain.fallable===true && this.flies!==true) {
        return false;
      }
      var cover = HTomb.World.covers[z][x][y];
      if (cover!==HTomb.Covers.NoCover && cover.liquid && this.swims!==true) {
        return false;
      }
      let e = this.entity;
      // can't go through a zone your master forbids
      if (e.minion) {
        let task = HTomb.World.tasks[c];
        if (task && task.template==="ForbidTask" && task.assigner===e.minion.master) {
          return false;
        }
      }
      // can't go through solid feature
      var feature = HTomb.World.features[c];
      if (feature && feature.solid===true && this.phases!==true) {
        if (!feature.owner || feature.owner.actor.team!==e.actor.team) {
          return false;
        }
      }
      ////Passability dependent on position
      var dx = x-(x0 || e.x);
      var dy = y-(y0 || e.y);
      var dz = z-(z0 || e.z);
      // a way to check whether the square itself is allowed, for certain checks
      if (dx===0 && dy===0 && dz===0) {
        return true;
      }
      // non-flyers can't climb diagonally
      if (this.flies!==true && dz!==0 && (dx!==0 || dy!==0)) {
        return false;
      // non-flyers need a slope in order to go up
      }
      var t = HTomb.World.tiles[z-dz][x-dx][y-dy];
      if (dz===+1 && this.flies!==true && t.zmove!==+1) {
        return false;
      }
      var tu = HTomb.World.tiles[z+1-dz][x-dx][y-dy];
      // non-phasers can't go through a ceiling
      if (dz===+1 && this.phases!==true && tu.fallable!==true && tu.zmove!==-1) {
        return false;
      }
      // non-phasers can't go down through a floor
      if (dz===-1 && t.fallable!==true && t.zmove!==-1 && this.phases!==true) {
        return false;
      }
      if (this.walks===true) {
        return true;
      }
      if (this.flies===true) {
        return true;
      }
      if (this.swims===true && cover && cover.liquid) {
        return true;
      }
      return false;
    }
  });

  return HTomb;
})(HTomb);
