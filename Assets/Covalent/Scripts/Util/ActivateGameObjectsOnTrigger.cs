using System.Collections.Generic;
using UnityEngine;

namespace Covalent.Scripts.Util
{
	public class ActivateGameObjectsOnTrigger : MonoBehaviour
	{
		[SerializeField] private List<GameObject> turnOnGameObjects;
		[SerializeField] private List<GameObject> turnOffGameObjects;
		[SerializeField] private OnTriggerType onTriggerType;

		private void OnTriggerEnter2D(Collider2D other)
		{
			if (onTriggerType == OnTriggerType.OnTriggerEnter)
			{
				Triggered();
			}
		}

		private void OnTriggerExit2D(Collider2D other)
		{
			if (onTriggerType == OnTriggerType.OnTriggerExit)
			{
				Triggered();
			}
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