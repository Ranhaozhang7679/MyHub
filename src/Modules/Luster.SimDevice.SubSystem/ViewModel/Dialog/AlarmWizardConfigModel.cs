using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class SubTypeMapping
    {
        public string ErrorType { get; set; }
        public List<ComboOption> Options { get; set; } = new List<ComboOption>();
    }

    public class AlarmWizardConfigModel
    {
        public List<ComboOption> ErrorTypes { get; set; } = new List<ComboOption>();
        public List<SubTypeMapping> ErrorSubTypes { get; set; } = new List<SubTypeMapping>();
        public List<ComboOption> Components { get; set; } = new List<ComboOption>();
        public List<ComboOption> SubComponents { get; set; } = new List<ComboOption>();
        public List<ComboOption> RepairActions { get; set; } = new List<ComboOption>();

        public static AlarmWizardConfigModel GetDefaultConfig()
        {
            var config = new AlarmWizardConfigModel();
            
            // 报警类型
            config.ErrorTypes.Add(new ComboOption { Value = "M", Header = "Motion Error (M)" });
            config.ErrorTypes.Add(new ComboOption { Value = "C", Header = "Pneumatic Cylinder Error (C)" });
            config.ErrorTypes.Add(new ComboOption { Value = "U", Header = "Vacuum Error (U)" });
            config.ErrorTypes.Add(new ComboOption { Value = "S", Header = "Sensor Error (S)" });
            config.ErrorTypes.Add(new ComboOption { Value = "L", Header = "Laser Error (L)" });
            config.ErrorTypes.Add(new ComboOption { Value = "W", Header = "Screwing Error (W)" });
            config.ErrorTypes.Add(new ComboOption { Value = "N", Header = "Software Related Error (N)" });
            config.ErrorTypes.Add(new ComboOption { Value = "V", Header = "Vision Error (V)" });
            config.ErrorTypes.Add(new ComboOption { Value = "T", Header = "Scanning Error (T)" });
            config.ErrorTypes.Add(new ComboOption { Value = "F", Header = "Safety Error (F)" });
            config.ErrorTypes.Add(new ComboOption { Value = "E", Header = "Material Shortage Error (E)" });
            config.ErrorTypes.Add(new ComboOption { Value = "O", Header = "Other (O)" });

            // 报警子类
            // M
            var motionSubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - Motor/Robot Overload" },
                new ComboOption { Value = "02", Header = "02 - Not in position" },
                new ComboOption { Value = "03", Header = "03 - Timeout" },
                new ComboOption { Value = "04", Header = "04 - Negative Position" },
                new ComboOption { Value = "05", Header = "05 - Positive Position" },
                new ComboOption { Value = "06", Header = "06 - Motor Enable Error" },
                new ComboOption { Value = "07", Header = "07 - Communication Error" },
                new ComboOption { Value = "08", Header = "08 - Max Attempts reached" },
                new ComboOption { Value = "99", Header = "99 - Other" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "M", Options = motionSubs });

            // C
            var pneumaticSubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - Negative Position" },
                new ComboOption { Value = "02", Header = "02 - Positive Position" },
                new ComboOption { Value = "03", Header = "03 - Communication Error" },
                new ComboOption { Value = "04", Header = "04 - Air pressure/Air flow error" },
                new ComboOption { Value = "99", Header = "99 - Other" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "C", Options = pneumaticSubs });

            // U
            var vacuumSubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - Vacuum Alarm" },
                new ComboOption { Value = "99", Header = "99 - Other" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "U", Options = vacuumSubs });

            // S
            var sensorSubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - Load Cell force abnormal" },
                new ComboOption { Value = "02", Header = "02 - Sensor detected/not detected" },
                new ComboOption { Value = "03", Header = "03 - Temperature alarm" },
                new ComboOption { Value = "04", Header = "04 - Communication Error" },
                new ComboOption { Value = "99", Header = "99 - Other" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "S", Options = sensorSubs });

            // L
            var laserSubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - Laser not enabled" },
                new ComboOption { Value = "02", Header = "02 - Abnormal data" },
                new ComboOption { Value = "03", Header = "03 - Communication Error" },
                new ComboOption { Value = "04", Header = "04 - Other" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "L", Options = laserSubs });
            
            // W
            var screwingSubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - Torque Alarm" },
                new ComboOption { Value = "02", Header = "02 - Cycle Alarm" },
                new ComboOption { Value = "03", Header = "03 - Communication Error" },
                new ComboOption { Value = "99", Header = "99 - Other" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "W", Options = screwingSubs });

            // N
            var softwareSubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - Value out of range" },
                new ComboOption { Value = "02", Header = "02 - Data save failure" },
                new ComboOption { Value = "03", Header = "03 - Communication Error" },
                new ComboOption { Value = "99", Header = "99 - Other" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "N", Options = softwareSubs });

            // V
            var visionSubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - Image Acquisition" },
                new ComboOption { Value = "02", Header = "02 - Image Processing" },
                new ComboOption { Value = "03", Header = "03 - Foolproofing" },
                new ComboOption { Value = "04", Header = "04 - Inspection Tolerance" },
                new ComboOption { Value = "05", Header = "05 - Other" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "V", Options = visionSubs });

            // T
            var scanningSubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - Barcode Scan Error" },
                new ComboOption { Value = "02", Header = "02 - Type Error" },
                new ComboOption { Value = "03", Header = "03 - Improper Serial Number length" },
                new ComboOption { Value = "04", Header = "04 - Material Match Error" },
                new ComboOption { Value = "05", Header = "05 - Process Error" },
                new ComboOption { Value = "06", Header = "06 - Communication Error" },
                new ComboOption { Value = "07", Header = "07 - Other" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "T", Options = scanningSubs });

            // F
            var safetySubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - E-Stop Pressed" },
                new ComboOption { Value = "02", Header = "02 - Light Curtain/Door alarm" },
                new ComboOption { Value = "99", Header = "99 - Other" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "F", Options = safetySubs });

            // E
            var materialSubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - Out of Material" },
                new ComboOption { Value = "02", Header = "02 - Almost out of Material warning" },
                new ComboOption { Value = "99", Header = "99 - Other" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "E", Options = materialSubs });
            
            // O
            var otherSubs = new List<ComboOption>
            {
                new ComboOption { Value = "99", Header = "99 - Other" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "O", Options = otherSubs });

            // --- Components ---
            config.Components.Add(new ComboOption { Value = "CV", Header = "Conveyor (CV)" });
            config.Components.Add(new ComboOption { Value = "DS", Header = "Dispense (DS)" });
            config.Components.Add(new ComboOption { Value = "EE", Header = "Electric (EE)" });
            config.Components.Add(new ComboOption { Value = "EF", Header = "End Effector (EF)" });
            config.Components.Add(new ComboOption { Value = "ES", Header = "Emergency Buttons (ES)" });
            config.Components.Add(new ComboOption { Value = "GA", Header = "Gantry (GA)" });
            config.Components.Add(new ComboOption { Value = "IP", Header = "IPC/Computer (IP)" });
            config.Components.Add(new ComboOption { Value = "LC", Header = "Load Cell (LC)" });
            config.Components.Add(new ComboOption { Value = "LO", Header = "Loader (LO)" });
            config.Components.Add(new ComboOption { Value = "PL", Header = "PLC (PL)" });
            config.Components.Add(new ComboOption { Value = "PN", Header = "Pneumatics (PN)" });
            config.Components.Add(new ComboOption { Value = "PS", Header = "Press (PS)" });
            config.Components.Add(new ComboOption { Value = "RB", Header = "Robot (RB)" });
            config.Components.Add(new ComboOption { Value = "SC", Header = "Safety interlock/door/barrier (SC)" });
            config.Components.Add(new ComboOption { Value = "SS", Header = "Sensor/Signal (SS)" });
            config.Components.Add(new ComboOption { Value = "UL", Header = "Unloader (UL)" });
            config.Components.Add(new ComboOption { Value = "VG", Header = "Vacuum generator (VG)" });
            config.Components.Add(new ComboOption { Value = "VS", Header = "CCD or other vision system (VS)" });
            config.Components.Add(new ComboOption { Value = "OO", Header = "Other (OO)" });

            // --- Sub-Components ---
            config.SubComponents.Add(new ComboOption { Value = "BL", Header = "Belts (BL)" });
            config.SubComponents.Add(new ComboOption { Value = "A", Header = "Axis (A*)" });
            config.SubComponents.Add(new ComboOption { Value = "BO", Header = "Bolt (BO)" });
            config.SubComponents.Add(new ComboOption { Value = "BR", Header = "Bracket (BR)" });
            config.SubComponents.Add(new ComboOption { Value = "CA", Header = "Cables (CA)" });
            config.SubComponents.Add(new ComboOption { Value = "CE", Header = "Connector (Electrical) (CE)" });
            config.SubComponents.Add(new ComboOption { Value = "CL", Header = "Clamp (CL)" });
            config.SubComponents.Add(new ComboOption { Value = "CO", Header = "Coupler (CO)" });
            config.SubComponents.Add(new ComboOption { Value = "CP", Header = "Connector (Pneumatic) (CP)" });
            config.SubComponents.Add(new ComboOption { Value = "CY", Header = "Cylinder (CY)" });
            config.SubComponents.Add(new ComboOption { Value = "DE", Header = "Driver (Electrical) (DE)" });
            config.SubComponents.Add(new ComboOption { Value = "DR", Header = "Driver (Robot) (DR)" });
            config.SubComponents.Add(new ComboOption { Value = "DV", Header = "Driver (Vision) (DV)" });
            config.SubComponents.Add(new ComboOption { Value = "H", Header = "Holder/Nest (General) (H*)" });
            config.SubComponents.Add(new ComboOption { Value = "ME", Header = "Mechanism (ME)" });
            config.SubComponents.Add(new ComboOption { Value = "M", Header = "Motor (M*)" });
            config.SubComponents.Add(new ComboOption { Value = "MP", Header = "Mechanical Part (MP)" });
            config.SubComponents.Add(new ComboOption { Value = "NE", Header = "Needle (NE)" });
            config.SubComponents.Add(new ComboOption { Value = "NO", Header = "Nozzle (NO)" });
            config.SubComponents.Add(new ComboOption { Value = "PC", Header = "PLC cable (PC)" });
            config.SubComponents.Add(new ComboOption { Value = "PO", Header = "PLC connector (PO)" });
            config.SubComponents.Add(new ComboOption { Value = "PR", Header = "PLC card (PR)" });
            config.SubComponents.Add(new ComboOption { Value = "RO", Header = "Rollers (RO)" });
            config.SubComponents.Add(new ComboOption { Value = "RT", Header = "Robot Tool (RT)" });
            config.SubComponents.Add(new ComboOption { Value = "SC", Header = "Screws (SC)" });
            config.SubComponents.Add(new ComboOption { Value = "SE", Header = "Sensor (Electrical) (SE)" });
            config.SubComponents.Add(new ComboOption { Value = "SF", Header = "Sensor Flow (SF)" });
            config.SubComponents.Add(new ComboOption { Value = "SH", Header = "Shim (SH)" });
            config.SubComponents.Add(new ComboOption { Value = "SL", Header = "Sensor (Load-cell) (SL)" });
            config.SubComponents.Add(new ComboOption { Value = "SO", Header = "Stopper (SO)" });
            config.SubComponents.Add(new ComboOption { Value = "SP", Header = "Sensor (Positioning) (SP)" });
            config.SubComponents.Add(new ComboOption { Value = "SR", Header = "Sensor (Pressure) (SR)" });
            config.SubComponents.Add(new ComboOption { Value = "TU", Header = "Tubes (TU)" });
            config.SubComponents.Add(new ComboOption { Value = "VD", Header = "Valve directional (VD)" });
            config.SubComponents.Add(new ComboOption { Value = "VP", Header = "Valve pressure (VP)" });
            config.SubComponents.Add(new ComboOption { Value = "VV", Header = "Valve vacuum (VV)" });
            config.SubComponents.Add(new ComboOption { Value = "PL", Header = "Peeler (PL)" });
            config.SubComponents.Add(new ComboOption { Value = "OO", Header = "Other (OO)" });

            // --- Repair Actions ---
            config.RepairActions.Add(new ComboOption { Value = "", Header = "<None>" });
            config.RepairActions.Add(new ComboOption { Value = "01", Header = "01 - adjusted" });
            config.RepairActions.Add(new ComboOption { Value = "02", Header = "02 - calibrated" });
            config.RepairActions.Add(new ComboOption { Value = "03", Header = "03 - checked" });
            config.RepairActions.Add(new ComboOption { Value = "04", Header = "04 - cleaned" });
            config.RepairActions.Add(new ComboOption { Value = "05", Header = "05 - erased" });
            config.RepairActions.Add(new ComboOption { Value = "06", Header = "06 - filled" });
            config.RepairActions.Add(new ComboOption { Value = "07", Header = "07 - fixed" });
            config.RepairActions.Add(new ComboOption { Value = "08", Header = "08 - lubricated" });
            config.RepairActions.Add(new ComboOption { Value = "09", Header = "09 - replaced" });
            config.RepairActions.Add(new ComboOption { Value = "10", Header = "10 - restored" });
            config.RepairActions.Add(new ComboOption { Value = "11", Header = "11 - stretched" });
            config.RepairActions.Add(new ComboOption { Value = "12", Header = "12 - tightened" });
            config.RepairActions.Add(new ComboOption { Value = "13", Header = "13 - trained" });
            config.RepairActions.Add(new ComboOption { Value = "14", Header = "14 - tuned" });
            config.RepairActions.Add(new ComboOption { Value = "15", Header = "15 - other" });

            return config;
        }
    }
}
