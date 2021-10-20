using TMPro;
using UnityEngine;

namespace Covalent.Scripts.UI.Debugging
{
	public class FrameRate : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI fpsText;
		[SerializeField] private float hudRefreshRate = 1f;

		private float timer;

		private void Update()
		{
			if (Time.unscaledTime > timer)
			{
				int fps = (int) (1f / Time.unscaledDeltaTime);
				fpsText.text = $"FPS {fps.ToString()}";
				timer = Time.unscaledTime + hudRefreshRate;
			}
		}
	}
}