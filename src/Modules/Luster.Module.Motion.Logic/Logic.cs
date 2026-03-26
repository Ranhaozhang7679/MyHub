using Luster.Module.Motion.Logic.Functions;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Xml.Linq;

namespace Luster.Module.Motion.Logic
{
    public class Logic : MotionModule, ILogic
    {
        public override void InitFunctions()
        {
            AddFunction<Group>();
            AddFunction<Loop>();
            AddFunction<AsyncGroup>();
            AddFunction<Parallel>();
            //AddFunction<NGGroup>();
            AddFunction<Judge>();
            AddFunction<Switch>();
            AddFunction<GoToModule>();
            AddFunction<Return>();
            AddFunction<CheckVariable>();
            AddFunction<SetVariable>();
            AddFunction<Delay>();
            AddFunction<ProEvent>();
            AddFunction<Alarm>();
            //AddFunction<UploadData>();
            AddFunction<RefGroup>();
            AddFunction<WaitStatus>();
            AddFunction<ResetVariable>();
            AddFunction<StationSet>();
            AddFunction<WaitCondition>();
            AddFunction<ConfirmButton>();
            AddFunction<CompeteCondition>();

            AddFunction<TimerJudge>();

            AddFunction<DialogBox>();

            AddFunction<Script>();

        }

        /// <summary>
        /// Ä£¿éµ¼³ö
        /// </summary>
        /// <returns></returns>
        public override XElement ExportXml()
        {
            var exportXml = base.ExportXml();
            XElement xChildren = new XElement("Children");

            foreach (var item in Children)
            {
                xChildren.Add(item.ExportXml());
            }

            exportXml.Add(xChildren);

            return exportXml;
        }
    }

    public class LogicCreator : MotionModuleCreator<Logic>
    {
        public override int Sort => 1;

        public override string Icon => "\xe64a";
    }
}
