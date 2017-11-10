// ****** This module implements Components, which are the basic units of functionality for creatures, items, and features
HTomb = (function(HTomb) {
  "use strict";
  var LEVELW = HTomb.Constants.LEVELW;
  var LEVELH = HTomb.Constants.LEVELH;
  var coord = HTomb.Utils.coord;

let Team = HTomb.Types.Type.extend({
    template: "Team",
    name: "team",
    members: null,
    enemies: null,
    allies: null,
    xenophobic: false,
    berserk: false,
    vendettas: null,
    teams: {},
    hostilityMatrix: {
      matrix: {},
      reset: function() {
        this.matrix = {};
      },
      onTurnBegin: function() {
        let matrix = this.matrix = {};
        let teams = HTomb.Types.Team.teams
        let keys = Object.keys(teams);
        for (let i=0; i<keys.length; i++) {
          // handle team-wide vendettas against individuals
          let one = teams[keys[i]];
          for (let j=0; j<one.vendettas.length; j++) {
            for (let m=0; m<one.members.length; m++) {
              let a = one.members[m];
              let s = a.spawnId;
              if (j===0) {
                matrix[s] = matrix[s] || {};
              }
              let b = one.vendettas[j];
              if (a===b) {
                continue;
              }
              let t = b.spawnId;
              let q = HTomb.Path.quickDistance(a.x,a.y,a.z,b.x,b.y,b.z);
              matrix[s][t] = q;
              matrix[t] = matrix[t] || {};
              matrix[t][s] = q;
            }
          }
          // handle inter-team hostility
          for (let j=i; j<keys.length; j++) {
            let two = teams[keys[j]];
            if (one.isHostile(two)) {
              for (let m=0; m<one.members.length; m++) {
                let a = one.members[m];
                let s = a.spawnId;
                for (let n=0; n<two.members.length; n++) {
                  if (n===0) {
                    matrix[s] = matrix[s] || {};
                  }
                  let b = two.members[n];
                  // no creature is ever hostile toward itself, even if it is berserk
                  if (a===b) {
                    continue;
                  }
                  let t = b.spawnId;
                  let q = HTomb.Path.quickDistance(a.x,a.y,a.z,b.x,b.y,b.z);
                  matrix[s][t] = q;
                  matrix[t] = matrix[t] || {};
                  matrix[t][s] = q;
                }
              }
            }
          }
        }
      }
    },
    onDefine: function(args) {
      this.members = [];
      this.enemies = args.enemies || [];
      this.allies = args.allies || [];
      this.vendettas = [];
      HTomb.Events.subscribe(this,"Destroy");
      HTomb.Types.Team.teams[this.template] = this;
    },
    onDestroy: function(event) {
      if (this.members.indexOf(event.entity)>-1) {
        this.members.splice(this.members.indexOf(event.entity),1);
      }
      if (this.vendettas.indexOf(event.entity)>-1) {
        this.vendettas.splice(this.vendettas.indexOf(event.entity),1);
      }
    },
    isHostile: function(team) {
      if (team===undefined) {
        return false;
      }
      if (typeof(team)==="string") {
        team = HTomb.Types[team];
      }
      if (this.berserk || team.berserk) {
        return true;
      } else if ((this.xenophobic || team.xenophobic) && (this!==team)) {
        return true;
      } else if (team.enemies.indexOf(this.template)>=0 || this.enemies.indexOf(team.template)>=0) {
        return true;
      } else {
        return false;
      }
    }
  });
  HTomb.Events.subscribe(HTomb.Types.Team.hostilityMatrix,"TurnBegin");


  // Teams might work better as Trackers, not Types
  // the player and affiliated minions
  Team.extend({
    template: "PlayerTeam",
    name: "player"
  });

  Team.extend({
    template: "DefaultTeam",
    name: "default"
  });

  // non-aggressive animals
  Team.extend({
    template: "AnimalTeam",
    name: "animals"
  });

  Team.extend({
    template: "GhoulTeam",
    name: "ghouls",
    enemies: ["PlayerTeam"]
  });

  Team.extend({
    template: "HungryPredatorTeam",
    name: "predators",
    enemies: ["PlayerTeam"]
    //xenophobic: true
  });

  Team.extend({
    template: "AngryNatureTeam",
    name: "angryNature",
    enemies: ["PlayerTeam","GhoulTeam"]
  });

  Team.extend({
    template: "HumanityTeam",
    name: "humanity",
    enemies: ["PlayerTeam","GhoulTeam","AngryNatureTeam"]
  });

  Team.extend({
    template: "RedAntTeam",
    name: "red ants"
  });

  return HTomb;
})(HTomb);
