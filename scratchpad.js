
Component.extend({
    template: "Research",
    name: "research",
    choices: [],
    library: [],
    current: null,
    onPlace: function(x,y,z,args) {
      HTomb.Events.subscribe(this, "TurnBegin");
    },
    // need to handle ingredients differently...
    choiceCommand: function(i) {
      let choices = this.choices;
      choices = choices.filter(e => this.entity.owner.researcher.researched.indexOf(e)===-1);
      if (i<choices.length) {
        if (this.current) {
          if (confirm("Really cancel current research?")) {
            this.current.cancel();
          } else {
            return;
          }
        }
        this.current = HTomb.Things[choices[i]].spawn({
          researcher: this.entity.owner,
          structure: this.template,
          turns: HTomb.Things[choices[i]].turns
        });
        // ugh...
      }
    },
    onFetch: function() {
      this.current.fulfilled = true;
    },
    cancelCommand: function() {
      if (this.current && confirm("Really cancel research?")) {
        let current = this.current;
        this.current = null;
        current.cancel();
      }
    },
    onTurnBegin: function() {
      if (this.current && this.current.fulfilled) {
        this.current.turns-=1;
        if (this.current.turns<=0) {
          this.current.complete();
        }
      }
    },
    commandsText: function() {
      return [
        "a-z: Begin research on lore.",
        "Delete: Cancel current research."
      ];
    },
    detailsText: function() {
      let txt = ["Research choices:"];
      let alphabet = "abcdefghijklmnopqrstuvwxyz";
      let choices = this.choices;
      choices = choices.filter(e => this.entity.owner.researcher.researched.indexOf(e)===-1);
      for (let i=0; i<choices.length; i++) {
        let choice = HTomb.Things[choices[i]];
        let msg = choice.name + " " + HTomb.Utils.listIngredients(choice.ingredients);
        if (this.entity.owner.owner.ownsAllIngredients(choice.ingredients)!==true) {
          msg = "%c{gray}" + msg;
        }
        msg = alphabet[i] + ") " + msg;
        txt.push(msg);
      }
      txt.push(" ");
      txt.push("Researching:");
      if (this.current===null) {
        txt.push("(nothing)");
      } else {
        txt.push("- " + this.current.name + " (" + this.current.turns + " turns.)");
      }
      return txt;
    }
  });


  Task.extend({
    template: "StructureTask",
    name: "need to modify",
    bg: "#773366",
    structure: null,
    workRange: 1,
    priority: 2,
    ingredients: {},
    validTile: function(x,y,z) {
      if (HTomb.World.explored[z][x][y]!==true) {
        return false;
      }
      if (HTomb.World.tiles[z][x][y]!==HTomb.Tiles.FloorTile) {
        return false;
      }
      let f = HTomb.World.features[coord(x,y,z)];
      if (f && f.structure && f.structure===this.structure) {
        return true;
      }
      else {
        return false;
      }
    },
    workOnTask: function() {
      this.expend();
      this.finish();
      this.complete();
    },
    finish: function() {
      this.structure.onFetch(this);
    },
    onDespawn: function() {
      if (this.structure) {
        this.structure.task = null;
      }
    }
  });