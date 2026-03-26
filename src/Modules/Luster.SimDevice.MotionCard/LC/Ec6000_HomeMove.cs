using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.MotionCard.LC
{
    class Ec6000_HomeMove
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="Mode"></param>
        /// <param name="homsts">-1:回零错误  1：正在回零  2：未获取到  3：回零成功  4：回零中断
        /// <param name="Offset"></param>
        /// <param name="Vel1"></param>
        /// <param name="Vel2"></param>
        /// <param name="Acc"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        public static short M60_HomMove(short axis, short Mode, ref short homsts, int Offset = 0, uint Vel1 = 5000, uint Vel2 = 1000, uint Acc = 100000, short card = 0)
        {
            short ret = ecat_motion.M_SetHomingMode(axis, 6, card);
            if (ret == 0)
            {
                ret = ecat_motion.M_SetHomingPrm(axis, Mode, Offset, Vel1, Vel2, Acc, 0, card);
                ret = ecat_motion.M_HomingStart(axis, card);
                System.Threading.Thread.Sleep(50);
                M60_HomStsJudgement(axis, out homsts, card);
                return ret;
            }
            else
            {
                return ret;
            }
        }

        public static short M60_HomMove(short axis, ref short homsts, short card = 0)
        {
            short ret = ecat_motion.M_SetHomingMode(axis, 6, card);
            if (ret == 0)
            {
                ret = ecat_motion.M_HomingStart(axis, card);
                System.Threading.Thread.Sleep(50);
                M60_HomStsJudgement(axis, out homsts, card);
                return ret;
            }
            else
            {
                return ret;
            }
        }

        private static short M60_HomStsJudgement(short axis,out short homsts,short card)
        {
            short ret = 0;int psts = 0; homsts = 2;
             ret =ecat_motion.M_GetSts(axis, out psts, 1, card);
            if ((psts & 65536) == 65536)//回零错误
                homsts = -1;
            else if (((psts & 131072) == 131072) && ((psts & 262144) == 262144))
                homsts = 3;
            else if (((psts & 131072) == 131072) && ((psts & 262144) == 0))
                homsts = 4;
            else if (((psts & 131072) == 0))
                homsts = 1;
            return ret;

        }

        public static short M60_WaitHoming(short axis,short timeout ,ref short homsts, short card)
        {
            short ret = 0;
            int Sts = 0; int count_timeout = 0;
            System.Threading.Thread.Sleep(50);
            //判断回零运动完成
            do
            {
                System.Threading.Thread.Sleep(10);
                ecat_motion.M_GetSts(axis, out Sts, 1, card);
                count_timeout++;
            }
            while ((Sts & 0x400) == 0x400 && (count_timeout * 10 < timeout));
            //如果超时直接取消回零并返回错误
            if ((count_timeout * 10 >= timeout))
            {
                M60_StopHome(axis,card);
                return -11;//回零超时
            }
            //如果未超时则判断回零状态
            System.Threading.Thread.Sleep(50);
            M60_HomStsJudgement(axis, out homsts, card);
            int count = 0;
            short mode = 0;
            //切换到控制模式8，失败则重复，重复10次
           ecat_motion.M_HomeCancelSingleAxis(axis, card);
            do
            {
                System.Threading.Thread.Sleep(50);
                ecat_motion.M_SetHomingMode(axis, 8, card);
                ecat_motion.M_EcatGetOperationMode(axis, ref mode, card);
                count++;
            } while ( mode!=8  &&    count < 10);
            //如果回零模式没有切回8则报错
            ecat_motion.M_EcatGetOperationMode(axis, ref mode, card);
            if (mode != 8)
            {
                return -12;//回零切换错误
            }
            //如果没有ALM信号则清除限位信号
            if ((Sts & 0x2) == 0)
                ecat_motion.M_ClrSts(axis, 1, card);

            return ret;
        }
        public static short M60_StopHome(short axis,short card)
        {
            short ret = 0;
            try
            {
                ret= ecat_motion.M_HomeCancelSingleAxis(axis, card);
            }
            catch
            {
                 ret = ecat_motion.M_HomeCancel(Convert.ToUInt32((long)1 << (short)(axis - 1)), card);
            }
            int count = 0;
            System.Threading.Thread.Sleep(10);
            //M60_HomStsJudgement(axis, out homsts, card);
            do
            {
                System.Threading.Thread.Sleep(50);
                ret = ecat_motion.M_SetHomingMode(axis, 8, card);
                count++;
            } while (ret != 0 && count < 10);
            return ret;
        }
    }
}
