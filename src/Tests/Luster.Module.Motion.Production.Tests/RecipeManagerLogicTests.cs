using DC.Authorization;
using DC.Authorization.Models;
using Luster.Module.Motion.Production;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Luster.Module.Motion.ProductionTests
{
    /// <summary>
    /// TES-33 P8-A/C:RecipeAuthItems 权限项 + RecipeManager 权限判定/复制目录 单测。
    /// RecipeManager 集成路径（ICommonBus 委托）待集成测试，软件层测纯逻辑。
    /// </summary>
    public class RecipeManagerLogicTests : IDisposable
    {
        private readonly string _tmpDir;
        public RecipeManagerLogicTests()
        {
            _tmpDir = Path.Combine(Path.GetTempPath(), "recipe_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tmpDir);
        }
        public void Dispose() { try { Directory.Delete(_tmpDir, true); } catch { } }

        #region RecipeAuthItems.ToRights

        [Fact]
        public void ToRights_6项全部转换()
        {
            Right[] rights = RecipeAuthItems.ToRights();
            Assert.Equal(6, rights.Length);
        }

        [Fact]
        public void ToRights_操作权限为Operation类型()
        {
            Right[] rights = RecipeAuthItems.ToRights();
            Right saveRight = Array.Find(rights, r => r.Name == "保存配方");
            Assert.NotNull(saveRight);
            Assert.Equal(RightType.Operation, saveRight.Type);
            Assert.Equal("配方管理", saveRight.ModuleName);
            Assert.Equal("配方操作", saveRight.ViewName);
        }

        [Fact]
        public void ToRights_VizRecipePanel为Visibility类型()
        {
            Right[] rights = RecipeAuthItems.ToRights();
            Right vizRight = Array.Find(rights, r => r.Name == "配方面板");
            Assert.NotNull(vizRight);
            Assert.Equal(RightType.Visibility, vizRight.Type);
        }

        [Fact]
        public void ToRights_字段映射对齐AuthItem()
        {
            Right[] rights = RecipeAuthItems.ToRights();
            Right switchRight = Array.Find(rights, r => r.Name == "切换配方");
            Assert.Equal(RecipeAuthItems.SwitchRecipe.Module, switchRight.ModuleName);
            Assert.Equal(RecipeAuthItems.SwitchRecipe.View, switchRight.ViewName);
            Assert.Equal(RecipeAuthItems.SwitchRecipe.Description, switchRight.Description);
            Assert.Equal(RecipeAuthItems.SwitchRecipe.Order, switchRight.SortOrder);
        }

        [Fact]
        public void RecipeAuthItems_覆盖P8A配方操作全集()
        {
            // 用户硬验收：配方加载/保存/导入/导出/切换/复制/删除
            Assert.NotNull(RecipeAuthItems.SaveRecipe);       // 保存
            Assert.NotNull(RecipeAuthItems.SwitchRecipe);     // 切换
            Assert.NotNull(RecipeAuthItems.CopyRecipe);       // 复制
            Assert.NotNull(RecipeAuthItems.DeleteRecipe);     // 删除
            Assert.NotNull(RecipeAuthItems.ImportExportRecipe);// 导入导出
            Assert.NotNull(RecipeAuthItems.VizRecipePanel);   // 可见
        }

        #endregion

        #region EvaluatePermission 权限判定

        [Fact]
        public void EvaluatePermission_未注入权限_放行()
        {
            bool ok = RecipeManager.EvaluatePermission(null, RecipeAuthItems.SaveRecipe, out string reason);
            Assert.True(ok);
            Assert.Null(reason);
        }

        [Fact]
        public void EvaluatePermission_有权限_放行()
        {
            var auth = new StubAuthFacade { HasAuthResult = true };
            bool ok = RecipeManager.EvaluatePermission(auth, RecipeAuthItems.SwitchRecipe, out string reason);
            Assert.True(ok);
            Assert.Null(reason);
        }

        [Fact]
        public void EvaluatePermission_无权限_拦截带原因()
        {
            var auth = new StubAuthFacade { HasAuthResult = false };
            bool ok = RecipeManager.EvaluatePermission(auth, RecipeAuthItems.DeleteRecipe, out string reason);
            Assert.False(ok);
            Assert.Contains("删除配方", reason);
            Assert.Contains("无", reason);
            Assert.Contains("权限", reason);
        }

        #endregion

        #region CopyDirectory 配方复制

        [Fact]
        public void CopyDirectory_递归复制文件和子目录()
        {
            string src = Path.Combine(_tmpDir, "src");
            string dst = Path.Combine(_tmpDir, "dst");
            Directory.CreateDirectory(Path.Combine(src, "sub"));
            File.WriteAllText(Path.Combine(src, "a.recipe"), "<Recipe/>");
            File.WriteAllText(Path.Combine(src, "a.data"), "data");
            File.WriteAllText(Path.Combine(src, "sub", "b.txt"), "b");

            RecipeManager.CopyDirectory(src, dst);

            Assert.True(File.Exists(Path.Combine(dst, "a.recipe")));
            Assert.True(File.Exists(Path.Combine(dst, "a.data")));
            Assert.True(File.Exists(Path.Combine(dst, "sub", "b.txt")));
            Assert.Equal("<Recipe/>", File.ReadAllText(Path.Combine(dst, "a.recipe")));
        }

        [Fact]
        public void CopyDirectory_源不存在_不抛()
        {
            string dst = Path.Combine(_tmpDir, "dst2");
            // 源不存在不应抛异常
            RecipeManager.CopyDirectory(Path.Combine(_tmpDir, "noexist"), dst);
            Assert.False(Directory.Exists(dst));
        }

        [Fact]
        public void CopyDirectory_覆盖已有目标文件()
        {
            string src = Path.Combine(_tmpDir, "src2");
            string dst = Path.Combine(_tmpDir, "dst3");
            Directory.CreateDirectory(src);
            Directory.CreateDirectory(dst);
            File.WriteAllText(Path.Combine(src, "x.txt"), "new");
            File.WriteAllText(Path.Combine(dst, "x.txt"), "old");

            RecipeManager.CopyDirectory(src, dst);

            Assert.Equal("new", File.ReadAllText(Path.Combine(dst, "x.txt")));
        }

        #endregion

        #region 配方权限项语义（对齐三角色）

        [Theory]
        [InlineData("保存配方", "配方管理", "配方操作")]
        [InlineData("切换配方", "配方管理", "配方操作")]
        [InlineData("新增复制配方", "配方管理", "配方操作")]
        [InlineData("删除配方", "配方管理", "配方操作")]
        [InlineData("导入导出配方", "配方管理", "配方操作")]
        public void 配方操作权限_分组对齐(string operation, string module, string view)
        {
            Right[] rights = RecipeAuthItems.ToRights();
            Right r = Array.Find(rights, x => x.Name == operation);
            Assert.Equal(module, r.ModuleName);
            Assert.Equal(view, r.ViewName);
            Assert.Equal(RightType.Operation, r.Type);
        }

        #endregion

        /// <summary>最小 IAuthorizationFacade stub（8 成员，仅 HasAuth/Audit/PopNoAuthNotification 用于测试）</summary>
        private class StubAuthFacade : IAuthorizationFacade
        {
            public bool HasAuthResult { get; set; } = true;
            public List<string> Audits { get; } = new List<string>();
            public int NoAuthPopCount { get; private set; }

            public Action PopLoginWindowAction { get; set; }
            public event EventHandler AuthChanged;
            public void RegisterRights(Right[] rights) { }
            public bool CheckAuth(AuthItem authItem, RightType rightType = RightType.Operation) => HasAuthResult;
            public bool HasAuth(AuthItem authItem, RightType rightType = RightType.Operation) => HasAuthResult;
            public void PopLoginWindow() { }
            public void PopNoAuthNotification(AuthItem authItem) { NoAuthPopCount++; }
            public void Audit<T>(string operation, string detail, T? before, T? after) { Audits.Add($"{operation}:{detail}"); }
        }
    }
}
