using Castrimaris.Core;

namespace Castrimaris.IO {

    public enum OpenAIAudioFormat {
        [StringValue("pcm16")] pcm16,
        [StringValue("g711_ulaw")] g711_ulaw,
        [StringValue("g711_alaw")] g711_alaw
    }

}
