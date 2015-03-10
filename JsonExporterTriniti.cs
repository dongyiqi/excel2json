using System;
using System.IO;
using System.Data;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace excel2json
{
    /// <summary>
    /// 将DataTable对象，转换成JSON string，并保存到文件中
    /// </summary>
    class JsonExporterTriniti
    {
        Dictionary<string, List<string>> m_Head;
        Dictionary<string, List<List<string>>> m_data;
        /// <summary>
        /// 构造函数：完成内部数据创建
        /// </summary>
        /// <param name="sheet">ExcelReader创建的一个表单</param>
        /// <param name="headerRows">表单中的那几行是表头</param>
        public JsonExporterTriniti(DataSet book, int headerRows, string strFileName)
        {
         // 取得数据
            int nCount = book.Tables.Count;
            for (int nSheet = 0; nSheet < book.Tables.Count; nSheet++)
            {
                DataTable sheet = book.Tables[nSheet];
                if (sheet.Rows.Count <= 0)
                {
                    throw new Exception("Excel Sheet中没有数据: " + book.DataSetName);
                }

                if (sheet.Columns.Count <= 0)
                    return;
                if (sheet.Rows.Count <= 0)
                    return;

                //导出列
                m_Head = new Dictionary<string, List<string>>();
                List<string> heads = new List<string>();
                foreach (DataColumn column in sheet.Columns)
                {
                    heads.Add(column.ColumnName);
                }
                m_Head.Add("header", heads);
                //导出行
                m_data = new Dictionary<string, List<List<string>>>();
                //--以第一列为ID，转换成ID->Object的字典
                int firstDataRow = headerRows - 1;
                List<List<string>> data = new List<List<string>>();
                for (int i = firstDataRow; i < sheet.Rows.Count; i++)
                {
                    List<string> rowData = new List<string>();
                    DataRow row = sheet.Rows[i];
                    foreach (DataColumn column in sheet.Columns)
                    {
                        object value = row[column];
                        rowData.Add(value.ToString());
                    }
                    data.Add(rowData);
                }
                m_data.Add("data", data);
                BuildSubJsonString(book.Tables.Count ==1 ?
                    Path.GetFileNameWithoutExtension(strFileName) : sheet.TableName
                    ,nSheet != book.Tables.Count-1);
            }       
        }


        public string BuildSubJsonString(string strSheetName, bool bAddComa)
        {
            if (m_Head == null)
                throw new Exception("JsonExporter内部数据为空。");

            string jsonHead = JsonConvert.SerializeObject(m_Head, Formatting.Indented);
            //-- 转换为JSON字符串
            string json = JsonConvert.SerializeObject(m_data, Formatting.Indented);
            json = json.Replace('{', ',');
            json = json.Replace('}', ' ');
            jsonHead = jsonHead.Replace('}', ' ');
            string str = jsonHead + json;
            str += "}";
            str = string.Format("\"{0}\":{1}", Path.GetFileNameWithoutExtension(strSheetName), str);
            if (bAddComa) str += ",";
            _strJson += str;
            return str;
        }
        public string _strJson;
    }
}
