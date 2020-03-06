using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace TP5.UI
{
	[RequireComponent(typeof(Classifier))]
	public class Predict : MonoBehaviour
	{
		#region Members
		public Text label;
		public Text confidence;

		protected Classifier classifier;
		protected ManualResetEvent stop;
		protected Thread predictor;
		protected int index;
		protected float max;
		#endregion

		#region MonoBehaviour callbacks
		protected void Awake()
		{
			classifier = GetComponent<Classifier>();
		}

		protected void Start()
		{
			stop = new ManualResetEvent(false);

			predictor = new Thread(Predictor);

			predictor.Start();
		}

		protected private void OnDestroy()
		{
			stop.Set();

			predictor.Join();
		}

		protected void Update()
		{
			label.text = ((index >= 0 && index < classifier.Gestures.Count) ? classifier.Gestures[index].name : "NULL");
			confidence.text = string.Format("{0:0.00} %", 100 * max);
		}
		#endregion

		#region Internal methods
		protected void Predictor()
		{
			while(!stop.WaitOne(0))
			{
				List<Classifier.Prediction> predictions = classifier.Predict();

				if(predictions.Count > 0)
				{
					max = predictions.Max(x => x.probability);
					index = predictions.FindIndex(x => x.probability == max);
				}
			}
		}
		#endregion
	}
}
