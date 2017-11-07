HTomb = (function(HTomb) {
  "use strict";
  let coord = HTomb.Utils.coord;

  // Might like to have animations

  let Entity = HTomb.Things.Entity;

  let Structure = Entity.extend({
    template: "Structure",
    name: "structure",
    owner: null,
    height: 3,
    width: 3,
    x: null,
    y: null,
    z: null,
    placed: false,
    squares: [],
    features: [],
    symbols: [],
    fgs: [],
    ingredients: [],
    tooltip: "A generic structure tooltip.",
    setSymbol: function(i,sym) {
      this.features[i].symbol = sym;
    },
    setColor: function(i,fg) {
      this.features[i].fg = fg;
    },
    onDefine: function(args) {
      if ((args.ingredients===undefined || args.ingredients.length===0) && args.height!==null && args.width!==null) {
        let ings = [];
        let h = args.height;
        let w = args.width;
        for (let i=0; i<w*h; i++) {
          ings.push({});
        }
        HTomb.Things[this.template].ingredients = ings;
      }
      HTomb.Things.Feature.extend({
        template: args.template+"Feature",
        name: args.name,
        bg: args.bg,
        Components: {
          Fixture: {}
        }
      });
    },
    spawn: function(args) {
      let o = Entity.spawn.call(this,args);
      o.features = [];
      o.options = HTomb.Utils.copy(o.options);
      return o;
    },
    place: function(x,y,z,args) {
      Entity.place.call(this,x,y,z,args);
      this.owner.owner.structures.push(this);
      this.placed = true;
      return this;
    },
    validPlace: function(x,y,z) {
      let xs = [];
      let yz = [];
      for (let i=0; i<this.width; i++) {
        xs.push(x+i-Math.floor(this.width));
      }
      for (let j=0; j<this.height; j++) {
        ys.push(y+j-Math.floor(this.height));
      }
      for (let i of xs) {
        for (let j of ys) {
          if (HTomb.World.features[coord(i,j,z)]) {
            return false;
          } else if (HTomb.World.Tiles[z][i][j]!==HTomb.Tiles.FloorTile) {
            return false;
          } else {
            return true;
          }
        }
      }
    },
    isPlaced: function() {
      return this.placed;
    },
    remove: function() {
      this.owner.owner.structures.splice(this.owner.owner.structures.indexOf(this),1);
      HTomb.Events.unsubscribeAll(this);
      this.placed = false;
      Entity.remove.call(this);
    },
    highlight: function(bg) {
      for (let i=0; i<this.features.length; i++) {
        this.features[i].highlightColor = bg;
      }
    },
    unhighlight: function() {
      for (let i=0; i<this.features.length; i++) {
        if (this.features[i].highlightColor) {
          delete this.features[i].highlightColor;
        }
      }
    },
    commandsText: function() {
      return;
    },
    headerText: function() {
      return;
    },
    structureText: function() {
      let txt = [
        "%c{orange}**Esc: Done.**",
        "Wait: NumPad 5 / Control+Space.",
        "Click / Space: Select.",
        "Enter: Toggle Pause.",
        " ",
        "%c{yellow}Structure: "+this.describe({capitalized: true, atCoordinates: true})+".",
      ];
      if (this.commandsText()) {
        txt = txt.concat(this.commandsText());
      }
      if (this.components) {
        for (let component of this.components) {
          if (component.commandsText) {
            txt = txt.concat(component.commandsText());
          }
        }
      }
      txt = txt.concat(["Tab: Next structure."," "]);
      if (this.components) {
        for (let component of this.components) {
          if (component.detailsText) {
            txt = txt.concat(component.detailsText());
          }
        }
      }
      return txt;
    },
    totalIngredients: function() {
      let ings = {};
      if (HTomb.Debug.noingredients) {
        return ings;
      }
      for (let i=0; i<this.ingredients.length; i++) {
        let ingr = this.ingredients[i];
        for (let ing in ingr) {
          ings[ing] = ings[ing] || 0;
          ings[ing] += ingr[ing];
        }
      }
      return ings;
    },
    neededIngredients: function() {
      if (this.ingredients.length===0) {
        return {};
      }
      let ings = {};
      if (HTomb.Debug.noingredients) {
        return {};
      }
      for (let i=0; i<this.ingredients.length; i++) {
        if (this.features[i]) {
          continue;
        } else {
          let x = this.squares[i][0];
          let y = this.squares[i][1];
          let z = this.squares[i][2];
          let f = HTomb.World.features[coord(x,y,z)];
          if (f && f.template==="IncompleteFeature" && f.makes===this.template+"Feature") {
            continue;
          }
        }
        let ingr = this.ingredients[i];
        for (let ing in ingr) {
          ings[ing] = ings[ing] || 0;
          ings[ing] += ingr[ing];
        }
      }
      return ings;
    }
  });
  // Add commands with similar component dynamically
  for (let command of ["up","down","left","right","more","less","cancel","choice"]) {
    Structure[command+"Command"] = function(args) {
      if (this.components) {
        for (let component of this.components) {
          if (component[command+"Command"]) {
            component[command+"Command"](args);
          }
        }
      }
    };
  }

return HTomb;
})(HTomb);