﻿using System;

namespace KelpNet.Common.Tools
{
    class Initializer
    {
        //初期値が入力されなかった場合、この関数で初期化を行う
        public static void InitWeight(NdArray array, Real? masterScale = null)
        {
            Real localScale = 1 / (Real)Math.Sqrt(2);
            int fanIn = GetFans(array.Shape);
            Real s = localScale * (Real)Math.Sqrt(2.0 / fanIn);

            for (int i = 0; i < array.Data.Length; i++)
            {
                array.Data[i] = Normal(s) * (masterScale ?? 1);
            }
        }

        private static Real Normal(Real? scale = null)
        {
            Mother.Sigma = scale ?? (Real)0.05;
            return Mother.RandomNormal();
        }

        private static int GetFans(int[] shape)
        {
            int result = 1;

            for (int i = 1; i < shape.Length; i++)
            {
                result *= shape[i];
            }

            return result;
        }
    }
}
