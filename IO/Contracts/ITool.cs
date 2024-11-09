using OpenAI;

namespace Castrimaris.IO.Contracts {

    public interface ITool {
        public Tool ToTool();
    }

}