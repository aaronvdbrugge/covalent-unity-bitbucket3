using System.Collections.Generic;
using UnityEngine;

namespace Covalent.Scripts.Util
{
	public class ActivationScript : MonoBehaviour
	{
		[SerializeField] private List<GameObject> turnOnGameObjects;
		[SerializeField] private List<GameObject> turnOffGameObjects;

		private void Start()
		{
			Triggered();
		}

		private void Triggered()
		{
			for (int i = 0; i < turnOnGameObjects.Count; i++)
			{
				turnOnGameObjects[i].SetActive(true);
			}

			for (int i = 0; i < turnOffGameObjects.Count; i++)
			{
				turnOffGameObjects[i].SetActive(false);
			}
		}
	}
}