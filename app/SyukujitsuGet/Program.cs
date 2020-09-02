using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace syukujitsu
{
    /// <summary>
    /// WEB アプリで利用する祝日 json を生成する
    /// </summary>
    class Program
    {
        /// <summary>
        /// コマンドライン指定
        /// 
        /// outFile = 出力CSVファイル
        /// url = 入力URL
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task Main(string[] args)
        {
            // SJIS を利用するためのおまじない
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            // コマンドライン指定
            var builder = new ConfigurationBuilder();
            builder.AddCommandLine(args);
            var config = builder.Build();

            // HTTP で ファイルをダウンロード
            string url = "https://www8.cao.go.jp/chosei/shukujitsu/syukujitsu.csv";

            // コマンドライン指定
            if (config["url"] != null)
            {
                url = config["url"];
            }
            string csvFile = config["csvFile"]; // CSV ファイルの出力先
            string jsonFile = config["jsonFile"]; // Jsonファイルの出力先


            string csvData = await WGet(url, System.Text.Encoding.GetEncoding("SJIS")); // SJIS で ファイルを取得

            // 指定されたファイルに出力
            if (csvFile != null)
            {
                using (var sw = new StreamWriter(csvFile, false, Encoding.GetEncoding("SJIS")))
                {
                    await sw.WriteAsync(csvData);
                    await sw.FlushAsync();
                    sw.Close();
                }
            }

            // ダウンロードした ファイルから 日付の Dictionaryを作成する
            var dateDic = ParseCsvToDic(csvData);
            
            // 最低限の入力データチェック
            if (dateDic.Count == 0)
            {
                Console.Error.WriteLine("取得したデータ不備の可能性");
                System.Environment.Exit(1);
            }
            // JSON を生成する
            string dateJson = DateDicToJson(dateDic);

            if (jsonFile == null)
            {
                Console.WriteLine(dateJson);
            }
            else
            {
                var sw = new StreamWriter(jsonFile, false, Encoding.UTF8);
                await sw.WriteAsync(dateJson);
                await sw.FlushAsync();
                sw.Close();
            }
        }


        /* 入力ファイル形式

2018/12/23,天皇誕生日
2018/12/24,休日

         */

        /* 出力文字列形式 : 日付がキーの HashMap 形式
        {
"2019-01-01": "元日",
"2019-01-14": "成人の日",
"2019-02-11": "建国記念の日",
"2019-03-21": "春分の日",
"2019-04-29": "昭和の日",
        }
         */

        static IOrderedDictionary ParseCsvToDic(string csvData)
        {
            var dateDic = new OrderedDictionary();
            StringReader sr = new StringReader(csvData);
            string s = sr.ReadLine();
            while (s != null)
            {
                string[] data = s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (data.Length == 2)
                {
                    DateTime tm;
                    if (DateTime.TryParse(data[0], out tm) && data[1].Length > 0)
                    {
                        // 日付 、 祝日の名称
                        dateDic.Add(tm.ToString("yyyy-MM-dd"), data[1]);
                    }
                }

                s = sr.ReadLine();
            }

            return dateDic;
        }

        /// <summary>
        /// 祝日のJSON 形式に変換する
        /// </summary>
        /// <param name="csvData"></param>
        /// <returns></returns>
        static string DateDicToJson(IOrderedDictionary dateDic)
        {
            // 2年前の 1月1日 から未来の分までのデータを抽出する
            OrderedDictionary outDic = new OrderedDictionary();
            DateTime fromDate = new DateTime(DateTime.Now.Year - 2, 1, 1);

            foreach(DictionaryEntry item in dateDic)
            {
                DateTime tm;
                if (DateTime.TryParse(item.Key as string, out tm))
                {
                    if (tm > fromDate)
                    {
                        // 出力データに登録
                        outDic.Add(item.Key, item.Value);
                    }
                }
            }

            // 出力データを JSON 変換
            var options = new JsonSerializerOptions
            {
                // https://docs.microsoft.com/ja-jp/dotnet/standard/serialization/system-text-json-how-to
                // すべての言語セットをエスケープせずにシリアル化するには、UnicodeRanges.All を使用します。
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            };

            string dateJson = System.Text.Json.JsonSerializer.Serialize(outDic, options);

            return dateJson;

        }

        static async Task<string> WGet(string url, System.Text.Encoding encoding)
        {
            HttpClient http = new HttpClient();
            var st = await http.GetStreamAsync(url);
            var sr = new System.IO.StreamReader(st, encoding);
            string ans = await sr.ReadToEndAsync();
            return ans;
        }
    }
}
