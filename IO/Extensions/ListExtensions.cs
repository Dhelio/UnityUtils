#if OPENAI

using Castrimaris.IO.Contracts;
using OpenAI;
using System.Collections.Generic;

namespace Castrimaris.IO {

    public static class ListExtensions {

        public static List<Tool> ToToolsList(this List<ITool> target) {
            var list = new List<Tool>(target.Count);
            foreach (var itool in target) {
                list.Add(itool.ToTool());
            }
            return list;
        }

        public static List<Tool> ToToolsList(this ITool[] target) {
            var list = new List<Tool>(target.Length);
            foreach (var itool in target) {
                list.Add(itool.ToTool());
            }
            return list;
        }

    }

}

#endif