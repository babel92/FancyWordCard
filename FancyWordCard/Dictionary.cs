using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace WordCard
{
    class Entry
    {
        public String Word;
        public String Explanation;
        public Entry(String W, String E)
        {
            Word = W;
            Explanation = E;
        }
    }

    class Dictionary
    {
        private String m_path;
        private List<Entry> m_content;
        private Random m_random;
        public Dictionary(String Path)
        {
            using (StreamReader sr = new StreamReader(Path))
            {
                m_path = Path;
                m_content=new List<Entry>();
                m_random = new Random();
                String tmp = sr.ReadLine();
                while (tmp != null)
                {
                    if (tmp.Length == 0)
                        return;
                    string[] split = tmp.Split('\t');
                    m_content.Add(new Entry(split[0],split[1]));
                    tmp = sr.ReadLine();
                }
                sr.Close();
            }
        }

        public void WriteDictionary(String Path)
        {
            StreamWriter sw = new StreamWriter(Path);
            foreach (Entry e in m_content)
            {
                sw.WriteLine(e.Word + '\t' + e.Explanation);
            }
            sw.Close();
        }

        public String GetRandomEntry()
        {
            Entry item=m_content[m_random.Next(m_content.Count)];
            return item.Word + '\n' + item.Explanation;
        }
    }
}
