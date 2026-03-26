using Luster.Module.Motion.Stations.Functions;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Xml.Linq;

namespace Luster.Module.Motion.Stations
{
    public class Stations : MotionModule, ILogic
    {
        public override void InitFunctions()
        {
            //AddFunction<FirstStation>();
            //AddFunction<Station>();
            //AddFunction<LastStation>();
            AddFunction<FreeStation>();
            //AddFunction<PlcStation>();  
            AddFunction<HomeStation>();
            AddFunction<TestStation>();
            AddFunction<NGStation>();
            AddFunction<ResetStation>();
            AddFunction<StartStation>();
            AddFunction<BackgroundStation>();


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

    public class LogicCreator : MotionModuleCreator<Stations>
    {
        public override int Sort => 0;

        public override string Icon => "\xe693";
    }
}
