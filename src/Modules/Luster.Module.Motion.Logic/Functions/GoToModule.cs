using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Functions;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using Luster.TaskFlow.Motion.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Logic.Functions
{
    /// <summary>
    /// 跳转模块
    /// </summary>
    public class GoToModule : MotionFunction, INote, IRefFunction, IGoToFunction
    {
        [NotEmpty]
        [Parameter("跳转条件", 1, CN = "跳转条件")]
        public LExpression Express { get; set; }

        [NotEmpty]
        [Parameter("跳转模块", 2, CN = "跳转模块", CanRef = ParamRef.None)]
        public LModule GoModule { get; set; }

        public override string[] NoteParams => new string[] { "跳转" };

        /// <summary>
        /// 构造函数
        /// </summary>
        public GoToModule()
        {
            this.Tips = "跳转到某个模块";
            this.Icon = "\xe6cd";
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="errMsg">错误消息</param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            bool isResult = Express.GetResult(MyOwner);
            if (isResult)
            {
                var goTo = MyOwner.Parameters[nameof(GoModule)];
                var lModule = goTo.Value as LModule;
                if (lModule != null)
                {
                    var module = MyOwner.TaskModules[lModule.ID] as IMotionModule;
                    if (module != null)
                    {
                        if (module.Station == null)
                        {
                            module.UpdateStation();
                        }

                        if (module.Station != MyOwner.Station)
                        {
                            errMsg = $"跳转模块工站:{module.Station.Name} 和当前工站:{MyOwner.Station} 不匹配!";
                            return false;
                        }

                        // 跳转模块
                        // JumpModule = module;
                        // module.IsJumped = true;

                        // 模块跳转
                        MyOwner.OnLog(LogType.Info, $"工站跳转开始：{MyOwner.Alias}->{module.Alias}");

                        //if (module.Station.TaskFunction is IFreeStation f)
                        //{
                        //    f.GoTo(module);

                        //    // 模块跳转
                        //    MyOwner.OnLog(LogType.Info, $"工站跳转开始：{MyOwner.Alias}->{module.Alias}");
                        //}
                    }
                }
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual List<LReference> GetReferences()
        {
            var list = GetReferences(nameof(Express));
            var p = MyOwner.Parameters[nameof(GoModule)];
            if (p != null && p.Value != null && p.Value is LModule m)
            {
                list.Add(new LReference()
                {
                    ByRefName = nameof(GoModule),
                    RefID = m.ID,
                    RefName = m.Name,
                });
            }

            return list;
        }

        public override string GetNote(INote note)
        {
            string noteStr = $"跳转-";

            if (GoModule != null)
            {

            }

            return noteStr;
        }

        /// <summary>
        /// 跳转成功
        /// </summary>
        public bool GetJump(out IMotionModule jumpModule)
        {
            jumpModule = null;

            if (Express!=null&&!Express.GetResult(MyOwner)) return false;
            if (GoModule != null && MyOwner.TaskModules.Contains(GoModule.ID))
            {
                jumpModule = MyOwner.TaskModules[GoModule.ID] as IMotionModule;
            }

            return jumpModule != null;
        }
    }
}
