/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/21/2018
 * Time: 3:10 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
//using System;
//using System.Collections.Generic;
//
//namespace Hecatomb
//{
	/// <summary>
	/// Description of AStar.
	/// </summary>
//	public static class Tiles
	{
		public static AStar(
			int x0, int y0, int z0,
			int x1, int y1, int z1,
			bool useFirst=false, bool useLast=true
			// searcher, searchee, searchTimeout, cacheAfter, cacheTimeout, maxTries, maxLength
		) {
			// should check enclosed right up front
			Coord[] dirs = Movement.Directions26;
			Random.Shuffle(dirs);
			// scores could be a sparse3Darray I think
			// checked could be as well
			// current and next are coords, I think
//		}	
//	}
//}
//
//    // fastest possible lookup
//    // random bias should be okay
//    var dirs = [
//      [ 0, -1],
//      [ 1, -1],
//      [ 1,  0],
//      [ 1,  1],
//      [ 0,  1],
//      [-1,  1],
//      [-1,  0],
//      [-1, -1]
//    ].randomize();
//    var current, next, this_score, h_score, crd;
//    var checked = {}, scores = {}, retrace = {}, path = [], pathLength = {};
//    // it costs zero to get to the starting square
//    scores[coord(x0,y0,z0)] = 0;
//    //square that need to be checked
//    //three-dimensional coordinate, and estimated (heuristic) distance
//    var tocheck = [[x0,y0,z0,this_score+h(x0,y0,z0,x1,y1,z1)]];
//    // keep checking until the algorithm finishes
//    while (tocheck.length>0) {
//      // choose the highest-priority square
//      current = tocheck.shift();
//      // calculate the fast lookup
//      crd = coord(current[0],current[1],current[2]);
//      // check if we have found the target square (or maybe distance==1?)
//      if (  (current[0]===x1 && current[1]===y1 && current[2]===z1)
//            //|| (useLast===false && HTomb.Utils.arrayInArray([x1,y1,z1],HTomb.Tiles.touchableFrom(current[0],current[1],current[2]))>-1)
//              || (useLast===false && HTomb.Tiles.isTouchableFrom(x1,y1,z1,current[0],current[1],current[2]))
//        ) {
//      // if (current[6]===1) {
//        // start with the goal square
//        path = [[current[0],current[1],current[2]]];
//        // until we get back to the starting square...
//        while (current[0]!==x0 || current[1]!==y0 || current[2]!==z0) {
//          // retrace the path by one step
//          current = retrace[crd];
//          // calculate the fast coordinate
//          crd = coord(current[0],current[1],current[2]);
//          // add the next square to the returnable path
//          path.unshift([current[0],current[1],current[2]]);
//        }
//        // return the complete path
//        if (path.length>0 && useFirst===false) {
//          path.shift();
//        }
//        if (searcher && searchee && cacheTimeout && cacheAfter!==undefined && path.length>cacheAfter) {
//          let t = path[cacheAfter];
//          t = HTomb.Tiles.getTileDummy(t[0],t[1],t[2]);
//          cacheTimeout += ROT.RNG.getUniformInt(-1,1);
//          HTomb.Path.successes[searcher.spawnId+","+searchee.spawnId] = [cacheTimeout,t];
//        }
//        //if (path.length>0 && useLast===false) {
//        //  path.pop();
//        //}
//        return path;
//      }
//      // we are now checking this square
//      checked[crd] = true;
//      // loop through neighboring cells
//      for (var i=-1; i<8; i++) {
//        // -1 is the place where we check for portals
//        if (i===-1) {
//          // right now cannot handle multiple portals in one square
//          if (tiles[current[2]][current[0]][current[1]].zmove!==undefined) {
//            next = [current[0],current[1],current[2]+tiles[current[2]][current[0]][current[1]].zmove];
//          } else {
//            continue;
//          }
//        } else {
//          // grab a neighboring square
//          next = [current[0]+dirs[i][0],current[1]+dirs[i][1],current[2]];
//        }
//        crd = coord(next[0],next[1],next[2]);
//        // if this one has been checked already then skip it
//        if (checked[crd]) {
//          //HTomb.GUI.drawAt(next[0],next[1],"X","purple","black");
//          continue;
//        }
//        squaresTried+=1;
//        // otherwise set the score equal to the distance from the starting square
//          // this assumes a uniform edge cost of 1
//        this_score = scores[coord(current[0],current[1],current[2])]+1;
//        // if there is already a better score for this square then skip it
//        //if (scores[crd]!==undefined && scores[crd]<=this_score) {
//        if (scores[crd]!==undefined && (scores[crd]<=this_score || this_score>=maxLength)) {
//          //HTomb.GUI.drawAt(next[0],next[1],"X","yellow","black");
//          continue;
//        }
//        // if the move is not valid then skip it
//        if (canPass(next[0],next[1],next[2],current[0],current[1],current[2])===false) {
//        //if (canPass(next[0],next[1],next[2])===false) {
//          //HTomb.GUI.drawAt(next[0],next[1],"X","red","black");
//          continue;
//        }
//        h_score = this_score + h(next[0],next[1],next[2],x1,y1,z1);
//        if (isNaN(h_score)) {
//          console.log(this_score);
//          console.log(next);
//          console.log([x1,y1,z1]);
//          alert("scoring failed!");
//        }
//        //HTomb.GUI.drawAt(next[0],next[1],"X","green","black");
//        // now add it to the to-do list unless it already has a better score on there
//        for (var j=0; j<tocheck.length; j++) {
//          // if this score is better than the one being checked...
//          if (h_score<=tocheck[j][3]) {
//            // insert it into the priority queue based on estimated distance
//            tocheck.splice(j,0,[next[0],next[1],next[2],h_score]);
//            // use this as a flag
//            h_score = -1;
//            break;
//          }
//        }
//        // if it is worse than the worst score on the list, add to the end
//        if (h_score>-1) {
//          //tocheck.push([next[0],next[1],next[2],this_score+abs(next[0]-x1)+abs(next[1]-y1)+abs(next[2]-z1)]);
//          tocheck.push([next[0],next[1],next[2],h_score]);
//        }
//        // set the parent square in the potential path
//        retrace[crd] = [current[0],current[1],current[2]];
//        //pathLength[crd] = pathLength[coord(current[0],current[1],current[2])]+1 || 0;
//        // save the new best score for this square
//        scores[crd] = this_score;
//      }
//      if (squaresTried>=maxTries) {
//        break;
//      }
//    }
//    console.log(searcher);
//    console.log(searchee);
//    console.log("path failed after " + squaresTried);
//    if (searcher && searchee && searchTimeout) {
//      let combo = searcher.spawnId+","+searchee.spawnId;
//      searchTimeout += ROT.RNG.getUniformInt(-1,1);
//      HTomb.Path.failures[combo] = [searchTimeout,squaresTried];
//    }
//    //for (let len in pathLength) {
//    //  stats.maxLength = Math.max(stats.maxLength,pathLength[len]);
//    //}
//    //HTomb.Path.benchmarks.failed.push(stats);
//    return false;
//  };