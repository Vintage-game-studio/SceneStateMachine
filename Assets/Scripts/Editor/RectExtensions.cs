using System;
using UnityEngine;

/**
		This class extends the funcitonality of a rectangle.
		Based on http://martinecker.com/martincodes/unity-editor-window-zooming/
**/
namespace Assets.Editor.Bon
{
	public static class RectExtensions
	{

		private static Vector2 _tmpTopLeft = new Vector2();

		public static Vector2 TopLeft(this Rect rect)
		{
			_tmpTopLeft.Set(rect.xMin, rect.yMin);
			return _tmpTopLeft;
		}

		public static Rect ScaleSizeBy(this Rect rect, float scale)
		{
			return rect.ScaleSizeBy(scale, rect.center);
		}

        public static void Expand(this Rect r1,Vector2 point)
        {
            r1.xMax = Mathf.Max(point.x, r1.xMax);
            r1.yMax = Mathf.Max(point.y, r1.yMax);

            r1.xMin = Mathf.Min(point.x, r1.xMin);
            r1.yMin = Mathf.Min(point.y, r1.yMin);
        }
        public static Rect Expand(this Rect r1,Rect r2)
        {
            r1.xMax = Mathf.Max(r2.xMax, r1.xMax);
            r1.yMax = Mathf.Max(r2.yMax, r1.yMax);

            r1.xMin = Mathf.Min(r2.xMin, r1.xMin);
            r1.yMin = Mathf.Min(r2.yMin, r1.yMin);
            return r1;
        }

        public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
		{
			Rect result = rect;
			result.x -= pivotPoint.x;
			result.y -= pivotPoint.y;
			result.xMin *= scale;
			result.xMax *= scale;
			result.yMin *= scale;
			result.yMax *= scale;
			result.x += pivotPoint.x;
			result.y += pivotPoint.y;
			return result;
		}

		public static Rect ScaleSizeBy(this Rect rect, Vector2 scale)
		{
			return rect.ScaleSizeBy(scale, rect.center);
		}

		public static Rect ScaleSizeBy(this Rect rect, Vector2 scale, Vector2 pivotPoint)
		{
			Rect result = rect;
			result.x -= pivotPoint.x;
			result.y -= pivotPoint.y;
			result.xMin *= scale.x;
			result.xMax *= scale.x;
			result.yMin *= scale.y;
			result.yMax *= scale.y;
			result.x += pivotPoint.x;
			result.y += pivotPoint.y;
			return result;
		}
	}
}
