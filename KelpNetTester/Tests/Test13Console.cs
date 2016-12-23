﻿using System;
using KelpNet.Common;
using KelpNet.Functions.Connections;
using KelpNet.Loss;
using KelpNet.Optimizers;

namespace KelpNetTester.Tests
{
    //ある学習済みフィルタで出力された画像を元に、そのフィルタと同等のフィルタを獲得する
    //コンソール版
    //移植元 : http://qiita.com/samacoba/items/958c02f455ca5f3a475d
    class Test13Console
    {
        public static void Run()
        {
            //目標とするフィルタを作成（実践であればココは不明な値となる）
            Deconvolution2D decon_core = new Deconvolution2D(1, 1, 15, 1, 7)
            {
                W = { Data = MakeOneCore() }
            };

            Deconvolution2D model = new Deconvolution2D(1, 1, 15, 1, 7);

            SGD optimizer = new SGD(learningRate: 0.00005); //大きいと発散する
            model.SetOptimizer(optimizer);
            MeanSquaredError meanSquaredError = new MeanSquaredError();

            //ランダムに点が打たれた画像を生成
            NdArray img_p = getRandomImage();

            //目標とするフィルタで学習用の画像を出力
            NdArray img_core = decon_core.Forward(img_p);

            //移植元では同じ教育画像で教育しているが、より実践に近い学習に変更
            for (int i = 0; i < 31; i++)
            {
                model.ClearGrads();

                //未学習のフィルタで画像を出力
                NdArray img_y = model.Forward(img_p);

                double loss;
                NdArray gy = meanSquaredError.Evaluate(img_y, img_core, out loss);

                model.Backward(gy);

                model.Update();

                Console.WriteLine("epoch" + i + " : " + loss);
            }
        }

        static NdArray getRandomImage(int N = 1, int img_w = 128, int img_h = 128)
        {
            // ランダムに0.1％の点を作る
            double[] img_p = new double[N * img_w * img_h];

            for (int i = 0; i < img_p.Length; i++)
            {
                img_p[i] = Mother.Dice.Next(0, 1000);
                img_p[i] = img_p[i] > 999 ? 0 : 1;
            }

            return new NdArray(img_p, new[] { N, img_h, img_w });
        }

        //１つの球状の模様を作成（ガウスですが）
        static double[] MakeOneCore()
        {
            int max_xy = 15;
            double sig = 5.0;
            double sig2 = sig * sig;
            double c_xy = 7;
            double[] core = new double[max_xy * max_xy];

            for (int px = 0; px < max_xy; px++)
            {
                for (int py = 0; py < max_xy; py++)
                {
                    double r2 = (px - c_xy) * (px - c_xy) + (py - c_xy) * (py - c_xy);
                    core[py * max_xy + px] = Math.Exp(-r2 / sig2) * 1;
                }
            }

            return core;
        }
    }
}