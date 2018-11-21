using System.Collections.Generic;

namespace RLBotAutoRunner
{
    public class Team
    {
        public Team(string name, int size, IEnumerable<string> cfgs, IEnumerable<IUniqueResource> resources)
        {
            Name = name;
            Size = size;
            CfgPool = cfgs;
            Resources = resources;
        }

        public string Name { get; }
        public int Size { get; }
        public IEnumerable<string> CfgPool { get; }
        public IEnumerable<IUniqueResource> Resources { get; }
    }
}