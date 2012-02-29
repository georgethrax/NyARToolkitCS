using System;
using System.Diagnostics;
using UnityEngine;
using jp.nyatla.nyartoolkit.cs.core;
namespace NyARUnityUtils
{
	/**
	 * UnityRasterは上下逆転しているので注意すること！
	 */
	public class NyARUnityRaster: NyARRgbRaster
    {
        /**
         * インスタンスを生成します。インスタンスは、UnityObjectの参照バッファを持ちます。
         */
        public NyARUnityRaster(int i_width, int i_heigth)
            : base(i_width, i_heigth, NyARBufferType.OBJECT_CS_Unity,true)
        {
        }
        /**
         * Readerとbufferを初期化する関数です。コンストラクタから呼び出します。
         * 継承クラスでこの関数を拡張することで、対応するバッファタイプの種類を増やせます。
         * @param i_size
         * ラスタのサイズ
         * @param i_raster_type
         * バッファタイプ
         * @param i_is_alloc
         * 外部参照/内部バッファのフラグ
         * @return
         * 初期化が成功すると、trueです。
         * @ 
         */
        protected override bool initInstance(NyARIntSize i_size, int i_raster_type, bool i_is_alloc)
        {
            //バッファの構築
            switch (i_raster_type)
            {
                case NyARBufferType.OBJECT_CS_Unity:
                    this._buf = i_is_alloc?new Color32[i_size.w*i_size.h]:null;
                    this._rgb_pixel_driver = new NyARRgbPixelDriver_CsUnity();
                    this._rgb_pixel_driver.switchRaster(this);
                    this._is_attached_buffer = i_is_alloc;
                    break;
                default:
                    return base.initInstance(i_size,i_raster_type,i_is_alloc);
            }
            //readerの構築
            return true;
        }
        /**
         * この関数は、ラスタに外部参照バッファをセットします。
         * 外部参照バッファの時にだけ使えます。
         */
        public override void wrapBuffer(object i_ref_buf)
        {
			throw new NyARException();
//            System.Diagnostics.Debug.Assert(!this._is_attached_buffer);//バッファがアタッチされていたら機能しない。
//            this._buf = i_ref_buf;
//            //ピクセルリーダーの参照バッファを切り替える。
//            this._rgb_pixel_driver.switchRaster(this);
        }
		/**
		 * WebTextureで更新します。
		 */
		public void updateByWebCamTexture(WebCamTexture i_wtx)
		{
			i_wtx.GetPixels32((Color32[])this._buf);
			return;
		}
		/*
        public override object createInterface(Type iIid)
        {
            if (iIid == typeof(INyARPerspectiveCopy))
            {
                return this.isEqualBufferType(NyARBufferType.OBJECT_CS_Bitmap) ? new PerspectiveCopy_CSBitmap(this) : NyARPerspectiveCopyFactory.createDriver(this);
            }
            if (iIid == typeof(NyARMatchPattDeviationColorData.IRasterDriver))
            {
                return NyARMatchPattDeviationColorData.RasterDriverFactory.createDriver(this);
            }
            if (iIid == typeof(INyARRgb2GsFilter))
            {
                //デフォルトのインタフェイス
                return NyARRgb2GsFilterFactory.createRgbAveDriver(this);
            }
            else if (iIid == typeof(INyARRgb2GsFilterRgbAve))
            {
                return NyARRgb2GsFilterFactory.createRgbAveDriver(this);
            }
            else if (iIid == typeof(INyARRgb2GsFilterRgbCube))
            {
                return NyARRgb2GsFilterFactory.createRgbCubeDriver(this);
            }
            else if (iIid == typeof(INyARRgb2GsFilterYCbCr))
            {
                return NyARRgb2GsFilterFactory.createYCbCrDriver(this);
            }
            if (iIid == typeof(INyARRgb2GsFilterArtkTh))
            {
                return this.isEqualBufferType(NyARBufferType.OBJECT_CS_Bitmap) ? new NyARRgb2GsFilterArtkTh_CsBitmap(this) : NyARRgb2GsFilterArtkThFactory.createDriver(this);
            }
            throw new NyARException();
        }*/
    }

    #region pixel drivers
/*    class NyARRgb2GsFilterRgbAve_CsBitmap : INyARRgb2GsFilterRgbAve
    {
        private NyARBitmapRaster _ref_raster;
        public NyARRgb2GsFilterRgbAve_CsBitmap(NyARBitmapRaster i_ref_raster)
        {
            Debug.Assert(i_ref_raster.getBitmap().PixelFormat == PixelFormat.Format32bppRgb);
            Debug.Assert(i_ref_raster.isEqualBufferType(NyARBufferType.OBJECT_CS_Bitmap));
            this._ref_raster = i_ref_raster;
        }
        public void convert(INyARGrayscaleRaster i_raster)
        {
            NyARIntSize s = this._ref_raster.getSize();
            this.convertRect(0, 0, s.w, s.h, i_raster);
        }
        private byte[] _work = new byte[4 * 8];
        public void convertRect(int l, int t, int w, int h, INyARGrayscaleRaster o_raster)
        {
            byte[] work = this._work;
            BitmapData bm = this._ref_raster.lockBitmap();
            NyARIntSize size = this._ref_raster.getSize();
            int bp = (l + t * size.w) * 4 + (int)bm.Scan0;
            int b = t + h;
            int row_padding_dst = (size.w - w);
            int row_padding_src = row_padding_dst * 4;
            int pix_count = w;
            int pix_mod_part = pix_count - (pix_count % 8);
            int dst_ptr = t * size.w + l;
            // in_buf = (byte[])this._ref_raster.getBuffer();
            switch (o_raster.getBufferType())
            {
                case NyARBufferType.INT1D_GRAY_8:
                    int[] out_buf = (int[])o_raster.getBuffer();
                    for (int y = t; y < b; y++)
                    {

                        int x = 0;
                        for (x = pix_count - 1; x >= pix_mod_part; x--)
                        {
                            int p = Marshal.ReadInt32((IntPtr)bp);
                            out_buf[dst_ptr++] = (((p >> 16) & 0xff) + ((p >> 8) & 0xff) + (p & 0xff)) / 3;
                            bp += 4;
                        }
                        for (; x >= 0; x -= 8)
                        {
                            Marshal.Copy((IntPtr)bp, work, 0, 32);
                            out_buf[dst_ptr++] = (work[0] + work[1] + work[2]) / 3;
                            bp += 4;
                            out_buf[dst_ptr++] = (work[4] + work[5] + work[6]) / 3;
                            bp += 4;
                            out_buf[dst_ptr++] = (work[8] + work[9] + work[10]) / 3;
                            bp += 4;
                            out_buf[dst_ptr++] = (work[12] + work[13] + work[14]) / 3;
                            bp += 4;
                            out_buf[dst_ptr++] = (work[16] + work[17] + work[18]) / 3;
                            bp += 4;
                            out_buf[dst_ptr++] = (work[20] + work[21] + work[22]) / 3;
                            bp += 4;
                            out_buf[dst_ptr++] = (work[24] + work[25] + work[26]) / 3;
                            bp += 4;
                            out_buf[dst_ptr++] = (work[28] + work[29] + work[30]) / 3;
                            bp += 4;
                        }
                        bp += row_padding_src;
                        dst_ptr += row_padding_dst;
                    }
                    this._ref_raster.unlockBitmap();
                    return;
                default:
                    INyARGsPixelDriver out_drv = o_raster.getGsPixelDriver();
                    for (int y = t; y < b; y++)
                    {
                        for (int x = 0; x < pix_count; x++)
                        {
                            int p = Marshal.ReadInt32((IntPtr)bp);
                            out_drv.setPixel(x, y, (((p >> 16) & 0xff) + ((p >> 8) & 0xff) + (p & 0xff)) / 3);
                            bp += 4;
                        }
                        bp += row_padding_src;
                    }
                    this._ref_raster.unlockBitmap();
                    return;
            }

        }
    }
	 

    sealed class NyARRgb2GsFilterArtkTh_CsBitmap : INyARRgb2GsFilterArtkTh
    {
        private NyARBitmapRaster _raster;
        public void doFilter(int i_h, INyARGrayscaleRaster i_gsraster)
        {
            NyARIntSize s = this._raster.getSize();
            this.doFilter(0, 0, s.w, s.h, i_h, i_gsraster);
        }

        public NyARRgb2GsFilterArtkTh_CsBitmap(NyARBitmapRaster i_raster)
        {
            Debug.Assert(i_raster.isEqualBufferType(NyARBufferType.OBJECT_CS_Bitmap));
            Debug.Assert(i_raster.getBitmap().PixelFormat==PixelFormat.Format32bppRgb);
            this._raster = i_raster;
        }
        private byte[] _work=new byte[4*8];
        public void doFilter(int i_l, int i_t, int i_w, int i_h, int i_th, INyARGrayscaleRaster i_gsraster)
        {
            Debug.Assert(i_gsraster.isEqualBufferType(NyARBufferType.INT1D_BIN_8));
            BitmapData bm = this._raster.lockBitmap();
            byte[] work = this._work;
            int[] output = (int[])i_gsraster.getBuffer();
            NyARIntSize s = this._raster.getSize();
            int th = i_th * 3;
            int skip_dst = (s.w - i_w);
            int skip_src = skip_dst * 4;
            int pix_count = i_w;
            int pix_mod_part = pix_count - (pix_count % 8);
            //左上から1行づつ走査していく
            int pt_dst = (i_t * s.w + i_l);
            int pt_src = pt_dst * 4+(int)bm.Scan0;
            for (int y = i_h - 1; y >= 0; y -= 1)
            {
                int x;
                int p;
                for (x = pix_count - 1; x >= pix_mod_part; x--)
                {
                    p = Marshal.ReadInt32((IntPtr)pt_src);
                    output[pt_dst++] = (((p >> 16) & 0xff) + ((p >> 8) & 0xff) + (p & 0xff)) <= th ? 0 : 1;
                    pt_src += 4;
                }
                for (; x >= 0; x -= 8)
                {
                    Marshal.Copy((IntPtr)pt_src, work, 0, 32);
                    output[pt_dst  ] = (work[0]+work[1]+work[2]) <= th ? 0 : 1;
                    output[pt_dst+1] = (work[4] + work[5] + work[6]) <= th ? 0 : 1;
                    output[pt_dst+2] = (work[8] + work[9] + work[10]) <= th ? 0 : 1;
                    output[pt_dst+3] = (work[12] + work[13] + work[14]) <= th ? 0 : 1;
                    output[pt_dst+4] = (work[16] + work[17] + work[18]) <= th ? 0 : 1;
                    output[pt_dst+5] = (work[20] + work[21] + work[22]) <= th ? 0 : 1;
                    output[pt_dst+6] = (work[24] + work[25] + work[26]) <= th ? 0 : 1;
                    output[pt_dst+7] = (work[28] + work[29] + work[30]) <= th ? 0 : 1;
                    pt_src += 32;
                    pt_dst += 8;
                }
                //スキップ
                pt_src += skip_src;
                pt_dst += skip_dst;
            }
            this._raster.unlockBitmap();
            return;
        }
    }*/
	
    sealed class NyARRgbPixelDriver_CsUnity : INyARRgbPixelDriver
    {
        /** 参照する外部バッファ */
        private Color32[] _ref_buf;
        private NyARIntSize _ref_size;
        public NyARIntSize getSize()
        {
            return this._ref_size;
        }
        /**
         * この関数は、指定した座標の1ピクセル分のRGBデータを、配列に格納して返します。
         */
        public void getPixel(int i_x, int i_y, int[] o_rgb)
        {
            //byte(BGRX)=int(XRGB)
            Color32 pix=this._ref_buf[i_x+(this._ref_size.h-1-i_y)*this._ref_size.w];
            o_rgb[0] = pix.r;// R
            o_rgb[1] = pix.g; // G
            o_rgb[2] = pix.b;    // B
            return;
        }

        /**
         * この関数は、座標群から、ピクセルごとのRGBデータを、配列に格納して返します。
         */
        public void getPixelSet(int[] i_x, int[] i_y, int i_num, int[] o_rgb)
        {
			int h1=this._ref_size.h-1;
            for (int i = i_num - 1; i >= 0; i--)
            {
				Color32 pix=this._ref_buf[i_x[i]+(h1-i_y[i])*this._ref_size.w];
	            o_rgb[i*3+0] = pix.r;// R
	            o_rgb[i*3+1] = pix.g; // G
	            o_rgb[i*3+2] = pix.b;    // B
            }
            return;
        }

        /**
         * この関数は、RGBデータを指定した座標のピクセルにセットします。
         */
        public void setPixel(int i_x, int i_y, int[] i_rgb)
        {
			int idx=i_x+(this._ref_size.h-1-i_y)*this._ref_size.w;
			this._ref_buf[idx].r=(byte)i_rgb[0];
			this._ref_buf[idx].g=(byte)i_rgb[1];
			this._ref_buf[idx].b=(byte)i_rgb[2];
            return;
        }

        /**
         * この関数は、RGBデータを指定した座標のピクセルにセットします。
         */
        public void setPixel(int i_x, int i_y, int i_r, int i_g, int i_b)
        {
			int idx=i_x+(this._ref_size.h-1-i_y)*this._ref_size.w;
			this._ref_buf[idx].r=(byte)i_r;
			this._ref_buf[idx].g=(byte)i_g;
			this._ref_buf[idx].b=(byte)i_b;
            return;
        }

        /**
         * この関数は、機能しません。
         */
        public void setPixels(int[] i_x, int[] i_y, int i_num, int[] i_intrgb)
        {
            NyARException.notImplement();
        }

        public void switchRaster(INyARRgbRaster i_raster)
        {
            this._ref_buf =(Color32[])(((NyARUnityRaster)i_raster).getBuffer());
            this._ref_size = i_raster.getSize();
        }
    }
	
	
    sealed class PerspectiveCopy_Unity : NyARPerspectiveCopy_Base
    {
        private NyARUnityRaster _ref_raster;
        public PerspectiveCopy_Unity(NyARUnityRaster i_ref_raster)
        {
            System.Diagnostics.Debug.Assert(i_ref_raster.isEqualBufferType(NyARBufferType.OBJECT_CS));
            this._ref_raster = i_ref_raster;
        }
        protected override bool onePixel(int pk_l, int pk_t, double[] cpara, INyARRaster o_out)
        {
            Color32[] in_pixs = (Color32[])this._ref_raster.getBuffer();
            int in_w = this._ref_raster.getWidth();
            int in_h = this._ref_raster.getHeight();

            int[] pat_data = (int[])o_out.getBuffer();
            //ピクセルリーダーを取得
            double cp0 = cpara[0];
            double cp3 = cpara[3];
            double cp6 = cpara[6];
            double cp1 = cpara[1];
            double cp4 = cpara[4];
            double cp7 = cpara[7];

            int out_w = o_out.getWidth();
            int out_h = o_out.getHeight();
            double cp7_cy_1 = cp7 * pk_t + 1.0 + cp6 * pk_l;
            double cp1_cy_cp2 = cp1 * pk_t + cpara[2] + cp0 * pk_l;
            double cp4_cy_cp5 = cp4 * pk_t + cpara[5] + cp3 * pk_l;
            int r, g, b, p;
            switch (o_out.getBufferType())
            {
                case NyARBufferType.INT1D_X8R8G8B8_32:
                    p = 0;
                    for (int iy = 0; iy < out_h; iy++)
                    {
                        //解像度分の点を取る。
                        double cp7_cy_1_cp6_cx = cp7_cy_1;
                        double cp1_cy_cp2_cp0_cx = cp1_cy_cp2;
                        double cp4_cy_cp5_cp3_cx = cp4_cy_cp5;

                        for (int ix = 0; ix < out_w; ix++)
                        {
                            //1ピクセルを作成
                            double d = 1 / (cp7_cy_1_cp6_cx);
                            int x = (int)((cp1_cy_cp2_cp0_cx) * d);
                            int y = (int)((cp4_cy_cp5_cp3_cx) * d);
                            if (x < 0) { x = 0; } else if (x >= in_w) { x = in_w - 1; }
                            if (y < 0) { y = 0; } else if (y >= in_h) { y = in_h - 1; }
						
							Color32 pix=in_pixs[x + y * in_w];
                            //
                            pat_data[p] = ((pix.r << 16) & 0xff)|((pix.g << 8) & 0xff)| pix.b;
							//r = (px >> 16) & 0xff;// R
                            //g = (px >> 8) & 0xff; // G
                            //b = (px) & 0xff;    // B
                            cp7_cy_1_cp6_cx += cp6;
                            cp1_cy_cp2_cp0_cx += cp0;
                            cp4_cy_cp5_cp3_cx += cp3;
                            //pat_data[p] = (r << 16) | (g << 8) | ((b & 0xff));
                            //pat_data[p] = px;

                            p++;
                        }
                        cp7_cy_1 += cp7;
                        cp1_cy_cp2 += cp1;
                        cp4_cy_cp5 += cp4;
                    }
                    return true;
                default:
                    //ANY to RGBx
                    if (o_out is INyARRgbRaster)
                    {
                        INyARRgbPixelDriver out_reader = ((INyARRgbRaster)o_out).getRgbPixelDriver();
                        for (int iy = 0; iy < out_h; iy++)
                        {
                            //解像度分の点を取る。
                            double cp7_cy_1_cp6_cx = cp7_cy_1;
                            double cp1_cy_cp2_cp0_cx = cp1_cy_cp2;
                            double cp4_cy_cp5_cp3_cx = cp4_cy_cp5;

                            for (int ix = 0; ix < out_w; ix++)
                            {
                                //1ピクセルを作成
                                double d = 1 / (cp7_cy_1_cp6_cx);
                                int x = (int)((cp1_cy_cp2_cp0_cx) * d);
                                int y = (int)((cp4_cy_cp5_cp3_cx) * d);
                                if (x < 0) { x = 0; } else if (x >= in_w) { x = in_w - 1; }
                                if (y < 0) { y = 0; } else if (y >= in_h) { y = in_h - 1; }

                            	Color32 px = in_pixs[x + y * in_w];
                                r = px.r;// R
                                g = px.g;// G
                                b = px.b;// B
                                cp7_cy_1_cp6_cx += cp6;
                                cp1_cy_cp2_cp0_cx += cp0;
                                cp4_cy_cp5_cp3_cx += cp3;
                                out_reader.setPixel(ix, iy, r, g, b);
                            }
                            cp7_cy_1 += cp7;
                            cp1_cy_cp2 += cp1;
                            cp4_cy_cp5 += cp4;
                        }
                        return true;
                    }
                    break;
            }
            return false;
        }
        protected override bool multiPixel(int pk_l, int pk_t, double[] cpara, int i_resolution, INyARRaster o_out)
        {
            Color32[] in_pixs = (Color32[])this._ref_raster.getBuffer();
            int in_w = this._ref_raster.getWidth();
            int in_h = this._ref_raster.getHeight();
            int res_pix = i_resolution * i_resolution;

            //ピクセルリーダーを取得
            double cp0 = cpara[0];
            double cp3 = cpara[3];
            double cp6 = cpara[6];
            double cp1 = cpara[1];
            double cp4 = cpara[4];
            double cp7 = cpara[7];
            double cp2 = cpara[2];
            double cp5 = cpara[5];

            int out_w = o_out.getWidth();
            int out_h = o_out.getHeight();
            if (o_out is INyARRgbRaster)
            {
                INyARRgbPixelDriver out_reader = ((INyARRgbRaster)o_out).getRgbPixelDriver();
                for (int iy = out_h - 1; iy >= 0; iy--)
                {
                    //解像度分の点を取る。
                    for (int ix = out_w - 1; ix >= 0; ix--)
                    {
                        int r, g, b;
                        r = g = b = 0;
                        int cy = pk_t + iy * i_resolution;
                        int cx = pk_l + ix * i_resolution;
                        double cp7_cy_1_cp6_cx_b = cp7 * cy + 1.0 + cp6 * cx;
                        double cp1_cy_cp2_cp0_cx_b = cp1 * cy + cp2 + cp0 * cx;
                        double cp4_cy_cp5_cp3_cx_b = cp4 * cy + cp5 + cp3 * cx;
                        for (int i2y = i_resolution - 1; i2y >= 0; i2y--)
                        {
                            double cp7_cy_1_cp6_cx = cp7_cy_1_cp6_cx_b;
                            double cp1_cy_cp2_cp0_cx = cp1_cy_cp2_cp0_cx_b;
                            double cp4_cy_cp5_cp3_cx = cp4_cy_cp5_cp3_cx_b;
                            for (int i2x = i_resolution - 1; i2x >= 0; i2x--)
                            {
                                //1ピクセルを作成
                                double d = 1 / (cp7_cy_1_cp6_cx);
                                int x = (int)((cp1_cy_cp2_cp0_cx) * d);
                                int y = (int)((cp4_cy_cp5_cp3_cx) * d);
                                if (x < 0) { x = 0; } else if (x >= in_w) { x = in_w - 1; }
                                if (y < 0) { y = 0; } else if (y >= in_h) { y = in_h - 1; }
                            	Color32 px = in_pixs[x + y * in_w];
                                r = px.r;// R
                                g = px.g;// G
                                b = px.b;// B
                                cp7_cy_1_cp6_cx += cp6;
                                cp1_cy_cp2_cp0_cx += cp0;
                                cp4_cy_cp5_cp3_cx += cp3;
                            }
                            cp7_cy_1_cp6_cx_b += cp7;
                            cp1_cy_cp2_cp0_cx_b += cp1;
                            cp4_cy_cp5_cp3_cx_b += cp4;
                        }
                        out_reader.setPixel(ix, iy, r / res_pix, g / res_pix, b / res_pix);
                    }
                }
                return true;
            }
            return false;
        }
    }
    #endregion
}


