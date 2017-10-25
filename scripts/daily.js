HTomb = (function(HTomb) {
  "use strict";

  // **** Handle daily cycle
  HTomb.Constants.STARTHOUR = 8;
  //HTomb.Constants.STARTHOUR = 16;
  HTomb.Constants.DAWN = 6;
  HTomb.Constants.DUSK = 17;
  HTomb.Constants.MONTH = 12;

  // !!!!This might eventually be a register / tracker
  // !!!!!shouldn't really be on Time
  HTomb.Time.dailyCycle = {
    hour: HTomb.Constants.STARTHOUR,
    minute: 0,
    day: 0,
    turn: 0,
    reset: function() {
      this.hour = HTomb.Constants.STARTHOUR;
      this.minute = 0;
      this.day = 0;
      this.turn = 0;
    },
    onTurnEnd: function() {
      this.turn+=1;
      this.minute+=1;
      if (this.minute>=60) {
        this.minute = 0;
        this.hour+=1;
        if (this.hour>=24) {
          this.hour = 0;
          this.day+=1;
        }
      }
    },
    onTurnBegin: function() {
      if (this.minute===0) {
        if (this.hour===this.times.dawn) {
          HTomb.GUI.pushMessage("The sun is coming up.");
          HTomb.World.validate.lighting();
        } else if (this.hour===this.times.dusk) {
          HTomb.GUI.pushMessage("Night is falling.");
          HTomb.World.validate.lighting();
        }
      }
      if ((this.hour>=this.times.dawn && this.hour<this.times.dawn+2)
        || (this.hour>=this.times.dusk && this.hour<this.times.dusk+2)) {
        if (this.turn%5===0) {
          HTomb.World.validate.lighting(undefined,HTomb.World.validate.lowestExposed);
        }
      }
    },
    //sunlight: {symbol: "\u263C"},
    sunlight: {symbol: "\u2600"},
    waning: {symbol: "\u263E", light: 32},
    twilight: {symbol: "\u25D2"},
    fullMoon: {symbol: "\u26AA", light: 64},
    waxing: {symbol: "\u263D", light: 32},
    newMoon: {symbol: "\u25CF", light: 1},
    times: {
      dawn: HTomb.Constants.DAWN,
      dusk: HTomb.Constants.DUSK,
      waxing: 2,
      fullMoon: 5,
      waning: 8,
      newMoon: 11,
      order: ["waxing","fullMoon","waning","newMoon"]
    },
    getPhase: function() {
      if (this.hour<this.times.dusk && this.hour>=this.times.dawn+1) {
        return this.sunlight;
      } else if (this.hour<this.times.dusk+1 && this.hour>=this.times.dawn) {
        return this.twilight;
      } else {
        return this.getMoon();
      }
      console.log(["how did we reach this?",this.day,this.tally]);
    },
    getMoon: function() {
      var phase = this.day%HTomb.Constants.MONTH;
      var tally = 0;
      for (var i=0; i<this.times.order.length; i++) {
        tally+=this.times[this.times.order[i]];
        if (phase<=tally) {
          return this[this.times.order[i]];
        }
      }
    },
    lightLevel: function() {
      var dawn = HTomb.Constants.DAWN;
      var dusk = HTomb.Constants.DUSK;
      var darkest = 64;
      var light, moonlight;
      if (this.hour < dawn || this.hour >= dusk+1) {
        moonlight = this.getMoon().light;
        return Math.round(darkest+moonlight);
      } else if (this.hour < dawn+1) {
        moonlight = this.getMoon().light;
        light = Math.min(255,(this.minute/60)*(255-darkest)+darkest+moonlight);
        return Math.round(light);
      } else if (this.hour >= dusk) {
        moonlight = this.getMoon().light;
        light = Math.min(255,((60-this.minute)/60)*(255-darkest)+darkest+moonlight);
        return Math.round(light);
      } else {
        return 255;
      }
    }
  };

  return HTomb;
})(HTomb);
