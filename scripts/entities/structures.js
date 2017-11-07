HTomb = (function(HTomb) {
  "use strict";
  let coord = HTomb.Utils.coord;

  // Might like to have animations

  let Structure = HTomb.Things.Structure;

  Structure.extend({
    template: "Workshop",
    name: "workshop",
    symbols: ["\u25AE","/","\u2699","\u2261","\u25AA",".","\u2692",".","\u25A7"],
    bg: "#665555",
    tooltip: "(The workshop produces basic goods.)",
    fgs: ["#BB9922","#BB9922",HTomb.Constants.FLOORFG,"#BB9922","#BB9922",HTomb.Constants.FLOORFG,HTomb.Constants.FLOORFG,HTomb.Constants.FLOORFG,"#BB9922"],
     ingredients: [
       {WoodPlank: 1},
       {},
       {WoodPlank: 1},
       {},
       {Rock: 2},
       {},
       {WoodPlank: 1},
       {},
       {WoodPlank: 1}
    ],
    Components: {
      Producer: {
        makes: [
          "WorkAxe",
          "UnplacedDoor",
          "UnplacedTorch",
          "UnplacedSpearTrap"
        ]
      },
      StructureLight: {}
    }
  });

  Structure.extend({
    template: "Stockpile",
    name: "stockpile",
    tooltip: "(The stockpile stores and tallies basic resources.)",
    bg: "#444455",
    symbols: [".",".","\u25AD","\u2234","#","\u2630","\u25A7",".","\u25AF"],
    fgs: ["#BB9922",HTomb.Constants.FLOORFG,HTomb.Constants.FLOORFG,"#BB9922",HTomb.Constants.FLOORFG,"#BB9922",HTomb.Constants.FLOORFG,HTomb.Constants.FLOORFG,"#BB9922"],
    ingredients: [
      {Rock: 1},
      {},
      {Rock: 1},
      {},
      {WoodPlank: 2},
      {},
      {Rock: 1},
      {},
      {Rock: 1}
    ],
    Components: {
      Storage: {
        stores: ["WoodPlank","Rock","TradeGoods"]
      },
      StructureLight: {}
    }
  });

  Structure.extend({
    template: "BlackMarket",
    name: "black market",
    tooltip: "(The black market lets you trade for goods from faraway lands.)",
    symbols: ["\u00A3",".",".",".","\u2696","$","\u00A2","\u20AA","\u00A4"],
    fgs: ["#552222",HTomb.Constants.FLOORFG,HTomb.Constants.FLOORFG,HTomb.Constants.FLOORFG,"#888844","#888844","#225522","#333333","#222266"],
    bg: "#555544",
    offers: [],
    trades: [
      {
        price: {
          Rock: 3
        },
        offer: {
          TradeGoods: 1
        }
      },
      {
        price: {
          WoodPlank: 3
        },
        offer: {
          TradeGoods: 1
        }
      },
      {
        price: {
          TradeGoods: 1
        },
        offer: {
          WoodPlank: 1,
          Rock: 1
        },
        turns: 50
      }
    ],
    task: null,
    awaiting: [],
    onPlace: function() {
      HTomb.Events.subscribe(this,"TurnBegin");
    },
    onTurnBegin: function() {
      if (ROT.RNG.getUniformInt(1,100)===1) {
        let MAXOFFERS = 4;
        this.trades = HTomb.Utils.shuffle(this.trades);
        this.offers.unshift(this.trades[0]);
        if (this.offers.length>MAXOFFERS) {
          this.offers.pop();
        }
      }
      let i = 0;
      while (i<this.awaiting.length) {
        let a = this.awaiting[i];
        a.turns-=1;
        if (a.turns<=0) {
          for (let ing in a.offer) {
            let item = HTomb.Things[ing].spawn({n: a.offer[ing]});
            item.place(this.x, this.y, this.z);
            this.awaiting.splice(i,1);
          }
        } else {
          i+=1;
        }
      }
    },
    choiceCommand: function(i) {
      if (this.offers.length>i) {
        if (this.task===null || confirm("Really cancel unfulfilled trade?")) {
          let o = this.offers[i];
          let ings = o.price;
          let task = HTomb.Things.TradeTask({
            assigner: this.owner,
            structure: this,
            ingredients: ings,
            offer: o.offer,
            turns: (o.turns!==undefined) ? o.turns : HTomb.Things.TradeTask.turns
          });
          if (this.task) {
            this.task.cancel();
          }
          this.task = task;
          task.place(this.x, this.y, this.z);
          this.offers.splice(i,1);
          if (HTomb.Debug.noingredients || Object.keys(ings).length===0) {
            task.workOnTask();
          }
        }
      }
    },
    cancelCommand: function() {
      if (this.task && confirm("Really cancel unfulfilled trade?")) {
        this.task.cancel();
      }
    },
    structureText: function() {
      let txt = [
        "%c{orange}**Esc: Done.**",
        "Wait: NumPad 5 / Control+Space.",
        "Click / Space: Select.",
        "Enter: Toggle Pause.",
        "a-z: Accept an offer.",
        " ",
        "%c{yellow}Structure: "+this.describe({capitalized: true, atCoordinates: true})+".",
      ];
      if (this.components) {
        for (let component of this.components) {
          if (component.commandsText) {
            txt = txt.concat(component.commandsText());
          }
        }
      }
      txt = txt.concat(["Tab: Next structure."," "]);
      txt.push(" ");
      txt.push("Offers:");
      let alphabet = 'abcdefghijklmnopqrstuvwxyz';
      for (let i=0; i<this.offers.length; i++) {
        let o = this.offers[i];
        let s = alphabet[i] + ") " + HTomb.Utils.listIngredients(o.price) + " : " + HTomb.Utils.listIngredients(o.offer);
        if (this.owner.owner.ownsAllIngredients(o.price)!==true) {
          s = "%c{gray}" + s;
        }
        txt.push(s);
      }
      txt.push(" ");
      txt.push("Gathering:");
      if (this.task) {
        txt.push(HTomb.Utils.listIngredients(this.task.ingredients));
      }
      txt.push(" ");
      txt.push("Awaiting:");
      for (let i=0; i<this.awaiting.length; i++) {
        let a = this.awaiting[i];
        txt.push("- " + HTomb.Utils.listIngredients(a.offer) + " (" + a.turns + " turns.)");
      }
      txt.push(" ");
      if (this.components) {
        for (let component of this.components) {
          if (component.detailsText) {
            txt = txt.concat(component.detailsText());
          }
        }
      }
      return txt;
    },
    ingredients: [
      {Rock: 1}, {}, {WoodPlank: 1},
      {}, {TradeGoods: 1}, {},
      {WoodPlank: 1}, {}, {Rock: 1}
    ],
    Components: {
      //Storage: {
      //  stores: ["TradeGoods"]
      //},
      StructureLight: {}
    }
  });

  Structure.extend({
    template: "Sanctum",
    name: "sanctum",
    tooltip: "(The sanctum grants you more sanity and researchable spells.)",
    symbols: ["\u2625",".","\u2AEF",".","\u2135",".","\u2AEF","\u2606","\u263F"],
    fgs: ["magenta",HTomb.Constants.FLOORFG,"cyan",HTomb.Constants.FLOORFG,"green",HTomb.Constants.FLOORFG,"yellow","red","orange"],
    bg: "#222244",
    // do we want some sort of mana activation thing?,
    Components: {
      Research: {
        choices: ["CondenseEctoplasm","PoundOfFlesh","StepIntoShadow"]
      },
      StructureLight: {}
    },
  });

  Structure.extend({
    template: "GuardPost",
    name: "guard post",
    defenseRange: 3,
    defenseBonus: 1,
    rallying: false,
    onPlace: function() {
      HTomb.Events.subscribe(this, "Act");
      HTomb.Events.subscribe(this, "Attack");
    },
    onAttack: function(event) {
      let v = event.target;
      let m = event.modifiers;
      if (v.minion && v.minion.master && v.minion.master===this.owner) {
        if (HTomb.Path.quickDistance(this.x, this.y, this.z, v.x, v.y, v.z)<s.defenseRange) {
          m.evasion = Math.max(m.evasion, this.defenseBonus);
        }
      }
      return event;
    },
    onAct: function(event) {
      if (this.rallying!==true) {
        return;
      }
      let actor = event.actor;
      if (!actor.entity.minion || actor.entity.minion.master!==this.owner) {
        return;
      }
      if (actor.acted===false) {
        actor.alert.act(actor);
      }
      if (actor.acted===false) {
        actor.patrol(this.x,this.y,this.z, {
          min: 0,
          max: this.defenseRange,
          searcher: actor.entity,
          searchee: this,
          searchTimeout: 10
        });
      }
    },
    highlight: function(bg) {
      Structure.highlight.call(this,bg);
      let z = this.z;
      for (let x=-3; x<=3; x++) {
        for (let y=-3; y<=3; y++) {
          if (HTomb.Path.quickDistance(this.x, this.y, z, this.x+x, this.y+y, z) <= 3) {
            HTomb.GUI.Panels.gameScreen.highlitTiles[coord(this.x+x,this.y+y,z)] = "#779944";
          }
        }
      }
    },
    commandsText: function() {
      let txt = "a) ";
      if (this.rallying===false) {
        txt+="rally defenders here.";
      } else {
        txt+="dismiss rallied defenders.";
      }
      return [txt];
    },
    choiceCommand: function(i) {
      if (i===0) {
        if (this.rallying) {
          this.rallying = false;
        } else {
          this.rallying = true;
        }
      }
    },
    unhighlight: function() {
      let z = this.z;
      for (let x=-3; x<=3; x++) {
        for (let y=-3; y<=3; y++) {
          if (HTomb.Path.quickDistance(this.x, this.y, z, this.x+x, this.y+y, z) <= 3) {
            if (HTomb.GUI.Panels.gameScreen.highlitTiles[coord(this.x+x,this.y+y,z)]) {
              delete HTomb.GUI.Panels.gameScreen.highlitTiles[coord(this.x+x,this.y+y,z)]; 
            }
          }
        }
      }
      Structure.unhighlight.call(this);
    },
    tooltip: "(The guard post warns of incoming attacks and provides a rallying point for defenders.)",
    ingredients: [
      {},{},{},
      {},{Rock:1,WoodPlank:1},{},
      {},{},{}
    ],
    symbols: ["\u2694",".","\u2658",".",".",".","\u2658",".","\u2694"],
    fgs: ["#BBBBBB",HTomb.Constants.FLOORFG,"#BBBBBB",HTomb.Constants.FLOORFG,HTomb.Constants.FLOORFG,HTomb.Constants.FLOORFG,"#BBBBBB",HTomb.Constants.FLOORFG,"#BBBBBB"],
    bg: "#555577",
    Components: {
      StructureLight: {}
    }
  });


return HTomb;
})(HTomb);