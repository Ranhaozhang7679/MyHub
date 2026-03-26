using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Motion.DataStruct.DataModels;
using System.Xml.Linq;
using Luster.Motion.DataStruct.Enums;

namespace Luster.Motion.DataStruct.FXVirtual
{
    public class MCDGenerator
    {
        public XElement GenerateMCDXml(List<VIO> vioList, List<VAxis> axisList)
        {
            // 创建根元素
            XElement root = new XElement("SignalAdapters",
                new XAttribute("Version", "nx1872"),
                new XElement("SignalAdapter",
                    new XAttribute("Name", "ControlSignals")
                )
            );

            XElement adapter = root.Element("SignalAdapter");
            int signalId = 2000; // 起始信号ID

            // 1. 添加参数 (Parameters)
            adapter.Add(GenerateParameters(axisList));

            // 2. 添加信号 (Signals)
            var signals = new List<XElement>();
            signalId = GenerateSignals(vioList, axisList, signals, signalId);
            adapter.Add(new XElement("Signals", signals));

            // 3. 添加公式 (Formulas)
            adapter.Add(GenerateFormulas(axisList));

            // 4. 添加最大信号ID
            adapter.Add(new XElement("MaxSignalId", signalId.ToString()));

            return root;
        }

        public XElement GenerateParameters(List<VAxis> axisList)
        {
            XElement parameters = new XElement("Parameters");

            foreach (var axis in axisList)
            {
                string obj = axis.Name;

                AddParameter(parameters, $"{obj}_Speed", obj, "Position Control", "Velocity");
                AddParameter(parameters, $"{obj}_Acc", obj, "Position Control", "Acceleration");
                AddParameter(parameters, $"{obj}_Dec", obj, "Position Control", "Deceleration");
                AddParameter(parameters, $"{obj}_Pos", obj, "Position Control", "Location");
            }

            return parameters;
        }
        private void AddParameter(XElement parent, string alias, string obj, string objType, string param)
        {
            parent.Add(new XElement("Parameter",
                new XAttribute("Alias", alias),
                new XAttribute("Object", obj),
                new XAttribute("ObjectType", objType),
                new XAttribute("Parameter", param)
            ));
        }

        private int GenerateSignals(List<VIO> vioList, List<VAxis> axisList, List<XElement> signals, int startId)
        {
            int signalId = startId;
            var ioGroups = GroupIOByCard(vioList);

            // 处理数字IO信号
            foreach (var cardGroup in ioGroups)
            {
                string cardName = cardGroup.Key;
                var inIO = cardGroup.Value.Where(v => v.Behavior == IOBehavior.Input).ToList();
                var outIO = cardGroup.Value.Where(v => v.Behavior == IOBehavior.Output).ToList();

                // 输入IO (作为输出信号)
                signalId = ProcessIOGroup(signals, inIO, cardName, "IN", "Output", "Get", ref signalId);

                // 输出IO (作为输入信号)
                signalId = ProcessIOGroup(signals, outIO, cardName, "OUT", "Input", "Set", ref signalId);
            }

            // 处理轴信号
            foreach (var axis in axisList)
            {
                string obj = axis.Name;
                bool isRotate = obj.Contains("_Rotate");
                // 运动参数信号 (输入)
                signalId = AddAxisStateSignal(signals, obj, "SetRsv0", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetRsv1", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetRsv2", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetRsv3", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetRsv4", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetRsv5", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetJog", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetDir", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetRsv8", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetRsv9", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetRsv10", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetRsv11", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetRsv12", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetRsv13", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetRun", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetSevon", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetRsv16", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetRsv17", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetRsv18", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetRsv19", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetDstp", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetRdy", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetEz", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetInp", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetSln", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetSlp", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetRsv26", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetOrg", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetEmg", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetEln", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetElp", "bool", "Input", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "SetAlm", "bool", "Input", "false", ref signalId);

                signalId = AddAxisSignal(signals, obj, "SetSpeed", "double", "Input", "0.000000",
                    isRotate ? "Angular Velocity" : "Velocity",
                    isRotate ? "degrees/sec" : "mm/sec", ref signalId);

                signalId = AddAxisSignal(signals, obj, "SetAcc", "double", "Input", "0.000000",
                    isRotate ? "Angular Acceleration" : "Acceleration",
                    isRotate ? "degrees/sec^2" : "mm/sec^2", ref signalId);

                signalId = AddAxisSignal(signals, obj, "SetDec", "double", "Input", "0.000000",
                    isRotate ? "Angular Acceleration" : "Acceleration",
                    isRotate ? "degrees/sec^2" : "mm/sec^2", ref signalId);

                signalId = AddAxisSignal(signals, obj, "SetPos", "double", "Input", "0.000000",
                    isRotate ? "Angle" : "Length",
                    isRotate ? "degrees" : "mm", ref signalId);

                // 反馈信号 (输出)
                signalId = AddAxisStateSignal(signals, obj, "GetRsv0", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetRsv1", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetRsv2", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetRsv3", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetRsv4", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetRsv5", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetJog", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetDir", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetRsv8", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetRsv9", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetRsv10", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetRsv11", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetRsv12", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetRsv13", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetRun", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetSevon", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetRsv16", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetRsv17", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetRsv18", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetRsv19", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetDstp", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetRdy", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetEz", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetInp", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetSln", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetSlp", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetRsv26", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetOrg", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetEmg", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetEln", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetElp", "bool", "Output", "false", ref signalId);
                signalId = AddAxisStateSignal(signals, obj, "GetAlm", "bool", "Output", "false", ref signalId);

                signalId = AddAxisSignal(signals, obj, "GetSpeed", "double", "Output", "0.000000",
                    isRotate ? "Angular Velocity" : "Velocity",
                    isRotate ? "degrees/sec" : "mm/sec", ref signalId);

                signalId = AddAxisSignal(signals, obj, "GetAcc", "double", "Output", "0.000000",
                    isRotate ? "Angular Acceleration" : "Acceleration",
                    isRotate ? "degrees/sec^2" : "mm/sec^2", ref signalId);

                signalId = AddAxisSignal(signals, obj, "GetDec", "double", "Output", "0.000000",
                    isRotate ? "Angular Acceleration" : "Acceleration",
                    isRotate ? "degrees/sec^2" : "mm/sec^2", ref signalId);

                signalId = AddAxisSignal(signals, obj, "GetPos", "double", "Output", "0.000000",
                    isRotate ? "Angle" : "Length",
                    isRotate ? "degrees" : "mm", ref signalId);

                //轴的AxisState信号

            }

            return signalId;
        }
        private Dictionary<string, List<VIO>> GroupIOByCard(List<VIO> vioList)
        {
            var groups = new Dictionary<string, List<VIO>>();

            foreach (var vio in vioList)
            {
                if (!groups.ContainsKey(vio.CardName))
                {
                    groups[vio.CardName] = new List<VIO>();
                }
                groups[vio.CardName].Add(vio);
            }

            return groups;
        }

        private int ProcessIOGroup(List<XElement> signals, List<VIO> ioList, string cardName,
                                  string ioTypePrefix, string xmlIoType, string suffix, ref int signalId)
        {
            int numIO = (int)Math.Ceiling(ioList.Count / 32f) * 32;

            for (int i = 0; i < numIO; i++)
            {
                signalId++;
                string name;

                if (i < ioList.Count)
                {
                    name = $"{cardName}_{ioList[i].Name}_{suffix}";
                }
                else
                {
                    name = $"{cardName}_{ioTypePrefix}_Reserved{i}_{suffix}";
                }

                string dataType = ioList[0].IOType == IOType.Digital ? "bool" : "double";
                string initValue = (dataType == "bool") ? "false" : "0.0";

                signals.Add(CreateSignal(dataType, xmlIoType, initValue, name, signalId));
            }

            return signalId;
        }

        private int AddAxisSignal(List<XElement> signals, string obj, string nameSuffix,
                                string dataType, string ioType, string initValue,
                                string measure, string unit, ref int signalId)
        {
            signalId++;
            signals.Add(new XElement("Signal",
                new XAttribute("DataType", dataType),
                new XAttribute("IOType", ioType),
                new XAttribute("InitValue", initValue),
                new XAttribute("Measure", measure),
                new XAttribute("Name", $"{obj}_{nameSuffix}"),
                new XAttribute("SignalId", signalId),
                new XAttribute("Unit", unit)
            ));
            return signalId;
        }

        private int AddAxisStateSignal(List<XElement> signals, string obj, string nameSuffix,
                                string dataType, string ioType, string initValue, ref int signalId)
        {
            signalId++;
            signals.Add(new XElement("Signal",
                new XAttribute("DataType", dataType),
                new XAttribute("IOType", ioType),
                new XAttribute("InitValue", initValue),
                new XAttribute("Name", $"{obj}_{nameSuffix}"),
                new XAttribute("SignalId", signalId)
            ));
            return signalId;
        }

        private XElement CreateSignal(string dataType, string ioType, string initValue, string name, int signalId)
        {
            return new XElement("Signal",
                new XAttribute("DataType", dataType),
                new XAttribute("IOType", ioType),
                new XAttribute("InitValue", initValue),
                new XAttribute("Name", name),
                new XAttribute("SignalId", signalId)
            );
        }

        public XElement GenerateFormulas(List<VAxis> axisList)
        {
            XElement formulas = new XElement("Formulas");

            foreach (var axis in axisList)
            {
                string obj = axis.Name;

                // 运动参数公式
                AddFormula(formulas, $"{obj}_Speed", $"{obj}_SetSpeed");
                AddFormula(formulas, $"{obj}_GetSpeed", $"{obj}_Speed");

                AddFormula(formulas, $"{obj}_Acc", $"{obj}_SetAcc");
                AddFormula(formulas, $"{obj}_GetAcc", $"{obj}_Acc");

                AddFormula(formulas, $"{obj}_Dec", $"{obj}_SetDec");
                AddFormula(formulas, $"{obj}_GetDec", $"{obj}_Dec");

                AddFormula(formulas, $"{obj}_Pos", $"{obj}_SetPos");
                AddFormula(formulas, $"{obj}_GetPos", $"{obj}_Pos");
            }

            return formulas;
        }

        private void AddFormula(XElement parent, string assignTo, string formula)
        {
            parent.Add(new XElement("Formula",
                new XAttribute("AssignTo", assignTo),
                new XAttribute("Formula", formula)
            ));
        }
    }

}

