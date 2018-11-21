using System.IO;
using System.Collections.Generic;
using System.Text;

using Section = System.Collections.Generic.Dictionary<string, string>;

namespace RLBotAutoRunner
{
    public class INIParser
    {
        public INIParser() { }
        public INIParser(TextReader reader, INIMode mode) { Parse(reader, mode); }
        public INIParser(string path, INIMode mode) { using (var s = new StreamReader(path, Encoding.ASCII)) Parse(s, mode); }
        public INIParser(INIParser other) { using (var s = new StringReader(other.ToString())) Parse(s, INIMode.CompactEquals); } // TODO: Improve perf

        private readonly Dictionary<string, Section> iniStructure = new Dictionary<string, Section>();
        public string this[string groupKey, string valueKey]
        {
            get
            {
                if (iniStructure.TryGetValue(groupKey, out var group) && group.TryGetValue(valueKey, out var value))
                    return value;
                else return null;
            }
            set
            {
                if (!iniStructure.TryGetValue(groupKey, out var group))
                    iniStructure.Add(groupKey, group = new Section());
                group[valueKey] = value;
            }
        }
        
        public void Append(string groupKey, params (string Key, string Value)[] pairs)
        {
            if (!iniStructure.TryGetValue(groupKey, out var group))
                iniStructure.Add(groupKey, group = new Section());
            foreach (var (Key, Value) in pairs)
                group[Key] = Value;
        }

        public void Parse(TextReader reader, INIMode mode)
        {
            string currentGroup = null;
            string line = null;
            while ((line = reader.ReadLine()) != null)
            {
                int eqPos;
                if (line.Length > 0)
                {
                    if (line[0] == '[')
                        currentGroup = line.Substring(1, line.IndexOf(']') - 1);
                    else if (line[0] != ';' && (eqPos = line.IndexOf('=')) != -1)
                    {
                        var key = line.Substring(0, mode == INIMode.SpacedEquals ? eqPos - 1 : eqPos);
                        var value = line.Substring(mode == INIMode.SpacedEquals ? eqPos + 2 : eqPos + 1);
                        this[currentGroup, key] = value; // TODO: Improve perf
                    }
                }
            }
        }

        public string ToString(INIMode mode)
        {
            var sb = new StringBuilder();
            foreach (var group in iniStructure)
            {
                sb.Append('[');
                sb.Append(group.Key);
                sb.Append(']');
                sb.AppendLine();
                foreach (var value in group.Value)
                {
                    sb.Append(value.Key);
                    if (mode == INIMode.CompactEquals)
                        sb.Append('=');
                    else sb.Append(" = ");
                    sb.Append(value.Value);
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }
        public override string ToString() => ToString(INIMode.CompactEquals);
    }

    public enum INIMode
    {
        CompactEquals,
        SpacedEquals
    }
}
