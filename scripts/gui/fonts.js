HTomb = (function(HTomb) {
  "use strict";
  var Constants = HTomb.Constants;

  var font = "Noto Sans";
  var size = 18;
  var FONTFAMILY = Constants.FONTFAMILY = font;
  var FONTSIZE = Constants.FONTSIZE = size;
  var CHARHEIGHT = Constants.CHARHEIGHT = size;
  var CHARWIDTH = Constants.CHARWIDTH = size;
  var XSKEW = Constants.XSKEW = 10;
  var YSKEW = Constants.YSKEW = 10;

  // Dimensions of the display panels
  var GAMEW = 500;
  var GAMEH = 500;
  var SCREENW = Constants.SCREENW = Math.floor(GAMEW/CHARWIDTH);
  var SCREENH = Constants.SCREENH = Math.floor(GAMEH/CHARHEIGHT);
  console.log("Playing area will be " + SCREENW + "x" + SCREENH + ".");

  font = "PT Mono";
  size = 15;
  var TEXTFONT = Constants.TEXTFONT = font;
  var TEXTSIZE = Constants.TEXTSIZE = size;
  var TEXTWIDTH = Constants.TEXTWIDTH = 10;
  var TEXTSPACING = Constants.TEXTSPACING = 1;

  var TOTALH = Constants.TOTALH = GAMEH+8*TEXTSIZE;
  var TOTALW = Constants.TOTALW = 900;
  var MENUW = Constants.MENUW = Math.floor((TOTALW-GAMEW)/TEXTWIDTH);
  var MENUH = Constants.MENUH = parseInt(TOTALH/TEXTSIZE);
  var STATUSH = Constants.STATUSH = 2;
  var SCROLLH = Constants.SCROLLH = 6;
  var SCROLLW = Constants.SCROLLW = 50;

  var ALERTHEIGHT = Constants.ALERTHEIGHT = 12;
  var ALERTWIDTH = Constants.ALERTWIDTH = 30;

  HTomb.Fonts = {};
  HTomb.Fonts.lookupSymbol = {};
  HTomb.Fonts.textLookup = {};
  let missingChars = ["\uFFFF","\uFFFD"];
  HTomb.Fonts.charFound = function(chr, font) {
    let canvas = document.createElement("canvas");
    let ctx = canvas.getContext('2d');
    let h = 20;
    let w = 20;
    canvas.width = w;
    canvas.height = h;
    ctx.font = "10px " + font;
    ctx.fillText(chr,0,h/2);
    let arr = ctx.getImageData(0,0,w,h).data;
    let arrs = [];
    for (let i=0; i<missingChars.length; i++) {
      ctx.clearRect(0,0,w,h);
      ctx.fillText(missingChars[i],0,h/2);
      arrs.push(ctx.getImageData(0,0,w,h).data);
    }
    for (let i=0; i<arr.length; i++) {
      let bit = arr[i];
      let allDiffer = true;
      for (let j=0; j<arrs.length; j++) {
        if (bit===arrs[j][i]) {
          allDiffer = false;
        }
      }
      if (allDiffer) {
        return true;
      }
    }
    return false;
  };
  HTomb.Fonts.getBackup = function(symbol, font) {
    let backups = {
      "\u26E7": "\u2606"
    }
    let sym = backups[symbol] || "X";
    if (HTomb.Fonts.charFound(sym, font)===false) {
      sym = HTomb.Fonts.getBackup(sym, font);
    }
    return sym;
  }
  return HTomb;
})(HTomb);
