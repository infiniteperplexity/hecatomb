

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

//fetch a specific item
HTomb.Types.defineRoutine({
    template: "FetchItem",
    name: "fetch item",
    act: function(ai, args) {
      args = args || {};
      let cr = ai.entity;
      let claims = false;
      // if this is a minion of the player, be ready to stake claims
      if (cr.minion && cr.minion.master === HTomb.Player) {
        claims = true;
      }
      if (cr.inventory===undefined) {
        return;
      }
      // this mostly only matters for claims and defaults
      let task = args.task || null;
      let item = args.item;
      if (item===undefined && task) {
        item = task.item || null;
      }
      if (item===null) {
        return;
      }
      if (!item.isPlaced() || !HTomb.Tiles.isReachableFrom(item.x,item.y,item.z,cr.x,cr.y,cr.z,{
            canPass: cr.movement.boundMove(),
            searcher: cr,
            searchee: item,
            searchTimeout: 10
        })) {
        task.cancel();
      }
      let t = cr.ai.target;
      // if we are standing on it, pick it up
      if (cr.x===t.x && cr.y===t.y && cr.z===c.z) {
        cr.inventory.pickup(item);
        cr.ai.target = null;
      // otherwise walk toward it
      } else {
        cr.ai.walkToward(t.x,t.y,t.z, {
           searcher: cr,
           searchee: t,
           searchTimeout: 10
        });
      }
    }
  });
