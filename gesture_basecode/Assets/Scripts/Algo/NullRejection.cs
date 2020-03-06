using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TP5.Algo
{
	public class NullRejection
	{
		#region Members
		protected List<float> interDistances = new List<float>();
		protected float threshold;
		#endregion

		#region Public methods
		public void Train(IList<Classifier.Gesture> gestures)
		{
            if (gestures.Count > 1)
            {
                //Calcul des distances entre toutes paires de gestes d'apprentissage
                for (int i = 0; i < gestures.Count; i++)
                {
                    for (int j = 0; j < gestures.Count; j++)
                    {
                        if (i != j)
                        {
                            interDistances.Add(DTW.Distance(gestures[i].frames, gestures[j].frames));
                        }
                    }
                }

                //Calcul du seuil T
                threshold = interDistances[0];
                for (int i = 0; i < interDistances.Count; i++)
                {
                    if (interDistances[i] < threshold)
                    {
                        threshold = interDistances[i];
                    }
                }
            }
		}

		public void AddNullClass(List<Classifier.Prediction> data, float factor)
		{
            float dist = float.MaxValue;
			if(data.Count > 0)
			{
				for (int i = 0; i < data.Count; i++)
                {
                    dist = Mathf.Min(dist,Mathf.Abs(threshold-data[i].distance));
                }
                data.Add(new Classifier.Prediction(dist * factor));
            }
            
        }
		#endregion
	}
}
