using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Unihan.Properties;

namespace Unihan
{
    public class StrokeLookup : IDisposable
    {
        private static StrokeLookup _instance;

        public static StrokeLookup Instance 
        { 
            get
            {
                if (_instance == null)
                {
                    _instance = new StrokeLookup();
                }
                return _instance;
            }
        }

        // 利用 stream，存放筆劃資訊，以位移值取得筆劃數
        private Stream _stream;

        private StrokeLookup()
        {
            InitialLookupTable();
        }

        private void InitialLookupTable()
        {
            var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dataPath = Path.Combine(binPath, "Unihan.Data");
            var filePath = Path.Combine(dataPath, "Unihan_DictionaryLikeData.txt");
            var lookupPath = Path.Combine(dataPath, "Unihan_DictionaryLikeData.strokes");

            if (!File.Exists(lookupPath) || File.GetLastWriteTime(filePath) > File.GetLastWriteTime(lookupPath))
            {
                using (var stream = new FileStream(lookupPath, FileMode.Create, FileAccess.ReadWrite))
                {
                    GenerateStrokeData(filePath, stream);
                }
            }

            //TODO: 若改為以 MemoryStream 載入查表資料，也可以善用記憶體優勢
            _stream = new FileStream(lookupPath, FileMode.Open, FileAccess.Read);
        }

        private void GenerateStrokeData(string filePath, Stream outputStream)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream))
                {
                    var line = string.Empty;

                    while ((line = reader.ReadLine()) != null)
                    {
                        // 非有效行
                        if (string.IsNullOrEmpty(line) || !line.StartsWith("U+"))
                        {
                            continue;
                        }

                        // 每行只切為三分
                        var datas = line.Split(new[] { '\t', ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);

                        // 格式不符或不含有筆劃資訊就忽略
                        if (datas.Length < 3 || datas[1] != "kTotalStrokes")
                        {
                            continue;
                        }

                        // U+3400 轉為 uint
                        var hex = datas[0].Substring(2);
                        var code = uint.Parse(hex, NumberStyles.HexNumber);

                        // 筆劃資訊
                        var stroke = byte.Parse(datas[2]);

                        // Padding (補足間隙的不存在字元)
                        var gap = code - outputStream.Length;

                        if (gap > 1)
                        {
                            outputStream.Seek(0, SeekOrigin.End);
                            while (gap-- > 1)
                            {
                                outputStream.WriteByte(0);
                            }
                        }

                        outputStream.Seek(code, SeekOrigin.Begin);
                        outputStream.WriteByte(stroke);
                    }
                }
            }
        }

        public IEnumerable<CharStroke> GetStrokes(string source)
        {
            foreach (var chr in source)
            {
                yield return new CharStroke { Character = chr, Stroke = GetStroke(chr) };
            }
        }

        public int GetStroke(char source)
        {
            var code = (uint)source;

            if (code >= 0 && code < _stream.Length)
            {
                _stream.Seek(code, SeekOrigin.Begin);
                return _stream.ReadByte();
            }
            return 0;
        }

        public void Dispose()
        {
            if (_stream != null)
            {
                _stream.Dispose();
            }
        }
    }
}
