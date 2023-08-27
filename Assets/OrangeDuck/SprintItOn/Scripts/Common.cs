using System;
using UnityEditor;
using UnityEngine;

namespace SprintItOn
{
    public static class Common
    {
        public static float Lerp(float x, float y, float a)
        {
            return (1.0f - a) * x + a * y;
        }

        public static float Damper(float x, float g, float factor)
        {
            return Lerp(x, g, factor);
        }

        public static float DamperBad(float x, float g, float damping, float dt)
        {
            return Lerp(x, g, damping * dt);
        }

        public static float DamperExponential(float x, float g, float damping, float dt, float ft = 1f / 60f)
        {
            return Lerp(x, g, 1 - Mathf.Pow(1.0f / (1.0f - ft * damping), -dt / ft));
        }

        public static float FastNegExp(float x)
        {
            return 1.0f / (1.0f + x + 0.48f * x * x + 0.235f * x * x * x);
        }

        public static float DamperExact(float x, float g, float halfLife, float dt, float eps = 1e-5f)
        {
            return Lerp(x, g, 1 - FastNegExp(0.69314718056f * dt / halfLife + eps));
        }

        public static void SprintDamperBad(ref float x, ref float v, float g, float q, float stiffness, float damping, float dt)
        {
            v += dt * stiffness * (g - x) + dt * damping * (q - v);
            x += dt * v;
        }

        
        public static float Square(float x)
        {
            return x * x;
        }

        public static void SprintDamperExact(ref float x, ref float v, float xGoal, float vGoal, float frequency, float halflife, float dt, float eps = 1e-5f)
        {
            float g = xGoal;
            float q = vGoal;
            float s = FrequencyToStiffness(frequency);
            float d = HalfLifeToDamping(halflife);
            float c = g + (d * q) / (s + eps);
            float y = d / 2.0f;

            if (Mathf.Abs(s - (d * d) / 4.0f) < eps)
            {
                float j0 = x - c;
                float j1 = v + j0 * y;

                float eydt = FastNegExp(y * dt);

                x = j0 * eydt + dt * j1 * eydt + c;
                v = -y * j0 * eydt - y * dt * j1 * eydt + j1 * eydt;
            }
            else if (s - (d * d) / 4.0f > 0.0)
            {
                float w = Mathf.Sqrt(s - (d * d) / 4.0f);
                float j = Mathf.Sqrt(Square(v + y * (x - c)) / (w * w + eps) + Square(x - c));
                float p = FastAtan((v + (x - c) * y) / (-(x - c) * w + eps));

                j = (x - c) > 0.0f ? j : -j;

                float eydt = FastNegExp(y * dt);

                x = j * eydt * Mathf.Cos(w * dt + p) + c;
                v = -y * j * eydt * Mathf.Cos(w * dt + p) - w * j * eydt * Mathf.Sin(w * dt + p);
            }
            else if (s - (d * d) / 4.0f < 0.0)
            {
                float y0 = (d + Mathf.Sqrt((d * d) - 4.0f * s)) / 2.0f;
                float y1 = (d - Mathf.Sqrt((d * d) - 4.0f * s)) / 2.0f;
                float j1 = (c * y0 - x * y0 - v) / (y1 - y0);
                float j0 = x - j1 - c;

                float ey0dt = FastNegExp(y0 * dt);
                float ey1dt = FastNegExp(y1 * dt);

                x = j0 * ey0dt + j1 * ey1dt + c;
                v = -y0 * j0 * ey0dt - y1 * j1 * ey1dt;
            }
        }

        public static void SprintDamperExactRatio(ref float x, ref float v, float xGoal, float vGoal, float dampingRatio, float halflife, float dt, float eps = 1e-5f)
        {
            float g = xGoal;
            float q = vGoal;
            float d = HalfLifeToDamping(halflife);
            float s = DampingRatioToStiffness(dampingRatio, d);
            float c = g + (d * q) / (s + eps);
            float y = d / 2.0f;

            if (Mathf.Abs(s - (d * d) / 4.0f) < eps)
            {
                float j0 = x - c;
                float j1 = v + j0 * y;

                float eydt = FastNegExp(y * dt);

                x = j0 * eydt + dt * j1 * eydt + c;
                v = -y * j0 * eydt - y * dt * j1 * eydt + j1 * eydt;
            }
            else if (s - (d * d) / 4.0f > 0.0)
            {
                float w = Mathf.Sqrt(s - (d * d) / 4.0f);
                float j = Mathf.Sqrt(Square(v + y * (x - c)) / (w * w + eps) + Square(x - c));
                float p = FastAtan((v + (x - c) * y) / (-(x - c) * w + eps));

                j = (x - c) > 0.0f ? j : -j;

                float eydt = FastNegExp(y * dt);

                x = j * eydt * Mathf.Cos(w * dt + p) + c;
                v = -y * j * eydt * Mathf.Cos(w * dt + p) - w * j * eydt * Mathf.Sin(w * dt + p);
            }
            else if (s - (d * d) / 4.0f < 0.0)
            {
                float y0 = (d + Mathf.Sqrt((d * d) - 4.0f * s)) / 2.0f;
                float y1 = (d - Mathf.Sqrt((d * d) - 4.0f * s)) / 2.0f;
                float j1 = (c * y0 - x * y0 - v) / (y1 - y0);
                float j0 = x - j1 - c;

                float ey0dt = FastNegExp(y0 * dt);
                float ey1dt = FastNegExp(y1 * dt);

                x = j0 * ey0dt + j1 * ey1dt + c;
                v = -y0 * j0 * ey0dt - y1 * j1 * ey1dt;
            }
        }

        public static void CriticalSprintDamperExact(ref float x, ref float v, float xGoal, float vGoal, float halflife, float dt)
        {
            float g = xGoal;
            float q = vGoal;
            float d = HalfLifeToDamping(halflife);
            float c = g + (d * q) / ((d * d) / 4.0f);
            float y = d / 2.0f;
            float j0 = x - c;
            float j1 = v + j0 * y;
            float eydt = FastNegExp(y * dt);

            x = eydt * (j0 + j1 * dt) + c;
            v = eydt * (v - j1 * y * dt);
        }

        public static void SimpleSprintDamperExact(ref float x, ref float v, float xGoal, float halflife, float dt)
        {
            float y = HalfLifeToDamping(halflife) / 2.0f;
            float j0 = x - xGoal;
            float j1 = v + j0 * y;
            float eydt = FastNegExp(y * dt);

            x = eydt * (j0 + j1 * dt) + xGoal;
            v = eydt * (v - j1 * y * dt);
        }

        private static float FrequencyToStiffness(float frequency)
        {
            return Square(2.0f * Mathf.PI * frequency);
        }

        private static float HalfLifeToDamping(float halflife, float eps = 1e-5f)
        {
            return (4.0f * 0.69314718056f) / (halflife + eps);
        }

        private static float FastAtan(float x)
        {
            float z = Mathf.Abs(x);
            float w = z > 1.0f ? 1.0f / z : z;
            float y = (Mathf.PI / 4.0f) * w - w * (w - 1.0f) * (0.2447f + 0.0663f * w);
            return CopySign(z > 1.0f ? Mathf.PI / 2.0f - y : y, x);
        }

        private static float CopySign(float a, float b)
        {
            return Mathf.Abs(a) * Mathf.Sign(b);
        }

        private static float DampingRatioToStiffness(float ratio, float damping)
        {
            return Square(damping / (ratio * 2.0f));
        }
    }

}