HTomb = (function(HTomb) {
  "use strict";
  var coord = HTomb.Utils.coord;

  HTomb.Particles.emitters = [];
  var emitter = {
    // position of origin
    x: null,
    y: null,
    z: null,
    // directional tendency
    dirx: 0,
    diry: 0,
    dirz: 0,
    // directional randomness
    dirr: 0,
    // distance from origin
    dist: 0,
    distz: 0,
    distr: 1,
    // velocity
    v: 0.1,
    vz: 0,
    vr: 0,
    //a: -0.01,
    a: 0,
    // rate of emission
    tick: 0,
    rate: 20,
    //how many particles to emit before going away
    n: 100,
    // lifetime
    t: 5,
    tr: 1,
    fg: "#FF0000",
    // rgb randomness
    rr: 10,
    gr: 10,
    br: 10,
    alpha: 0.75,
    fade: 0.9,
    alwaysVisible: false,
    particles : null,
    //chars: "abcdefghijlmnopqrstuvwxyz",
    chars: ["\u037C","\u037D","\u03A6","\u03A8","\u03A9","\u03B1","\u03B3","\u03B4","\u03B6"],
    emit: function() {
      if (this.n<=0) {
        return;
      } else {
        this.n-=1;
      }
      if (this.particles===null) {
        this.particles = [];
      }
      var p = {};
      //set a bunch of properties

      var a,r;
      //randomize starting position
      //assume no directional tendency for now
      a = ROT.RNG.getUniform()*2*Math.PI;
      r = this.dist+this.distr*ROT.RNG.getNormal(0,1); //this can drop below zero
      p.x = this.x+r*Math.cos(a);
      p.y = this.y+r*Math.sin(a);
      p.z = this.z+0;
      //assume no z coordinate
      //randomize starting velocity
      // actually for now just have them spew outward
      r = this.v+this.vr*ROT.RNG.getNormal(0,1);
      p.vx = r*Math.cos(a);
      p.vy = r*Math.sin(a);
      p.vz = 0;
      p.ax = this.a*Math.cos(a);
      p.ay = this.a*Math.sin(a);
      p.az = 0;
      p.life = this.t+this.tr*ROT.RNG.getNormal(0,1);
      var s = this.fg;
      p.fg = ROT.Color.randomize(s,this.rr,this.gr,this.br);
      p.symbol = this.randomChar();
      p.alpha = this.alpha;
      p.alwaysVisible = this.alwaysVisible;
      this.onEmit(p);
      this.particles.push(p);
    },
    update: function(spd) {
      if (this.rate>(spd/1000)) {
        for (var t=0; t<this.rate; t++) {
          this.emit();
        }
      } else {
        this.tick+=(spd/1000);
        if (this.tick>=spd/(1000*this.rate)) {
          this.tick = 0;
          this.emit();
        }
      }
      // or maybe afterwards?
      this.preUpdate();
      if (this.particles) {
        var expired = [];
        for (var i=0; i<this.particles.length; i++) {
          var p = this.particles[i];
          p.x += p.vx;
          p.y += p.vy;
          p.z += p.vz;
          p.vx += p.ax;
          p.vy += p.ay;
          p.vz += p.az;
          p.symbol = this.randomChar();
          p.life-=1;
          p.alpha*=this.fade;
          if (p.life<=0) {
            expired.push(p);
          }
          this.eachUpdate(p);
          //do something with each particle
        }
        //
        for (var j=0; j<expired.length; j++) {
          if (this.particles.indexOf(expired[j])!==-1) {
            this.particles.splice(this.particles.indexOf(expired[j]),1);
          }
        }
      }
      this.postUpdate();
      if (this.particles.length===0) {
        HTomb.Particles.emitters.splice(HTomb.Particles.emitters.indexOf(this));
      }
    },
    randomChar: function() {
      var s = Math.floor(Math.random()*this.chars.length);
      return this.chars[s];
    },
    onEmit: function(p) {},
    preUpdate: function() {},
    postUpdate: function() {},
    eachUpdate: function(p) {}
  };

  HTomb.Particles.update = function(spd) {
    for (var i=0; i<HTomb.Particles.emitters.length; i++) {
      HTomb.Particles.emitters[i].update(spd);
    }
  };

  HTomb.Particles.addEmitter = function(x,y,z,args,modifiers) {
    args = args || {};
    var e = Object.create(emitter);
    e.x = x;
    e.y = y;
    e.z = z;
    for (let arg in args) {
      e[arg] = args[arg];
    }
    for (let arg in modifiers) {
      e[arg] = modifiers[arg];
    }
    e.fg = ROT.Color.fromString(e.fg);
    e.particles = [];
    e.emit();
    HTomb.Particles.emitters.push(e);
    HTomb.Time.startParticles();
  }

  HTomb.Particles.Liquid = {chars: ["\u00B7","\u2022","\u2234","\u2235","\u25CB","\u25CF","\u25E6"]};
  HTomb.Particles.Blood = HTomb.Utils.merge(HTomb.Particles.Liquid,{fg: "red"});
  HTomb.Particles.Acid = HTomb.Utils.merge(HTomb.Particles.Liquid,{fg: "#55FF11"});
  HTomb.Particles.Spatter = {t: 2, alpha: 0.5, fade: 0.5, n: 25};
  HTomb.Particles.Spray = {dist: 1, alpha: 0.75, fade: 0.75, n: 50};
  HTomb.Particles.SpellTarget = {fg: "black", dist: 3, v: -0.5};
  HTomb.Particles.SpellCast = {fg: "black", v: 0.5, dist: 1};
  HTomb.Particles.DryadEffect = {fg: "#88AA00", rr: 30, chars: ["\u2663","\u2660","\u2698","\u2618"]};
  HTomb.Particles.Anger = HTomb.Utils.merge(HTomb.Particles.Spray,{chars: ["!","\u2757","\u2762","\u203C","\u2755","\u2694","\u2639"], dist: 0});


  return HTomb;
})(HTomb);
