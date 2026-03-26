using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.Integration.Web
{

    public class OperationMethod
    {
        // 停止
        public static string ToEnginer = "ToEnginer";

        // 启动
        public static string ToRun = "ToRun";
    }

    public class ResultStatus
    {
        public string Status { get; set; }

        public string Message { get; set; }
        // 2025-5-10 wyy 添加两个属性值，Hive2.6
        // 客户系统返回的注册状态
        public string onboarding_status { get; set; }
        // 客户系统返回的报文信息
        public string msg { get; set; }

        public string Data { get; set; }

        public int RecordNum { get; set; }

        public int ErrorNum { get; set; }

        public string ErrorData { get; set; }

        /// <summary>
        /// 线体触发单站操作指令，包括整线启停功能，以及整线切机功能，如：CheckToEngineer/ToEngineer/CheckToRun/ToRun/CheckChangeRecipe/ChangeRecipe
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// 配套切机指令，为切机配方名称 或者产品二维码
        /// </summary>
        public string ActionData1 { get; set; }

        /// <summary>
        /// FA指令，目录名称：SN_时间\StationID\日志|图片
        /// </summary>
        public string ActionData2 { get; set; }

        /// <summary>
        /// ftp路径
        /// </summary>
        public string FtpPath { get; set; }


        public bool IsSuccess => Status == "OK";
    }
}
