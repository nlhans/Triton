namespace Triton.Collections
{
    using System;
    using System.Collections.Generic;
    using System.IO;


    using Triton;

    public class StoredStringList
    {
        private List<string> List = new List<string>();
        private string _File;

        public StoredStringList(string file)
        {
            Filter = false;
            _File = file;

            TritonBase.Close += new AnonymousSignal(Save);

            this.Load();
        }

        public StoredStringList(string file, bool f)
        {
            Filter = f;
            _File = file;

            TritonBase.Close += new AnonymousSignal(Save);

            this.Load();
        }

        public bool Filter { get { return this._Filter; } set { this._Filter = value; } }
        private bool _Filter = false;

        public int Count
        {
            get
            {
                return this.List.Count;
            }
        }

        private void Load()
        {
            if (File.Exists(_File))
            {
                List.Clear();
                using (StreamReader rRead = new StreamReader(_File))
                {
                    while (rRead.EndOfStream == false)
                    {
                        this.Add(rRead.ReadLine());
                    }
                    rRead.Close();
                }
            }
        }

        public string Get(int index)
        {
            if (this.List.Count - 1 >= index)
            {
                return this.List[index];
            }
            return string.Empty;
        }

        public void RemoveAt(int index)
        {
            this.List.RemoveAt(index);
        }

        public void Add(string txt)
        {
            if (_Filter)
            {
                if (List.Contains(txt))
                    return;
            }
            List.Add(txt);
        }

        public string[] Items
        {
            get
            {
                return List.ToArray();
            }
        }

        public bool Contains(string txt)
        {
            return List.Contains(txt);
        }

        public void Save()
        {
            using (StreamWriter wWrite = new StreamWriter(_File))
            {
                List.ForEach(delegate(string s) { wWrite.WriteLine(s); });
                wWrite.Close();
            }

        }

    }
}
