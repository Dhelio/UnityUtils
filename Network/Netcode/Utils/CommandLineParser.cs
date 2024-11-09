using Castrimaris.Network.Data;
using System;
using System.Collections.Generic;

//TODO place in appropriate namespace
namespace Castrimaris.Network.Netcode {

    /// <summary>
    /// Class used to manage the command line arguments to start the game in the desired mode.
    /// Without any arg the game will start in the mode set in the inspector before the game start or build.
    /// Default args and options include:
    /// -Mode : host, server, client
    /// -Address : 0.0.0.0~255.255.255.255
    /// -Port : 0~65535
    /// -ServerListenAddress : 0.0.0.0~255.255.255.255
    /// </summary>
    public class CommandLineParser {

        #region PRIVATE VARIABLES
        private const string TAG = nameof(CommandLineParser);
        #endregion

        #region PRIVATE METHODS

        private static Dictionary<string, string> GetCommandlineArgs() {
            Dictionary<string, string> argDictionary = new Dictionary<string, string>();

            var args = Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length; ++i) {
                var arg = args[i].ToLower();
                if (arg.StartsWith("-")) {
                    var value = i < args.Length - 1 ? args[i + 1].ToLower() : null;
                    value = (value?.StartsWith("-") ?? false) ? null : value;

                    argDictionary.Add(arg, value);
                }
            }

            return argDictionary;
        }

        #endregion

        #region PUBLIC METHODS
        public static bool ParseCommands(out ConnectionAddressData data) {

            data = new ConnectionAddressData() {
                ServerListenAddress = null,
                Address = null,
                Port = null
            };

            
            var args = GetCommandlineArgs();
            bool hasArguments = false;

            if (args.TryGetValue("-Address", out string address)) {
                data.Address = address;
                hasArguments = true;
            }

            if (args.TryGetValue("-Port", out string port)) {
                if (ushort.TryParse(port, out ushort ushortPort)) {
                    data.Port = ushortPort;
                    hasArguments = true;
                }
            }

            if (args.TryGetValue("-ServerListenAddress", out string serverListenAddress)) {
                data.ServerListenAddress = serverListenAddress;
                hasArguments = true;
            }

            return hasArguments;
        }
        #endregion

    }
}
