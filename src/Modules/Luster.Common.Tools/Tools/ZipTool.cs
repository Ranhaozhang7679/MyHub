using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.Tools
{
    /// <summary>
    /// 压缩和解压
    /// </summary>
    public class ZipTool
    {
        private static int BufferSize = 1024;

        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="destZipFile"></param>
        /// <param name="sourceList"></param>
        /// <param name="comment"></param>
        /// <param name="password"></param>
        /// <param name="compressionLevel"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static bool CompressFile(string destZipFile, string[] sourceList, string comment = null, string password = null, int compressionLevel = 6)
        {
            var gbk = Encoding.GetEncoding("GBK");
            ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage = gbk.CodePage;
            if (compressionLevel >= 0 && compressionLevel <= 9)
            {
                bool flag = false;
                try
                {
                    string directoryName = Path.GetDirectoryName(destZipFile);
                    if (!Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                    Dictionary<string, string> dictionary = PrepareFileSystementities(sourceList);
                    using (FileStream baseOutputStream = new FileStream(destZipFile, FileMode.Create))
                    {
                        using (ZipOutputStream zipOutputStream = new ZipOutputStream(baseOutputStream))
                        {
                            zipOutputStream.Password = password;
                            zipOutputStream.SetComment(comment);
                            zipOutputStream.SetLevel(compressionLevel);
                            foreach (string key in dictionary.Keys)
                            {
                                if (File.Exists(key))
                                {
                                    try
                                    {
                                        FileInfo fileInfo = new FileInfo(key);
                                        using (FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                                        {
                                            ZipEntry zipEntry = new ZipEntry(dictionary[key]);
                                            zipEntry.DateTime = fileInfo.LastWriteTime;
                                            zipEntry.Size = fileStream.Length;
                                            zipOutputStream.PutNextEntry(zipEntry);
                                            int num = 0;
                                            byte[] buffer = new byte[BufferSize];
                                            do
                                            {
                                                num = fileStream.Read(buffer, 0, BufferSize);
                                                zipOutputStream.Write(buffer, 0, num);
                                            }
                                            while (num == BufferSize);
                                            fileStream.Close();
                                        }
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                                else
                                {
                                    ZipEntry entry = new ZipEntry(dictionary[key] + "/");
                                    zipOutputStream.PutNextEntry(entry);
                                }
                            }
                            zipOutputStream.Flush();
                            zipOutputStream.Finish();
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            throw new ArgumentOutOfRangeException("compressionLevel", $"value range: 0 ~ 9, current range: {compressionLevel}");
        }

        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="targetPath"></param>
        /// <param name="fileAction"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        public static bool DecomparessFile(string sourceFile, string targetPath, Action<string> fileAction = null, string password = "")
        {
            if (!File.Exists(sourceFile))
            {
                throw new FileNotFoundException($"未能找到文件 '{sourceFile}' ");
            }
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            try
            {
                using (ZipInputStream zipInputStream = new ZipInputStream(File.OpenRead(sourceFile)))
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        zipInputStream.Password = password;
                    }
                    ZipEntry nextEntry;
                    while ((nextEntry = zipInputStream.GetNextEntry()) != null)
                    {
                        if (!nextEntry.IsDirectory)
                        {
                            string text = Path.Combine(targetPath, Path.GetDirectoryName(nextEntry.Name));
                            string text2 = Path.Combine(text, Path.GetFileName(nextEntry.Name));
                            if (!Directory.Exists(text))
                            {
                                Directory.CreateDirectory(text);
                            }
                            if (!string.IsNullOrEmpty(text2))
                            {
                                try
                                {
                                    using (FileStream fileStream = new FileStream(text2, FileMode.Create))
                                    {
                                        int num = 2048;
                                        byte[] array = new byte[num];
                                        while (true)
                                        {
                                            num = zipInputStream.Read(array, 0, array.Length);
                                            if (num <= 0)
                                            {
                                                break;
                                            }
                                            fileStream.Write(array, 0, num);
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                }
                                fileAction?.Invoke(text2);
                            }
                        }
                    }
                }
            }
            catch (Exception ex2)
            {
                throw new Exception($"解压文件发生错误: {ex2.Message}");
            }
            return true;
        }

        public static byte[] CompressBytes(byte[] sourceBytes, string password = null, int compressionLevel = 6)
        {
            if (compressionLevel < 0 || compressionLevel > 9)
            {
                throw new ArgumentOutOfRangeException("compressionLevel", $"value range: 0 ~ 9, current range: {compressionLevel}");
            }
            byte[] result = new byte[0];
            if (sourceBytes.Length != 0)
            {
                try
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (MemoryStream memoryStream2 = new MemoryStream(sourceBytes))
                        {
                            using (ZipOutputStream zipOutputStream = new ZipOutputStream(memoryStream))
                            {
                                zipOutputStream.Password = password;
                                zipOutputStream.SetLevel(compressionLevel);
                                ZipEntry zipEntry = new ZipEntry("ZipBytes");
                                zipEntry.DateTime = DateTime.Now;
                                zipEntry.Size = sourceBytes.Length;
                                zipOutputStream.PutNextEntry(zipEntry);
                                int num = 0;
                                byte[] buffer = new byte[BufferSize];
                                do
                                {
                                    num = memoryStream2.Read(buffer, 0, BufferSize);
                                    zipOutputStream.Write(buffer, 0, num);
                                }
                                while (num == BufferSize);
                                memoryStream2.Close();
                                zipOutputStream.Flush();
                                zipOutputStream.Finish();
                                result = memoryStream.ToArray();
                                zipOutputStream.Close();
                            }
                        }
                    }
                }
                catch (Exception innerException)
                {
                    throw new Exception("压缩字节数组发生错误", innerException);
                }
            }
            return result;
        }

        public static byte[] DecompressBytes(byte[] sourceBytes, string password = null)
        {
            byte[] result = new byte[0];
            if (sourceBytes.Length != 0)
            {
                try
                {
                    using (MemoryStream baseInputStream = new MemoryStream(sourceBytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (ZipInputStream zipInputStream = new ZipInputStream(baseInputStream))
                            {
                                zipInputStream.Password = password;
                                ZipEntry nextEntry = zipInputStream.GetNextEntry();
                                if (nextEntry != null)
                                {
                                    byte[] buffer = new byte[BufferSize];
                                    int num = 0;
                                    do
                                    {
                                        num = zipInputStream.Read(buffer, 0, BufferSize);
                                        memoryStream.Write(buffer, 0, num);
                                    }
                                    while (num == BufferSize);
                                    memoryStream.Flush();
                                    result = memoryStream.ToArray();
                                    memoryStream.Close();
                                }
                                zipInputStream.Close();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"解压字节数组发生错误: {ex.Message}");
                }
            }
            return result;
        }

        private static Dictionary<string, string> PrepareFileSystementities(string[] sourceFileEntityPathList)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string text = "";
            foreach (string text2 in sourceFileEntityPathList)
            {
                string text3 = text2;
                if (text3.EndsWith("\\"))
                {
                    text3 = text3.Remove(text3.LastIndexOf("\\"));
                }
                text = Path.GetDirectoryName(text3) + "\\";
                if (text.EndsWith(":\\\\"))
                {
                    text = text.Replace("\\\\", "\\");
                }
                Dictionary<string, string> allFileSystemEntities = GetAllFileSystemEntities(text3, text);
                foreach (string key in allFileSystemEntities.Keys)
                {
                    if (!dictionary.ContainsKey(key))
                    {
                        dictionary.Add(key, allFileSystemEntities[key]);
                    }
                }
            }
            return dictionary;
        }

        private static Dictionary<string, string> GetAllFileSystemEntities(string source, string topDirectory)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add(source, source.Replace(topDirectory, ""));
            if (Directory.Exists(source))
            {
                string[] directories = Directory.GetDirectories(source, "*.*", SearchOption.AllDirectories);
                string[] array = directories;
                foreach (string text in array)
                {
                    dictionary.Add(text, text.Replace(topDirectory, ""));
                }
                string[] files = Directory.GetFiles(source, "*.*", SearchOption.AllDirectories);
                string[] array2 = files;
                foreach (string text2 in array2)
                {
                    dictionary.Add(text2, text2.Replace(topDirectory, ""));
                }
            }
            return dictionary;
        }

        /// <summary>
        /// 压缩文件夹
        /// </summary>
        /// <param name="zipfile">目标zip压缩文件</param>
        /// <param name="source">源目录</param>
        /// <param name="s">ZipOutputStream对象</param>
        /// <param name="parentPath">和source相同</param>
        public static bool CompressFolder(string zipfile, string source, int blockSize = 1024, int compressionLevel = 6, string password = "")
        {
            bool Result = true;
            try
            {
                using (ZipOutputStream zipStream = new ZipOutputStream(File.Create(zipfile)))
                {
                    zipStream.SetLevel(compressionLevel);
                    if (password != null)
                    {
                        zipStream.Password = password;
                    }
                    Result = CompressFolder2(zipStream, source, source, blockSize);
                    zipStream.Flush();
                    zipStream.Close();
                }
                return Result;
            }
            catch (Exception ex)
            {
                //log4net.LogManager.GetLogger("DEBUG").Info($"ICSharpCodeTool.CompressFolder zipfile=[{zipfile}],source=[{source}],\r\n Exception [{ex}]");
                return false;
            }
        }

        private static bool CompressFolder2(ZipOutputStream zipStream, string source, string parentPath, int blockSize = 1024)
        {
            string[] filenames = Directory.GetFileSystemEntries(source);
            foreach (string file in filenames)
            {
                if (Directory.Exists(file))
                {
                    if (!CompressFolder2(zipStream, file, source, blockSize))   //递归压缩子文件夹
                        return false;
                }
                else
                {
                    //打开压缩文件
                    using (FileStream streamToZip = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        var name = file.Replace(parentPath, "");
                        var entry = new ZipEntry(name)
                        {
                            DateTime = DateTime.Now
                        };
                        zipStream.PutNextEntry(entry);

                        byte[] buffer = new byte[blockSize];

                        int size = streamToZip.Read(buffer, 0, buffer.Length);
                        zipStream.Write(buffer, 0, size);
                        try
                        {
                            while (size < streamToZip.Length)
                            {
                                int sizeRead = streamToZip.Read(buffer, 0, buffer.Length);
                                zipStream.Write(buffer, 0, sizeRead);
                                size += sizeRead;
                            }
                        }
                        catch (Exception ex)
                        {
                            //log4net.LogManager.GetLogger("DEBUG").Info($"[file={file}] ICSharpCodeTool.CompressFolder Exception [{ex}]");
                            zipStream.Flush();
                            streamToZip.Close();
                            return false;
                        }
                        zipStream.Flush();
                        streamToZip.Close();
                    }
                }
            }
            return true;
        }
    }
}
