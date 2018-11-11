using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SexiLove.Extensions.Helpers
{
    public class CSVCell
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string Contents { get; set; }
    }
    public class CSVParser : List<IList<string>>
    {

        private readonly string _path;
        private readonly Encoding _encoding;
        public CSVParser(string path, Encoding encoding)
        {
            _path = path;
            _encoding = encoding;
        }

        public void ParseDocument(int fromIndex = 0)
        {
            StreamReader csv = new StreamReader(_path, _encoding);
            for (int i = 0; i < fromIndex; i++)
            {
                csv.ReadLine();
            }
            Regex regx = new Regex(";(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            while (!csv.EndOfStream)
            {
                var line = csv.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    line = line.Replace("\r", "").Replace("\n", "");
                    String[] parts = regx.Split(line);

                    for (int p = 0; p < parts.Length; p++)
                    {
                        parts[p] = parts[p].Trim(' ');
                        parts[p] = parts[p].Replace("\"\"", "\"");
                        if (parts[p].StartsWith("\"") && parts[p].EndsWith("\""))
                            parts[p] = parts[p].Substring(1, parts[p].Length - 2);
                        if (parts[p].EndsWith("\"") && parts[p].IndexOf("\"", 0, parts[p].Length - 1) < 0)
                            parts[p] = parts[p].Substring(0, parts[p].Length - 1);
                    }
                    Add(parts);
                }
            }
        }

  /*      public IEnumerable<IList<string>> GetDocument(int fromIndex = 0)
        {

            TextReader csv = new StreamReader(_path, _encoding);

            for (int i = 0; i < fromIndex; i++)
            {
                csv.ReadLine();
            }
            IList<string> result = new List<string>();
            StringBuilder curValue = new StringBuilder();
            char c;
            c = (char)csv.Read();
            while (csv.Peek() != -1)
            {
                switch (c)
                {
                    case ',': //empty field
                        result.Add("");
                        c = (char)csv.Read();
                        break;
                    case '"': //qualified text
                    case '\'':
                        char q = c;
                        c = (char)csv.Read();
                        bool inQuotes = true;
                        while (inQuotes && csv.Peek() != -1)
                        {
                            if (c == q)
                            {
                                c = (char)csv.Read();
                                if (c != q)
                                    inQuotes = false;
                            }

                            if (inQuotes)
                            {
                                curValue.Append(c);
                                c = (char)csv.Read();
                            }
                        }
                        result.Add(curValue.ToString());
                        curValue = new StringBuilder();
                        if (c == ',') c = (char)csv.Read(); // either ',', newline, or endofstream
                        break;
                    case '\n': //end of the record
                    case '\r':
                        //potential bug here depending on what your line breaks look like
                        if (result.Count > 0) // don't return empty records
                        {
                            yield return result;
                            result = new List<string>();
                        }
                        c = (char)csv.Read();
                        break;
                    default: //normal unqualified text
                        while (c != ',' && c != '\r' && c != '\n' && csv.Peek() != -1)
                        {
                            curValue.Append(c);
                            c = (char)csv.Read();
                        }
                        result.Add(curValue.ToString());
                        curValue = new StringBuilder();
                        if (c == ',') c = (char)csv.Read(); //either ',', newline, or endofstream
                        break;
                }

            }
            if (curValue.Length > 0) //potential bug: I don't want to skip on a empty column in the last record if a caller really expects it to be there
                result.Add(curValue.ToString());
            if (result.Count > 0)
                yield return result;
        }
*/
        public CSVCell getCell(int columnNum, int rowNum)
        {
            var cell = new CSVCell { Column = columnNum, Row = rowNum, Contents = "" };
            if (rowNum >= Count)
                return cell;
            var row = this[rowNum];
            if (columnNum >= row.Count) return cell;
            cell.Contents = row[columnNum];
            return cell;
        }
    }
}