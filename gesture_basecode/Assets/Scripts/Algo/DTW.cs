using System;
using System.Collections.Generic;
using UnityEngine;

namespace TP5.Algo
{
	public class DTW
	{
		public static float Distance(IList<Classifier.Frame> gesture, IList<Classifier.Frame> data)
		{
			float shortest_distance = float.MaxValue;

            int n = gesture.Count;
            int m = data.Count;

            if (n>0 && m>0)
            {
                float[,] tableau = new float[n, m];
                tableau[0, 0] = FramesDistance(gesture[0], data[0]);

                //Remplissage de la première colonne
                for (int i = 1 ; i<n ; i++)
                {
                    tableau[i, 0] = FramesDistance(gesture[i], data[0]) + tableau[i-1,0];
                }
                //Remplissage de la première ligne
                for (int j = 1; j < m; j++) 
                {
                    tableau[0, j] = FramesDistance(gesture[0], data[j]) + tableau[0, j-1];
                }
                //Remplissage du reste
                for (int i = 1; i < n; i++) 
                {
                    for (int j = 1; j < m; j++)
                    {
                        tableau[i, j] = FramesDistance(gesture[i], data[j]) + Mathf.Min(tableau[i-1, j], Mathf.Min(tableau[i-1, j-1], tableau[i, j-1]));
                    }
                }
                shortest_distance = tableau[n - 1, m - 1];
            }
            
            return shortest_distance;
		}

        protected static float FramesDistance(Classifier.Frame f1, Classifier.Frame f2)
		{
            float dist = 0f;

            dist += Vector3.Distance(f1.thumb, f2.thumb);
            dist += Vector3.Distance(f1.index, f2.index);
            dist += Vector3.Distance(f1.middle, f2.middle);
            dist += Vector3.Distance(f1.ring, f2.ring);
            dist += Vector3.Distance(f1.pinky, f2.pinky);

            return dist;
		}
	}
}
