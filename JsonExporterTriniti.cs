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
        public JsonExporterTriniti(DataTable sheet, int headerRows)
        {
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
        }

        /// <summary>
        /// 将内部数据转换成Json文本，并保存至文件
        /// </summary>
        /// <param name="jsonPath">输出文件路径</param>
        public void SaveToFile(string filePath, Encoding encoding)
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
            //-- 保存文件
            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (TextWriter writer = new StreamWriter(file, encoding))
                    writer.Write(str);
            }
        }
    }
}
