using UnityEngine;
using UnityEngine.EventSystems;

namespace Covalent.Scripts.UI
{
	public class ClickFeature : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
	{
		private bool _tapping;
		
		public void OnPointerDown(PointerEventData eventData)
		{
			Debug.Log($"Finger down on: {gameObject.name}");
			_tapping = true;
		}
		
		public void OnPointerUp(PointerEventData eventData)
		{
			Debug.Log($"Pointer up: {gameObject.name}");
			if (_tapping)
			{
				Debug.Log($"Trigger: {gameObject.name}");
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			Debug.Log($"Pointer left: {gameObject.name}");
			_tapping = false;
		}
	}
}