using SoLinkDSDK.Scale;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SocketLeitorNetCore.Classes
{
    public sealed class SingletonScale
    {
        static SingletonScale _instance;
        private static readonly object padLock = new object();
        private static string _BALANCA;
        public static MachineSetup machineconfig;
        public SDKScale Scale { get; set; }

        public static SingletonScale instance
        {
            get
            {
                if (_instance is null)
                {
                    lock (padLock)
                    {
                        if (_instance is null)
                        {
                            _instance = new SingletonScale();
                        }
                    }
                }

                return _instance;
            }
        }

        private SingletonScale()
        {
            try
            {
                _BALANCA = machineconfig.Balanca;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Scale = new SDKScale($@"./config/{ _BALANCA}", $@"./config/{ _BALANCA}", false);
                }
                else
                {
                    Scale = new SDKScale($@".\config\{ _BALANCA}", $@"./config/{ _BALANCA}", false);
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"./config/Erro.txt", ex.Message + Scale.LastError);
            }
        }
    }
}
