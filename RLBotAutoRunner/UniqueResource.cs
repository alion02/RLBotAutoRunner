using System;
using System.Collections.Generic;

namespace RLBotAutoRunner
{
    public interface IUniqueResource : IEquatable<IUniqueResource>
    {
        void Start();
        void Stop();
    }

    public static class UniqueResource
    {
        public static HashSet<IUniqueResource> Global = new HashSet<IUniqueResource>();

        public static bool TryParse(string s, out IUniqueResource resource) // TODO: Improve parsing
        {
            resource = FileResource.Create(s);
            return true;
        }
    }
}