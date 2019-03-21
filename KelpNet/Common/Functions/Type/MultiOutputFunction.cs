﻿using System;
using System.Collections.Generic;

namespace KelpNet
{
    [Serializable]
    public abstract class MultiOutputFunction : Function
    {
        protected Func<NdArray, NdArray[]> SingleInputForward;
        protected Action<NdArray[], NdArray> SingleOutputBackward;

        List<NdArray[]> PrevOutputs = new List<NdArray[]>();
        List<NdArray[]> UsedPrevOutputs = new List<NdArray[]>();

        protected MultiOutputFunction(string name, string[] inputNames = null, string[] outputNames = null) : base(name, inputNames, outputNames)
        {
        }

        public override NdArray[] Forward(params NdArray[] xs)
        {
            PrevInputs.Add(xs);

            xs[0].UseCount++;
            xs[0].UseCount++;

            NdArray[] result = SingleInputForward(xs[0]);
            PrevOutputs.Add(result);

            return result;
        }

        public override void Backward(params NdArray[] ys)
        {
            NdArray[] xs = PrevInputs[PrevInputs.Count - 1];

#if DEBUG
            if (xs == null || xs.Length != 1) throw new Exception("引数が正しくありません");
#endif
            xs[0].UseCount--;
            //出力した両方で使用が終わったら
            if (xs[0].UseCount <= 0)
            {
                if (xs[0].Grad == null) xs[0].ClearGrad();

                InitGrad();
                BackwardCountUp();

                PrevInputs.RemoveAt(PrevInputs.Count - 1);
                NdArray[] prevys = PrevOutputs[PrevOutputs.Count - 1];
                PrevOutputs.RemoveAt(PrevOutputs.Count - 1);

                SingleOutputBackward(prevys, xs[0]);

                UsedPrevInputs.Add(xs);
                if (PrevInputs.Count == 0)
                {
                    PrevInputs.AddRange(UsedPrevInputs);
                    UsedPrevInputs.Clear();
                }

                UsedPrevOutputs.Add(prevys);
                if (PrevOutputs.Count == 0)
                {
                    PrevOutputs.AddRange(UsedPrevOutputs);
                    UsedPrevOutputs.Clear();
                }
            }
        }

        public override NdArray[] Predict(params NdArray[] xs)
        {
            return SingleInputForward(xs[0]);
        }
    }
}
