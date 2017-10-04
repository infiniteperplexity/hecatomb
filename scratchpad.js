

Behaviors.extend({
  template: "Craftable",
  name: "craftable",
  labor: 10,
  ingredients: {},
  onSetUp: function() {
    for (let t in HTomb.Things.templates) {
      let template = HTomb.Things.templates[t];
      if (template.Behaviors && template.Behaviors.Craftable) {
        let item = HTomb.Things.templates.Item.extend({
          template: template.template+"Item",
          name: template.name
        });
        item.fg = template.fg || "white";
        item.symbol = template.symbol || "?";
        item.ingredients = template.Behaviors.Craftable.ingredients || this.ingredients;
        item.labor = template.Behaviors.Craftable.labor || this.labor;
        template.ingredients = {};
        template.ingredients[template.template+"Item"] = 1;
      }
    }
  }
});