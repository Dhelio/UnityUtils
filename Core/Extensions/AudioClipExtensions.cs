using Castrimaris.Core.Utilities;
using System;
using System.Text;
using UnityEngine;

namespace Castrimaris.Core.Extensions {
    public static class AudioClipExtensions {

        /// <summary>
        /// Creates a byte array from this <see cref="AudioClip"/>
        /// </summary>
        public static byte[] GetBytes(this AudioClip target) => AudioClipUtilities.GetBytesFromClip(target);

        /// <summary>
        /// Creates an <see cref="AudioClip"/> from this byte array
        /// </summary>
        public static AudioClip GetAudioClip(this byte[] target) => AudioClipUtilities.GetClipFromBytes(target);

        /// <summary>
        /// Creates from this <see cref="AudioClip"/> a pcm16 byte array encoded as a base64 string. Useful for reliable web audio transfer or data saving.
        /// </summary>
        /// <param name="target"></param>
        public static string GetPcm16Base64String(this AudioClip target) => AudioClipUtilities.GetPcm16Base64String(target);

        /// <summary>
        /// Converts this <see cref="AudioClip"/> to a base 64 string.
        /// </summary>
        public static string GetBase64(this AudioClip target) {
            var bytes = target.GetBytes();
            var bytesBase64 = Convert.ToBase64String(bytes);
            return bytesBase64;
        }

        /// <summary>
        /// Creates an <see cref="AudioClip"/> from this base64 string.
        /// </summary>
        public static AudioClip GetAudioClip(this string target) {
            var bytes = Convert.FromBase64String(target);
            var clip = AudioClipUtilities.GetClipFromBytes(bytes);
            return clip;
        }
    }
}