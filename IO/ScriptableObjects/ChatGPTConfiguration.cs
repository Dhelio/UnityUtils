#if OPENAI

using OpenAI.Chat;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Castrimaris.IO.ScriptableObjects {

    /// <summary>
    /// Generic assistant configuration <see cref="ScriptableObject"/> to configure an assistant behaviour
    /// </summary>
    [CreateAssetMenu(fileName = "ChatGPT Configuration", menuName = "Castrimaris/ScriptableObjects/ChatGPT Configuration")]
    public class ChatGPTConfiguration : ScriptableObject {

        [Header("Parameters")]
        [SerializeField] private List<Message> messages = new List<Message>();

        /// <summary>
        /// Returns all the configuration messages as an array
        /// </summary>
        public Message[] MessagesArray => messages.ToArray();

        /// <summary>
        /// Returns all the configuration messages as a list
        /// </summary>
        public List<Message> Messages { get {

                //We're removing the name, as it can cause 400 Bad Request errors
                var result = new List<Message>();
                foreach (var message in messages) {
                    result.Add(new Message(message.Role, message.Content.ToString()));
                }

                return result;
            }
        }

        /// <summary>
        /// Returns all the configuration messages as a single string
        /// </summary>
        public string MessagesString {
            get {
                var result = string.Join("", Messages.Select(message => message.Content.ToString()));
                return result;
            }
        }

    }
}

#endif