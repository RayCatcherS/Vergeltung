using UnityEngine;

namespace MagicLightmapSwitcher
{
	public static class TransformExtensions
	{
		public static bool MLS_IsVisibleFrom(this Transform transform, Camera camera)
		{
			Bounds transformBounds = new Bounds(transform.position, transform.localScale);
			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);

			return GeometryUtility.TestPlanesAABB(planes, transformBounds);
		}
	}
}
