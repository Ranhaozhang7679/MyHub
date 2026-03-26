using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Common.DataStruct.Extensions;



namespace Luster.Common.Tools.Tools
{
    /// <summary>
    /// SFTP服务器
    /// </summary>
    /// <summary>
    /// SFTP客户端操作类
    /// </summary>
    public class SftpTool
    {
        #region 字段或属性
        private SftpClient sftp;
        /// <summary>
        /// SFTP连接状态
        /// </summary>
        public bool Connected { get { return sftp.IsConnected; } }
        #endregion

        #region 构造
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="port">端口</param>
        /// <param name="user">用户名</param>
        /// <param name="pwd">密码</param>
        public SftpTool(string ip, string user, string pwd, int port = 22)
        {
            sftp = new SftpClient(ip, port, user, pwd);
            Connect();
        }

        ~SftpTool()
        {
            Disconnect();
        }
        #endregion

        #region 连接SFTP
        /// <summary>
        /// 连接SFTP
        /// </summary>
        /// <returns>true成功</returns>
        public bool Connect()
        {
            try
            {
                if (!Connected)
                {
                    sftp.Connect();
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("连接SFTP失败，原因：{0}", ex.Message));
            }
        }
        #endregion

        #region 断开SFTP
        /// <summary>
        /// 断开SFTP
        /// </summary> 
        public void Disconnect()
        {
            try
            {
                if (sftp != null && Connected)
                {
                    sftp.Disconnect();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("断开SFTP失败，原因：{0}", ex.Message));
            }
        }
        #endregion

        #region SFTP上传文件


        public bool IsExist(string path)
        {
            bool result = false;
            try
            {

                if (!Connected)
                    Connect();
                result = sftp.Exists(path);
                //Disconnect();
            }
            catch (Exception ex)
            {
                //LogManager.GetLogger("Debug").Info($"SFTP文件上传失败，原因:{ex}");
                throw new Exception(string.Format("SFTP文件上传失败，原因：{0}", ex.Message));
            }
            return result;
        }

        public void CreateDirectory(string remotePath)
        {
            try
            {
                if (!Connected)
                    Connect();
                sftp.CreateDirectory(remotePath);
                //Disconnect();
            }
            catch (Exception ex)
            {
                //LogManager.GetLogger("Debug").Info($"SFTP文件上传失败，原因:{ex}");
                throw new Exception(string.Format("SFTP文件上传失败，原因：{0}", ex.Message));
            }
        }


        /// <summary>
        /// SFTP上传文件
        /// </summary>
        /// <param name="localPath">本地文件全路径 例：G:\\Project\\logo.png</param>
        /// <param name="remotePath">远程路径  例：/logo.png</param>
        public bool Put(string localPath, string remotePath)
        {
            try
            {
                using (var file = File.OpenRead(localPath))
                {
                    Connect();
                    sftp.UploadFile(file, remotePath);
                    Disconnect();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region SFTP获取文件
        /// <summary>
        /// SFTP获取文件
        /// </summary>
        /// <param name="remotePath">远程路径</param>
        /// <param name="localPath">本地路径</param>
        public void Get(string remotePath, string localPath)
        {
            try
            {
                Connect();
                var byt = sftp.ReadAllBytes(remotePath);
                Disconnect();
                File.WriteAllBytes(localPath, byt);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("SFTP文件获取失败，原因：{0}", ex.Message));

            }

        }
        #endregion

        #region 删除SFTP文件
        /// <summary>
        /// 删除SFTP文件 
        /// </summary>
        /// <param name="remoteFile">远程路径</param>
        public void Delete(string remoteFile)
        {
            try
            {
                Connect();
                sftp.Delete(remoteFile);
                Disconnect();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("SFTP文件删除失败，原因：{0}", ex.Message));
            }
        }
        #endregion



    }
}
