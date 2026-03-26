using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Luster.Common.Tools
{
    public class SerializationTool
    {
        #region XML序列化与逆序列化方法

        /// <summary>
        /// 文件化XML序列化，fileName：不带后缀名的序列化文件路径
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="filename">文件路径</param>
        public static void XmlSerialize(object obj, string fileName)
        {
            string filename = fileName + ".xml";
            FileStream fs = null;
            try
            {
                fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                XmlSerializer serializer = new XmlSerializer(obj.GetType());
                serializer.Serialize(fs, obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }

        /// <summary>
        /// 文件化XML反序列化，fileName：不带后缀名的序列化文件路径
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <param name="fileName">文件路径</param>
        public static object XmlDeserialize(Type type, string fileName)
        {
            string filename = fileName + ".xml";
            FileStream fs = null;
            try
            {
                if (!File.Exists(filename)) return null;
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                XmlSerializer serializer = new XmlSerializer(type);
                return serializer.Deserialize(fs);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }



        #endregion XML序列化与逆序列化方法
    }
}