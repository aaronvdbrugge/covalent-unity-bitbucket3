using UnityEngine;

namespace Covalent.HomeIsland
{
	public class GlowTarget : MonoBehaviour
	{
		[SerializeField] private SpriteRenderer outlineInner;
		[SerializeField] private SpriteRenderer outlineOuter;
		[SerializeField] private SpriteRenderer outlineGlow;
		[SerializeField] private Material glowMaterial;

		public SpriteRenderer OutlineInner => outlineInner;
		public SpriteRenderer OutlineOuter => outlineOuter;
		public SpriteRenderer OutlineGlow => outlineGlow;
		public Material GlowMaterial => glowMaterial;
	}
}