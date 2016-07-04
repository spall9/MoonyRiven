﻿using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace MoonyRiven
{
    static class UltimateCone
    {
        public static bool IsLastConeValid => Environment.TickCount - LastConeSetTick < 1000;

        private static int LastConeSetTick;
        private static Geometry.Polygon _LastBestCone;
        public static Geometry.Polygon LastBestCone
        {
            set
            {
                LastConeSetTick = Environment.TickCount;
                _LastBestCone = value;
            }
            get { return _LastBestCone; }
        }

        public static IEnumerable<Vector2> LastPredictedInsidePositions { get; set; }

        public static void Draw()
        {
            if (!IsLastConeValid || !LastPredictedInsidePositions.Any())
                return;

            LastBestCone.Draw(System.Drawing.Color.DodgerBlue, 3);
            foreach (Vector2 position in LastPredictedInsidePositions)
            {
                new Circle(new ColorBGRA(new Vector4(255, 0, 0, 1)), 100, 2).Draw(position.To3D());
            }
        }
    }
}
