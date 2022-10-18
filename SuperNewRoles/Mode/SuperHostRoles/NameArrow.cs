using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    public static class NameArrow
    {
        static float GetAngle(Vector2 start, Vector2 target)
        {
            Vector2 dt = target - start;
            float rad = Mathf.Atan2(dt.x, dt.y);
            float degree = rad * Mathf.Rad2Deg;

            if (degree < 0)
            {
                degree += 360;
            }

            return degree;
        }

        private static bool FloatRange(float f1, float f, float f2) => f1 < f && f < f2;


        public static string ArrowAngleString(Vector2 from,Vector2 to)
        {
            var angle = GetAngle(from, to);
            if (FloatRange(337.5f, angle, 360f) || FloatRange(0f, angle, 22.5f)) return "↑";
            if (FloatRange(22.5f, angle, 67.5f)) return "↗";
            if (FloatRange(67.5f, angle, 112.5f)) return "→";
            if (FloatRange(112.5f, angle, 157.5f)) return "↘";
            if (FloatRange(157.5f, angle, 202.5f)) return "↓";
            if (FloatRange(202.5f, angle, 247.5f)) return "↙";
            if (FloatRange(247.5f, angle, 292.5f)) return "←";
            if (FloatRange(292.5f, angle, 337.5f)) return "↖";
            Logger.Info(angle.ToString(), "namearr");
            return "";
        }
    }
}