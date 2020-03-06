using System;
using UnityEngine;
using UnityEngine.UI;

namespace TP5.UI
{
	[RequireComponent(typeof(Classifier))]
	public class Train : MonoBehaviour
	{
		#region Members
		public GameObject trailPrefab;
		public Toggle defined;
		public Slider slider;
		public InputField gestureName;
		public Button add;
		public Button record;
		public Text predicted;
		public Text confidence;

		protected Classifier classifier;
		protected bool recording;
		#endregion

		#region Getter / Setters
		protected Classifier.Gesture CurrentGesture
		{
			get
			{
				int index = (int) slider.value;

				if(index < 0 || index >= classifier.Gestures.Count)
				{
					throw new IndexOutOfRangeException(string.Format("Invalid index '{0}' for gestures. Only '{1}' gestures defined.", index, classifier.Gestures.Count));
				}

                return classifier.Gestures[index];
			}
		}
		#endregion

		#region MonoBehaviour callbacks
		protected void Awake()
		{
			classifier = GetComponent<Classifier>();
		}

		protected void Start()
		{
			classifier.Gestures.Add(new Classifier.Gesture(0));

			classifier.UpdateCallbacks += OnUpdate;

			gestureName.text = CurrentGesture.name;

			recording = false;

			foreach(Leap.Unity.FingerModel finger in classifier.Fingers)
			{
				if(trailPrefab != null)
				{
					Instantiate(trailPrefab, finger.bones[3]);
				}
			}
		}

		protected void Update()
		{
			if(Input.GetKeyDown(KeyCode.Return))
			{
				OnAddGesture();
			}

			if(Input.GetKeyDown(KeyCode.Space))
			{
				OnRecordGesture();
			}
		}
		#endregion

		#region UI callbacks
		public void OnSliderChanged(float value)
		{
			if(recording)
			{
				StopRecord();
			}

			Classifier.Gesture g = CurrentGesture;

			defined.isOn = g.frames.Count > 0;
			gestureName.text = g.name;
		}

		public void OnNameChanged(string value)
		{
			CurrentGesture.name = value;
		}

		public void OnAddGesture()
		{
			if(recording)
			{
				StopRecord();
			}

			slider.maxValue += 1;

			classifier.Gestures.Add(new Classifier.Gesture((int) slider.maxValue));

			slider.value = slider.maxValue;
		}

		public void OnRecordGesture()
		{
			if(recording)
			{
				StopRecord();
			}
			else
			{
				StartRecord();
			}
		}
		#endregion

		#region Internal methods
		protected void StartRecord()
		{
			Text t = record.GetComponentInChildren<Text>();

			if(t != null)
			{
				t.text = "Stop";
			}

			record.targetGraphic.color = new Color32(255, 192, 0, 255);

			defined.isOn = true;

			recording = true;
		}

		protected void StopRecord()
		{
			Text t = record.GetComponentInChildren<Text>();

			if(t != null)
			{
				t.text = "Record";
			}

			record.targetGraphic.color = Color.white;

			recording = false;

			classifier.Train();
		}

		protected void OnUpdate(Classifier.Frame frame)
		{
			if(recording)
			{
				CurrentGesture.frames.Add(frame);
			}
		}
		#endregion
	}
}
