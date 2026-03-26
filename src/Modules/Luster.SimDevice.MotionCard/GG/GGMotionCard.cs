#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       GGMotionCard
* 机器名称:       F05852
* 命名空间:       Luster.SimDevice.MotionCard.GG
* 文 件 名:       GGMotionCard.cs
* 创建时间:       2022/10/14 16:02:08
* 作    者:       F05852
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       leifan@lusterinc.com 
* 唯一标识：      28004d9b-342e-4c9f-a83a-940134dd710e
* 登录用户:       樊磊
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/14 16:02:08
* 修 改 人:		  F05852
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Adapter;
using Luster.SimDevice.MotionCard.LC;
using Luster.SimDevice.MotionCards;
using Luster.SimDevice.Real;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using static Luster.SimDevice.MotionCard.GG.mc;
using static System.Collections.Specialized.BitVector32;

namespace Luster.SimDevice.MotionCard.GG
{
    /// <summary>
    /// 固高板卡 
    /// 1.IO索引是从0开始，调用者的IO索引也应该从0开始。待软件更新索引支持0开始
    /// 固高卡的单位是plus/ms 不是plus/s,这个是个坑，注意！！！
    /// </summary>
    public class GGMotionCard : MotionCardBase, IMotionCard
    {
        public override string Brand => "固高";

        /// <summary>
        /// 主卡的插槽（插在电脑PCIE接口上的卡）
        /// </summary>
        private short cardNo = 0;

        /// <summary>
        /// 睡眠时间
        /// </summary>
        private readonly int SleepTime = 5;


        #region 内部变量

        #region 内部类型
        /// <summary>
        /// 主卡对应的外接卡类型
        /// </summary>
        enum CardType
        {
            Null = 0,
            /// <summary>
            /// 轴卡
            /// </summary>
            AxisCard = 1,
            /// <summary>
            /// 扩展卡
            /// </summary>
            ExtCard = 2
        }

        #endregion

        /// <summary>
        /// 一张控制卡，有两个内核。内核1序号
        /// </summary>
        private short coreNum1 = 1;
        /// <summary>
        /// 一张控制卡，有两个内核。内核2序号
        /// </summary>
        private short coreNum2 = 2;

        /// <summary>
        /// 内核1的轴数
        /// </summary>
        private short axisCountCore1 = 0;
        /// <summary>
        /// 内核2的轴数
        /// </summary>
        private short axisCountCore2 = 0;

        /// <summary>
        /// 内核1的通用DI数量
        /// </summary>
        short count_GPI_Core1 = 0;
        /// <summary>
        /// 内核2的通用DI数量
        /// </summary>
        short count_GPI_Core2 = 0;
        /// <summary>
        /// 内核1的扩展DI数量
        /// </summary>
        short count_EXT_DI_Core1 = 0;
        /// <summary>
        /// 内核2的扩展DI数量
        /// </summary>
        short count_EXT_DI_Core2 = 0;

        /// <summary>
        /// 内核1的通用DO数量
        /// </summary>
        short count_GPO_Core1 = 0;
        /// <summary>
        /// 内核2的通用DO数量
        /// </summary>
        short count_GPO_Core2 = 0;
        /// <summary>
        /// 内核1的扩展DO数量
        /// </summary>
        short count_EXT_DO_Core1 = 0;
        /// <summary>
        /// 内核1的扩展DO数量
        /// </summary>
        short count_EXT_DO_Core2 = 0;

        /// <summary>
        /// 内核1的通用AI数量
        /// </summary>
        short count_ADC_Core1 = 0;
        /// <summary>
        /// 内核2的通用AI数量
        /// </summary>
        short count_ADC_Core2 = 0;
        /// <summary>
        /// 内核1的扩展AI数量
        /// </summary>
        short count_EXT_AI_Core1 = 0;
        /// <summary>
        /// 内核2的扩展AI数量
        /// </summary>
        short count_EXT_AI_Core2 = 0;

        /// <summary>
        /// 内核1的通用AO数量
        /// </summary>
        short count_DAC_Core1 = 0;
        /// <summary>
        /// 内核2的通用AO数量
        /// </summary>
        short count_DAC_Core2 = 0;
        /// <summary>
        /// 内核1的扩展AO数量
        /// </summary>
        short count_EXT_AO_Core1 = 0;
        /// <summary>
        /// 内核2的扩展AO数量
        /// </summary>
        short count_EXT_AO_Core2 = 0;

        // 保存内部回零是否完成，一个主卡最大支持24根轴
        Dictionary<int, bool> zeroDone = new Dictionary<int, bool>()
        {
            {1, false},{2, false},{3, false},{4, false},{5, false},{6, false},{7, false},{8, false},
            {9, false},{10, false},{11, false},{12, false},{13, false},{14, false},{15, false},{16, false},
            {17, false},{18, false},{19, false},{20, false},{21, false},{22, false},{23, false},{24, false}
        };

        /// <summary>
        /// 类的静态变量，代表连接主卡的个数。
        /// </summary>
        private static int initCardCount = 0;

        /// <summary>
        /// 锁变量，初始化、关闭卡使用
        /// </summary>
        private object locker = new object();
        #endregion

        #region 内部函数

        /// <summary>
        /// 根据外部调用轴号，计算出主卡对应的内核序号、轴序号
        /// </summary>
        /// <param name="axisNo">外部调用轴号，从1开始</param>
        /// <param name="coreNum">内核序号，从1开始</param>
        /// <param name="axisNum">内部轴号，从1开始</param>
        /// <returns></returns>
        private void GetAxisNum(int axisNo, out short coreNum, out short axisNum)
        {
            coreNum = 0;
            axisNum = 0;

            if (axisNo > 0 && axisNo <= axisCountCore1)
            {
                coreNum = coreNum1;
                axisNum = (short)axisNo;
            }
            else if (axisNo > axisCountCore1 && axisNo <= axisCountCore1 + axisCountCore2)
            {
                coreNum = coreNum2;
                axisNum = (short)(axisNo - axisCountCore1); ;
            }
            else
            {
                string msgHead = $"{Brand} 检查轴编号失败。";
                throw new ArgumentOutOfRangeException($"{msgHead}原因：轴编号 {axisNo} 超出允许范围[{1},{axisCountCore1 + axisCountCore2}]。");
            }
        }

        /// <summary>
        /// 根据外部调用DI序号，计算出主卡对应的内核序号、扩展卡类型、扩展卡DI序号
        /// </summary>
        /// <param name="DiNo">外部调用DI序号，从1开始</param>
        /// <param name="coreNum">内核序号，从1开始</param>
        /// <param name="cardType">扩展卡类型</param>
        /// <param name="diNum">扩展卡DI序号，从1开始</param>
        /// <returns></returns>
        private void GetDiNum(int DiNo, out short coreNum, out CardType cardType, out short diNum)
        {
            coreNum = 0;
            cardType = CardType.Null;
            diNum = 0;

            if (DiNo > 0 && DiNo <= count_GPI_Core1)
            {
                coreNum = coreNum1;
                cardType = CardType.AxisCard;
                diNum = (short)DiNo;
            }
            else if (DiNo > count_GPI_Core1 && DiNo <= count_GPI_Core1 + count_GPI_Core2)
            {
                coreNum = coreNum2;
                cardType = CardType.AxisCard;
                diNum = (short)(DiNo - count_GPI_Core1);
            }
            else if (DiNo > count_GPI_Core1 + count_GPI_Core2
                && DiNo <= count_GPI_Core1 + count_GPI_Core2 + count_EXT_DI_Core1)
            {
                coreNum = coreNum1;
                cardType = CardType.ExtCard;
                diNum = (short)(DiNo - count_GPI_Core1 - count_GPI_Core2);
            }
            else if (DiNo > count_GPI_Core1 + count_GPI_Core2 + count_EXT_DI_Core1
                && DiNo <= count_GPI_Core1 + count_GPI_Core2 + count_EXT_DI_Core1 + count_EXT_DI_Core2)
            {
                coreNum = coreNum2;
                cardType = CardType.ExtCard;
                diNum = (short)(DiNo - count_GPI_Core1 - count_GPI_Core2 - count_EXT_DI_Core1);
            }
            else
            {
                string msgHead = $"{Brand} 检查DI编号失败。";
                throw new ArgumentOutOfRangeException($"{msgHead}原因：DI编号 {DiNo} 超出允许范围[{1},{count_GPI_Core1 + count_GPI_Core2 + count_EXT_DI_Core1 + count_EXT_DI_Core2}]。");
            }
        }

        /// <summary>
        /// 根据外部调用Do序号，计算出主卡对应的内核序号、扩展卡类型、扩展卡Do序号
        /// </summary>
        /// <param name="DoNo">外部调用Do序号，从1开始</param>
        /// <param name="coreNum">内核序号，从1开始</param>
        /// <param name="cardType">扩展卡类型</param>
        /// <param name="doNum">扩展卡Do序号，从1开始</param>
        /// <returns></returns>
        private void GetDoNum(int DoNo, out short coreNum, out CardType cardType, out short doNum)
        {
            coreNum = 0;
            cardType = CardType.Null;
            doNum = 0;

            if (DoNo > 0 && DoNo <= count_GPO_Core1)
            {
                coreNum = coreNum1;
                cardType = CardType.AxisCard;
                doNum = (short)DoNo;
            }
            else if (DoNo > count_GPO_Core1 && DoNo <= count_GPO_Core1 + count_GPO_Core2)
            {
                coreNum = coreNum2;
                cardType = CardType.AxisCard;
                doNum = (short)(DoNo - count_GPO_Core1);
            }
            else if (DoNo > count_GPO_Core1 + count_GPO_Core2
                && DoNo <= count_GPO_Core1 + count_GPO_Core2 + count_EXT_DO_Core1)
            {
                coreNum = coreNum1;
                cardType = CardType.ExtCard;
                doNum = (short)(DoNo - count_GPO_Core1 - count_GPO_Core2);
            }
            else if (DoNo > count_GPO_Core1 + count_GPO_Core2 + count_EXT_DO_Core1
                && DoNo <= count_GPO_Core1 + count_GPO_Core2 + count_EXT_DO_Core1 + count_EXT_DO_Core2)
            {
                coreNum = coreNum2;
                cardType = CardType.ExtCard;
                doNum = (short)(DoNo - count_GPO_Core1 - count_GPO_Core2 - count_EXT_DO_Core1);
            }
            else
            {
                string msgHead = $"{Brand} 检查DO编号失败。";
                throw new ArgumentOutOfRangeException($"{Brand} DO编号 {DoNo} 超出允许范围[{1},{count_GPO_Core1 + count_GPO_Core2 + count_EXT_DO_Core1 + count_EXT_DO_Core2}]。");
            }
        }

        /// <summary>
        /// 根据外部调用AI序号，计算出主卡对应的内核序号、扩展卡类型、扩展卡AI序号
        /// </summary>
        /// <param name="AiNo">外部调用DI序号，从1开始</param>
        /// <param name="coreNum">内核序号，从1开始</param>
        /// <param name="cardType">扩展卡类型</param>
        /// <param name="aiNum">扩展卡AI序号，从1开始</param>
        /// <returns></returns>
        private void GetAiNum(int AiNo, out short coreNum, out CardType cardType, out short aiNum)
        {
            coreNum = 0;
            cardType = CardType.Null;
            aiNum = 0;

            if (AiNo > 0 && AiNo <= count_ADC_Core1)
            {
                coreNum = coreNum1;
                cardType = CardType.AxisCard;
                aiNum = (short)AiNo;
            }
            else if (AiNo > count_ADC_Core1 && AiNo <= count_ADC_Core1 + count_ADC_Core2)
            {
                coreNum = coreNum2;
                cardType = CardType.AxisCard;
                aiNum = (short)(AiNo - count_ADC_Core1);
            }
            else if (AiNo > count_ADC_Core1 + count_ADC_Core2
                && AiNo <= count_ADC_Core1 + count_ADC_Core2 + count_EXT_AI_Core1)
            {
                coreNum = coreNum1;
                cardType = CardType.ExtCard;
                aiNum = (short)(AiNo - count_ADC_Core1 - count_ADC_Core2);
            }
            else if (AiNo > count_ADC_Core1 + count_ADC_Core2 + count_EXT_AI_Core1
                && AiNo <= count_ADC_Core1 + count_ADC_Core2 + count_EXT_AI_Core1 + count_EXT_AI_Core2)
            {
                coreNum = coreNum2;
                cardType = CardType.ExtCard;
                aiNum = (short)(AiNo - count_ADC_Core1 - count_ADC_Core2 - count_EXT_AI_Core1);
            }
            else
            {
                string msgHead = $"{Brand} 检查AI编号失败。";
                throw new ArgumentOutOfRangeException($"{Brand} AI编号 {AiNo} 超出允许范围[{1},{count_ADC_Core1 + count_ADC_Core2 + count_EXT_AI_Core1 + count_EXT_AI_Core2}]。");
            }
        }

        /// <summary>
        /// 根据外部调用Ao序号，计算出主卡对应的内核序号、扩展卡类型、扩展卡Ao序号
        /// </summary>
        /// <param name="AoNo">外部调用Ao序号，从1开始</param>
        /// <param name="coreNum">内核序号，从1开始</param>
        /// <param name="cardType">扩展卡类型</param>
        /// <param name="doNum">扩展卡Ao序号，从1开始</param>
        /// <returns></returns>
        private void GetAoNum(int AoNo, out short coreNum, out CardType cardType, out short aoNum)
        {
            coreNum = 0;
            cardType = CardType.Null;
            aoNum = 0;

            if (AoNo > 0 && AoNo <= count_DAC_Core1)
            {
                coreNum = coreNum1;
                cardType = CardType.AxisCard;
                aoNum = (short)AoNo;
            }
            else if (AoNo > count_DAC_Core1 && AoNo <= count_DAC_Core1 + count_DAC_Core2)
            {
                coreNum = coreNum2;
                cardType = CardType.AxisCard;
                aoNum = (short)(AoNo - count_DAC_Core1);
            }
            else if (AoNo > count_DAC_Core1 + count_DAC_Core2
                && AoNo <= count_DAC_Core1 + count_DAC_Core2 + count_EXT_AO_Core1)
            {
                coreNum = coreNum1;
                cardType = CardType.ExtCard;
                aoNum = (short)(AoNo - count_DAC_Core1 - count_DAC_Core2);
            }
            else if (AoNo > count_DAC_Core1 + count_DAC_Core2 + count_EXT_AO_Core1
                && AoNo <= count_DAC_Core1 + count_DAC_Core2 + count_EXT_AO_Core1 + count_EXT_AO_Core2)
            {
                coreNum = coreNum2;
                cardType = CardType.ExtCard;
                aoNum = (short)(AoNo - count_DAC_Core1 - count_DAC_Core2 - count_EXT_AO_Core1);
            }
            else
            {
                string msgHead = $"{Brand} 检查AO编号失败。";
                throw new ArgumentOutOfRangeException($"{Brand} AO编号 {AoNo} 超出允许范围[{1},{count_DAC_Core1 + count_DAC_Core2 + count_EXT_AO_Core1 + count_EXT_AO_Core2}]。");
            }
        }

        /// <summary>
        /// 配置轴回零完成状态
        /// </summary>
        /// <param name="uAxisNo">轴号</param>
        /// <param name="isHome">是否回零完成标志</param>
        private void SetAxisHome(int uAxisNo, bool isHome)
        {
            if (zeroDone.ContainsKey(uAxisNo))
            {
                zeroDone[uAxisNo] = isHome;
            }
            else
            {
                zeroDone.Add(uAxisNo, isHome);
            }
        }

        /// <summary>
        /// 扫描获取板卡挂载外置模块的资源
        /// </summary>
        private void ScanResource()
        {
            ScanCoreResource(coreNum1, out axisCountCore1, out count_GPI_Core1, out count_GPO_Core1, out count_ADC_Core1, out count_DAC_Core1,
                out count_EXT_DI_Core1, out count_EXT_DO_Core1, out count_EXT_AI_Core1, out count_EXT_AO_Core1);

            ScanCoreResource(coreNum2, out axisCountCore2, out count_GPI_Core2, out count_GPO_Core2, out count_ADC_Core2, out count_DAC_Core2,
                out count_EXT_DI_Core2, out count_EXT_DO_Core2, out count_EXT_AI_Core2, out count_EXT_AO_Core2);

            string msgHead = $"{Brand}";
            OnLog(LogType.Debug, $"{msgHead} 扫描资源成功!"
                + $"内核{coreNum1}资源：最大轴数{axisCountCore1}、通用数字输入个数{count_GPI_Core1}、通用数字输出个数{count_GPO_Core1}、通用模拟量输入个数{count_ADC_Core1}、通用模拟量输出个数{count_DAC_Core1}"
                + $"、扩展数字输入个数{count_EXT_DI_Core1}、扩展数字输出个数{count_EXT_DO_Core1}、扩展模拟量输入个数{count_EXT_AI_Core1}、扩展模拟量输出个数{count_EXT_AO_Core1}；"
                + $"内核{coreNum2}资源：最大轴数{axisCountCore2}、通用数字输入个数{count_GPI_Core2}、通用数字输出个数{count_GPO_Core2}、通用模拟量输入个数{count_ADC_Core2}、通用模拟量输出个数{count_DAC_Core2}"
                + $"、扩展数字输入个数{count_EXT_DI_Core2}、扩展数字输出个数{count_EXT_DO_Core2}、扩展模拟量输入个数{count_EXT_AI_Core2}、扩展模拟量输出个数{count_EXT_AO_Core2}；");
        }

        /// <summary>
        /// 扫描内核上挂载的外接模块、扩展模块的资源数量
        /// </summary>
        /// <param name="coreNum">内核编号</param>
        /// <param name="axisCount">最大轴数</param>
        /// <param name="gpiCount">通用数字量输入点数量</param>
        /// <param name="gpoCount">通用数字量输出点数量</param>
        /// <param name="adcCount">通用模拟量输入点数量</param>
        /// <param name="dacCount">通用模拟量输出点数量</param>
        /// <param name="extDiCount">扩展数字量输入点数量</param>
        /// <param name="extDoCount">扩展数字量输出点数量</param>
        /// <param name="extAiCount">扩展模拟量输入点数量</param>
        /// <param name="extAoCount">扩展模拟量输出点数量</param>
        private void ScanCoreResource(short coreNum, out short axisCount,
            out short gpiCount, out short gpoCount, out short adcCount, out short dacCount,
            out short extDiCount, out short extDoCount, out short extAiCount, out short extAoCount)
        {
            string msgHead = $"{Brand}";
            string mgsHeadCore = $"{msgHead}_内核{coreNum} 扫描资源失败。";

            axisCount = 0;
            gpiCount = 0; gpoCount = 0; adcCount = 0; dacCount = 0;
            extDiCount = 0; extDoCount = 0; extAiCount = 0; extAoCount = 0;

            // 中转变量 (引用类型变量不能在委托中赋值)
            short axisCountTemp = 0;
            short gpiCountTemp = 0, gpoCountTemp = 0, adcCountTemp = 0, dacCountTemp = 0;
            short extDiCountTemp = 0, extDoCountTemp = 0, extAiCountTemp = 0, extAoCountTemp = 0;

            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                // 获取内核上挂载的模块数量 （模块等同于从站）
                ret = GTN_GetResCount(coreNum, mc.MC_TERMINAL, out short pCount);
                if (ret != 0)
                {
                    err = $"{mgsHeadCore}。原因：获取外接模块数量失败。错误码：{ret}";
                    return false;
                }
                for (short i = 1; i <= pCount; i++)
                {
                    // 获取模块上的最大可用轴数
                    ret = mc_ringnet.GTN_GetTerminalMap(coreNum, mc.MC_AXIS, i, out mc_ringnet.TTerminalMap pMap);
                    if (ret != 0)
                    {
                        err = $"{mgsHeadCore}。原因：获取模块上的最大可用轴数失败。错误码：{ret}";
                        return false;
                    }
                    axisCountTemp += pMap.dataCount;

                    // 获取模块上的通用数字量IO数量
                    ret = mc_ringnet.GTN_GetTerminalMap(coreNum, mc.MC_GPI, i, out pMap);
                    if (ret != 0)
                    {
                        err = $"{mgsHeadCore}。原因：获取模块上的通用数字量输入点数量失败。错误码：{ret}";
                        return false;
                    }
                    gpiCountTemp += pMap.dataCount;

                    ret = mc_ringnet.GTN_GetTerminalMap(coreNum, mc.MC_GPO, i, out pMap);
                    if (ret != 0)
                    {
                        err = $"{mgsHeadCore}。原因：获取模块上的通用数字量输出点数量失败。错误码：{ret}";
                        return false;
                    }
                    gpoCountTemp += pMap.dataCount;

                    // 获取模块上的通用模拟量IO数量
                    ret = mc_ringnet.GTN_GetTerminalMap(coreNum, mc.MC_AU_ADC, i, out pMap);
                    if (ret != 0)
                    {
                        err = $"{mgsHeadCore}。原因：获取模块上的通用模拟量输入点数量失败。错误码：{ret}";
                        return false;
                    }
                    adcCountTemp += pMap.dataCount;

                    ret = mc_ringnet.GTN_GetTerminalMap(coreNum, mc.MC_AU_DAC, i, out pMap);
                    if (ret != 0)
                    {
                        err = $"{mgsHeadCore}。原因：获取模块上的通用模拟量输出点数量失败。错误码：{ret}";
                        return false;
                    }
                    dacCountTemp += pMap.dataCount;

                    // 获取从站外接扩展模块的数量
                    ret = GTN_GetExtModuleCount(coreNum, i, out short pExtCount);
                    if (ret != 0)
                    {
                        err = $"{mgsHeadCore}。原因：获取从站外接扩展模块的数量失败。错误码：{ret}";
                        return false;
                    }
                    for (short j = 1; j <= pExtCount; j++)
                    {
                        // 获取扩展模块的类型 以及对应的资源数量
                        ret = GTN_GetExtModuleType(coreNum, i, j, out mc_cfg.TExtModuleType pModuleType);
                        if (ret != 0)
                        {
                            err = $"{mgsHeadCore}。原因：获取扩展模块的类型以及对应资源数量失败。错误码：{ret}";
                            return false;
                        }

                        if (pModuleType.type == 1 || pModuleType.type == 3)
                        {
                            extDiCountTemp += pModuleType.input;
                        }
                        if (pModuleType.type == 2 || pModuleType.type == 3)
                        {
                            extDoCountTemp += pModuleType.output;
                        }
                        if (pModuleType.type == 4 || pModuleType.type == 6)
                        {
                            extAiCountTemp += pModuleType.input;
                        }
                        if (pModuleType.type == 5 || pModuleType.type == 6)
                        {
                            extAoCountTemp += pModuleType.output;
                        }
                    }
                };

                return true;
            });

            axisCount = axisCountTemp;
            gpiCount = gpiCountTemp;
            gpoCount = gpoCountTemp;
            adcCount = adcCountTemp;
            dacCount = dacCountTemp;
            extDiCount = extDiCountTemp;
            extDoCount = extDoCountTemp;
            extAiCount = extAiCountTemp;
            extAoCount = extAoCountTemp;
        }

        /// <summary>
        /// 输出轴的清错信号,重置轴状态标志
        /// </summary>
        /// <param name="axisNo">轴编号</param>
        /// <param name="holdTime">信号保持时间。单位：毫秒</param>
        private void ClearAxisError(int axisNo, int holdTime = 100)
        {
            CheckInit();

            short coreNum = 0;
            short axisNum = 0;

            GetAxisNum(axisNo, out coreNum, out axisNum);

            string msgHead = $"{Brand} 清除轴{axisNo}错误失败。";
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                int maskValue = 1 << axisNum;
                ret = GTN_SetDo(coreNum, mc.MC_CLEAR, maskValue);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：输出轴清错信号失败。错误码：{ret}";
                    return false;
                }
                Thread.Sleep(holdTime);
                ret = GTN_SetDo(coreNum, mc.MC_CLEAR, 0);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：关闭轴清错信号失败。错误码：{ret}";
                    return false;
                }

                ret = GTN_ClrSts(coreNum, axisNum, 1);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：重置轴状态标志失败。错误码：{ret}";
                    return false;
                }

                return true;
            });
        }



        #endregion

        #region 设备通用方法
        public override void InitApi()
        {
            string msgHead = $"{Brand}";
            string mgsHeadCore1 = $"{msgHead}_内核{coreNum1} 初始化失败。";
            string mgsHeadCore2 = $"{msgHead}_内核{coreNum2} 初始化失败。";

            if (HasInit())
            {
                OnLog(LogType.Info, $"{msgHead} 已初始化完成。请勿重复调用!");
                return;
            }

            var pcie = Adapter as PCIe;
            if (pcie == null)
            {
                OnLog(LogType.Error, $"{msgHead} 初始化失败。原因：未配置插槽信息!");
                return;
            }

            // 设置编号 获取主卡对应的两个内核编号
            cardNo = (short)pcie.Slot;
            coreNum1 = (short)(cardNo * 2 + 1);
            coreNum2 = (short)(cardNo * 2 + 2);

            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                // 1.打开控制卡 ，只打开1次。（需要结果多张卡冲突的问题）
                lock (locker)
                {
                    if (initCardCount <= 0)
                    {
                        ret = mc.GTN_Open(mc.CHANNEL_PCIE, 2);
                        if (0 != ret)
                        {
                            Interlocked.Exchange(ref initCardCount, 0);
                            err = $"{msgHead} 初始化失败。原因：打开控制卡失败。错误码：{ret}";
                            return false;
                        }
                    }
                    Interlocked.Increment(ref initCardCount);
                }

                // 2.初始化扩展模块
                ret = GTN_ExtModuleInit(coreNum1, 1);
                if (ret != 0)
                {
                    err = $"{mgsHeadCore1}。原因：初始化扩展模块失败。错误码：{ret}";
                    return false;
                }
                ret = GTN_ExtModuleInit(coreNum2, 1);
                if (ret != 0)
                {
                    err = $"{mgsHeadCore2}。原因：初始化扩展模块失败。错误码：{ret}";
                    return false;
                }

                // 3.复位卡的内核：内核1、内核2 。复位内核的时候会将轴当前的位置清零
                //ret = GTN_Reset(coreNum1);
                //if (ret != 0)
                //{
                //    err = $"{mgsHeadCore1}。原因：重置内核失败。错误码：{ret}";
                //    return false;
                //}
                //ret = GTN_Reset(coreNum2);
                //if (ret != 0)
                //{
                //    err = $"{mgsHeadCore2}。原因：重置内核失败。错误码：{ret}";
                //    return false;
                //}

                string cfgCore1FileName = "gtn_core_1.cfg";
                string cfgCore2FileName = "gtn_core_2.cfg";
                // 4.加载轴卡内核1、内核2的配置文件。（可以增加配置文件没有不加载检测,区分相对路径和绝对路径) 
                // 设置编码器的计数模式，默认是外部编码器计数模式。步进马达用脉冲计数器方式；私服马达用外部编码器计数方式
                // 轴配置文件必须加载。板卡断电后内部参数会丢失。
                ret = GTN_LoadConfig(coreNum1, cfgCore1FileName);
                if (ret != 0)
                {
                    err = $"{mgsHeadCore1}。原因：加载配置文件:{cfgCore1FileName}失败。错误码：{ret}";
                    return false;
                }
                ret = GTN_LoadConfig(coreNum2, cfgCore2FileName);
                if (ret != 0)
                {
                    err = $"{mgsHeadCore2}。原因：加载配置文件:{cfgCore2FileName}失败。错误码：{ret}";
                    return false;
                }

                //6.扫描从站信息 获取控制卡最大外接轴数、通用IO数量、扩展IO数量
                ScanResource();
                //axisCountCore1 = 5;
                //axisCountCore2 = 0;

                //count_GPI_Core1 = 44;
                //count_GPI_Core2 = 0;
                //count_EXT_DI_Core1 = 32;
                //count_EXT_DI_Core2 = 0;

                //count_GPO_Core1 = 20;
                //count_GPO_Core2 = 0;
                //count_EXT_DO_Core1 = 32;
                //count_EXT_DO_Core2 = 0;

                //count_ADC_Core1 = 0;
                //count_ADC_Core2 = 0;
                //count_EXT_AI_Core1 = 0;
                //count_EXT_AI_Core2 = 0;

                //count_DAC_Core1 = 0;
                //count_DAC_Core2 = 0;
                //count_EXT_AO_Core1 = 0;
                //count_EXT_AO_Core2 = 0;

                // 7.清除驱动器报警标志、 跟随误差越限标志、 限位触发标志。
                // 8.设置电机到位检测误差带
                int band = 20;
                int time = 4;
                for (short axisIndex = 1; axisIndex <= axisCountCore1; axisIndex++)
                {
                    ret = GTN_ClrSts(coreNum1, axisIndex, 1);
                    if (ret != 0)
                    {
                        err = $"{mgsHeadCore1}。原因：清除轴{axisIndex}状态失败。错误码：{ret}";
                        return false;
                    }
                    ret = GTN_SetAxisBand(coreNum1, axisIndex, band, time);
                    if (ret != 0)
                    {
                        err = $"{mgsHeadCore1}。原因：设置轴{axisIndex}到位误差检测失败。错误码：{ret}";
                        return false;
                    }
                }
                for (short axisIndex = 1; axisIndex <= axisCountCore2; axisIndex++)
                {
                    ret = GTN_ClrSts(coreNum2, axisIndex, 1);
                    if (ret != 0)
                    {
                        err = $"{mgsHeadCore2}。原因：清除轴{axisIndex}状态失败。错误码：{ret}";
                        return false;
                    }
                    ret = GTN_SetAxisBand(coreNum2, axisIndex, band, time);
                    if (ret != 0)
                    {
                        err = $"{mgsHeadCore2}。原因：设置轴{axisIndex}到位误差检测失败。错误码：{ret}";
                        return false;
                    }
                }

                return true;
            });

            // 修改设备的状态
            OnStatusChanged(DeviceStatus.Online);
        }
        #endregion

        #region 扫描从站信息
        /// <summary>
        /// 扫描轴
        /// </summary>
        /// <param name="axisNum"></param>
        /// <returns></returns>
        public void ScanAxis(out uint AxisNum)
        {
            CheckInit();

            AxisNum = (uint)(axisCountCore1 + axisCountCore2);
        }

        /// <summary>
        /// 扫描IO
        /// </summary>
        /// <param name="digitalIn"></param>
        /// <param name="digitalOut"></param>
        /// <returns></returns>
        public void ScanDigitalIO(out uint DiNum, out ushort DoNum)
        {
            CheckInit();

            DiNum = (uint)(count_GPI_Core1 + count_GPI_Core2 + count_EXT_DI_Core1 + count_EXT_DI_Core2);
            DoNum = (ushort)(count_GPO_Core1 + count_GPO_Core2 + count_EXT_DO_Core1 + count_EXT_DO_Core2);
        }

        /// <summary>
        /// 扫描模拟信号
        /// </summary>
        /// <param name="anglogIn"></param>
        /// <param name="anglogOut"></param>
        /// <returns></returns>
        public void ScanAnglog(out ushort AiNum, out ushort AoNum)
        {
            CheckInit();

            AiNum = (ushort)(count_ADC_Core1 + count_ADC_Core2 + count_EXT_AI_Core1 + count_EXT_AI_Core2);
            AoNum = (ushort)(count_DAC_Core1 + count_DAC_Core2 + count_EXT_AO_Core1 + count_EXT_AO_Core2);
        }
        #endregion

        #region 轴卡通用方法
        /// <summary>
        /// 确认轴卡是否运动完成，默认超时时间20s。
        /// </summary>
        /// <param name="precision">精度。目标位置和设定位置之间允许的误差。</param>
        /// <param name="axisNo">轴号</param>
        /// <returns></returns>
        public bool CheckMotionDone(int precision, int axisNo = 0,double targetPulse=0)
        {
            CheckInit();

            short coreNum = 0;
            short axisNum = 0;
            double currentPos = 0;  // 当前位置，实时位置
            double targetPos = 0;   // 设定位置，目标位置
            bool done = false;      // 到位标志

            GetAxisNum(axisNo, out coreNum, out axisNum);

            string msgHead = $"{Brand} 检查轴{axisNo}到位信号失败。";
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                ret = GTN_GetPrfPos(coreNum, axisNum, out targetPos, 1, out uint pClock);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：获取轴目标位置失败。错误码：{ret}";
                    return false;
                }

                if (!GetAxisStatus(axisNo)[AxisStatus.Moving])  // 规划器运动标志
                //if (GetAxisStatus(axisNo)[AxisStatus.OnPos])  // 电机到位标志 到位后置1
                {
                    ret = GTN_GetEncPos(coreNum, axisNum, out currentPos, 1, out uint pClock1);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：获取轴实际位置失败。错误码：{ret}";
                        return false;
                    }

                    // 判定是否在允许的位置误差范围内
                    if (Math.Abs(currentPos - targetPos) < precision)
                    {
                        done = true;
                        OnLog(LogType.Debug, $"{Brand}_轴{axisNo} 执行 CheckMotionDone 成功。目标位置:{targetPos},当前位置:{currentPos}!");
                        return true;
                    }
                }

                return true;
            });

            return done;
        }

        #region 模拟信号IO
        /// <summary>
        /// 读取模拟量输入电压值
        /// </summary>
        /// <param name="index">Ai序号</param>
        /// <returns></returns>
        public double GetAnalogIn(int index)
        {
            CheckInit();

            short coreNum = 0;
            short aiNum = 0;
            CardType type = CardType.Null;
            double adcVoltage = 0;

            GetAiNum(index, out coreNum, out type, out aiNum);

            string msgHead = $"{Brand} 获取模拟输入信号{index}失败。";
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                if (type == CardType.AxisCard)
                {
                    ret = GTN_GetAuAdc(coreNum, aiNum, out adcVoltage, 1, out uint pClock);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：调用函数 GTN_GetAuAdc 失败。错误码：{ret}";
                        return false;
                    }
                }
                else if (type == CardType.ExtCard)
                {
                    ret = GTN_GetExtAi(coreNum, aiNum, out adcVoltage, 1);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：调用函数 GTN_GetExtAi 失败。错误码：{ret}";
                        return false;
                    }
                }
                else
                {
                    err = $"{msgHead}。原因：不应该执行到的代码区。内核：{coreNum}、索引：{aiNum}、类型：{type}";
                    return false;
                }

                return true;
            });

            return adcVoltage;
        }

        /// <summary>
        /// 读取非轴模拟量输出电压值
        /// </summary>
        /// <param name="index">Ao序号</param>
        /// <returns></returns>
        public double GetAnalogOut(int index)
        {
            CheckInit();

            short coreNum = 0;
            short aoNum = 0;
            CardType type = CardType.Null;
            double adcVoltage = 0;

            GetAoNum(index, out coreNum, out type, out aoNum);

            string msgHead = $"{Brand} 获取模拟输出信号{index}失败。";
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                if (type == CardType.AxisCard)
                {
                    short tempVoltage = 0;
                    ret = GTN_GetAuDac(coreNum, aoNum, out tempVoltage, 1, out uint pClock);
                    adcVoltage = tempVoltage;
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：调用函数 GTN_GetAuDac 失败。错误码：{ret}";
                        return false;
                    }
                }
                else if (type == CardType.ExtCard)
                {
                    ret = GTN_GetExtAo(coreNum, aoNum, out adcVoltage, 1);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：调用函数 GTN_GetExtAo 失败。错误码：{ret}";
                        return false;
                    }
                }
                else
                {
                    err = $"{msgHead}。原因：不应该执行到的代码区。内核：{coreNum}、索引：{aoNum}、类型：{type}";
                    return false;
                }

                return true;
            });

            return adcVoltage;
        }

        /// <summary>
        /// 设置非轴模拟量输出电压值
        /// </summary>
        /// <param name="index">Ao序号</param>
        /// <param name="analogVal">待设定的电压值。单位：V。范围：-10~10</param>
        /// <returns></returns>
        public void SetAnalogOut(int index, double analogVal)
        {
            CheckInit();

            short coreNum = 0;
            short aoNum = 0;
            CardType type = CardType.Null;

            short adcVoltage = (short)analogVal;

            GetAoNum(index, out coreNum, out type, out aoNum);

            string msgHead = $"{Brand} 设置模拟输出信号{index}失败。";
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                if (type == CardType.AxisCard)
                {
                    ret = GTN_SetAuDac(coreNum, aoNum, ref adcVoltage, 1);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：调用函数 GTN_SetAuDac 失败。错误码：{ret}";
                        return false;
                    }
                }
                else if (type == CardType.ExtCard)
                {
                    ret = GTN_SetExtAo(coreNum, aoNum, ref analogVal, 1);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：调用函数 GTN_SetExtAo 失败。错误码：{ret}";
                        return false;
                    }
                }
                else
                {
                    err = $"{msgHead}。原因：不应该执行到的代码区。内核：{coreNum}、索引：{aoNum}、类型：{type}";
                    return false;
                }

                return true;
            });
        }
        #endregion

        #region 数字信号
        /// <summary>
        /// 读取数字量输入值
        /// </summary>
        /// <param name="index">Di序号</param>
        /// <returns></returns>
        public bool GetDigitalIn(int index)
        {
            CheckInit();

            short coreNum = 0;
            short diNum = 0;
            CardType type = CardType.Null;
            short value = 0;

            GetDiNum(index, out coreNum, out type, out diNum);

            string msgHead = $"{Brand} 获取数字输入信号{index}失败。";
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                if (type == CardType.AxisCard)
                {
                    short arraySize = 3;
                    int[] tempValue = new int[arraySize]; // 32*3=96
                    ret = GTN_GetDiEx(coreNum, mc.MC_GPI, out tempValue[0], arraySize);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：调用函数 GTN_GetDiEx 失败。错误码：{ret}";
                        return false;
                    }
                    bool find = false;
                    int arrayIndex = 0;
                    while (arrayIndex < arraySize)
                    {
                        if (diNum > 32 * arrayIndex && diNum <= 32 * (arrayIndex + 1))
                        {
                            value = (short)(tempValue[arrayIndex] & (1 << (diNum - 1)));
                            find = true;
                            break;
                        }
                        arrayIndex++;
                    }
                    if (!find)
                    {
                        err = $"{msgHead}。原因：调用函数 GTN_GetDiEx 失败。错误:{diNum}超出最大读取范围[{1},{arraySize * 32}]";
                        return false;
                    }

                    //ret = GTN_GetDiBit(coreNum, mc.MC_GPI, diNum, out value);
                    //if (ret != 0)
                    //{
                    //    err = $"{msgHead}。原因：调用函数 GTN_GetDiBit 失败。错误码：{ret}";
                    //    return false;
                    //}
                }
                else if (type == CardType.ExtCard)
                {
                    ret = GTN_GetExtDiBit(coreNum, diNum, out value);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：调用函数 GTN_GetExtDiBit 失败。错误码：{ret}";
                        return false;
                    }
                }
                else
                {
                    err = $"{msgHead}。原因：不应该执行到的代码区。内核：{coreNum}、索引：{diNum}、类型：{type}";
                    return false;
                }

                return true;
            });

            return value == 1;
        }

        /// <summary>
        /// 读取数字量输出值
        /// </summary>
        /// <param name="index">Do索引，从0开始</param>
        /// <returns></returns>
        public bool GetDigitalOut(int index)
        {
            CheckInit();

            short coreNum = 0;
            short doNum = 0;
            CardType type = CardType.Null;
            short value = 0;

            GetDoNum(index, out coreNum, out type, out doNum);

            string msgHead = $"{Brand} 获取数字输出信号{index}失败。";
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                if (type == CardType.AxisCard)
                {
                    short arraySize = 3;
                    int[] tempValue = new int[arraySize]; // 32*3=96
                    ret = GTN_GetDoEx(coreNum, mc.MC_GPO, out tempValue[0], arraySize);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：调用函数 GTN_GetDoEx 失败。错误码：{ret}";
                        return false;
                    }
                    bool find = false;
                    int arrayIndex = 0;
                    while (arrayIndex < arraySize)
                    {
                        if (doNum > 32 * arrayIndex && doNum <= 32 * (arrayIndex + 1))
                        {
                            value = (short)(tempValue[arrayIndex] & (1 << (doNum - 1)));
                            find = true;
                            break;
                        }
                        arrayIndex++;
                    }
                    if (!find)
                    {
                        err = $"{msgHead}。原因：调用函数 GTN_GetExtDo 失败。错误:{doNum}超出最大读取范围[{1},{arraySize * 32}]";
                        return false;
                    }
                }
                else if (type == CardType.ExtCard)
                {
                    int tempValue = 0;
                    ret = GTN_GetExtDo(coreNum, doNum, out tempValue);
                    value = (short)(tempValue & (1 << 0)); //一次性读出从doNum开始的结果，0位就是doNum对应的实时值。
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：调用函数 GTN_GetExtDo 失败。错误码：{ret}";
                        return false;
                    }
                }
                else
                {
                    err = $"{msgHead}。原因：不应该执行到的代码区。内核：{coreNum}、索引：{doNum}、类型：{type}";
                    return false;
                }

                return true;
            });

            return value == 1;
        }

        /// <summary>
        /// 设置数字量输出值
        /// </summary>
        /// <param name="index"></param>
        /// <param name="digitalOut"></param>
        /// <exception cref="DeviceException"></exception>
        public void SetDigitalOut(int index, bool digitalOut)
        {
            CheckInit();

            short coreNum = 0;
            short doNum = 0;
            CardType type = CardType.Null;
            short value = (short)(digitalOut ? 1 : 0);

            GetDoNum(index, out coreNum, out type, out doNum);

            string msgHead = $"{Brand} 设置数字输出信号{index}失败。";
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                if (type == CardType.AxisCard)
                {
                    ret = GTN_SetDoBit(coreNum, mc.MC_GPO, doNum, value);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：调用函数 GTN_SetDoBit 失败。错误码：{ret}";
                        return false;
                    }
                }
                else if (type == CardType.ExtCard)
                {
                    ret = GTN_SetExtDoBit(coreNum, doNum, value);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：调用函数 GTN_SetExtDoBit 失败。错误码：{ret}";
                        return false;
                    }
                }
                else
                {
                    err = $"{msgHead}。原因：不应该执行到的代码区。内核：{coreNum}、索引：{doNum}、类型：{type}";
                    return false;
                }

                return true;
            });
        }
        #endregion

        /// <summary>
        /// 对轴进行使能操作。会先清除报警；在执行使能动作；最后再检测使能是否成功
        /// </summary>
        /// <param name="axisNo">轴编号</param>
        /// <param name="isOn">使能状态</param>
        /// <returns></returns>
        public void ServOn(int axisNo, bool isOn)
        {
            CheckInit();

            short coreNum = 0;
            short axisNum = 0;

            GetAxisNum(axisNo, out coreNum, out axisNum);
            string msgHead = $"{Brand} 改变轴使能{axisNo}使能状态失败。";

            // 如果有报警，先清除报警后
            if (GetAxisStatus(axisNo)[AxisStatus.Alarm])
            {
                // 输出轴的清错信号,重置轴状态标志 
                ClearAxisError(axisNo);

                // 清除报警后，检测报警是否已经清除
                // 报警清除失败，
                // 清除驱动器报警标志、 跟随误差越限标志、 限位触发标志。
                // 1.只有当驱动器没有报警时才能清除轴状态字的报警标志；
                // 2.只有当跟随误差正常以后，才能清除跟随误差越限标志；
                // 3.只有当离开限位开关，或者规划位置在软限位行程以内时才能清除轴状态字的限位触发标志。
                if (GetAxisStatus(axisNo)[AxisStatus.Alarm])
                {
                    throw new Exception($"{msgHead}原因：通过软件复位无法消除轴报警。");
                }
            }

            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                // 执行轴使能动作；完成后再检测轴使能是否成功
                if (isOn)
                {
                    ret = GTN_AxisOn(coreNum, axisNum);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：轴执行上使能动作失败。错误码：{ret}";
                        return false;
                    }

                    // 再次判定轴使能切换是否成功
                    Thread.Sleep(100);
                    if (!GetAxisStatus(axisNo)[AxisStatus.SvOn])
                    {
                        err = $"{msgHead}。原因：轴执行上使能动作后，仍处于下使能状态。";
                        return false;
                    }
                }
                else
                {
                    ret = GTN_AxisOff(coreNum, axisNum);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：轴执行下使能动作失败。错误码：{ret}";
                        return false;
                    }

                    // 再次判定轴使能切换是否成功
                    Thread.Sleep(100);
                    if (GetAxisStatus(axisNo)[AxisStatus.SvOn])
                    {
                        err = $"{msgHead}。原因：轴执行下使能动作后，仍处于上使能状态。错误码：{ret}";
                        return false;
                    }
                }

                return true;
            });
        }

        /// <summary>
        /// 状态重置。 会先清除报警；在执行使能动作；最后再检测使能是否成功
        /// </summary>
        /// <param name="axisNo">轴编号</param>
        public void ResetState(int axisNo)
        {
            // 输出轴的清错信号,重置轴状态标志 
            ClearAxisError(axisNo);

            // 对轴进行使能操作
            ServOn(axisNo, true);
        }

        /// <summary>
        /// 获取当前状态
        /// </summary>
        /// <param name="axisNo">轴编号</param>
        /// <returns></returns>
        public Dictionary<AxisStatus, bool> GetAxisStatus(int axisNo, bool IsThrowException = true)
        {
            CheckInit();

            short coreNum = 0;
            short axisNum = 0;

            int asixStatus = 0;
            int orgValue = 0;
            int posValue = 0;
            int negValue = 0;
            int almValue = 0;

            GetAxisNum(axisNo, out coreNum, out axisNum);

            string msgHead = $"{Brand} 获取{axisNo}状态标志失败。";
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                ret = GTN_ClrSts(coreNum, axisNum, 1);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：重置轴状态标志失败。错误码：{ret}";
                    return false;
                }

                ret = GTN_GetSts(coreNum, axisNum, out asixStatus, 1, out uint pClock);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：获取轴状态标志失败。错误码：{ret}";
                    return false;
                }

                ret = GTN_GetDi(coreNum, MC_HOME, out orgValue);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：获取轴原点信号失败。错误码：{ret}";
                    return false;
                }

                ret = GTN_GetDi(coreNum, MC_LIMIT_POSITIVE, out posValue);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：获取轴正限位信号失败。错误码：{ret}";
                    return false;
                }

                ret = GTN_GetDi(coreNum, MC_LIMIT_NEGATIVE, out negValue);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：获取轴负限位信号失败。错误码：{ret}";
                    return false;
                }

                ret = GTN_GetDi(coreNum, MC_ALARM, out almValue);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：获取轴报警信号失败。错误码：{ret}";
                    return false;
                }

                return true;
            });

            // 这几个标志触发后，不会自动清除，需要手动调用GTN_ClrSts清除
            //bool bFlagAlarm = (asixStatus & 0x2) != 0;                  // 伺服报警标志                  
            bool bFlagMError = (asixStatus & 0x10) != 0;                // 跟随误差越限标志
            //bool bFlagPosLimit = (asixStatus & 0x20) != 0;              // 正限位触发标志
            //bool bFlagNegLimit = (asixStatus & 0x40) != 0;              // 负限位触发标志
            bool bFlagSmoothStop = (asixStatus & 0x80) != 0;            // 平滑停止标志
            bool bFlagAbruptStop = (asixStatus & 0x100) != 0;           // 急停标志
            // 这几个标志触发后，会自动更新
            bool bFlagServoOn = (asixStatus & 0x200) != 0;              // 伺服使能标志
            bool bFlagMotion = (asixStatus & 0x400) != 0;               // 规划器运动标志
            bool bFlagFinish = (asixStatus & 0x800) != 0;               // 电机到位标志 默认不生效，设置轴到位误差带后生效

            bool bFlagAlarm = (1 == (short)(almValue & (1 << (axisNum - 1))));                      // 伺服报警标志  
            bool bFlagPosLimit = (1 == (short)(posValue & (1 << (axisNum - 1))));                   // 正限位信号
            bool bFlagNegLimit = (1 == (short)(negValue & (1 << (axisNum - 1))));                   // 负限位信号
            bool bFlagOrgin = (0 == (short)(orgValue & (1 << (axisNum - 1))));                      // 原点信号

            // 如果急停、报警，清空回零完成状态
            if (bFlagAlarm || bFlagMError)
            {
                SetAxisHome(axisNo, false);
            }
            // 获取回零完成标志
            bool bFlagHomeFinish = zeroDone.FirstOrDefault(item => item.Key == axisNo).Value;       // 回零是否完成

            return new Dictionary<AxisStatus, bool>()
            {
                {AxisStatus.Alarm,bFlagAlarm},
                {AxisStatus.Emg, bFlagAbruptStop},
                {AxisStatus.Mel,bFlagNegLimit},
                {AxisStatus.Pel,bFlagPosLimit},
                {AxisStatus.Moving,bFlagMotion},
                {AxisStatus.OnPos, bFlagFinish},
                { AxisStatus.SvOn,bFlagServoOn},
                {AxisStatus.Org,bFlagOrgin},
                {AxisStatus.IsHome, bFlagHomeFinish}
            };
        }

        /// <summary>
        /// 获取轴当前位置 返回值单位：mm
        /// </summary>
        /// <param name="axisNo">轴序号</param>
        /// <param name="perPulse">脉冲毫米比</param>
        /// <returns></returns>
        /// <exception cref="DeviceException"></exception>
        public double GetCurrentPos(int axisNo, double perPulse)
        {
            CheckInit();

            short coreNum = 0;
            short axisNum = 0;
            double currentPos = 0;  //当前位置

            GetAxisNum(axisNo, out coreNum, out axisNum);

            string msgHead = $"{Brand} 获取轴{axisNo}实时位置失败。";
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                ret = GTN_GetEncPos(coreNum, axisNum, out currentPos, 1, out uint pClock);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：获取轴实时位置失败。错误码：{ret}";
                    return false;
                }

                return true;
            });

            double pos = currentPos / perPulse;
            pos = Math.Round(pos, 3);

            return pos;
        }

        /// <summary>
        /// 修改轴的当前位置为设定值。应用场景：转盘旋转越界重置位置使用
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="perPulse">脉冲毫米比或脉冲角度比</param>
        /// <param name="position">位置。单位：毫米或角度</param>
        public override void SetCurrentPos(int axisNo, double perPulse, double position)
        {
            CheckInit();

            short coreNum = 0;
            short axisNum = 0;
            int setPos = (int)(position * perPulse);  //当前位置

            GetAxisNum(axisNo, out coreNum, out axisNum);

            string msgHead = $"{Brand} 获取轴{axisNo}实时位置失败。";
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                // 这里的脉冲值范围  [-2147483648,2147483648]
                ret = GTN_SetEncPos(coreNum, axisNum, setPos);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：获取轴实时位置失败。错误码：{ret}";
                    return false;
                }

                return true;
            });

        }

        /// <summary>
        /// 停止轴运动
        /// </summary>
        /// <param name="axisNo">轴序号</param>
        /// <param name="isAll">是否停止所有轴</param>
        public void Stop(int axisNo, bool isAll = false)
        {
            CheckInit();

            short coreNum = 0;
            short axisNum = 0;
            int mask;
            int option;              // 对应位为 0 时表示平滑停止对应的轴或坐标系，
                                     // 对应位为 1 时表示急停对应的轴或坐标系
                                     // 两种方式本质是一样的，只是在配置文件中配置的减速度不一致

            GetAxisNum(axisNo, out coreNum, out axisNum);

            if (!isAll)
            {
                mask = 1 << (axisNum - 1);
                option = 0 << (axisNum - 1);
            }
            else
            {
                mask = 0xFFF;  // 一个内核最多支持12根轴
                option = 0;
            }
            string msgHead = $"{Brand} 轴{axisNo}执行停止动作失败。";
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                ret = GTN_Stop(coreNum, mask, option);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：轴停止失败。错误码：{ret}";
                    return false;
                }

                return true;
            });

            // 修改设备的状态
            OnStatusChanged(DeviceStatus.Stop);
        }

        /// <summary>
        /// 轴回零。同步方式，需等待回零完成。默认超时时间是30s
        /// </summary>
        /// <param name="axisNo">轴序号</param>
        /// <param name="homeMode">回零模式</param>
        /// <param name="high">回零高速，单位：mm/s </param>
        /// <param name="low">回零低速，单位：mm/s </param>
        /// <param name="perPlus">脉冲毫米比。单位：plus/mm。</param>
        /// <param name="homeAcc">回零加速度。单位：mm/s2</param>
        /// <param name="Offset">回零完成后，设置零点偏移位</param>
        /// <exception cref="DeviceException"></exception>
        /// <exception cref="DeviceTimeoutException"></exception>
        public void Home(int axisNo, HomeMode homeMode, double high, double low, double perPlus, double homeAcc, double Offset, AxisPML axisPML)
        {
            CheckInit();

            short coreNum = 0;
            short axisNum = 0;
            mc.THomePrm tHomePrm = new mc.THomePrm();//回零参数结构体

            GetAxisNum(axisNo, out coreNum, out axisNum);

            string msgHead = $"{Brand} 轴{axisNo}执行回零动作失败。";
            // 检查轴处于上使能状态
            if (!GetAxisStatus(axisNo)[AxisStatus.SvOn])
            {
                throw new Exception($"{msgHead}原因：轴未上使能。");
            }
            // 检查轴处于运动停止状态
            if (GetAxisStatus(axisNo)[AxisStatus.Moving])
            {
                double currentPos = GetCurrentPos(axisNo, perPlus);
                currentPos = Math.Round((currentPos / perPlus), 3);
                throw new Exception($"{msgHead}原因：轴还在运动中，不能执行新动作。当前位置:{currentPos} 目标位置:{0}。");
            }

            switch (homeMode)
            {
                case HomeMode.NegativeToZ://负向寻EZ
                    tHomePrm.mode = mc.HOME_MODE_INDEX;
                    tHomePrm.indexDir = -1;
                    tHomePrm.moveDir = -1;
                    break;
                case HomeMode.PositiveToZ://正向寻EZ
                    tHomePrm.mode = mc.HOME_MODE_INDEX;
                    tHomePrm.indexDir = 1;
                    tHomePrm.moveDir = 1;
                    break;
                case HomeMode.Negative://负向
                    tHomePrm.mode = mc.HOME_MODE_LIMIT;
                    tHomePrm.moveDir = -1;
                    break;
                case HomeMode.Positive://正向
                    tHomePrm.mode = mc.HOME_MODE_LIMIT;
                    tHomePrm.moveDir = 1;
                    break;
                case HomeMode.PositiveHome://正向原点
                    tHomePrm.mode = mc.HOME_MODE_LIMIT_HOME;
                    tHomePrm.moveDir = 1;
                    break;
                case HomeMode.NegativeHome://负向原点
                    tHomePrm.mode = mc.HOME_MODE_LIMIT_HOME;
                    tHomePrm.moveDir = -1;
                    break;
                case HomeMode.NegativeMotorHome://步进负向原点
                    tHomePrm.mode = mc.HOME_MODE_HOME;
                    tHomePrm.moveDir = -1;
                    break;
                case HomeMode.PositiveMotorHome://步进正向原点
                    tHomePrm.mode = mc.HOME_MODE_HOME_INDEX;
                    tHomePrm.moveDir = 1;
                    break;
                default:
                    SafeNativeMethod(() => { return false; }, $"{Brand}_轴{axisNo} 执行 Home 失败：不应该执行到的代码区。");
                    break;
            }
            tHomePrm.velHigh = high * perPlus / 1000;
            tHomePrm.velLow = low * perPlus / 1000;
            tHomePrm.acc = homeAcc * perPlus / 1000 / 1000;
            tHomePrm.homeOffset = (int)(Offset * perPlus);
            tHomePrm.edge = 0;  //0：下降沿，非0：上升沿
            tHomePrm.escapeStep = 5000;//限位回原点时反向脱离距离或者压在感应器上脱离的距离

            // 清空回零完成标志
            SetAxisHome(axisNo, false);
            // 停止待回零的轴
            Stop(axisNo);

            // 输出轴的清错信号,重置轴状态标志 
            ClearAxisError(axisNo);

            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                ret = GTN_ZeroPos(coreNum, axisNum, 1);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：设置回零开始位置为零。错误码：{ret}";
                    return false;
                }

                ret = GTN_GoHome(coreNum, axisNum, ref tHomePrm);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：执行回零动作失败。错误码：{ret}";
                    return false;
                }
                Thread.Sleep(500);

                return true;
            });
        }

        /// <summary>
        /// 回零完成状态
        /// </summary>
        /// <param name="axisNo"></param>
        /// <returns></returns>
        public override bool CheckHomeDone(int axisNo = 0)
        {
            CheckInit();

            short coreNum = 0;
            short axisNum = 0;
            mc.THomeStatus tHomeStatus = new mc.THomeStatus();//回零状态结构体
            bool isDone = false;      // 回零完成标志

            GetAxisNum(axisNo, out coreNum, out axisNum);
            string msgHead = $"{Brand} 轴{axisNo}检查回零完成失败。";

            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                ret = GTN_GetHomeStatus(coreNum, axisNum, out tHomeStatus);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：获取回零状态失败。错误码：{ret}";
                    return false;
                }

                // 回零完成检测回零阶段
                if (tHomeStatus.run == 0 && tHomeStatus.stage == mc.HOME_STAGE_END && tHomeStatus.error == mc.HOME_ERROR_NONE)
                {
                    Thread.Sleep(100);
                    ret = GTN_ZeroPos(coreNum, axisNum, 1);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：设置回零完成位置为零。错误码：{ret}";
                        return false;
                    }

                    isDone = true;
                }

                // 判定设备状态如果被调用者停止了，直接退出 回零完成检测
                //if (Status == DeviceStatus.Stop)
                //{
                //    err = $"{msgHead} 原因：被调用者终止检查。";
                //}

                // 记录回零完成状态
                SetAxisHome(axisNo, isDone);

                return true;
            });

            return isDone;
        }

        /// <summary>
        /// 回零取消
        /// </summary>
        /// <param name="axisNo">轴号</param>
        public void HomeCancel(int axisNo)
        {
            CheckInit();

            // 停止待回零的轴
            Stop(axisNo);

            // 修改设备的状态
            OnStatusChanged(DeviceStatus.Stop);
        }

        /// <summary>
        /// 执行点动
        /// </summary>
        /// <param name="vel">运动速度。单位：mm/s。值为正数，正向点动；值为负值，负向点动</param>
        /// <param name="acc">加速度。单位：mm/s2。</param>
        /// <param name="dec">减速度。单位：mm/s2.</param>
        /// <param name="perPlus">脉冲毫米比。单位：plus/mm。</param>
        /// <param name="slineTime">到位稳定时间。单位：mm。(暂时没有用到)</param>
        /// <param name="axisNo">轴序号</param>
        public void Jog(double vel, double acc, double dec, double perPlus, double slineTime, int axisNo, AxisPML axisPML)
        {
            CheckInit();

            short coreNum = 0;
            short axisNum = 0;
            int mask = 0;
            mc.TJogPrm jogPrm = new mc.TJogPrm();

            GetAxisNum(axisNo, out coreNum, out axisNum);

            string msgHead = $"{Brand} 轴{axisNo}执行Jog动作失败。";
            // 检查轴处于上使能状态
            if (!GetAxisStatus(axisNo)[AxisStatus.SvOn])
            {
                throw new Exception($"{msgHead}原因：轴未上使能。");
            }
            // 检查轴处于运动停止状态
            if (GetAxisStatus(axisNo)[AxisStatus.Moving])
            {
                double currentPos = GetCurrentPos(axisNo, perPlus);
                currentPos = Math.Round((currentPos / perPlus), 3);
                throw new Exception($"{msgHead}原因：轴还在运动中，不能执行新动作。当前位置:{currentPos}。");
            }

            jogPrm.acc = acc * perPlus / 1000 / 1000;
            jogPrm.dec = dec * perPlus / 1000 / 1000;
            jogPrm.smooth = 0.9;  // 到位稳定时间，平滑系数。 取值范围： [0, 1)。平滑系数的数值越大，加减速过程越平稳。
            double interVel = vel * perPlus / 1000;

            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                ret = GTN_PrfJog(coreNum, axisNum);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：设置轴运动模式为Jog模式失败。错误码：{ret}";
                    return false;
                }

                ret = GTN_SetJogPrm(coreNum, axisNum, ref jogPrm);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：设置轴点动参数失败。错误码：{ret}";
                    return false;
                }

                ret = GTN_SetVel(coreNum, axisNum, interVel);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：设置轴运动速度失败。错误码：{ret}";
                    return false;
                }

                mask = 1 << (axisNum - 1);
                ret = GTN_Update(coreNum, mask);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：轴点动开始执行失败。错误码：{ret}";
                    return false;
                }

                return true;
            });
        }

        /// <summary>
        /// 执行绝对运动或相对运动
        /// </summary>
        /// <param name="pos">目标位置。单位：mm。</param>
        /// <param name="vel">运动速度。单位：mm/s。</param>
        /// <param name="acc">加速度。单位：mm/s2。</param>
        /// <param name="dec">减速度。单位：mm/s2.</param>
        /// <param name="perPlus">脉冲毫米比。单位：plus/mm。</param>
        /// <param name="slineTime">到位稳定时间。单位：mm。</param>
        /// <param name="isAbsMove">是否是绝对运动。</param>
        /// <param name="axisNo">轴序号</param>
        public void Move(double pos, double vel, double acc, double dec, double perPlus, double slineTime, bool isAbsMove, int axisNo, AxisPML axisPML)
        {
            CheckInit();

            short coreNum = 0;
            short axisNum = 0;
            int mask = 0;
            mc.TTrapPrm trapPrm = new mc.TTrapPrm();
            trapPrm.acc = acc * perPlus / 1000 / 1000;
            trapPrm.dec = dec * perPlus / 1000 / 1000;
            trapPrm.velStart = 0;
            trapPrm.smoothTime = (short)slineTime;
            double interVel = vel * perPlus / 1000;
            double interPos = pos * perPlus;

            GetAxisNum(axisNo, out coreNum, out axisNum);

            string msgHead = $"{Brand} 轴{axisNo}执行Move动作失败。";

            var status = GetAxisStatus(axisNo);
            // 检查轴处于上使能状态
            if (!status[AxisStatus.SvOn])
            {
                OnLog(LogType.Warning, $"轴:{axisNum}使能失败,重新使能!");
                ServOn(axisNo, true);
                Thread.Sleep(200); // 给轴上使能
            }

            double currentPos = GetCurrentPos(axisNo, perPlus);
            double targetPos = Math.Round(interPos / perPlus, 3);
            if (!isAbsMove) // 如果是相对运动，目标位置等于当前位置加上示教位置.如果位置越界，需要特殊考虑（针对转盘）
            {
                targetPos += currentPos;
            }
            OnLog(LogType.Debug, $"{Brand}_轴{axisNo} 开始Move。运动速度:{vel} 加速度:{acc} 减速度:{dec} 脉冲毫米比:{perPlus} 当前位置:{currentPos} 目标位置:{targetPos}");

            // 检查轴处于运动停止状态
            if (status[AxisStatus.Moving])
            {
                SafeNativeMethod(() => { return false; }, $"{Brand}_轴{axisNo} 执行 Move 失败：轴还在运动中，不能执行新动作。当前位置:{currentPos} 目标位置:{targetPos}。");
            }

            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                ret = GTN_PrfTrap(coreNum, axisNum);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：设置轴运动模式为点位模式失败。错误码：{ret}";
                    return false;
                }

                ret = GTN_SetTrapPrm(coreNum, axisNum, ref trapPrm);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：设置轴点位运动参数失败。错误码：{ret}";
                    return false;
                }

                ret = GTN_SetVel(coreNum, axisNum, interVel);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：设置轴运动速度失败。错误码：{ret}";
                    return false;
                }

                if (!isAbsMove) // 如果是相对运动，目标位置等于当前位置加上示教位置
                {
                    ret = GTN_GetEncPos(coreNum, axisNum, out currentPos, 1, out uint pClock);
                    interPos += currentPos;
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：获取轴运动前实时位置失败。错误码：{ret}";
                        return false;
                    }
                }
                ret = GTN_SetPos(coreNum, axisNum, (int)interPos);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：设置轴运动目标位置失败。错误码：{ret}";
                    return false;
                }

                ret = GTN_SetAxisBand(coreNum, axisNum, band: 20, time: 4);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：设置轴到位误差检测范围。错误码：{ret}";
                    return false;
                }

                mask = 1 << (axisNum - 1);
                ret = GTN_Update(coreNum, mask);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：轴点位运动开始执行失败。错误码：{ret}";
                    return false;
                }

                return true;
            });
        }

        /// <summary>
        /// 多轴插补运动。等待完成，超时时间默认20s。暂时支持同一时间只能有一个插补动作。最多同时支持两个插补动作
        /// </summary>
        /// <param name="axisId">轴序号数组</param>
        /// <param name="pos">位置数组</param>
        /// <param name="perPlusArr">脉冲毫米比数组</param>
        /// <param name="vel">最大速度数组</param>
        /// <param name="acc">加速度数组</param>
        public void MoveLine(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc)
        {
            CheckInit();

            int timeout = 20 * 1000;
            int runtime = 0;

            string msgHead = $"{Brand} 执行直线插补动作失败。";
            if (!(axisId != null && pos != null && perPlusArr != null && vel != null && acc != null
                && axisId.Count > 1 && axisId.Count < 5 // 最多允许有4个轴同时插补
                && axisId.Count == pos.Count
                && pos.Count == perPlusArr.Count
                && perPlusArr.Count == vel.Count
                && vel.Count == acc.Count))
            {
                throw new Exception($"{msgHead}原因：输入参数非法。");
            }

            // 先检查两个坐标系是否被使用，如果被使用，需要等待

            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                // 1.检查执行插补的轴当前状态：使能，停止。
                int count = axisId.Count;
                short[] coreNumArr = new short[count];
                short[] axisNumArr = new short[count];
                for (int i = 0; i < count; i++)
                {
                    GetAxisNum(axisId[i], out coreNumArr[i], out axisNumArr[i]);

                    // 检查轴处于上使能状态
                    if (!GetAxisStatus(axisId[i])[AxisStatus.SvOn])
                    {
                        throw new Exception($"{msgHead}原因：轴{axisNumArr[i]}未上使能。");
                    }
                    // 检查轴处于运动停止状态
                    if (GetAxisStatus(axisId[i])[AxisStatus.Moving])
                    {
                        double currentPos = GetCurrentPos(axisId[i], perPlusArr[i]);
                        currentPos = Math.Round((currentPos / perPlusArr[i]), 3);
                        double targetPos = Math.Round(pos[i] / perPlusArr[i], 3);
                        throw new Exception($"{msgHead}原因：轴{axisNumArr[i]}还在运动中，不能执行新动作。当前位置:{currentPos} 目标位置:{targetPos}。");
                    }
                }

                // 2.执行插补的轴必须是同一个内核的轴。
                if (coreNumArr.GroupBy(x => x).Count() != 1)
                {
                    throw new Exception($"{msgHead}原因：执行插补运动的轴必须是同一个内核的轴。");
                }

                // 3.设置插补参数
                short coreNum = coreNumArr[0];
                short crdNum = 1;           // 插补坐标系的序号，范围：[1,2]
                short fifoNum = 0;          // 插补坐标系对应的缓存队列序号，范围：[0,1]

                double[] interPosArr = new double[count];
                double[] interVelArr = new double[count];
                double[] interAccArr = new double[count];
                for (int i = 0; i < count; i++)
                {
                    interPosArr[i] = pos[i] * perPlusArr[i];
                    interVelArr[i] = vel[i] * perPlusArr[i] / 1000;
                    interAccArr[i] = acc[i] * perPlusArr[i] / 1000 / 1000;
                }

                // 插补轴占位符
                short[] axisMask = new short[8];
                for (int i = 0; i < count; i++)
                {
                    axisMask[axisId[i] - 1] = (short)(i + 1);
                }

                // XY 建立坐标系
                TCrdPrm crdPrm = new TCrdPrm();
                // GTN_GetCrdPrm(coreNum, crdNum, out crdPrm);
                crdPrm.dimension = (short)count;                                    // 坐标系维数
                crdPrm.synVelMax = interVelArr.Max();                               // 最大合成速度
                crdPrm.synAccMax = interAccArr.Max();                               // 最大加速度： 1pulse/ms^2
                crdPrm.evenTime = 50;                                               // 最小匀速时间： 50ms
                crdPrm.profile1 = axisMask[0];                                                // 规划器1对应到X轴
                crdPrm.profile2 = axisMask[1];                                                // 规划器2对应到Y轴
                crdPrm.profile3 = axisMask[2];                                                  // 规划器2对应到Z轴
                crdPrm.profile4 = axisMask[3];                                                  // 规划器2对应到A轴
                crdPrm.profile5 = axisMask[4];                                                // 规划器1对应到X轴
                crdPrm.profile6 = axisMask[5];                                                // 规划器2对应到Y轴
                crdPrm.profile7 = axisMask[6];                                                  // 规划器2对应到Z轴
                crdPrm.profile8 = axisMask[7];                                                  // 规划器2对应到A轴
                crdPrm.setOriginFlag = 1;                                           // 表示需要指定坐标系的原点坐标的规划位置
                crdPrm.originPos1 = 0;                                              // 坐标系的原点坐标的规划位置为每根轴的原点位置
                crdPrm.originPos2 = 0;
                crdPrm.originPos3 = 0;
                crdPrm.originPos4 = 0;
                crdPrm.originPos5 = 0;
                crdPrm.originPos6 = 0;
                crdPrm.originPos7 = 0;
                crdPrm.originPos8 = 0;
                // 建立1号坐标系，设置坐标系参数
                ret = GTN_SetCrdPrm(coreNum, crdNum, ref crdPrm);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：设置插补坐标系参数失败。错误码：{ret}";
                    return false;
                }

                // 即将把数据存入坐标系1的FIFO0中，所以要首先清除此缓存区中的数据
                ret = GTN_CrdClear(coreNum, crdNum, fifoNum);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：清空插补FIFI数据失败。错误码：{ret}";
                    return false;
                }

                // 即将把数据存入坐标系1的FIFO0中，所以要首先清除此缓存区中的数据
                if (count == 2)
                {
                    ret = GTN_LnXY(coreNum, crdNum, (int)interPosArr[0], (int)interPosArr[1], interVelArr.Max(), interAccArr.Max(), 0, fifoNum);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：添加插补数据失败。错误码：{ret}";
                        return false;
                    }
                }
                else if (count == 3)
                {
                    ret = GTN_LnXYZ(coreNum, crdNum, (int)interPosArr[0], (int)interPosArr[1], (int)interPosArr[2], interVelArr.Max(), interAccArr.Max(), 0, fifoNum);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：添加插补数据失败。错误码：{ret}";
                        return false;
                    }
                }
                else if (count == 4)
                {
                    ret = GTN_LnXYZA(coreNum, crdNum, (int)interPosArr[0], (int)interPosArr[1], (int)interPosArr[2], (int)interPosArr[3], interVelArr.Max(), interAccArr.Max(), 0, fifoNum);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：添加插补数据失败。错误码：{ret}";
                        return false;
                    }
                }

                // 5.启动坐标系1的FIFO0的插补运动
                ret = GTN_CrdStart(coreNum, crdNum, fifoNum);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：执行插补动作开始。错误码：{ret}";
                    return false;
                }

                //6. 等待插补动作完成
                while (true)
                {
                    short run = 0;      // 坐标系运动状态查询变量
                    int segment = 0;    // 坐标系运动完成段查询变量

                    ret = GTN_CrdStatus(coreNum, crdNum, out run, out segment, fifoNum);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：获取插补运动状态失败。错误码：{ret}";
                        return false;
                    }

                    // segment 当前执行的段号
                    if (run == 0 && segment >= 1) { break; } //插补完成

                    //超时检测
                    Thread.Sleep(SleepTime);
                    runtime += SleepTime;
                    if (runtime > timeout)
                    {
                        err = $"{msgHead}。原因：检测运动完成超时（20s）！。错误码：{ret}";
                        return false;
                    }

                    // 判定设备状态如果被调用者停止了，直接退出 回零完成检测
                    //if (Status == DeviceStatus.Stop)
                    //{
                    //    err = $"{msgHead}。原因：被调用者终止动作！。错误码：{ret}";
                    //    return false;
                    //}
                }

                return true;
            });
        }

        /// <summary>
        /// 圆弧插补
        /// </summary>
        /// <param name="axisId">轴号list</param>
        /// <param name="pos">位置list</param>
        /// <param name="perPlusArr">脉冲比list</param>
        /// <param name="vel">速度list</param>
        /// <param name="acc">加速度list</param>
        /// <param name="radius">半径</param>
        /// <param name="dir">方向</param>
        public void MoveCircle(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc, double radius, short dir)
        {
            CheckInit();

            int timeout = 20 * 1000;
            int runtime = 0;

            string msgHead = $"{Brand} 执行直线插补动作失败。";
            if (!(axisId != null && pos != null && perPlusArr != null && vel != null && acc != null
                 && axisId.Count > 1 && axisId.Count < 3 // 最多允许有2个轴同时插补
                 && axisId.Count == pos.Count
                 && pos.Count == perPlusArr.Count
                 && perPlusArr.Count == vel.Count
                 && vel.Count == acc.Count))
            {
                throw new Exception($"{msgHead}原因：输入参数非法。");
            }

            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                // 1.检查执行插补的轴当前状态：使能，停止。
                int count = axisId.Count;
                short[] coreNumArr = new short[count];
                short[] axisNumArr = new short[count];
                for (int i = 0; i < count; i++)
                {
                    GetAxisNum(axisId[i], out coreNumArr[i], out axisNumArr[i]);

                    // 检查轴处于上使能状态
                    if (!GetAxisStatus(axisId[i])[AxisStatus.SvOn])
                    {
                        throw new Exception($"{msgHead}原因：轴{axisNumArr[i]}未上使能。");
                    }
                    // 检查轴处于运动停止状态
                    if (GetAxisStatus(axisId[i])[AxisStatus.Moving])
                    {
                        double currentPos = GetCurrentPos(axisId[i], perPlusArr[i]);
                        currentPos = Math.Round((currentPos / perPlusArr[i]), 3);
                        double targetPos = Math.Round(pos[i] / perPlusArr[i], 3);
                        throw new Exception($"{msgHead}原因：轴{axisNumArr[i]}还在运动中，不能执行新动作。当前位置:{currentPos} 目标位置:{targetPos}。");
                    }
                }

                // 2.执行插补的轴必须是同一个内核的轴。
                if (coreNumArr.GroupBy(x => x).Count() != 1)
                {
                    throw new Exception($"{msgHead}原因：执行插补运动的轴必须是同一个内核的轴。");
                }

                // 3.设置插补参数
                short coreNum = coreNumArr[0];
                short crdNum = 1;           // 插补坐标系的序号，范围：[1,2]
                short fifoNum = 0;          // 插补坐标系对应的缓存队列序号，范围：[0,1]

                double[] interPosArr = new double[count];
                double[] interVelArr = new double[count];
                double[] interAccArr = new double[count];
                double interRadius = radius * perPlusArr[0]; // 半径换算：默认用的是第一个轴的脉冲毫米比
                for (int i = 0; i < count; i++)
                {
                    interPosArr[i] = pos[i] * perPlusArr[i];
                    interVelArr[i] = vel[i] * perPlusArr[i] / 1000;
                    interAccArr[i] = acc[i] * perPlusArr[i] / 1000 / 1000;
                }

                // 插补轴占位符
                short[] axisMask = new short[8];
                for (int i = 0; i < count; i++)
                {
                    axisMask[axisId[i] - 1] = (short)(i + 1);
                }

                // XY 建立坐标系
                TCrdPrm crdPrm = new TCrdPrm();
                // GTN_GetCrdPrm(coreNum, crdNum, out crdPrm);
                crdPrm.dimension = (short)count;                                    // 坐标系维数
                crdPrm.synVelMax = interVelArr.Max();                               // 最大合成速度
                crdPrm.synAccMax = interAccArr.Max();                               // 最大加速度： 1pulse/ms^2
                crdPrm.evenTime = 50;                                               // 最小匀速时间： 50ms
                crdPrm.profile1 = axisMask[0];                                                // 规划器1对应到X轴
                crdPrm.profile2 = axisMask[1];                                                // 规划器2对应到Y轴
                crdPrm.profile3 = axisMask[2];                                                  // 规划器2对应到Z轴
                crdPrm.profile4 = axisMask[3];                                                  // 规划器2对应到A轴
                crdPrm.profile5 = axisMask[4];                                                // 规划器1对应到X轴
                crdPrm.profile6 = axisMask[5];                                                // 规划器2对应到Y轴
                crdPrm.profile7 = axisMask[6];                                                  // 规划器2对应到Z轴
                crdPrm.profile8 = axisMask[7];                                                  // 规划器2对应到A轴
                crdPrm.setOriginFlag = 1;                                           // 表示需要指定坐标系的原点坐标的规划位置
                crdPrm.originPos1 = 0;                                              // 坐标系的原点坐标的规划位置为每根轴的原点位置
                crdPrm.originPos2 = 0;
                crdPrm.originPos3 = 0;
                crdPrm.originPos4 = 0;
                crdPrm.originPos5 = 0;
                crdPrm.originPos6 = 0;
                crdPrm.originPos7 = 0;
                crdPrm.originPos8 = 0;
                // 建立1号坐标系，设置坐标系参数
                ret = GTN_SetCrdPrm(coreNum, crdNum, ref crdPrm);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：设置插补坐标系参数失败。错误码：{ret}";
                    return false;
                }

                // 即将把数据存入坐标系1的FIFO0中，所以要首先清除此缓存区中的数据
                ret = GTN_CrdClear(coreNum, crdNum, fifoNum);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：清空插补FIFI数据失败。错误码：{ret}";
                    return false;
                }

                // 即将把数据存入坐标系1的FIFO0中，所以要首先清除此缓存区中的数据
                if (count == 2)
                {
                    ret = GTN_ArcXYR(coreNum, crdNum, (int)interPosArr[0], (int)interPosArr[1], radius, dir, interVelArr.Max(), interAccArr.Max(), 0, fifoNum);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：添加插补数据失败。错误码：{ret}";
                        return false;
                    }
                }

                // 5.启动坐标系1的FIFO0的插补运动
                ret = GTN_CrdStart(coreNum, crdNum, fifoNum);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：执行插补动作开始。错误码：{ret}";
                    return false;
                }

                //6. 等待插补动作完成
                while (true)
                {
                    short run = 0;      // 坐标系运动状态查询变量
                    int segment = 0;    // 坐标系运动完成段查询变量

                    ret = GTN_CrdStatus(coreNum, crdNum, out run, out segment, fifoNum);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：获取插补运动状态失败。错误码：{ret}";
                        return false;
                    }

                    if (run == 0 && segment >= 1) { break; } //插补完成

                    //超时检测
                    Thread.Sleep(SleepTime);
                    runtime += SleepTime;
                    if (runtime > timeout)
                    {
                        err = $"{msgHead}。原因：检测运动完成超时（20s）！。错误码：{ret}";
                        return false;
                    }

                    // 判定设备状态如果被调用者停止了，直接退出 回零完成检测
                    if (Status == DeviceStatus.Stop)
                    {
                        err = $"{msgHead}。原因：被调用者终止动作！。错误码：{ret}";
                        return false;
                    }
                }

                return true;
            });


        }
        #endregion

        protected override void Dispose(bool isDispose)
        {
            base.Dispose(isDispose);
            if (!HasInit()) return;

            string msgHead = $"{Brand}";
            string mgsHeadCore1 = $"{msgHead}_内核{coreNum1} 释放控制卡失败。";
            string mgsHeadCore2 = $"{msgHead}_内核{coreNum2} 释放控制卡失败。";

            // 关闭轴使能
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                for (short axisIndex = 1; axisIndex <= axisCountCore1; axisIndex++)
                {
                    ret = GTN_AxisOff(coreNum1, axisIndex);
                    if (ret != 0)
                    {
                        err = $"{mgsHeadCore1}。原因：关闭轴{axisIndex}使能失败。错误码：{ret}";
                        return false;
                    }
                }

                for (short axisIndex = 1; axisIndex <= axisCountCore2; axisIndex++)
                {
                    ret = GTN_AxisOff(coreNum1, axisIndex);
                    if (ret != 0)
                    {
                        err = $"{mgsHeadCore2}。原因：关闭轴{axisIndex}使能失败。错误码：{ret}";
                        return false;
                    }
                }

                return true;
            });

            // 关闭控制卡
            SafeNativeMethod((out string err) =>
            {
                err = "";

                lock (locker)
                {
                    Interlocked.Decrement(ref initCardCount);

                    if (initCardCount == 0)
                    {
                        // 控制卡关闭
                        ret = GTN_Close();
                        if (ret != 0)
                        {
                            err = $"{msgHead}。原因：关闭轴卡失败。错误码：{ret}";
                            return false;
                        }
                    }
                }

                return true;
            });

            // 修改设备的状态
            OnStatusChanged(DeviceStatus.Offline);
        }


        /// <summary>
        /// 直线插补段
        /// </summary>
        /// <param name="coreNum"></param>
        /// <param name="crdNum"></param>
        /// <param name="fifoNum"></param>
        /// <param name="interPosArr"></param>
        /// <param name="interVelArr"></param>
        /// <param name="interAccArr"></param>
        /// <param name="radius"></param>
        /// <param name="dir"></param>
        /// <param name="msgHead"></param>
        /// <returns></returns>
        private (bool, string) MoveLineInterPolationSegment(short coreNum, short crdNum, short fifoNum, double[] interPosArr, double[] interVelArr, double[] interAccArr, double radius = 0, short dir = 0,
            string msgHead = "")
        {
            bool okng = true;
            string err = "";
            int count = interPosArr.Length;
            short ret = 0;

            // 即将把数据存入坐标系1的FIFO0中，所以要首先清除此缓存区中的数据
            if (count == 2)
            {
                ret = GTN_LnXY(coreNum, crdNum, (int)interPosArr[0], (int)interPosArr[1], interVelArr.Max(), interAccArr.Max(), 0, fifoNum);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：添加插补数据失败。错误码：{ret}";
                    okng = false;
                }
            }
            else if (count == 3)
            {
                ret = GTN_LnXYZ(coreNum, crdNum, (int)interPosArr[0], (int)interPosArr[1], (int)interPosArr[2], interVelArr.Max(), interAccArr.Max(), 0, fifoNum);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：添加插补数据失败。错误码：{ret}";
                    okng = false;

                }
            }
            else if (count == 4)
            {
                ret = GTN_LnXYZA(coreNum, crdNum, (int)interPosArr[0], (int)interPosArr[1], (int)interPosArr[2], (int)interPosArr[3], interVelArr.Max(), interAccArr.Max(), 0, fifoNum);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：添加插补数据失败。错误码：{ret}";
                    okng = false;

                }
            }

            return (okng, err);
        }

        /// <summary>
        /// 圆弧插补段
        /// </summary>
        /// <param name="coreNum"></param>
        /// <param name="crdNum"></param>
        /// <param name="fifoNum"></param>
        /// <param name="interPosArr"></param>
        /// <param name="interVelArr"></param>
        /// <param name="interAccArr"></param>
        /// <param name="radius"></param>
        /// <param name="dir"></param>
        /// <param name="msgHead"></param>
        /// <returns></returns>
        private (bool, string) MoveCircleInterPolationSegment(short coreNum, short crdNum, short fifoNum, double[] interPosArr, double[] interVelArr, double[] interAccArr, double radius, short dir,
                string msgHead = "")
        {
            bool okng = true;
            string err = "";
            int count = interPosArr.Length;
            short ret = 0;

            // 即将把数据存入坐标系1的FIFO0中，所以要首先清除此缓存区中的数据
            if (count == 2)
            {
                ret = GTN_ArcXYR(coreNum, crdNum, (int)interPosArr[0], (int)interPosArr[1], radius, dir, interVelArr.Max(), interAccArr.Max(), 0, fifoNum);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：添加插补数据失败。错误码：{ret}";
                    okng = false;
                }
            }

            return (okng, err);
        }

        /// <summary>
        /// 执行直线插补
        /// </summary>
        /// <param name="axisId"></param>
        /// <param name="pos"></param>
        /// <param name="perPlusArr"></param>
        /// <param name="vel"></param>
        /// <param name="acc"></param>
        private void MoveLine1(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc)
        {
            InterPolationMove(axisId, pos, perPlusArr, vel, acc, radius: 0, dir: 0, MoveLineInterPolationSegment);
        }

        /// <summary>
        /// 执行圆弧插补
        /// </summary>
        /// <param name="axisId"></param>
        /// <param name="pos"></param>
        /// <param name="perPlusArr"></param>
        /// <param name="vel"></param>
        /// <param name="acc"></param>
        /// <param name="radius"></param>
        /// <param name="dir"></param>
        private void MoveCircle1(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc, double radius, short dir)
        {
            InterPolationMove(axisId, pos, perPlusArr, vel, acc, radius, dir, MoveCircleInterPolationSegment);
        }

        /// <summary>
        /// 插补
        /// </summary>
        /// <param name="axisId"></param>
        /// <param name="pos"></param>
        /// <param name="perPlusArr"></param>
        /// <param name="vel"></param>
        /// <param name="acc"></param>
        /// <param name="radius"></param>
        /// <param name="dir"></param>
        /// <param name="SegmentFunc"></param>
        /// <exception cref="Exception"></exception>
        private void InterPolationMove(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc, double radius, short dir,
             Func<short, short, short, double[], double[], double[], double, short, string, (bool, string)> SegmentFunc = null)
        {
            CheckInit();

            int timeout = 20 * 1000;
            int runtime = 0;

            string msgHead = $"{Brand} 执行直线插补动作失败。";
            if (!(axisId != null && pos != null && perPlusArr != null && vel != null && acc != null
                && axisId.Count > 1 && axisId.Count < 5 // 最多允许有4个轴同时插补
                && axisId.Count == pos.Count
                && pos.Count == perPlusArr.Count
                && perPlusArr.Count == vel.Count
                && vel.Count == acc.Count))
            {
                throw new Exception($"{msgHead}原因：输入参数非法。");
            }

            // 先检查两个坐标系是否被使用，如果被使用，需要等待

            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                // 1.检查执行插补的轴当前状态：使能，停止。
                int count = axisId.Count;
                short[] coreNumArr = new short[count];
                short[] axisNumArr = new short[count];
                for (int i = 0; i < count; i++)
                {
                    GetAxisNum(axisId[i], out coreNumArr[i], out axisNumArr[i]);

                    // 检查轴处于上使能状态
                    if (!GetAxisStatus(axisId[i])[AxisStatus.SvOn])
                    {
                        throw new Exception($"{msgHead}原因：轴{axisNumArr[i]}未上使能。");
                    }
                    // 检查轴处于运动停止状态
                    if (GetAxisStatus(axisId[i])[AxisStatus.Moving])
                    {
                        double currentPos = GetCurrentPos(axisId[i], perPlusArr[i]);
                        currentPos = Math.Round((currentPos / perPlusArr[i]), 3);
                        double targetPos = Math.Round(pos[i] / perPlusArr[i], 3);
                        throw new Exception($"{msgHead}原因：轴{axisNumArr[i]}还在运动中，不能执行新动作。当前位置:{currentPos} 目标位置:{targetPos}。");
                    }
                }

                // 2.执行插补的轴必须是同一个内核的轴。
                if (coreNumArr.GroupBy(x => x).Count() != 1)
                {
                    throw new Exception($"{msgHead}原因：执行插补运动的轴必须是同一个内核的轴。");
                }

                // 3.设置插补参数
                short coreNum = coreNumArr[0];
                short crdNum = 1;           // 插补坐标系的序号，范围：[1,2]
                short fifoNum = 0;          // 插补坐标系对应的缓存队列序号，范围：[0,1]

                double[] interPosArr = new double[count];
                double[] interVelArr = new double[count];
                double[] interAccArr = new double[count];
                for (int i = 0; i < count; i++)
                {
                    interPosArr[i] = pos[i] * perPlusArr[i];
                    interVelArr[i] = vel[i] * perPlusArr[i] / 1000;
                    interAccArr[i] = acc[i] * perPlusArr[i] / 1000 / 1000;
                }

                // 插补轴占位符
                short[] axisMask = new short[8];
                for (int i = 0; i < count; i++)
                {
                    axisMask[i] = (short)axisId[i];
                }

                // XY 建立坐标系
                TCrdPrm crdPrm = new TCrdPrm();
                // GTN_GetCrdPrm(coreNum, crdNum, out crdPrm);
                crdPrm.dimension = (short)count;                                    // 坐标系维数
                crdPrm.synVelMax = interVelArr.Max();                               // 最大合成速度
                crdPrm.synAccMax = interAccArr.Max();                               // 最大加速度： 1pulse/ms^2
                crdPrm.evenTime = 50;                                               // 最小匀速时间： 50ms
                crdPrm.profile1 = axisMask[0];                                                // 规划器1对应到X轴
                crdPrm.profile2 = axisMask[1];                                                // 规划器2对应到Y轴
                crdPrm.profile3 = axisMask[2];                                                  // 规划器2对应到Z轴
                crdPrm.profile4 = axisMask[3];                                                  // 规划器2对应到A轴
                crdPrm.profile5 = axisMask[4];                                                // 规划器1对应到X轴
                crdPrm.profile6 = axisMask[5];                                                // 规划器2对应到Y轴
                crdPrm.profile7 = axisMask[6];                                                  // 规划器2对应到Z轴
                crdPrm.profile8 = axisMask[7];                                                  // 规划器2对应到A轴
                crdPrm.setOriginFlag = 1;                                           // 表示需要指定坐标系的原点坐标的规划位置
                crdPrm.originPos1 = 0;                                              // 坐标系的原点坐标的规划位置为每根轴的原点位置
                crdPrm.originPos2 = 0;
                crdPrm.originPos3 = 0;
                crdPrm.originPos4 = 0;
                crdPrm.originPos5 = 0;
                crdPrm.originPos6 = 0;
                crdPrm.originPos7 = 0;
                crdPrm.originPos8 = 0;
                // 建立1号坐标系，设置坐标系参数
                ret = GTN_SetCrdPrm(coreNum, crdNum, ref crdPrm);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：设置插补坐标系参数失败。错误码：{ret}";
                    return false;
                }

                // 即将把数据存入坐标系1的FIFO0中，所以要首先清除此缓存区中的数据
                ret = GTN_CrdClear(coreNum, crdNum, fifoNum);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：清空插补FIFI数据失败。错误码：{ret}";
                    return false;
                }

                // 4.插入一段 直线插补段、或圆弧插补段
                var segmentResult = SegmentFunc?.Invoke(coreNum, crdNum, fifoNum, interPosArr, interVelArr, interAccArr, radius, dir, msgHead);
                err = segmentResult.Value.Item2;
                if (!segmentResult.Value.Item1) { return false; }

                // 5.启动坐标系1的FIFO0的插补运动
                ret = GTN_CrdStart(coreNum, crdNum, fifoNum);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：执行插补动作开始。错误码：{ret}";
                    return false;
                }

                //6. 等待插补动作完成
                while (true)
                {
                    short run = 0;      // 坐标系运动状态查询变量
                    int segment = 0;    // 坐标系运动完成段查询变量

                    ret = GTN_CrdStatus(coreNum, crdNum, out run, out segment, fifoNum);
                    if (ret != 0)
                    {
                        err = $"{msgHead}。原因：获取插补运动状态失败。错误码：{ret}";
                        return false;
                    }

                    // segment 当前执行的段号
                    if (run == 0 && segment >= 1) { break; } //插补完成

                    //超时检测
                    Thread.Sleep(SleepTime);
                    runtime += SleepTime;
                    if (runtime > timeout)
                    {
                        err = $"{msgHead}。原因：检测运动完成超时（20s）！。错误码：{ret}";
                        return false;
                    }

                    // 判定设备状态如果被调用者停止了，直接退出 回零完成检测
                    //if (Status == DeviceStatus.Stop)
                    //{
                    //    err = $"{msgHead}。原因：被调用者终止动作！。错误码：{ret}";
                    //    return false;
                    //}
                }

                return true;
            });
        }


        #region 比较器相关函数

        /// <summary>
        /// 比较器功能：初始化比较参数
        /// </summary>
        /// <param name="encoderNum">输入编码器号。对应的是轴号</param>
        /// <param name="cmpIndex">比较器号</param>
        /// <param name="outNum">输出点序号号</param>
        /// <param name="pulseWidth">输出脉宽</param>
        /// <param name="cmpPoses">比较点位值</param>
        private void CMPInit(short encoderNum, short cmpIndex, short outNum, ushort pulseWidth, int[] cmpPoses)
        {
            CheckInit();

            short coreNum = coreNum1;   //内核号
            short slaveNum = 1;          //模块序号 todo：需要先遍历资源，之后才能拿出来用
            short permit = 0x2;         //设置输出通道的模式 （0x2：第一路位置比较输出）
            // 按位设置硬件输出通道信号输出的类型
            // 从 bit0 - bit15 按位表示对应信号类型输出， 1：控制权打开； 0：控制权关闭
            // Bit0: 通用 IO 指令输出（当 dataType 为 MC_HSO 时，该值无效）
            // Bit1: 第一路位置比较输出
            // Bit2: 第二路位置比较输出
            // Bit3: 使能激光开关光输出（当 dataType 为 MC_GPO 时，该值无效）
            // Bit4: 使能 PWM 信号输出（当 dataType 为 MC_GPO 时，该值无效）
            // Bit5 - bit15 对于 403 模块保留

            // 比较功能只能用 内核1。

            string msgHead = $"{Brand} 位置比较 {cmpIndex} 比较设定失败。";
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                // 1.清空位置比较输出缓存 缓存区FIFO：[1,8]
                ret = GTN_PosCompareClear(coreNum, cmpIndex);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：清空位置比较输出缓存失败。错误码：{ret}";
                    return false;
                }

                // 2.关闭位置比较
                ret = GTN_PosCompareStop(coreNum, cmpIndex);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：关闭位置比较失败。错误码：{ret}";
                    return false;
                }

                // 3.设定输出通道
                ret = mc_ringnet.GTN_SetTerminalPermitEx(coreNum, slaveNum, MC_GPO, ref permit, outNum, 1);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：设定输出通道失败。错误码：{ret}";
                    return false;
                }

                // 4.设定位置比较输出模式
                TPosCompareMode mode = new TPosCompareMode();
                mode.mode = 0; //FIFO模式
                mode.dimension = 1; //一维
                mode.sourceMode = 0; //0-编码器， 1-冲计数器
                mode.outputMode = 0; // 0-脉冲， 1-电平， 2-电平自动翻转
                // mode.outputPulseWidth = 10 * 1000; //设置脉冲宽度 最大65.535ms
                mode.outputPulseWidth = pulseWidth; //设置脉冲宽度 最大65.535ms
                mode.sourceX = encoderNum; //x轴对应的实际轴为轴1
                mode.sourceY = 0;
                mode.errorBand = 10;
                mode.outputCounter = 1;//保留，需要大于0
                ret = GTN_SetPosCompareMode(coreNum, cmpIndex, ref mode);
                if (ret != 0)
                {
                    err = $"{msgHead},原因：设定位置比较输出模式失败。错误码：{ret}";
                    return false;
                }

                // 5.设置位置比较 位置值，输出通道号，比较段号
                TPosCompareData comparedata;
                for (int i = 0; i < cmpPoses.Length; i++)
                {
                    comparedata.pos = cmpPoses[i];
                    comparedata.segmentNumber = (uint)i;
                    ushort outNumMask = (ushort)(1 << (outNum - 1));
                    comparedata.hso = outNumMask;      //位置比较输出hso通道的输出数值,按位表示HSObit0-bit9对应
                                                       //HSO0-HSO9， bit15:表示逻辑位。在激光功能和位置比较输出
                                                       //功能复用时生效；脉冲模式： 0表示无输出， 1表示输出脉冲；
                                                       //电平模式： 0表示无输出， 1表示有输出。
                    comparedata.gpo = outNumMask;      //通用GPO通道的输出数值，按位表示GPO， bit0-bit15分别对应
                                                       //GPO0-GPO15。脉冲模式： 0表示无输出， 1表示输出脉冲；
                                                       //电平模式： 0表示拉低， 1表示拉高。
                    ret = GTN_PosCompareData(coreNum, cmpIndex, ref comparedata);
                    if (ret != 0)
                    {
                        err = $"{msgHead},原因：设置位置比较位置值，输出通道号，比较段号失败。错误码：{ret}";
                        return false;
                    }
                }

                return true;
            });

        }

        /// <summary>
        /// 指定通道输出脉宽信号
        /// </summary>
        /// <param name="cmpIndex"></param>
        /// <param name="outNum"></param>
        /// <param name="pulseWidth"></param>
        private void CMPOutput(short cmpIndex, short outNum, ushort pulseWidth)
        {
            CheckInit();

            short coreNum = coreNum1;   //内核号
            short slaveNum = 1;          //模块序号 todo：需要先遍历资源，之后才能拿出来用
            short permit = 0x2;         //设置输出通道的模式 （0x2：第一路位置比较输出）
            // 按位设置硬件输出通道信号输出的类型
            // 从 bit0 - bit15 按位表示对应信号类型输出， 1：控制权打开； 0：控制权关闭
            // Bit0: 通用 IO 指令输出（当 dataType 为 MC_HSO 时，该值无效）
            // Bit1: 第一路位置比较输出
            // Bit2: 第二路位置比较输出
            // Bit3: 使能激光开关光输出（当 dataType 为 MC_GPO 时，该值无效）
            // Bit4: 使能 PWM 信号输出（当 dataType 为 MC_GPO 时，该值无效）
            // Bit5 - bit15 对于 403 模块保留

            // 比较功能只能用 内核1。
            string msgHead = $"{Brand} 位置比较 {cmpIndex} 输出口 {outNum} 输出脉宽信号失败。";
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                // 输出指定脉宽
                ret = GTN_PosComparePulse(coreNum, cmpIndex, 0, 0, pulseWidth);
                if (ret != 0)
                {
                    err = $"{msgHead}。错误码：{ret}";
                    return false;
                }

                return true;
            });

        }

        /// <summary>
        /// 重新开始比较输出
        /// </summary>
        /// <param name="cmpIndex"></param>
        private void CMPRestart(short cmpIndex)
        {
            CheckInit();

            short coreNum = coreNum1;   //内核号

            string msgHead = $"{Brand} 位置比较 {cmpIndex} 重新开始比较失败。";
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                // 关闭位置比较
                ret = GTN_PosCompareStop(coreNum, cmpIndex);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：关闭预设比较器比较失败。错误码：{ret}";
                    return false;
                }

                // 打开位置比较
                ret = GTN_PosCompareStart(coreNum, cmpIndex);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：打开预设比较器比较失败。错误码：{ret}";
                    return false;
                }

                return true;
            });

        }

        /// <summary>
        /// 获取已触发的比较个数
        /// </summary>
        /// <param name="cmpIndex"></param>
        /// <param name="cmpOutCount"></param>
        private void CMPGetCmpCount(short cmpIndex, short outNum, out int cmpOutCount)
        {
            CheckInit();

            cmpOutCount = 0;
            short coreNum = coreNum1;   //内核号
            TPosCompareStatus posCompareStatus;
            TPosCompareInfo posCompareInfo;
            int tempCmpOutCount = 0;

            string msgHead = $"{Brand} 位置比较 {cmpIndex} 获取输出口比较输出个数失败。";
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                // 读取位置比较输输出功能状态
                ret = GTN_PosCompareStatus(coreNum, cmpIndex, out posCompareStatus);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：读取位置比较输输出功能状态失败。错误码：{ret}";
                    return false;
                }
                tempCmpOutCount = (int)posCompareStatus.pulseCount;

                // 读取位置比较输出相关信息
                //ret = GTN_PosCompareInfo(coreNum, cmpIndex, out posCompareInfo);
                //if (ret != 0)
                //{
                //    err = $"{msgHead}。原因：读取位置比较输出相关信息失败。错误码：{ret}";
                //    return false;
                //}

                return true;
            });

            cmpOutCount = tempCmpOutCount;


        }

        #endregion

        #region SDO和PDO读写
        public void SDORead(short slave, short index, short subindex, short data_size, out int value, short count)
        {
            throw new NotImplementedException();
        }

        public void SDOWrite(short slave, short index, short subindex, int data, short data_size)
        {
            throw new NotImplementedException();
        }

        public void PDORead(short axis, short index, short subindex, short data_size, ref int value, short count)
        {
            throw new NotImplementedException();
        }

        public void PDOWrite(short axis, short index, short subindex, int data, short data_size)
        {
            throw new NotImplementedException();
        }

        public void AxisContinuousMove(int axisNo, double acc, double dec, double perPulse, List<double> pos, List<double> vel)
        {
            throw new NotImplementedException();
        }
        #endregion

    }

}