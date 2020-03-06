using System.Collections.Generic;
using UnityEngine;

namespace TP5
{
	public class Classifier : MonoBehaviour
	{
		public class Prediction
		{
			public float distance;
			public float probability;

			public Prediction(float distance)
			{
				this.distance = distance;

				probability = 0;
			}
		}

		public class Frame
		{
			public Vector3 thumb;
			public Vector3 index;
			public Vector3 middle;
			public Vector3 ring;
			public Vector3 pinky;
		}

		public class Gesture
		{
			public string name;
			public List<Frame> frames;

			public Gesture(int index)
			{
				name = string.Format("Gesture {0}", index + 1);

				frames = new List<Frame>();
			}
		}

		#region Members
		public const byte nbFingers = 5;
		
		[Range(0.001f, 0.2f)]
		public float captureFrequency = 0.02f; // In seconds
		[Range(0.5f, 10f)]
		public float predictionWindowSize = 1.5f; // In seconds
		[Range(0.01f, 10f)]
		public float nullRejectionFactor = 2f;
		[Range(0.01f, 10f)]
		public float softmaxFactor = 1f;

		public delegate void UpdateCallback(Frame frame);

		public event UpdateCallback UpdateCallbacks;

		protected List<Gesture> gestures;
		protected Queue<Frame> window;
		protected List<Frame> frames;
		protected List<Prediction> predictions;
		protected Algo.NullRejection nullRejection;
		protected Leap.Unity.FingerModel[] fingers;
		protected Transform palm;
		#endregion

		#region Getters / Setters
		public List<Gesture> Gestures
		{
			get { return gestures; }
		}

		public Leap.Unity.FingerModel[] Fingers
		{
			get { return fingers; }
		}

		public Transform Palm
		{
			get { return palm; }
		}
		#endregion

		#region MonoBehaviour callbacks
		protected void Awake()
		{
			Time.fixedDeltaTime = captureFrequency;

			gestures = new List<Gesture>();
			window = new Queue<Frame>(GetPredictionWindowSize());
			frames = new List<Frame>(GetPredictionWindowSize());
			predictions = new List<Prediction>();
			nullRejection = new Algo.NullRejection();

			foreach(Leap.Unity.RigidHand hand in FindObjectsOfType<Leap.Unity.RigidHand>())
			{
				if(hand.Handedness == Leap.Unity.Chirality.Right)
				{
					palm = hand.palm;

					fingers = new Leap.Unity.FingerModel[nbFingers];

					foreach(Leap.Unity.FingerModel finger in hand.fingers)
					{
						fingers[(int) finger.fingerType] = finger;
					}
				}
			}
		}

		protected void Update()
		{
			if(Time.fixedDeltaTime != captureFrequency)
			{
				Time.fixedDeltaTime = captureFrequency;
			}
		}

		protected void FixedUpdate()
		{
			Frame frame = new Frame();

			for(byte i = 0; i < nbFingers; ++i)
			{
				Vector3 pos = palm.InverseTransformPoint(fingers[i].GetTipPosition());

				switch((Leap.Finger.FingerType) i)
				{
					case Leap.Finger.FingerType.TYPE_THUMB:
						frame.thumb = pos;
						break;
					case Leap.Finger.FingerType.TYPE_INDEX:
						frame.index = pos;
						break;
					case Leap.Finger.FingerType.TYPE_MIDDLE:
						frame.middle = pos;
						break;
					case Leap.Finger.FingerType.TYPE_RING:
						frame.ring = pos;
						break;
					case Leap.Finger.FingerType.TYPE_PINKY:
						frame.pinky = pos;
						break;
				}
			}

			lock(window)
			{
				while(window.Count >= GetPredictionWindowSize())
				{
					window.Dequeue();
				}

				window.Enqueue(frame);
			}

			UpdateCallbacks.Invoke(frame);
		}
		#endregion

		#region Public methods
		public void Train()
		{
			nullRejection.Train(gestures);
		}

		public List<Prediction> Predict()
		{
			frames.Clear();
			predictions.Clear();

			lock(window)
			{
				foreach(Frame frame in window)
				{
					frames.Add(frame);
				}
			}

			int nb_gestures = gestures.Count;

			for(int i = 0; i < nb_gestures; ++i)
			{
				predictions.Add(new Prediction(Algo.DTW.Distance(gestures[i].frames, frames)));
			}

			nullRejection.AddNullClass(predictions, nullRejectionFactor);

			Algo.Softmax.Normalize(predictions, softmaxFactor);

            return predictions;
		}
		#endregion

		#region Internal methods
		protected int GetPredictionWindowSize()
		{
			return (int) (predictionWindowSize / captureFrequency);
		}
		#endregion
	}
}
