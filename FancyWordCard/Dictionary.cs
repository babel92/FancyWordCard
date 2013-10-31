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
                    m_content.Add(new Entry(tmp.Substring(2, tmp.LastIndexOf('#') - 2), tmp.Substring(tmp.LastIndexOf('#')+1 , tmp.Length - tmp.LastIndexOf('#')-1)));
                    tmp = sr.ReadLine();
                }
            }
        }

        public String GetRandomEntry()
        {
            Entry item=m_content[m_random.Next(m_content.Count-1)];
            return item.Word + '\n' + item.Explanation;
        }
    }
}
