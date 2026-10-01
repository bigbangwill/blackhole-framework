using UnityEngine;
using Blackhole.SharedKernel.Domain;

namespace Blackhole.SharedKernel.Infrastructure
{
    public static class Vector2Extensions
    {
        public static Point2 ToPoint2(this Vector2 vector)
        {
            return new Point2(vector.x, vector.y);
        }

        public static Vector2 ToVector2(this Point2 point)
        {
            return new Vector2(point.X, point.Y);
        }

        public static Point2Int ToPoint2Int(this Vector2Int vector)
        {
            return new Point2Int(vector.x, vector.y);
        }

        public static Vector2Int ToVector2Int(this Point2Int point)
        {
            return new Vector2Int(point.X, point.Y);
        }
    }
}