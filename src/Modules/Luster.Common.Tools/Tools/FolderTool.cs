#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FolderUtility
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Utility
* 文 件 名:       FolderUtility.cs
* 创建时间:       2021/12/17 16:13:57
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      664afa3b-e139-477c-b5e2-8ac9fbf07775
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2021
* 修改时间:		  2021/12/17 16:13:57
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Common.Tools
{
    /// <summary>
    /// 文件夹工具
    /// </summary>
    public class FolderTool
    {
        /// <summary>
        /// 支持创建隐藏文件夹
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="isHidden"></param>
        public static void CreateDir(string dir, bool isHidden = false)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (isHidden)
            {
                File.SetAttributes(dir, FileAttributes.Hidden);
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="dir"></param>
        public static void DeleteDir(string dir)
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Copy 文件夹下面所有文件
        /// </summary>
        /// <param name="srcDir"></param>
        /// <param name="dstDir"></param>
        /// <param name="isRecursion"></param>
        public static void CopyFiles(string srcDir, string dstDir, bool isRecursion = true)
        {
            if (!Directory.Exists(dstDir))
            {
                Directory.CreateDirectory(dstDir);
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(srcDir);
            FileInfo[] files = directoryInfo.GetFiles();
            foreach (FileInfo fileInfo in files)
            {
                File.Copy(fileInfo.FullName, Path.Combine(dstDir, Path.GetFileName(fileInfo.FullName)), true);
            }
            if (isRecursion)
            {
                DirectoryInfo[] directories = directoryInfo.GetDirectories();
                foreach (DirectoryInfo directoryInfo2 in directories)
                {
                    CopyFiles(directoryInfo2.FullName, Path.Combine(dstDir, directoryInfo2.Name), isRecursion);
                }
            }
        }

        /// <summary>
        /// 多线程Copy
        /// </summary>
        /// <param name="srcDir"></param>
        /// <param name="dstDir"></param>
        /// <param name="threadCount"></param>
        /// <param name="source"></param>
        /// <param name="isRecursion"></param>
        public static void CopyFiles(string srcDir, string dstDir, int threadCount, CancellationTokenSource source, bool isRecursion = true)
        {
            if (!Directory.Exists(dstDir))
            {
                Directory.CreateDirectory(dstDir);
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(srcDir);
            List<FileInfo> list = directoryInfo.GetFiles().ToList();
            if (threadCount > 1)
            {
                int num = list.Count / threadCount + list.Count % threadCount;
                List<Task> list2 = new List<Task>();
                for (int i = 0; i < threadCount; i++)
                {
                    List<FileInfo> state = list.Skip(i * num).Take(num).ToList();
                    Task item = Task.Factory.StartNew(delegate (object files)
                    {
                        List<FileInfo> list3 = files as List<FileInfo>;
                        foreach (FileInfo item2 in list3)
                        {
                            if (source.IsCancellationRequested)
                            {
                                break;
                            }
                            File.Copy(item2.FullName, Path.Combine(dstDir, Path.GetFileName(item2.FullName)), true);
                        }
                    }, state, source.Token);
                    list2.Add(item);
                }
                Task.WaitAll(list2.ToArray());
            }
            else
            {
                FileInfo[] files2 = directoryInfo.GetFiles();
                foreach (FileInfo fileInfo in files2)
                {
                    File.Copy(fileInfo.FullName, Path.Combine(dstDir, Path.GetFileName(fileInfo.FullName)), true);
                }
            }
            if (isRecursion)
            {
                DirectoryInfo[] directories = directoryInfo.GetDirectories();
                foreach (DirectoryInfo directoryInfo2 in directories)
                {
                    CopyFiles(directoryInfo2.FullName, Path.Combine(dstDir, directoryInfo2.Name), isRecursion);
                }
            }
        }
    }
}