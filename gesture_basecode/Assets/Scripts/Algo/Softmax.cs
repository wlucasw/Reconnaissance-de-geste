using System.Collections.Generic;
using UnityEngine;

namespace TP5.Algo
{
	public class Softmax
	{
		public static void Normalize(List<Classifier.Prediction> data, float b = 1)
		{
            int K = data.Count;
            if (K > 0)
			{

                //Calcul de la somme d'exponentielles
                float somme = 0f;
                for (int j = 0; j < K; j++)
                {
                    somme += Mathf.Exp(-b * data[j].distance);
                }

                for (int i = 0; i < K; i++)
                {
                    data[i].probability = Mathf.Exp(-b * data[i].distance) / somme;
                }
            }
		}
	}
}
