

Behaviors.extend({
  template: "Craftable",
  name: "craftable",
  labor: 10,
  ingredients: {},
  dismantle: function() {

  }
});

Behaviors.extend({
  template: "Fixture",
  name: "fixture",
  unplacedFg: null,
  unplacedSymbol: null,
  makeLabor: 10,
  placeLabor: 5,
  ingredients: {},
  dismantle: function() {}
});

Items.extend({
  template: "UnplacedFixture",
  name: "unplaced fixture",
  symbol: "\u25AB",
  fg: "#BB9922",
  makes: null,
  labor: 5
});

Behaviors.extend({
  template: "Harvestable",
  name: "harvestable",
  yields: {},
  harvest: function() {

  }
})




// the initial seed
Math.seed = 6;
 
// in order to work 'Math.seed' must NOT be undefined,
// so in any case, you HAVE to provide a Math.seed
Math.seededRandom = 

HTomb.Utils.seed = 6;
