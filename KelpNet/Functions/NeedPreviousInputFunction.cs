﻿using System;
using System.Collections.Generic;
using KelpNet.Common;
#if !DEBUG
using System.Threading.Tasks;
#endif

namespace KelpNet.Functions
{
    //前回の入出力を自動的に扱うクラステンプレート
    [Serializable]
    public abstract class NeedPreviousInputFunction : Function
    {
        //後入れ先出しリスト
        private readonly List<NdArray[]> _prevInput = new List<NdArray[]>();

        protected abstract NdArray NeedPreviousForward(NdArray x);
        protected abstract NdArray NeedPreviousBackward(NdArray gy, NdArray prevInput);

        protected NeedPreviousInputFunction(string name, int inputCount = 0, int oututCount = 0) : base(name, inputCount, oututCount)
        {
        }

        protected override NdArray ForwardSingle(NdArray x)
        {
            this._prevInput.Add(new[] { new NdArray(x) });

            return this.NeedPreviousForward(x);
        }


        protected override NdArray[] ForwardSingle(NdArray[] x)
        {
            //コピーを格納
            NdArray[] prevInput = new NdArray[x.Length];
            for (int i = 0; i < prevInput.Length; i++)
            {
                prevInput[i] = new NdArray(x[i]);
            }

            this._prevInput.Add(prevInput);


            NdArray[] prevoutput = new NdArray[x.Length];

#if DEBUG
            for(int i = 0; i < x.Length; i ++)
#else
            Parallel.For(0, x.Length, i =>
#endif
            {
                prevoutput[i] = this.NeedPreviousForward(x[i]);
            }
#if !DEBUG
            );
#endif
            
            return prevoutput;
        }

        protected override NdArray BackwardSingle(NdArray gy)
        {
            var prevInput = this._prevInput[this._prevInput.Count-1][0];
            this._prevInput.RemoveAt(this._prevInput.Count - 1);

            return this.NeedPreviousBackward(gy, prevInput);
        }

        protected override NdArray[] BackwardSingle(NdArray[] gy)
        {
            var prevInput = this._prevInput[this._prevInput.Count-1];
            this._prevInput.RemoveAt(this._prevInput.Count - 1);

            NdArray[] result = new NdArray[gy.Length];

#if DEBUG
            for (int i = 0; i < gy.Length; i++)
#else
            Parallel.For(0, gy.Length, i =>
#endif
            {
                result[i] = this.NeedPreviousBackward(gy[i], prevInput[i]);
            }
#if !DEBUG
            );
#endif

            return result;
        }

        public override NdArray Predict(NdArray input)
        {
            return this.NeedPreviousForward(input);
        }

        public override NdArray[] Predict(NdArray[] x)
        {
            NdArray[] prevoutput = new NdArray[x.Length];
#if DEBUG
            for(int i = 0; i < x.Length; i ++)
#else
            Parallel.For(0, x.Length, i =>
#endif
            {
                prevoutput[i] = this.NeedPreviousForward(x[i]);
            }
#if !DEBUG
            );
#endif
            return prevoutput;
        }
    }
}