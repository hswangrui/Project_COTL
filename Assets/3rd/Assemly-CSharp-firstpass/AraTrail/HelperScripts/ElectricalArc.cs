using System;
using UnityEngine;

namespace Ara
{
	[RequireComponent(typeof(AraTrail))]
	public class ElectricalArc : MonoBehaviour
	{
		private AraTrail trail;

		public Transform source;

		public Transform target;

		public int points = 20;

		public float burstInterval = 0.5f;

		public float burstRandom = 0.2f;

		public float speedRandom = 2f;

		public float positionRandom = 0.1f;

		private float accum;

		private void OnEnable()
		{
			trail = GetComponent<AraTrail>();
			trail.emit = false;
		}

		private void Update()
		{
			accum += Time.deltaTime;
			if (accum >= burstInterval)
			{
				ChangeArc();
				accum = (0f - burstInterval) * UnityEngine.Random.value * burstRandom;
			}
		}

		private void ChangeArc()
		{
			trail.points.Clear();
			if (source != null && target != null)
			{
				for (int i = 0; i < points; i++)
				{
					float num = (float)i / (float)(points - 1);
					float num2 = Mathf.Sin(num * (float)Math.PI);
					Vector3 vector = Vector3.Lerp(source.position, target.position, num);
					trail.points.Add(new AraTrail.Point(vector + UnityEngine.Random.onUnitSphere * positionRandom * num2, UnityEngine.Random.onUnitSphere * speedRandom * num2, Vector3.up, Vector3.forward, Color.white, 1f, 0f, burstInterval * 2f));
				}
			}
		}
	}
}
