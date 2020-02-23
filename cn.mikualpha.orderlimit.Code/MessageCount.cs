using Native.Sdk.Cqp.EventArgs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Caching;

class MessageCount
{
    private static MessageCount ins = null;
    private List<Option> Options = new List<Option>();

    public static MessageCount GetInstance()
    {
        if (ins == null) ins = new MessageCount();
        return ins;
    }

    private MessageCount() {
        string path = ApiModel.CQApi.AppDirectory + "words.ini";
        InitalizeFile(path);
        ReadFromFile(path);
    }

    private bool InitalizeFile(string path)
    {
        if (File.Exists(path)) return false;
        if (!Directory.Exists(ApiModel.CQApi.AppDirectory)) Directory.CreateDirectory(ApiModel.CQApi.AppDirectory);

        FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
        StreamWriter writer = new StreamWriter(fs);
        writer.Write("//格式为[调用词]#[规定时间内调用次数限制]#[规定时间长度(秒)]"
            + "\n" + "//例如:"
            + "\n" + "示例指令#3#180");
        writer.Close();
        fs.Close();
        return true;
    }

    protected void ReadFromFile(string _path)
    {
        if (!File.Exists(_path)) return;

        using (StreamReader sr = new StreamReader(_path))
        {
            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains("//")) continue;
                string[] temp = line.Split(new char[] { '#', '$' }, StringSplitOptions.RemoveEmptyEntries);
                Option option = new Option(temp[0].Trim(), int.Parse(temp[1].Trim()), int.Parse(temp[2].Trim()));
                Options.Add(option);
            }
        }
    }

    public bool processGroupMessage(CQGroupMessageEventArgs e)
    {
        for (int i = 0; i < Options.Count; ++i)
        {
            Option option = Options[i];
            if (e.Message.Text.Contains(option.word))
            {
                if (NeedIntercept("Group", e.FromGroup.Id, option.word, option.count, option.time))
                {
                    ApiModel.CQLog.Debug("指令拦截", "[" + option.word + "]调用次数已达上限，请稍后再试。");
                    return true;
                }
            }
        }
        return false;
    }

    public bool NeedIntercept(string type, long from, string word, int limit, int timeout)
    {
        string keyName = type + "-" + from.ToString() + "-" + word;
        var objCache = HttpRuntime.Cache;

        if (objCache.Get(keyName) == null)
        {
            objCache.Insert(keyName, 1, null, DateTime.Now.AddSeconds(timeout), TimeSpan.Zero, CacheItemPriority.High, null);
            return false;
        }

        int count = (int)objCache.Get(keyName);
        if (count >= limit) return true;

        objCache.Insert(keyName, count + 1, null, DateTime.Now.AddSeconds(timeout), TimeSpan.Zero, CacheItemPriority.High, null);
        return false;
    }   

    protected class Option
    {
        public Option(string _word, int _count, int _time, bool _allEqual = false)
        {
            word = _word;
            count = _count;
            time = _time;
            allEqual = _allEqual;
        } 
        public string word { get; set; }
        public int count { get; set; }
        public int time { get; set; }
        public bool allEqual { get; set; }
    }
}

