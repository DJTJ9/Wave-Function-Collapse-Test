#if UNITY_EDITOR
using System;

namespace Kamgam.MouseShortcuts
{
    public class MouseCommandAttribute : Attribute
    {
        public string Id;

        public MouseCommandAttribute(string id)
        {
            this.Id = id;
        }
    }
}
#endif
