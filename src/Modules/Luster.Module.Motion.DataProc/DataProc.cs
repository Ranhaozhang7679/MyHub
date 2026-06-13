using Luster.Module.Motion.DataProc.Functions;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Xml.Linq;

namespace Luster.Module.Motion.DataProc
{
    public class DataProc : MotionModule
    {
        public override void InitFunctions()
        {
            AddFunction<StringMerge>();
            AddFunction<GenString>();
            AddFunction<Rename>();
            AddFunction<GenRandomNumber>();
            AddFunction<StringParse>();
            AddFunction<Calculator>();
            AddFunction<LogicCalculator>();
            AddFunction<DataTransfer>();
            AddFunction<JSONParse>();
            AddFunction<ReadDataFile>();
            AddFunction<GetByDataBase>();
            AddFunction<CalcTime>();
            AddFunction<CarrierBlackList>();
            AddFunction<TableCreate>();
            AddFunction<GetAverage>();
            AddFunction<RunExe>();
            AddFunction<SplitIntToBit>();
            AddFunction<JoinBitToInt>();
            AddFunction<SplitString>();
            AddFunction<TxtReader>();
            AddFunction<TxtWriter>();
            AddFunction<VisionProcessData>();
            AddFunction<WebConfigRead>();
            AddFunction<ConditionTimer>();
            AddFunction<TableInsert>();
            AddFunction<GetSlopeIntercept>();
            AddFunction<CopyFile>();
        }
    }

    public class LogicCreator : MotionModuleCreator<DataProc>
    {
        public override int Sort => 10;

        public override string Icon => "\xe64a";
    }
}
