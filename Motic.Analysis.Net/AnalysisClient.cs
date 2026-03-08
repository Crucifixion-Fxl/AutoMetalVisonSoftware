using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Motic.Analysis.Net
{
    public enum Moac_retCode
    {
        RC_ACCEPT= 0x00,//命令己接收
        RC_FINISH = 0x01,//命令己执行
        RC_ERR_NET = 0x02,//网络出错
        RC_ERR_INTERFACE = 0x03,//接口出错，或当前不支持
        RC_ERR_PARAM=0x04,//参数出错
        RC_ERR_MEMORY=0x05,//内存出错
        RC_ERR_MANUAL=0x06,//当前处于手动模式
        RC_ERR_NOCONNECT,//未建立连接
        RC_ERR_SNAP,//采图失败，需检查相机是否工作正常
        RC_ERR_SAVEIMAGE,//图片无法保存到指定路径，需检查给的的文件路径是否可写,或名称是否含有无效字符
        RC_ERR_UPLOADIMAGE,//图片无法上传到服务器，需检查给的的服务器是否有效,或名称是否含有无效字符
        RC_ERR_UPLOADFILE,//上传的文件不存在，需检查名称是否含有无效字符
        RC_ERR_FILENAME,//指定的图像文件名称错误，需检查名称是否含有无效字符
        RC_ERR_XYPOS,//载物台位置获取错误，需检查载物台是否工作正常
        RC_ERR_ZPOS,//Z轴位置获取错误，需检查Z轴是否工作正常
        RC_ERR_SETXY,//移动载物台未到位，载物台是否连接或是否到了边界
        RC_ERR_SETZ,//移动Z轴未到位，Z轴是否连接或是否到Z轴限位
        RC_ERR_NOEXPOSURE,//相机没有手动曝光功能
        RC_ERR_NOAUTOEXP,//相机没有自动曝光功能
        RC_ERR_RESOLUTION,//没有指定的分辨率
        RC_ERR_RESOLUTIONCOUNT,//分辨率个数错误
        RC_ERR_RESOLUTIONLIST,//分辨率列表获取错误
        RC_ERR_NOCAMERA,//相机未连接或未打开
        RC_ERR_IMGPROC,//没有对应的图像处理功能
        RC_ERR_BRIGHTNESS,//亮度参数错误
        RC_ERR_CONTRAST,//对比度参数错误
        RC_ERR_GAMMA,//伽马参数错误
        RC_ERR_ENHANCE,//增强参数错误
        RC_ERR_COLORCC,//色彩校正错误
        RC_ERR_GAIN,//增益参数错误
        RC_ERR_GETLIGHT,//获取光源亮度错误
        RC_ERR_SETLIGHT,//设置光源亮度错误
        RC_ERR_CALIBRATION,//获取校准参数错误，请检查是否已做好校准，或物镜是否转到位
        RC_ERR_CALIBZ,//标定Z轴焦面或限位错误，请检查轴是否连接正常，或物镜是否转到位
        RC_ERR_PRESET,//设定预设参数错误，请检查是否给定的是非法的预设名称
        RC_ERR_FOCUSIMAGE,//无法获取聚焦图像错误，请检查Z轴的位置，图像在指定聚焦区间是否没有无法聚焦
        RC_ERR_FOCUS,//聚焦失败，请检查Z轴的位置，图像在指定聚焦区间是否没有无法聚焦
        RC_ERR_SAMPLEHEIGHT,//无法设定样品高度，请检查Z轴是否连接正常，是否做个样品高度标定
        RC_ERR_SWITCHOBJ,//切换物镜失败，请检查物镜转换器工作正常
        RC_ERR_GETOBJ,//获取物镜失败，请检查物镜转换器工作正常
        RC_ERR_SCANPROCESS,//扫描程序调用失败
        RC_ERR_SCANCANCEL,//扫描中断
        RC_ERR_ROTATESTAGE,//旋转台异常，或没有安装旋转台
        RC_ERR_ROTATEANGLE,//旋转台角度设置错误
        RC_ERR_SCANMODE,//扫描模式设置失败
        RC_ERR_TOOMUCHPOINTS,//建模点太多超过9个
        RC_ERR_CREATEMODULE,//无法完成建模,可能是建模点不足
    };
    public enum Moac_status
    {
      S_READY = 0x01,//一切就绪，正在等待指令
      S_SCANING = 0x2,// 当前正处于扫描状态
      S_MOVINGXY = 0x04,// 正在移动载物台
      S_MOVINGZ = 0x08,// 正在移动Z轴
      S_SWICHING = 0x10,// 正在切换物镜
      S_TILING = 0x20,//正在拼接图像
      S_FAIL_XY = 0x1000,// 无法连接Z轴控制器(串口有问题)
      S_FAIL_Z = 0x2000,// 无法连接转换器控制器
      S_FAIL_SWICHING = 0x4000,//无法连接转换器控制器
      S_FAIL_CAMERA = 0x8000,//无法连接转换器控制器
      S_FAIL_ZCalib = 0x10000,//焦面标定的数据丢失
      S_FAIL_NOCONNECT = 0x20000//无法连接转换器控制器
    };
    public class AnalysisClient
    {
        private int m_client = 0;
        private MoacCallback m_cb;
        private class ClientEvent
        {
            static readonly object synclock = new object();
            static Dictionary<int, AnalysisClient> m_dic;
            static Delegate m_clientCB;
            private static void OnClientCallback(int client, int id, int status)
            {
                AnalysisClient ac = null;
                lock (synclock)
                {
                    if (m_dic != null && m_dic.ContainsKey(client))
                    {
                        ac = m_dic[client];
                    }
                }
                if(ac != null)
                {
                    ac.OnEventCallback(id, status);
                }
            }
            public static void AddClient(int client, AnalysisClient obj)
            {
                if (m_dic == null)
                {
                    lock (synclock)
                    {
                        if (m_dic == null)
                        {
                            m_dic = new Dictionary<int, AnalysisClient>();
                            m_clientCB = (ClientCallback)OnClientCallback;
                            Moac_setEventCallback(m_clientCB);
                        }
                    }
                }
                lock (synclock)
                {
                    if (m_dic != null)
                    {
                        m_dic[client] = obj;
                    }
                }
            }
            public static void RemoveClient(int client)
            {
                lock (synclock)
                {
                    if (m_dic != null && m_dic.ContainsKey(client))
                    {
                        m_dic.Remove(client);
                    }
                }
            }
        }
        

        /**
        * @brief 连接Analysis
        * @param ip Analysis运行所在机器的IP地址
        * @param port 连接Analysis的端口号
        * @return 成功返回true, flase返回失败，一般失败原因是网络不同或目标机器的Analysis未启动
        */
        public bool connect(char[] strip, int port)
        {
            m_client = Moac_connect(strip, port);
            if(m_client > 0)
            {
                ClientEvent.AddClient(m_client, this);
            }
            return (m_client > 0);
        }

        /**
        * @brief 断开连接Analysis
        */
        public void disconnect()
        {
            if (m_client > 0)
            {
                ClientEvent.RemoveClient(m_client);
                Moac_disconnect(m_client);
                m_client = 0;
            }

        }
        /**
        * @brief 获取API的版本号
        * @param ver 返回的版本号,需要至少8个字节长度的内存指针用于接收版本号，ver格式为"1.0.0"
        * @return 参考Moac_retCode定义
        */
        public Moac_retCode getVersion(byte[] ver)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_getVersion(m_client, ref ver[0]);
        }

        /**
        * @brief 获取当设备状态
        * @param status返回的状态,参考Moac_status定义
        * @return 参考Moac_retCode定义
        */
        public Moac_retCode getStatus(out uint status)
        {
            if (m_client <= 0)
            {
                status = (uint)Moac_status.S_FAIL_NOCONNECT;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_getStatus(m_client, out status);
        }

        /*
        * @brief 暂停正在进行的扫描
        * @param id 返回方法的ID,用该ID方法监听当前操作是否执行完
        * @return 参考Moac_retCode定义
        */
        public Moac_retCode pause(out int id)
        {
            if (m_client <= 0)
            {
                id = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_pause(m_client, out id);
        }

        /*
        *@brief从暂停中恢复运行
        *@param id 返回方法的ID,用该ID方法监听当前操作是否执行完
        * @return 参考Moac_retCode定义
        */
        public Moac_retCode resume(out int id)
        {
            if (m_client <= 0)
            {
                id = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_resume(m_client, out id);
        }

        /*
        * @brief从设备复位
        * @param id 返回方法的ID,用该ID方法监听当前操作是否执行完
        * @return 参考Moac_retCode定义
        */
        public Moac_retCode reset(out int id)
        {
            if (m_client <= 0)
            {
                id = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_reset(m_client, out id);
        }

        /*
        * @brief 取消正在进行的扫描
        * @param id 返回方法的ID,用该ID方法监听当前操作是否执行完
        * @return 参考Moac_retCode定义
        */
        public Moac_retCode cancel(out int id)
        {
            if (m_client <= 0)
            {
                id = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_cancel(m_client, out id);
        }

        /*
         *@brief获取载物台的当前坐标       
         *@param cx,cy 返回的当前坐标
         *@return 参考Moac_retCode定义
         */
        public Moac_retCode getXY(out float cx, out float cy)
        {
            if (m_client <= 0)
            {
                cx = 0.0f;
                cy = 0.0f;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_getXY(m_client, out cx, out cy);
        }

        /*
         *@brief移动载物台到指定位置
         *@param id 返回方法的ID,用该ID方法监听当前操作是否执行完
         *@param x,y 载物台的座标
         *@return 参考Moac_retCode定义
         */
        public Moac_retCode setXY(out int id, float x, float y)
        {
            if (m_client <= 0)
            {
                id = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_setXY(m_client, out id, x, y);
        }

        /*
         *@brief移动载物台到原点位置
         *@param id 返回方法的ID,用该ID方法监听当前操作是否执行完
         *@return 参考Moac_retCode定义
         */
        public Moac_retCode setXYOrigin(out int id)
        {
            if (m_client <= 0)
            {
                id = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_setXYOrigin(m_client, out id);
        }

        /*
         *@brief获取Z轴的当前坐标        
         *@param cz 返回的当前坐标
         *@return 参考Moac_retCode定义
         */
        public Moac_retCode getZ(out float cz)
        {
            if (m_client <= 0)
            {
                cz = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_getZ(m_client, out cz);
        }


        /*
        *@brief移动Z轴到指定位置
        *@param id 返回方法的ID,用该ID方法监听当前操作是否执行完
        *@param z Z轴的座标
        *@return 参考Moac_retCode定义
        */
        public Moac_retCode setZ(out int id, float z)
        {
            if (m_client <= 0)
            {
                id = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_setZ(m_client, out id, z);
        }


        /*
          *@brief设置物镜倍数
          *@param id 返回方法的ID,用该ID方法监听当前操作是否执行完
          *@param obj 物镜倍数，取值为5,10,20,40,50,100
          *@return 参考Moac_retCode定义
          */
        public Moac_retCode setObjective(out int id, int obj)
        {
            if (m_client <= 0)
            {
                id = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_setObjective(m_client,out id, obj);
        }


        /*
        *@brief获取摄像头的分辨率
        *@param x,y 返回当前的分辨率，如果不关心当前的分辨率，可以为0
        *@param w,h 返回的分辨率数组，如果不需要返回，可以为0，否则一般分辨率有3个，建议w/h至少有4个int内存
        *@param count 输入指定w,h的大小，返回实际分辨率个数，如果不关心可以为0
        *@return 参考Moac_retCode定义
        */
        public Moac_retCode getResolution(out int x, out int y, int[] w, int[] h, ref int count)
        {
            if (m_client <= 0)
            {
                x = y = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_getResolution(m_client,out x, out y, w, h, ref count);
        }

        /*
        *@brief修改摄像头的分辨率
        *@param w,h 分辨率尺寸
        *@return 参考Moac_retCode定义
        */
        public Moac_retCode setResolution(int w, int h)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_setResolution(m_client, w, h);
        }

        /*
        *@brief设置曝光
        *@param bAuto 是否启动自动曝光 true自动曝光，false手动曝光
        *@param value 设定曝光值，单位为毫秒,仅在手动曝光时有效
        *@return 参考Moac_retCode定义
        */
        public Moac_retCode setExposure(bool bAuto, float value)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_setExposure(m_client, bAuto, value);
        }

        /*
        *@brief获取曝光范围
        *@param fmin 返回的最小曝光值，单位为毫秒
        *@param fmax 返回的最大曝光值,单位为毫秒
        *@param fcur 返回的当前曝光值，单位为毫秒
        *@return 参考Moac_retCode定义
        */
        public Moac_retCode getExposure(out float fmin, out float fmax, out float fcur)
        {
            if (m_client <= 0)
            {
                fmin = fmax = fcur = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_getExposure(m_client, out fmin, out fmax, out fcur);
        }

        /*
        *@brief 设定亮度值
        *@param value 当前的亮度值，取值范围由getBrightness获取
        *@return 参考Moac_retCode定义
        */
        public Moac_retCode setBrightness(int val)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_setBrightness(m_client, val);
        }

        /*
        *@brief 获取亮度值
        *@param imin 返回的最小亮度值
        *@param imax 返回的最大亮度值
        *@param icur 返回的当前亮度值
        *@return 参考Moac_retCode定义
        */
        public Moac_retCode getBrightness(out int imin, out int imax, out int icur)
        {
            if (m_client <= 0)
            {
                imin = imax = icur = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_getBrightness(m_client, out imin, out imax, out icur);
        }

        /*
        *@brief 设定对比度值
        *@param value 当前的对比度值，取值范围由getContrast获取
        *@return 参考Moac_retCode定义
        */
        public Moac_retCode setContrast(int val)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_setContrast(m_client, val);
        }

        /*
        *@brief 获取对比度值
        *@param imin 返回的最小对比度值
        *@param imax 返回的最大对比度值
        *@param icur 返回的当前对比度值
        *@return 参考Moac_retCode定义
        */
        public Moac_retCode getContrast(out int imin, out int imax, out int icur)
        {
            if (m_client <= 0)
            {
                imin = imax = icur = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_getContrast(m_client, out imin, out imax, out icur);
        }

        /*
        *@brief 设置增益值
        *@param value 当前的增益值，取值范围由getGain获取
        *@return 参考Moc_retCode定义
        */
        public Moac_retCode setGain(int val)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_setGain(m_client, val);
        }

        /*
        *@brief 获取增益值
        *@param imin 返回的最小增益值
        *@param imax 返回的最大增益值
        *@param icur 返回的当前增益值
        *@return 参考Moc_retCode定义
        */
        public Moac_retCode getGain(out int imin, out int imax, out int icur)
        {
            if (m_client <= 0)
            {
                imin = imax = icur = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_getGain(m_client, out imin, out imax, out icur);
        }

        /*
        *@brief 设置光亮度值
        *@param value 当前的光亮度值，取值范围由getLightBrightness获取
        *@return 参考Moc_retCode定义
        */
        public Moac_retCode setLightBrightness(int val)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_setLightBrightness(m_client, val);
        }

        /*
        *@brief 获取光亮度值
        *@param imin 返回的最小光亮度值
        *@param imax 返回的最大光亮度值
        *@param icur 返回的当前光亮度值
        *@return 参考Moc_retCode定义
        */
        public Moac_retCode getLightBrightness(out int imin, out int imax, out int icur)
        {
            if (m_client <= 0)
            {
                imin = imax = icur = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_getLightBrightness(m_client, out imin, out imax, out icur);
        }

        /*
        *@brief 设定伽马值，伽马值0.1到10.0
        *@param g 设定的伽马值，伽马值取值范围为0.1~10.0
        *@return 参考Moac_retCode定义
        */
        public Moac_retCode setGamma(float g)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_setGamma(m_client, g);
        }

        /*
        *@brief 设定定图像增强
        *@param imin 下限值，取值范围为[0,255)默认为0
        *@param imax 上限值，取值范围为(0,255]默认为255
        *@return 参考Moac_retCode定义
        */
        public Moac_retCode setEnhance(int imin, int imax)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_setEnhance(m_client, imin, imax);
        }

        /*
	    *@brief 设置色彩校正值
	    *@param enabled 是否开启色彩校正，true开启, false关闭
	    *@param val 当前的色彩校正值，取值范围(-10, 10)
	    *@return 参考Moc_retCode定义
        */
        public Moac_retCode setColorCorrection(bool enabled, int val)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_setColorCorrection(m_client, enabled, val);
        }

        /*
        *@brief 获取色彩校正值
        *@param icur 返回的当前色彩校正值
        *@return 参考Moc_retCode定义
        */
        public Moac_retCode getColorCorrection(out bool enabled, out int icur)
        {
            if (m_client <= 0)
            {
                enabled = false;
                icur = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_getColorCorrection(m_client, out enabled, out icur);
        }

        /*
	    *@brief 设置RGB亮度值
	    *@param index  指定RGB通道索引值(0:R, 1:G, 2:B)
	    *@param value 当前的亮度值，取值范围由getLightBrightness获取
	    *@return 参考Moc_retCode定义
	    */
        public Moac_retCode setRGBBrightness(int idx, int val)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_setRGBBrightness(m_client, idx, val);
        }

        /*
	    *@brief 获取RGB亮度值
	    *@param index  指定RGB通道索引值(0:R, 1:G， 2:B)
	    *@param imin 返回的最小亮度值
	    *@param imax 返回的最大亮度值
	    *@param icur 返回的当前亮度值
	    *@return 参考Moc_retCode定义
	    */
        public Moac_retCode getRGBBrightness(int idx, out int imin, out int imax, out int icur)
        {
            if (m_client <= 0)
            {
                imin = imax = icur = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_getRGBBrightness(m_client, idx, out imin, out imax, out icur);
        }

        /*
        *@brief 设置RGB增益值
        *@param index  指定RGB通道索引值(0:R, 1:G, 2:B)
        *@param value 当前的增益值，取值范围由getLightBrightness获取
        *@return 参考Moc_retCode定义
        */
        public Moac_retCode setRGBGain(int idx, int val)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_setRGBGain(m_client, idx, val);
        }

        /*
        *@brief 获取RGB增益值
        *@param index  指定RGB通道索引值(0:R, 1:G， 2:B)
        *@param imin 返回的最小增益值
        *@param imax 返回的最大增益值
        *@param icur 返回的当前增益值
        *@return 参考Moc_retCode定义
        */
        public Moac_retCode getRGBGain(int idx, out int imin, out int imax, out int icur)
        {
            if (m_client <= 0)
            {
                imin = imax = icur = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_getRGBGain(m_client, idx, out imin, out imax, out icur);
        }

        /*
	    *@brief 加载图像参数预设值
	    *@param name 被加载预设值的名称
	    *@return 参考Moc_retCode定义
	    */
        public Moac_retCode setPreset(string name)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_setPreset(m_client, name);
        }

        /*
	    *@brief 获取白平衡状态
	    *@param bOpen 返回白平衡是否打开
	    *@return 参考Moc_retCode定义
	    */
        public Moac_retCode getWhiteBalance(out bool bOpen)
        {
            if (m_client <= 0)
            {
                bOpen = false;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_getWhiteBalance(m_client, out bOpen);
        }

        /*
	    *@brief 设置白平衡
	    *@param value 0表示关闭，1表示打开，2表示重新计算并打开
	    *@return 参考Moc_retCode定义
	    */
        public Moac_retCode setWhiteBalance(int value)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_setWhiteBalance(m_client, value);
        }

        /*
        *@brief 指定区域进行自动扫描
        *@param id 返回方法的ID,用该ID方法监听当前操作是否执行完
        *@param x0,y0 扫描的起点坐标
        *@param x1,y1 扫描的终点坐标
        *@param name指定扫描后的图像的保存名称，名称不含路径，保存路径由Moac_setFilePath指定
        *@return 参考Moac_retCode定义
        */
        public Moac_retCode autoScan(out int id, float x0, float y0, float x1, float y1, string name)
        {
            if (m_client <= 0)
            {
                id = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_autoScan(m_client, out id, x0, y0, x1, y1, name);
        }

        /*
        *@brief 指定拍照与扫描图像的保存路径
        *@param filepath文件的存放路径
        *@return 参考Moac_retCode定义
        */
        public Moac_retCode setFilePath(string filepath)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_setFilePath(m_client, filepath);
        }

        /*
        *@brief 获取校准参数
        *@param x 返回的水平方向的校准参数单位为um/pixel
        *@param y 返回的垂直方向的校准参数单位为um/pixel
        *@return 参考Moac_retCode定义
        */
        public Moac_retCode getCalibration(out float x, out float y)
        {
            if (m_client <= 0)
            {
                x = y = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_getCalibration(m_client, out x, out y);
        }

        /*
        *@brief 抓取一张摄像头的图像
        *@param jpg 指定内存用于接收抓取的jpg图像，内存大小可通过当前分辨率评估一般w*h*3
        *@param size 设定与返回jpg的内存长度
        *@param module ture使用已有模型调整到焦面后抓取, false 不调整焦面直接抓取
        *@return 参考Moac_retCode定义
        */
        public Moac_retCode snap(byte[] img, ref int size, bool module)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_snap(m_client, img, ref size, module);
        }

        /*
        *@brief 测量指定的坐标范围的实际尺寸
        *@param x0,y0 指定的坐标起点
        *@param x1,y1 指定的坐标终点
        *@param w,h 返回的实际图像宽高
        *@return 参考Moac_retCode定义
        */
        public Moac_retCode measure(float x0, float y0, float x1, float y1, out float w, out float h)
        {
            if (m_client <= 0)
            {
                w = h = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_measure(m_client, x0, y0, x1, y1, out w, out h);
        }

        /*
        *@brief 拍一张摄像头的图像
        *@param fullName 图像保存的文件名，全路径的文件名称
        *@param module ture使用已有模型调整到焦面后拍照, false 不调整焦面直接拍照
        *@return 参考Moac_retCode定义
        */
        public Moac_retCode capture(string fullname, bool module)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_capture(m_client, fullname, module);
        }

        /**
         * @brief 设置样品的高度，注意由于载物台可以手动移动，为了正确估计位置，请先做标定。
         *        标定方法是将手动载物台移动到一个固定位置，并在固件位置做标样的限位与焦面标定
         * @param id 命令执行的ID，通过回调函数返回对应ID的命令执行情况
         * @param h 样品的高度um为单位
         * @return 参考Moc_retCode定义
         */
        public Moac_retCode sampleHeight(out int id, int h)
        {
            if (m_client <= 0)
            {
                id = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_sampleHeight(m_client, out id, h);
        }

        /**
         * @brief 自动聚焦，由于载物台的可变范围太大，自动聚焦只会在指定位置上下小范围内查找焦面，为了尽量正确查找焦面，请先做标定。
         *        标定方法是将手动载物台移动到一个固定位置，并在固件位置做标样的限位与焦面标定
         * @param id 命令执行的ID，通过回调函数返回对应ID的命令执行情况
         * @param range 表示对焦的Z轴查找范围（默认为100),单位为微米
         * @return 参考Moc_retCode定义
         */
        public Moac_retCode autoFocus(out int id, int range)
        {
            if (m_client <= 0)
            {
                id = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_autoFocus(m_client, out id, range);
        }

        /**
        * @brief 设置自动扫描后在拼接图中显示的标尺。
        * @param show 控制标尺的显示，uShow=0表示不显示，1表示仅显示水平，2表示仅显示垂直，3表示水平与垂直都显示（默认为3）
        * @param clr 标尺的颜色RGB值每个占两位如0xFF0000表示红色，0x00FF00表示绿色，0x0000FF表示蓝色，默认为0表示黑色
        * @param size显示标尺的长度（微米单位），默认值为0表示自适应，由程序内部自动给出显示的长度
        * @param position 表示标尺显示显示的位置，0表示在右下角（默认），1表示左下角，2表示左上角，3表示右上角。
        * @param thickness 表示标尺线的像素宽度，type=0默认16个像素,type=1默认2个像素
        * @param type标尺类型 0表示拼接图中的标尺，1表示拍照图中的标尺
        * @return 参考Moc_retCode定义
        */
        public Moac_retCode scaleBar(uint show, uint clr, int size, int position, int thickness, int type)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_scaleBar(m_client, show, clr, size, position, thickness, type);
        }

        /**
        * @brief 设置是否以灰度图显示。
        * @param gray true表示灰度显示，false表示彩色显示（默认） 
        * @return 参考Moc_retCode定义
        */
        public Moac_retCode grayImage(bool gray)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_grayImage(m_client, gray);
        }

        /**
        * @brief 设置自动扫描的对焦功能 
        * @param type 表示扫描的模式，0表示直接扫描(如果有手动建模开启会自动应用)，1表示每个视场都做自动对焦操作进行扫描,
        *             2表示自动建模并用模型调整对焦进行扫描(注意如果有手动建模调用这个扫描后手动模型会被新的自动替换)，
        *             3表示进行EDF操作(如果有手动建模开启会自动应用),4表示建模加EDF(注意如果有手动建模调用这个扫描后手动模型会被新的自动替换)
        * @param range  对应操作的Z轴范围，单位为微米，仅当type=1，2，4 时有效, 默认range=100 表示聚焦的Z扫描范围
        * @param range1 对应操作的Z轴范围，单位为微米，仅当type=3，4 时有效, 默认range=20, 表示EDF的Z扫描范围
        * @param step仅当type=3,4 时有效,表示每个视场做EDF的层数
        * @return 参考Moc_retCode定义
        */
        public Moac_retCode scanMode(int type, int range, int range1, int step)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_scanMode(m_client, type, range, range1, step);
        }

        /**
         * @brief 设置自动/手动控制，在自动模式下可通过SDK开控制设置，手动模式下不可控制，只能反馈状态
         * @param bAuto 表示true为自动模式，false为手动模式
         * @return 参考Moc_retCode定义
         */
        public Moac_retCode setAutoControl(bool bAuto)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_setAutoControl(m_client, bAuto);
        }

        /**
        * @brief 查寻当前的自动/手动控制状态
        * @return true表示当前处于自动控制状态，false表示当前处于手动控制状态
        */
        public bool getAutoControl()
        {
            if (m_client <= 0) return false;
            return Moac_getAutoControl(m_client);
        }

        /*
        *@brief 拍一张执行EDF后的摄像头图像
        *@param id 返回方法的ID,用该ID方法监听当前操作是否执行完
        *@param step 表示EDF的z轴层数（默认为10）
        *@param range 设置EDF的Z轴查找范围（当前坐标-range ~ 当前坐标+range）, range默认为20,单位为微米，
        *@param fullname 图像保存的全路径名称，扩展名可以是jpg,bmp或png中的一个
        *@return 参考Moc_retCode定义
        */
        public Moac_retCode captureEx(out int id, int step, int range, string fullname)
        {
            if (m_client <= 0)
            {
                id = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_captureEx(m_client, out id, step, range, fullname);
        }

        /*
         *@brief获取载物台的旋转属性        
         *@param rotary 载物台是否支持旋转，1可旋转，0不可旋转
         *@return 参考Moac_retCode定义
        */
        public Moac_retCode getRotaryProperty(out int rotary)
        {
            if (m_client <= 0)
            {
                rotary = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_getRotaryProperty(m_client, out rotary);
        }

        /*
        *@brief获取A轴的当前角度        
        *@param ca 返回的当前角度
        *@return 参考Moac_retCode定义
        */
        public Moac_retCode getA(out float ca)
        {
            if (m_client <= 0)
            {
                ca = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_getA(m_client, out ca);
        }

        /*
        *@brief 设置旋转载物台旋转角度
        *@param id 返回方法的ID,用该ID方法监听当前操作是否执行完
        * @param a 载物台从当前位置(deg>0)顺时针或(deg<0)逆时针旋转abs(deg)角度  
        * @return 参考Moc_retCode定义
        */
        public Moac_retCode setA(out int id, float a)
        {
            if (m_client <= 0)
            {
                id = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_setA(m_client, out id, a);
        }

        /*
         *@brief 旋转载物台到指定样品位置
         *@param id 返回方法的ID,用该ID方法监听当前操作是否执行完
         * @param index   index=0,1,2...n  表示要旋转到第一个，第二个...第n+1个样品位置
         * @return 参考Moc_retCode定义
        */
        public Moac_retCode setSample(out int id, int index)
        {
            if (m_client <= 0)
            {
                id = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_setSample(m_client, out id, index);
        }

        /*
         *@brief 获取当前样品位置
         * @param index   返回的样品位置index=0,1,2...n  表示要旋转到第一个，第二个...第n+1个样品位置
         * @return 参考Moc_retCode定义
        */
        public Moac_retCode getSample(out int index)
        {
            if (m_client <= 0)
            {
                index = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_getSample(m_client, out index);
        }

        /*
        *@brief 设置九点建模扫描时的聚焦频率
        * @param times   times取值>=0。times=0表示自动设定; 当times=1,2,3...，表示每移动times个视场做一次聚焦
        * @return 参考Moc_retCode定义
        */
        public Moac_retCode setFocusingFrequency(int times)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_setFocusingFrequency(m_client, times, 0);
        }

        /*
        *@ brief 设置开启Htpp上传
        *@ param enable 是否开启http post模式，true 开启，false不开启
        *@ return 参考Moc_retCode定义,成功返回RC_FINISH 
        */
        public Moac_retCode enableHttpPost(bool enable)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_enableHttpPost(m_client, enable);
        }

        /*
        * @brief 设置Http上传表单中的参数名与参数值，可多次调用
        * @param key 参数名,
        * @param value 参数值
        *@ return 参考Moc_retCode定义,成功返回RC_FINISH
        */
        public Moac_retCode setHttpParam(string key, string value)
        {
            if (m_client <= 0) return Moac_retCode.RC_ERR_NOCONNECT;
            return (Moac_retCode)Moac_setHttpParam(m_client, key, value);
        }

        /*
        * @brief 设置Http上传图像与setHttpParam设定的参数
        * *@param id 返回方法的ID,用该ID方法监听当前操作是否执行完
        * @param path 需要上传图像所在的文件或文件夹路径，路径为绝对路径； 
        * 如：path=D:/dir/picture.jpg 表示上传dir目录中的picture.jpg文件； path=D:/dir 表示上传文件夹dir中所有图像文件
        *@ return 参考Moc_retCode定义,成功返回RC_FINISH
        */
        public Moac_retCode PostHttpImage(out int id, string path)
        {
            if (m_client <= 0)
            {
                id = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_PostHttpImage(m_client, out id, path);
        }

        /**
        * @brief 添加建模点用于建模，调用该接口设备会移动载物台到指定位置，做自动聚焦找到焦面z位置并记录。
        *        满足建模条件至少添加3点有效的建模点,理想情况是均匀分布的9个点，最后调用Moac_enableScanMode完成建模 
        * @param id 返回方法的ID,用该ID方法监听当前操作是否执行成功
        * @param x,y 载物台的座标
        * @param range 表示对焦的Z轴查找范围（默认为100),单位为微米
        */
        public Moac_retCode addModulePoint(out int id, float x, float y, int range)
        {
            if(m_client == 0)
            {
                id = 0;
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_addModulePoint(m_client, out id, x, y, range);
        }

        /**
       * @brief 添加建模点用于建模，调用该接口不需要自动找焦面，给定的z即指定位置的焦面，直接记录。
       *        满足建模条件至少添加3点有效的扫描点,理想情况是均匀分布的9个点，最后调用Moac_enableScanMode完成建模       
       * @param x,y 载物台的坐标
       * @param z z轴坐标，当前位置的焦面坐标
       */
        public Moac_retCode addModulePoint2(float x, float y, float z)
        {
            if (m_client <= 0)
            {
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_addModulePoint2(m_client, x, y, z);
        }

        /**
        *@brief 清除前面添加的所有扫描点
        */
        public Moac_retCode clearModulePoints()
        {
            if (m_client <= 0)
            {
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_clearModulePoints(m_client);
        }

        /**
         *@brief 启用扫描模型，如果修改过模型点会重新创建模型。启用模型后扫描模式Moac_scanMode设置type=0与3就会自动使用模型。         
         *@return 参考Moc_retCode定义,成功返回RC_FINISH,失败表示没有足够的模型点返回RC_ERR_PARAM
        */
        public Moac_retCode enableScanModule()
        {
            if(m_client< 0)
            {
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_enableScanModule(m_client);
        }

        /**
        *@brief 禁用扫描模型
       */
        public Moac_retCode disableScanModule()
        {
            if (m_client < 0)
            {
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_disableScanModule(m_client);
        }

        /**
        * @brief 设置自动曝光目标模式
        * @param client Moac_connect返回的连接标识
        * @param mode 0表示全图区域, 1表示局部最亮区域
        * @return 参考Moc_retCode定义,成功返回RC_FINISH
        */
        public Moac_retCode setAETargetMode(int mode)
        {
            if (m_client < 0)
            {
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_setAETargetMode(m_client, mode);
        }

        /**
        * @brief 设置自动聚焦区域
        *@param x,y 指定聚焦区域的左上点图像像素坐标
        *@param w,h 指定聚焦区域的大小，以图像像素为单位
        * @return 参考Moc_retCode定义,成功返回RC_FINISH
        */
        public Moac_retCode setFocusRoi(int x, int y, int w, int h)
        {
            if (m_client < 0)
            {
                return Moac_retCode.RC_ERR_NOCONNECT;
            }
            return (Moac_retCode)Moac_setFocusRoi(m_client, x,y,w, h);
        }

        /**
        *@brief 设定回调函数
        */
        public int setCallBack(MoacCallback cb)
        {
            if (m_client <= 0) return -1;            
            m_cb = cb;
            return 0;
        }
        public void OnEventCallback(int id, int status)
        {
            if(m_cb != null)
            {
                m_cb(id, status);
            }
        }
       
        const string DLL_NAME = "MoticAnalysisClient.dll";

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_connect", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_connect(char[] ip, int port);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_disconnect", CallingConvention = CallingConvention.Winapi)]
        public static extern void Moac_disconnect(int client);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_getVersion", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_getVersion(int client, ref byte ver);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_getStatus", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_getStatus(int client, out uint status);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_pause", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_pause(int client, out int id);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_resume", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_resume(int client, out int id);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_reset", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_reset(int client, out int id);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_cancel", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_cancel(int client, out int id);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setObjective", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setObjective(int client, out int id, int obj);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setXY", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setXY(int client, out int id, float x, float y);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setXYOrigin", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setXYOrigin(int client, out int id);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setZ", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setZ(int client, out int id, float z);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_getXY", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_getXY(int client, out float cx, out float cy);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_getZ", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_getZ(int client, out float cz);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_getResolution", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_getResolution(int client, out int x, out int y, int[] w, int[] h, ref int count);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setResolution", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setResolution(int client, int w, int h);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setExposure", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setExposure(int client, bool bAuto, float value);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_getExposure", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_getExposure(int client, out float fmin, out float fmax, out float fcur);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setBrightness", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setBrightness(int client, int val);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_getBrightness", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_getBrightness(int client, out int imin, out int imax, out int icur);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setContrast", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setContrast(int client, int val);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_getContrast", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_getContrast(int client, out int imin, out int imax, out int icur);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setGain", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setGain(int client, int val);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_getGain", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_getGain(int client, out int imin, out int imax, out int icur);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setLightBrightness", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setLightBrightness(int client, int val);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_getLightBrightness", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_getLightBrightness(int client, out int imin, out int imax, out int icur);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setColorCorrection", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setColorCorrection(int client, bool enabled, int val);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_getColorCorrection", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_getColorCorrection(int client, out bool enabled, out int icur);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setRGBBrightness", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setRGBBrightness(int client, int idx, int val);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_getRGBBrightness", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_getRGBBrightness(int client, int idx, out int imin, out int imax, out int icur);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setRGBGain", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setRGBGain(int client, int idx, int val);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_getRGBGain", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_getRGBGain(int client, int idx, out int imin, out int imax, out int icur);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setPreset", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setPreset(int client, string name);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setWhiteBalance", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setWhiteBalance(int client, int val);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_getWhiteBalance", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_getWhiteBalance(int client, out bool bOpen);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setGamma")]
        public static extern int Moac_setGamma(int client, float val);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setEnhance", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setEnhance(int client, int imin, int imax);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_autoScan", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_autoScan(int client, out int id, float x0, float y0, float x1, float y1, string name);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setFilePath", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setFilePath(int client, string filepath);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_getCalibration", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_getCalibration(int client, out float x, out float y);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_snap", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_snap(int client, byte[] jpg, ref int size, bool bModule);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_capture", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_capture(int client, string fullname, bool bModule);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_measure", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_measure(int client, float x0, float y0, float x1, float y1, out float w, out float h);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_sampleHeight", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_sampleHeight(int client, out int id, int h);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_autoFocus", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_autoFocus(int client, out int id, int range);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_scaleBar", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_scaleBar(int client, uint show, uint clr, int size, int position, int thickness, int type);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_grayImage", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_grayImage(int client, bool gray);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_scanMode", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_scanMode(int client, int type, int range, int range1, int step);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setAutoControl", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setAutoControl(int client, bool bAuto);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_getAutoControl", CallingConvention = CallingConvention.Winapi)]
        public static extern bool Moac_getAutoControl(int client);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_captureEx", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_captureEx(int client, out int id, int step, int range, string fullname);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_getRotaryProperty", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_getRotaryProperty(int client, out int r);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setA", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setA(int client, out int id, float a);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_getA", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_getA(int client, out float ca);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setSample", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setSample(int client, out int id, int index);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_getSample", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_getSample(int client, out int index);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setFocusingFrequency", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setFocusingFrequency(int client, int times, float step);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_enableHttpPost", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_enableHttpPost(int client, bool bOpen);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setHttpParam", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setHttpParam(int client, string key, string value);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_PostHttpImage", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_PostHttpImage(int client, out int id, string file);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_addModulePoint", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_addModulePoint(int client, out int id, float x, float y, int range);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_addModulePoint2", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_addModulePoint2(int client, float x, float y, float z);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_clearModulePoints", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_clearModulePoints(int client);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_enableScanModule", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_enableScanModule(int client);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_disableScanModule", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_disableScanModule(int client);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setAETargetMode", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setAETargetMode(int client, int mode);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, EntryPoint = "Moac_setFocusRoi", CallingConvention = CallingConvention.Winapi)]
        public static extern int Moac_setFocusRoi(int client, int x, int y, int w, int h);

        [DllImport(DLL_NAME, EntryPoint = "Moac_setEventCallback", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Moac_setEventCallback(Delegate callback);

        [UnmanagedFunctionPointerAttribute(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void MoacCallback(int id, int status); 

        [UnmanagedFunctionPointerAttribute(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void ClientCallback(int client, int id, int status);
    }
}
