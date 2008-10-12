﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using jp.nyatla.nyartoolkit.cs.core;
using jp.nyatla.nyartoolkit.cs.core2;

namespace jp.nyatla.nyartoolkit.cs.sandbox.x2
{
    /**
     * 歪み成分マップを使用するINyARCameraDistortionFactor
     * 内部マップをint(1:15:16)フォーマットの固定小数点で保持する。
     * 固定小数点で値を提供するインタフェイスを持ちます。
     */
    public class NyARFixedFloatObserv2IdealMap
    {
        private double[] _factor = new double[4];
        private int _stride;
        private int[] _mapx;
        private int[] _mapy;
        public NyARFixedFloatObserv2IdealMap(NyARCameraDistortionFactor i_distfactor, NyARIntSize i_screen_size)
        {
            NyARDoublePoint2d opoint = new NyARDoublePoint2d();
            this._mapx = new int[i_screen_size.w * i_screen_size.h];
            this._mapy = new int[i_screen_size.w * i_screen_size.h];
            this._stride = i_screen_size.w;
            int ptr = i_screen_size.h * i_screen_size.w - 1;
            //歪みマップを構築
            for (int i = i_screen_size.h - 1; i >= 0; i--)
            {
                for (int i2 = i_screen_size.w - 1; i2 >= 0; i2--)
                {
                    i_distfactor.observ2Ideal(i2, i, opoint);
                    this._mapx[ptr] = (int)(opoint.x * 65536);
                    this._mapy[ptr] = (int)(opoint.y * 65536);
                    ptr--;
                }
            }
            i_distfactor.getValue(this._factor);
            return;
        }
        /**
         * 点集合のi_start～i_numまでの間から、最大i_sample_count個の頂点を取得して返します。
         * i_sample_countは偶数である必要があります。
         * @param i_x_coord
         * @param i_y_coord
         * @param i_start
         * @param i_num
         * @param o_x_coord
         * @param o_y_coord
         * @param i_sample_count
         * @return
         */
        public int observ2IdealSampling(int[] i_x_coord, int[] i_y_coord, int i_start, int i_num, int[] o_x_coord, int[] o_y_coord, int i_sample_count)
        {
            Debug.Assert(i_sample_count % 2 == 0);
            int idx;
            if (i_num < i_sample_count)
            {
                for (int i = i_num - 1; i >= 0; i --)
                {
                    idx = i_x_coord[i_start + i] + i_y_coord[i_start + i] * this._stride;
                    o_x_coord[i] = this._mapx[idx];
                    o_y_coord[i] = this._mapy[idx];
                }
                return i_num;
            }
            else
            {
                //サンプリング個数分の点を、両端から半分づつ取ってくる。
                int st = i_start;
                int ed = i_start + i_num - 1;
                for (int i = i_sample_count - 1; i >= 0; i -= 2)
                {
                    idx = i_x_coord[st] + i_y_coord[st] * this._stride;
                    o_x_coord[i] = this._mapx[idx];
                    o_y_coord[i] = this._mapy[idx];
                    idx = i_x_coord[ed] + i_y_coord[ed] * this._stride;
                    o_x_coord[i - 1] = this._mapx[idx];
                    o_y_coord[i - 1] = this._mapy[idx];
                    ed--;
                    st++;
                }
                return i_sample_count;
            }
        }
    }
}