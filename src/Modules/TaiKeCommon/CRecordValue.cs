using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaiKeCommon
{
    public class CRecordValue
    {
        public string RecordValue(string savePath, string fileName, string title, string value)
        {
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            FileStream Record_File;
            string saveFullPath = savePath + fileName + ".csv";
            if (!File.Exists(saveFullPath))
            {
                Record_File = new FileStream(saveFullPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                StreamWriter Record_Title = new StreamWriter(Record_File, Encoding.Default);

                Record_Title.WriteLine(title);
                Record_Title.Flush();
                Record_File.Flush();
                Record_Title.Close();
                Record_File.Close();
            }

            try
            {
                Record_File = new FileStream(saveFullPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            }
            catch (IOException)
            {
                Excel_Manager e = new Excel_Manager();
                e.Kill_Process(fileName);
            }

            try
            {
                Record_File = new FileStream(saveFullPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                StreamWriter Record_Value = new StreamWriter(Record_File, Encoding.Default);

                Record_Value.WriteLine(value);   //WriteLine写完自动换行
                Record_Value.Flush();
                Record_File.Flush();
                Record_Value.Close();
                Record_File.Close();
            }
            catch (Exception ex)
            {

            }


            return "1";
        }
    }
    public class Excel_Manager
    {
        public void Kill_Process(string process_name)
        {
            System.Diagnostics.Process[] process;
            process = System.Diagnostics.Process.GetProcessesByName("EXCEL");

            //if (process.Length == 1)
            process_name = "Microsoft Excel - " + process_name;

            foreach (System.Diagnostics.Process excel_process in process)
            {
                // Console.WriteLine("title=" + excel_process.MainWindowTitle.ToString());
                if (string.Equals(excel_process.MainWindowTitle.ToString(), process_name))
                {
                    excel_process.Kill();
                }
            }

            Thread.Sleep(10);
        }
    }
}
