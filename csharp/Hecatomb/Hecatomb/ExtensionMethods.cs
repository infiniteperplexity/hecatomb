/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/21/2018
 * Time: 3:28 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hecatomb
{
	/// <summary>
	/// Description of ExtensionMethods.
	/// </summary>
	static class ExtensionMethods
	{	
	    public static void Shuffle<T> (this Random rng, T[] array)
	    {
	        int n = array.Length;
	        while (n > 1) 
	        {
	            int k = rng.Next(n--);
	            T temp = array[n];
	            array[n] = array[k];
	            array[k] = temp;
	        }
		}
	    
//	    public static T[] Shuffled<T> (this Random rng, T[] array) {
//			int n = array.Length;
//			T[] narray = new T[n];
//			for (int i=0; i<n; i++) {
//				narray[i] = array[i];
//			}
//			Random.Shuffle(narray);
//			return narray;
//		}
	}
}
