using System;
using System.IO;
using System.Data;
using System.Text;
using Excel;

namespace excel2json
{
    /// <summary>
    /// 应用程序
    /// </summary>
    sealed partial class Program
    {
        /// <summary>
        /// 应用程序入口
        /// </summary>
        /// <param name="args">命令行参数</param>
        static void Main(string[] args)
        {
            System.DateTime startTime = System.DateTime.Now;

            //-- 分析命令行参数
            var options = new Options();
            var parser = new CommandLine.Parser(with => with.HelpWriter = Console.Error);

            if (parser.ParseArgumentsStrict(args, options, () => Environment.Exit(-1)))
            {
                //-- 执行导出操作
                try
                {
                    Run(options);
                }
                catch (Exception exp)
                {
                    Console.WriteLine("Error: " + exp.Message);
                }
            }

            //-- 程序计时
            System.DateTime endTime = System.DateTime.Now;
            System.TimeSpan dur = endTime - startTime;
            Console.WriteLine(
                string.Format("[{0}]：\t转换完成[{1}毫秒].",
                Path.GetFileName(options.FilesPath),
                dur.Milliseconds)
                );
        }

        /// <summary>
        /// 根据命令行参数，执行Excel数据导出工作
        /// </summary>
        /// <param name="options">命令行参数</param>
        private static void Run(Options options)
        {
            //string excelPath = options.ExcelPath;
            //int header = options.HeaderRows;
            //-- 确定编码
            Encoding cd = new UTF8Encoding(false);
            if (options.Encoding != "utf8-nobom")
            {
                foreach (EncodingInfo ei in Encoding.GetEncodings())
                {
                    Encoding e = ei.GetEncoding();
                    if (e.EncodingName == options.Encoding)
                    {
                        cd = e;
                        break;
                    }
                }
            }

            string strFilesPath = options.FilesPath;
            string strCsPath = options.CSPath;
            DirectoryInfo folder = new DirectoryInfo(strFilesPath);
            string strJson = "";
            FileInfo[] files = folder.GetFiles("*.xlsx");
            int nFileIndex = 0;
            foreach (FileInfo file in files)
            {
                strJson += DoConvertFile(file.DirectoryName + "\\", file.Name, strCsPath, cd);
                if(++nFileIndex != files.Length)
                {
                    strJson += ",";
                }
            }

            
            string strJsonToWrite = "{" + strJson + "}";
            System.Diagnostics.Debug.Write(strJsonToWrite);
            // 加载Excel文件
            //-- 保存文件
            string strSaveJson = strFilesPath + "\\data.json";
            using (FileStream file = new FileStream(strSaveJson, FileMode.Create, FileAccess.Write))
            {
                using (TextWriter writer = new StreamWriter(file, cd))
                    writer.Write(strJsonToWrite);
            }
        }

        private static string DoConvertFile(string strPath, string strFileName, string strCsPath, Encoding cd)
        {
            using (FileStream excelFile = File.Open(strPath+strFileName, FileMode.Open, FileAccess.Read))
            {
                // Reading from a OpenXml Excel file (2007 format; *.xlsx)
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(excelFile);

                // The result of each spreadsheet will be created in the result.Tables
                excelReader.IsFirstRowAsColumnNames = true;
                DataSet book = excelReader.AsDataSet();

                // 数据检测
                if (book.Tables.Count < 1)
                {
                    throw new Exception("Excel文件中没有找到Sheet: " + strFileName);
                }

//                 // 取得数据
//                 DataTable sheet = book.Tables[0];
//                 if (sheet.Rows.Count <= 0)
//                 {
//                     throw new Exception("Excel Sheet中没有数据: " + strFileName);
//                 }


                string strJsonRet = "";
                //TODO:合并文件
                //-- 导出JSON文件
                if (true/*options.JsonPath != null && options.JsonPath.Length > 0*/)
                {
                    //JsonExporter exporter = new JsonExporter(sheet, header);
                    JsonExporterTriniti exporter = new JsonExporterTriniti(book, 3, strFileName);
//                     exporter.SaveToFile(strPath+Path.GetFileNameWithoutExtension(strFileName) + ".json"
//                         , cd);
                    strJsonRet = exporter._strJson;
//                    strJsonRet = string.Format("\"{0}\":{1}", Path.GetFileNameWithoutExtension(strFileName), strJson);                    
                }

                //-- 导出SQL文件
                if (false/*options.SQLPath != null && options.SQLPath.Length > 0*/)
                {
//                     SQLExporter exporter = new SQLExporter(sheet, header);
//                     exporter.SaveToFile(options.SQLPath, cd);
                }

                //-- 生成C#定义文件
//                 if (false/*options.CSharpPath != null && options.CSharpPath.Length > 0*/)
//                 {
//                     string excelName = Path.GetFileName(strFileName);
//                     CSDefineGenerator exporter = new CSDefineGenerator(sheet);
//                     exporter.ClassComment = string.Format("// Generate From {0}", excelName);
//                     exporter.SaveToFile(strCsPath + Path.GetFileNameWithoutExtension(strFileName) + ".cs", cd);
//                 }
                return strJsonRet;
            }
        }
        
    }
}
