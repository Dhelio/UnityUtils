using Castrimaris.Core;
using OpenAI;

namespace Castrimaris.IO {

    /// <summary>
    /// AI models developed by OpenAI. This enum is used to specify models to use.
    /// </summary>
    public enum Models {

        /// <summary>
        /// Even more capable than GPT-4, it is multimodal (accepts text or image), and it has the same intelligence as GPT-4 Turbo but is more efficient - 
        /// it generates text 2x faster and is 50% cheaper.
        /// </summary>
        [StringValue("gpt-4o")] GPT_4o,

        /// <summary>
        /// More capable than any GPT-3.5 model, able to do more complex tasks, and optimized for chat.
        /// Will be updated with our latest model iteration.
        /// </summary>
        [StringValue("gpt-4")] GPT_4,

        /// <summary>
        /// Same capabilities as the base gpt-4 mode but with 4x the context length.
        /// Will be updated with our latest model iteration.  Tokens are 2x the price of gpt-4.
        /// </summary>
        [StringValue("gpt-4-32k")] GPT4_32K,

        /// <summary>
        /// Because gpt-3.5-turbo performs at a similar capability to text-davinci-003 but at 10%
        /// the price per token, we recommend gpt-3.5-turbo for most use cases.
        /// </summary>
        [StringValue("gpt-3.5-turbo")] GPT3_5_Turbo,

        /// <summary>
        /// Same capabilities as the base gpt-3.5-turbo mode but with 4x the context length.
        /// Tokens are 2x the price of gpt-3.5-turbo. Will be updated with our latest model iteration.
        /// </summary>
        [StringValue("gpt-3.5-turbo-16k")] GPT3_5_Turbo_16K,

        /// <summary>
        /// The most powerful, largest engine available, although the speed is quite slow.<para/>
        /// Good at: Complex intent, cause and effect, summarization for audience
        /// </summary>
        [StringValue("text-davinci-003")] Davinci,

        /// <summary>
        /// For edit requests.
        /// </summary>
        [StringValue("text-davinci-edit-001")] DavinciEdit,

        /// <summary>
        /// The 2nd most powerful engine, a bit faster than <see cref="Davinci"/>, and a bit faster.<para/>
        /// Good at: Language translation, complex classification, text sentiment, summarization.
        /// </summary>
        [StringValue("text-curie-001")] Curie,

        /// <summary>
        /// The 2nd fastest engine, a bit more powerful than <see cref="Ada"/>, and a bit slower.<para/>
        /// Good at: Moderate classification, semantic search classification
        /// </summary>
        [StringValue("text-babbage-001")] Babbage,

        /// <summary>
        /// The smallest, fastest engine available, although the quality of results may be poor.<para/>
        /// Good at: Parsing text, simple classification, address correction, keywords
        /// </summary>
        [StringValue("text-ada-001")] Ada,

        /// <summary>
        /// The default model for <see cref="OpenAI.Embeddings.EmbeddingsEndpoint"/>.
        /// </summary>
        [StringValue("text-embedding-ada-002")] Embedding_Ada_002,

        /// <summary>
        /// The default model for <see cref="OpenAI.Audio.AudioEndpoint"/>.
        /// </summary>
        [StringValue("whisper-1")] Whisper1,

        /// <summary>
        /// The default model for <see cref="OpenAI.Moderations.ModerationsEndpoint"/>.
        /// </summary>
        [StringValue("text-moderation-latest")] Moderation_Latest,

        /// <summary>
        /// The default model for <see cref="OpenAI.Audio.SpeechRequest"/>s.
        /// </summary>
        [StringValue("tts-1")] TTS_1,

        [StringValue("tts-1-hd")] TTS_1HD,

        /// <summary>
        /// The default model for <see cref="OpenAI.Images.ImagesEndpoint"/>.
        /// </summary>
        [StringValue("dall-e-2")] DallE_2,

        [StringValue("dall-e-3")] DallE_3,
    }


}