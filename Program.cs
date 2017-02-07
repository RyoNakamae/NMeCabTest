using NMeCab;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;

namespace NMeCabTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //形態素解析対象のデータをファイルから取得
            var lines = File.ReadAllLines(ConfigurationManager.AppSettings["InFilePath"], Encoding.GetEncoding("shift_jis"));

            var mecab = new NMeCabParse();
            var outPutList = new List<string>();

            //所定のファイルに追記する
            using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["OutFilePath"], true, Encoding.UTF8))
            {
                foreach (var line in lines)
                {
                    //空行以外を解析して取得
                    if (string.IsNullOrEmpty(line)) continue;

                    #region 130文字以上の行は分割して保存する
                    //130文字以上の行は分割する
                    var mecabLine = mecab.Parse(line);

                    var list = new List<string>();
                    if (mecabLine.Length > 130)
                    {
                        var ss = mecabLine.Split('|');
                        var temp = "";
                        foreach(var s in ss)
                        {
                            if (temp.Length > 130)
                            {
                                list.Add(temp);
                                temp = "";
                            }
                            temp += s + "|";
                        }
                    }
                    else
                    {
                        list.Add(mecabLine);
                    }
                    #endregion

                    foreach(var text in list)
                    {
                        sw.WriteLine(text);
                    }
                }
            }

            //処理が終了したら入力もとの方はクリアする
            File.WriteAllText(ConfigurationManager.AppSettings["InFilePath"], "", Encoding.GetEncoding("shift_jis"));
        }

        //作業メモ
        //1.NMeCabをNuGetで取得
        //2.dic配下のファイルに対して「出力ディレクトリに常にコピーする」に設定
    }

    public class NMeCabParse
    {
        MeCabTagger mecab = null;
        public NMeCabParse()
        {
            mecab = MeCabTagger.Create();
        }

        public string Parse(string str)
        {
            var node = mecab.ParseToNode(str);

            var parseSentence = new List<string>();

            var cnt = 0;
            while (node != null)
            {
                #region 先頭行には解析対象のデータが入っているので飛ばす
                if (cnt == 0)
                {
                    cnt++;
                    node = node.Next;
                    continue;
                }
                #endregion

                parseSentence.Add(node.Surface);
                node = node.Next;
            }

            //後で使用する際のセパレータとして|を追加
            var res = string.Join("|", parseSentence);

            return res;
        }
    }

}
