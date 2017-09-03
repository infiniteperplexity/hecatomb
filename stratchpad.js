//conventions...
//....an items member should always be called this.items
//...an item should call the items object it belongs to this.container
class Items extends Array {
  push: function(item) {
    // does this properly handle full stacks
    if (item.container) {
      item.container.remove(item);
    }
    let n = Math.min(item.n, this.canFit(item));
    let exists = false;
    for (let it of this) {
      if (it.name===item.name) {
        it.n+=n;
        exists = true;
        break;
      }
    }
    if (exists) {
      item.n-=n;
      if (item.n===0) {
        item.despawn();
        item = null;
      }
    } else {
      this.push(item);
      item.container = this;
      return null;
    }
  }
  canFit: function(item) {
    // for now
    return true;
  }

    containsAny: function(template) {
      for (var i=0; i<this.items.length; i++) {
        if (this.items[i].template===template) {
          return true;
        }
      }
      return false;
    },
    countAll: function(template) {
      var tally = 0;
      for (var i=0; i<this.items.length; i++) {
        if (this.items[i].template===template) {
          tally+=this.items[i].item.n;
        }
      }
      return tally;
    },
    getFirst: function(template) {
      for (var i=0; i<this.items.length; i++) {
        if (this.items[i].template===template) {
          return this.items[i];
        }
      }
      return null;
    },
    getLast: function(template) {
      for (var i=this.items.length-1; i>=0; i--) {
        if (this.items[i].template===template) {
          return this.items[i];
        }
      }
      return null;
    },
    takeOne: function(i_or_t) {
      if (typeof(i_or_t)!=="string" && i_or_t.template) {
        i_or_t = i_or_t.template;
      }
      if (HTomb.Things.templates[i_or_t].stackable!==true) {
        var first = this.getFirst(i_or_t);
        return this.remove(first);
      } else {
        var last = this.getLast(i_or_t);
        if (last.item.n===1) {
          this.remove(last);
          return last;
        } else {
          last.item.n-=1;
          var single = HTomb.Things[last.template]();
          single.item.n = 1;
          return single;
        }
      }
    },
    take: function(i_or_t, n) {
      n = n || 1;
      if (typeof(i_or_t)!=="string" && i_or_t.template) {
        i_or_t = i_or_t.template;
      }
      let ing = {};
      ing[i_or_t] = n;
      if (this.hasAll(ing)!==true) {
        return false;
      }
      if (HTomb.Things.templates[i_or_t].Behaviors.Item.stackable!==true) {
        var first = this.getFirst(i_or_t);
        return this.remove(first);
      } else {
        var last = this.getLast(i_or_t);
        if (last.item.n<=n) {
          this.remove(last);
          return last;
        } else {
          last.item.n-=n;
          var taken = HTomb.Things[last.template]();
          taken.item.n = n;
          return taken;
        }
      }
    },
    takeSome: function(i_or_t, n) {
      n = n || 1;
      if (typeof(i_or_t)!=="string" && i_or_t.template) {
        i_or_t = i_or_t.template;
      }
      let ing = {};
      ing[i_or_t] = n;
      if (this.hasAll(ing)!==true) {
        n = this.countAll(i_or_t);
      }
      if (HTomb.Things.templates[i_or_t].Behaviors.Item.stackable!==true) {
        var first = this.getFirst(i_or_t);
        this.remove(first);
        return (first);
      } else {
        var last = this.getLast(i_or_t);
        if (last.item.n<=n) {
          this.remove(last);
          return last;
        } else {
          last.item.n-=n;
          var taken = HTomb.Things[last.template]();
          taken.item.n = n;
          return taken;
        }
      }
    },
    remove: function(item) {
      var indx = this.items.indexOf(item);
      if (indx>-1) {
        item.item.container = null;
        this.items.splice(indx,1);
        // should this only happen if it's on the ground?
        if (typeof(this.heldby)==="number") {
          item.remove();
          if (this.items.length===0) {
            delete HTomb.World.items[this.heldby];
            this.despawn();
          }
        }
        return item;
      }
    },
    takeItems: function(ingredients) {
      let items = [];
      if (this.hasAll(ingredients)!==true) {
        console.log("can't find the ingredients");
        return false;
      }
      for (let item in ingredients) {
        let n = ingredients[item];
        let taken = this.take(item,n);
        // need some error handling?
        items.push(taken);
      }
      return items;
    },
    hasAll: function(ingredients) {
      for (var ing in ingredients) {
        var n = ingredients[ing];
        // if we lack what we need, search for items
        if (this.countAll(ing)<n) {
          return false;
        }
      }
      return true;
    },
    missingIngredients: function(ingredients) {
      var miss = {};
      for (var ing in ingredients) {
        // if we lack what we need, search for items
        let n = ingredients[ing] - this.countAll(ing);
        if (n>0) {
          miss[ing] = n;
        }
      }
      return miss;
    },
    list: function() {
      var mesg = "";
      for (var i = 0; i<this.items.length; i++) {
        if (i>0) {
          mesg+=" ";
        }
        mesg+=this.items[i].describe({article: "indefinite"});
        if (i===this.items.length-2) {
          mesg = mesg + ", and";
        } else if (i<this.items.length-1) {
          mesg = mesg + ",";
        }
      }
      return mesg;
    },
    lineList: function(spacer) {
      var buffer = [];
      for (var i = 0; i<this.items.length; i++) {
        buffer.push([spacer,this.items[i].describe({article: "indefinite"})]);
      }
      return buffer;
    },
    head: function() {
      return this.items[0];
    },
    tail: function() {
      return this.items[this.items.length-1];
    },
    forEach: function(callb) {
      let ret = [];
      for (let i=0; i<this.items.length; i++) {
        ret.push(callb(this.items[i], this, i));
      }
      return ret;
    },
    exposeArray: function() {
      return this.items;
    }
  });
  Object.defineProperty(HTomb.Things.templates.Container,"length",{get: function() {return this.items.length;}});