using SocketLeitorNetCore.Classes;
using SocketLeitorNetCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Linq;
using SoInterface.Global;
using SoLinkDSDK.UHF;
using SoLinkLib.Global;
using SoLinkLib.UHF;
using System.Runtime.InteropServices;
using SoLinkDSDK.Scale;
using System.IO;

namespace SocketLeitorNetCore
{
    class Program
    { public static string peso;
        public static bool _isWeighing;
        public static int QuantidadeLida;
        public static string _LOGLEITOR = $"logLeitor-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}.txt";
        public static string PATHARQUIVOLOGPARAENVIO;
        public static  readonly string _LOGSENDNAME = $"log-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}.txt";
        private static IFileManager _IFileManager;
        private static SDKUHFReader _SDKUHFReader;
        private static Dictionary<string, string> DicTags = new Dictionary<string, string>();
        static Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        static Socket serverSocketqtd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        static Socket serverSocketBalanca = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        static private string guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        public static string IPConector;
        public static bool isReading = false;
        public static bool Validatag = false;
        public static Socket client = null;
        public static MachineSetup machineconfig;
     //   public static SDKScale SCALE;
        public static SDKScale _MyScale;
        static SHA1 sha1 = SHA1CryptoServiceProvider.Create();
        private static string _LEITORRFID;
        private static int cont;
        public static string _BALANCA;
        static void Main(string[] args)
        {
            _IFileManager = new FileManager();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                machineconfig = Newtonsoft.Json.JsonConvert.DeserializeObject<MachineSetup>(_IFileManager.LerArquivoInteiro(@"./config/", "applicationsettings.json"));
                PATHARQUIVOLOGPARAENVIO = @"./SENDLOG/";
            }
            else
            {
                machineconfig = Newtonsoft.Json.JsonConvert.DeserializeObject<MachineSetup>(_IFileManager.LerArquivoInteiro(@".\config\", "applicationsettings.json"));
                PATHARQUIVOLOGPARAENVIO = @".\SENDLOG\";
            }
            serverSocketqtd.Bind(new IPEndPoint(IPAddress.Any, 9414));
            serverSocketqtd.Listen(1); //just one socket
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 9413));
            serverSocket.Listen(1); //just one socket
            serverSocketBalanca.Bind(new IPEndPoint(IPAddress.Any, 9415));
            serverSocketBalanca.Listen(1); //just one socket
            InitReader();
            InstanceScale();
            serverSocket.BeginAccept(null, 0, OnAccept, null);
            serverSocketqtd.BeginAccept(null, 0, OnAcceptt, null);
            serverSocketBalanca.BeginAccept(null, 0, OnAcceptBalance, null);
            Console.Read();
        }
        private static void OnAcceptBalance(IAsyncResult result)
        {
           
            byte[] buffer = new byte[1024];
            try
            {
                Socket client = null;
                string headerResponse = "";
                if (serverSocketBalanca != null && serverSocketBalanca.IsBound)
                {
                    client = serverSocketBalanca.EndAccept(result);
                    var i = client.Receive(buffer);
                    headerResponse = (System.Text.Encoding.UTF8.GetString(buffer)).Substring(0, i);
                    // write received data to the console
                    //Console.WriteLine(headerResponse);
                    //Console.WriteLine("=====================");
                }
                if (client != null)
                {
                    /* Handshaking and managing ClientSocket */
                    var key = headerResponse.Replace("ey:", "`")
                              .Split('`')[1]                     // dGhlIHNhbXBsZSBub25jZQ== \r\n .......
                              .Replace("\r", "").Split('\n')[0]  // dGhlIHNhbXBsZSBub25jZQ==
                              .Trim();

                    // key should now equal dGhlIHNhbXBsZSBub25jZQ==
                    var test1 = AcceptKey(ref key);

                    var newLine = "\r\n";

                    var response = "HTTP/1.1 101 Switching Protocols" + newLine
                         + "Upgrade: websocket" + newLine
                         + "Connection: Upgrade" + newLine
                         + "Sec-WebSocket-Accept: " + test1 + newLine + newLine
                         //+ "Sec-WebSocket-Protocol: chat, superchat" + newLine
                         //+ "Sec-WebSocket-Version: 13" + newLine
                         ;

                    var pesos = peso;
                    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(pesos));
                    client.Send(System.Text.Encoding.UTF8.GetBytes(response));
                    var i = client.Receive(buffer); // wait for client to send a message
                    string browserSent = GetDecodedData(buffer, i);
                    //Console.WriteLine("BrowserSent: " + browserSent);

                    //Console.WriteLine("=====================");
                    //now send message to client
                    // DicTags.Add("funcionou", "funcionou");
                    client.Send(GetFrameFromString(Newtonsoft.Json.JsonConvert.SerializeObject(pesos)));
                    System.Threading.Thread.Sleep(10000);//wait for message to be sent
                    peso = "";

                }
            }
            catch (SocketException exception)
            {
                throw exception;
            }
            finally
            {
                if (serverSocketBalanca != null && serverSocketBalanca.IsBound)
                {
                    serverSocketBalanca.BeginAccept(null, 0, OnAcceptBalance, null);
                }
            }
        }
        private static void OnAcceptt(IAsyncResult result)
        {

            byte[] buffer = new byte[1024];
            try
            {
                Socket client = null;
                string headerResponse = "";
                if (serverSocketqtd != null && serverSocketqtd.IsBound)
                {
                    client = serverSocketqtd.EndAccept(result);
                    var i = client.Receive(buffer);
                    headerResponse = (System.Text.Encoding.UTF8.GetString(buffer)).Substring(0, i);
                    // write received data to the console
                    //Console.WriteLine(headerResponse);
                    //Console.WriteLine("=====================");
                }
                if (client != null)
                {
                    /* Handshaking and managing ClientSocket */
                    var key = headerResponse.Replace("ey:", "`")
                              .Split('`')[1]                     // dGhlIHNhbXBsZSBub25jZQ== \r\n .......
                              .Replace("\r", "").Split('\n')[0]  // dGhlIHNhbXBsZSBub25jZQ==
                              .Trim();

                    // key should now equal dGhlIHNhbXBsZSBub25jZQ==
                    var test1 = AcceptKey(ref key);

                    var newLine = "\r\n";

                    var response = "HTTP/1.1 101 Switching Protocols" + newLine
                         + "Upgrade: websocket" + newLine
                         + "Connection: Upgrade" + newLine
                         + "Sec-WebSocket-Accept: " + test1 + newLine + newLine
                         //+ "Sec-WebSocket-Protocol: chat, superchat" + newLine
                         //+ "Sec-WebSocket-Version: 13" + newLine
                         ;

                    int leituras = QuantidadeLida;
                    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(leituras));
                    client.Send(System.Text.Encoding.UTF8.GetBytes(response));
                    var i = client.Receive(buffer); // wait for client to send a message
                    string browserSent = GetDecodedData(buffer, i);
                    //Console.WriteLine("BrowserSent: " + browserSent);

                    //Console.WriteLine("=====================");
                    //now send message to client
                    // DicTags.Add("funcionou", "funcionou");
                    client.Send(GetFrameFromString(Newtonsoft.Json.JsonConvert.SerializeObject(leituras)));
                    System.Threading.Thread.Sleep(10000);//wait for message to be sent
 
                }
            }
            catch (SocketException exception)
            {
                throw exception;
            }
            finally
            {
                if (serverSocketqtd != null && serverSocketqtd.IsBound)
                {
                    serverSocketqtd.BeginAccept(null, 0, OnAcceptt, null);
                }
            }
        }
        #region Socket Antena
        private static void OnAccept(IAsyncResult result)
        {
           
            byte[] buffer = new byte[1024];
            try
            {
                Socket client = null;
                string headerResponse = "";
                if (serverSocket != null && serverSocket.IsBound)
                {
                    client = serverSocket.EndAccept(result);
                    var i = client.Receive(buffer);
                    headerResponse = (System.Text.Encoding.UTF8.GetString(buffer)).Substring(0, i);
                    // write received data to the console
                    //Console.WriteLine(headerResponse);
                    //Console.WriteLine("=====================");
                }
                if (client != null)
                {
                    /* Handshaking and managing ClientSocket */
                    var key = headerResponse.Replace("ey:", "`")
                              .Split('`')[1]                     // dGhlIHNhbXBsZSBub25jZQ== \r\n .......
                              .Replace("\r", "").Split('\n')[0]  // dGhlIHNhbXBsZSBub25jZQ==
                              .Trim();

                    // key should now equal dGhlIHNhbXBsZSBub25jZQ==
                    var test1 = AcceptKey(ref key);

                    var newLine = "\r\n";

                    var response = "HTTP/1.1 101 Switching Protocols" + newLine
                         + "Upgrade: websocket" + newLine
                         + "Connection: Upgrade" + newLine
                         + "Sec-WebSocket-Accept: " + test1 + newLine + newLine
                         //+ "Sec-WebSocket-Protocol: chat, superchat" + newLine
                         //+ "Sec-WebSocket-Version: 13" + newLine
                         ;

                   // iniciaLeitura();
                    Read();
                    while (cont <10 )
                    {
                            cont++;
                            Console.WriteLine(cont.ToString());
                            Thread.Sleep(1000);
                        }
                    _SDKUHFReader.StopReading();
                    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(DicTags));
                    client.Send(System.Text.Encoding.UTF8.GetBytes(response));
                    var i = client.Receive(buffer); // wait for client to send a message
                    string browserSent = GetDecodedData(buffer, i);
                    //Console.WriteLine("BrowserSent: " + browserSent);

                    //Console.WriteLine("=====================");
                    //now send message to client
                   // DicTags.Add("funcionou", "funcionou");
                    client.Send(GetFrameFromString(Newtonsoft.Json.JsonConvert.SerializeObject(DicTags)));
                    System.Threading.Thread.Sleep(10000);//wait for message to be sent
                    cont = 0;
                    QuantidadeLida = 0;
                    DicTags = new Dictionary<string, string>();
                }
            }
            catch (SocketException exception)
            {
                throw exception;
            }
            finally
            {
                if (serverSocket != null && serverSocket.IsBound)
                {
                    serverSocket.BeginAccept(null, 0, OnAccept, null);
                }
            }
        }
        #endregion

        #region Configuração Socket
        public static T[] SubArray<T>(T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
        private static string AcceptKey(ref string key)
        {
            string longKey = key + guid;
            byte[] hashBytes = ComputeHash(longKey);
            return Convert.ToBase64String(hashBytes);
        }
        private static byte[] ComputeHash(string str)
        {
            return sha1.ComputeHash(System.Text.Encoding.ASCII.GetBytes(str));
        }
        //Needed to decode frame
        public static string GetDecodedData(byte[] buffer, int length)
        {
            byte b = buffer[1];
            int dataLength = 0;
            int totalLength = 0;
            int keyIndex = 0;

            if (b - 128 <= 125)
            {
                dataLength = b - 128;
                keyIndex = 2;
                totalLength = dataLength + 6;
            }

            if (b - 128 == 126)
            {
                dataLength = BitConverter.ToInt16(new byte[] { buffer[3], buffer[2] }, 0);
                keyIndex = 4;
                totalLength = dataLength + 8;
            }

            if (b - 128 == 127)
            {
                dataLength = (int)BitConverter.ToInt64(new byte[] { buffer[9], buffer[8], buffer[7], buffer[6], buffer[5], buffer[4], buffer[3], buffer[2] }, 0);
                keyIndex = 10;
                totalLength = dataLength + 14;
            }

            if (totalLength > length)
                throw new Exception("The buffer length is small than the data length");

            byte[] key = new byte[] { buffer[keyIndex], buffer[keyIndex + 1], buffer[keyIndex + 2], buffer[keyIndex + 3] };

            int dataIndex = keyIndex + 4;
            int count = 0;
            for (int i = dataIndex; i < totalLength; i++)
            {
                buffer[i] = (byte)(buffer[i] ^ key[count % 4]);
                count++;
            }

            return Encoding.ASCII.GetString(buffer, dataIndex, dataLength);
        }
        //function to create  frames to send to client 
        /// <summary>
        /// Enum for opcode types
        /// </summary>
        public enum EOpcodeType
        {
            /* Denotes a continuation code */
            Fragment = 0,

            /* Denotes a text code */
            Text = 1,

            /* Denotes a binary code */
            Binary = 2,

            /* Denotes a closed connection */
            ClosedConnection = 8,

            /* Denotes a ping*/
            Ping = 9,

            /* Denotes a pong */
            Pong = 10
        }
        /// <summary>Gets an encoded websocket frame to send to a client from a string</summary>
        /// <param name="Message">The message to encode into the frame</param>
        /// <param name="Opcode">The opcode of the frame</param>
        /// <returns>Byte array in form of a websocket frame</returns>
        public static byte[] GetFrameFromString(string Message, EOpcodeType Opcode = EOpcodeType.Text)
        {

            byte[] response;
            byte[] bytesRaw = Encoding.Default.GetBytes(Message);
            byte[] frame = new byte[10];

            long indexStartRawData = -1;
            long length = (long)bytesRaw.Length;

            frame[0] = (byte)(128 + (int)Opcode);
            if (length <= 125)
            {
                frame[1] = (byte)length;
                indexStartRawData = 2;
            }
            else if (length >= 126 && length <= 65535)
            {
                frame[1] = (byte)126;
                frame[2] = (byte)((length >> 8) & 255);
                frame[3] = (byte)(length & 255);
                indexStartRawData = 4;
            }
            else
            {
                frame[1] = (byte)127;
                frame[2] = (byte)((length >> 56) & 255);
                frame[3] = (byte)((length >> 48) & 255);
                frame[4] = (byte)((length >> 40) & 255);
                frame[5] = (byte)((length >> 32) & 255);
                frame[6] = (byte)((length >> 24) & 255);
                frame[7] = (byte)((length >> 16) & 255);
                frame[8] = (byte)((length >> 8) & 255);
                frame[9] = (byte)(length & 255);

                indexStartRawData = 10;
            }

            response = new byte[indexStartRawData + length];

            long i, reponseIdx = 0;

            //Add the frame bytes to the reponse
            for (i = 0; i < indexStartRawData; i++)
            {
                response[reponseIdx] = frame[i];
                reponseIdx++;
            }

            //Add the data bytes to the response
            for (i = 0; i < length; i++)
            {
                response[reponseIdx] = bytesRaw[i];
                reponseIdx++;
            }

            return response;
        }
        #endregion

        #region Configuração Antena
        public static void InitReader()
        {

            if (_SDKUHFReader is null)
            {
                _LEITORRFID = machineconfig.LeitorModelo;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    _SDKUHFReader = new SDKUHFReader($@"./config/{_LEITORRFID}", $@"config/{_LEITORRFID}", false);
                }
                else
                {
                    _SDKUHFReader = new SDKUHFReader($@".\config\{_LEITORRFID}", $@"config\{_LEITORRFID}", false); ;
                }              
                //temos agora vários modo
                _SDKUHFReader.SetMode(UHFReaderMode.READING);

                _SDKUHFReader.TagDataType = UHFTagReturnType.EPC;
                _SDKUHFReader.OnReadALL += _SDKUHFReader_OnReadALL;
                _SDKUHFReader.OnReadEPC += _SDKUHFReader_OnReadEPC;
                _SDKUHFReader.OnConnectionParam += _SDKUHFReader_OnConnectionParam;
                _SDKUHFReader.OnAgentException += _SDKUHFReader_OnAgentException;
                _SDKUHFReader.OnDeviceException += _SDKUHFReader_OnDeviceException;
                //obrigatorio quando usa o starnottagmonitoring com o parametro NoNewTag=true
                _SDKUHFReader.OnNoNewTag += READER_OnNoNewTag;
                //obrigatorio quando usa o starnottagmonitoring com o parametro SendLeavetag=true
                _SDKUHFReader.OnTagLeave += _SDKUHFReader_OnTagLeave;
                _SDKUHFReader.OnPing += _SDKUHFReader_OnPing;
                //obrigatorio quando usa o starnottagmonitoring
                _SDKUHFReader.OnNoTag += _SDKUHFReader_OnNoTag;
                //obrigatorio para ter o stauts de leitura do leitor
                _SDKUHFReader.OnReadingStatus += _SDKUHFReader_OnReadingStatus;

                // Inicia Conexão
                if (!_SDKUHFReader.IsDeviceConnected())
                {
                    if (!_SDKUHFReader.ConnectDevice())
                    {
                        Console.WriteLine($"Não foi possivel conectar ao leitor!\n\n {IPConector} \n\n {_SDKUHFReader.LastError.Data}");
                        _SDKUHFReader.DisConnectDevice();
                    }
                }

            }
        }
        private static void _SDKUHFReader_OnReadingStatus(object Sender, GLBStatus Status)
        {
            switch (Status)
            {
                //case GLBStatus.OFF:
                //    this.rdpbStatusRFID.Invoke(new MethodInvoker(delegate { this.rdpbStatusRFID.Image = Properties.Resources.RedButton; }));
                //    break;
                //case GLBStatus.ON:
                //    this.rdpbStatusRFID.Invoke(new MethodInvoker(delegate { this.rdpbStatusRFID.Image = Properties.Resources.GreenButton; }));
                //    break;
            }
        }
        private static void _SDKUHFReader_OnNoTag(object Sender)
        {
            //Painel.Invoke(new MethodInvoker(delegate { Painel.Items.Add("NoTag ... "); }));
        }
        private static void _SDKUHFReader_OnConnectionParam(object Sender, GLBDeviceParam Param)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _IFileManager.CriarArquivo(@".\", "IFileManger", Param.ParamName + " - " + Param.ParamValue + " - " + Param.ParamWritable);
            }
            else
            {
                _IFileManager.CriarArquivo(@".\", "IFileManger", Param.ParamName + " - " + Param.ParamValue + " - " + Param.ParamWritable);
            }

            if (Param.ParamName == "URI")
            {
                IPConector = Param.ParamValue;
            }
        }
        private static void _SDKUHFReader_OnTagLeave(object Sender, string Epc)
        {
            //tag saiu do campo
        }
        private static void _SDKUHFReader_OnReadingStatus(object Sender)
        {
            //switch (Status)
            //{
            //case GLBStatus.OFF:
            //    this.rdpbStatusRFID.Invoke(new MethodInvoker(delegate { this.rdpbStatusRFID.Image = Properties.Resources.RedButton; }));
            //    break;
            //case GLBStatus.ON:
            //    this.rdpbStatusRFID.Invoke(new MethodInvoker(delegate { this.rdpbStatusRFID.Image = Properties.Resources.GreenButton; }));
            //    break;
            // }
        }
        private static void _SDKUHFReader_OnReadALL(object Sender, UHFTagReturnALL Tag)
        {
            //try
            //{
            //    Console.WriteLine(Tag.Epc);
            //    if (!DicTags.ContainsKey(Tag.Epc))
            //    {
            //        DicTags.Add(Tag.Epc, Tag.Epc);
            //    }
            //    else
            //    {
            //        cont++;
            //        Thread.Sleep(400);
            //    }
            //    if (cont > 50)
            //    {
            //        isReading = false;
            //    }
            //}
            //catch (Exception e)
            //{
            //    _IFileManager.GravarLinhaNoArquivo(PATHARQUIVOLOGPARAENVIO, _LOGLEITOR, $"[{DateTime.Now}] => [OnReadEpc] ERRO: {e.Message}");
            //}
        }
        private static void _SDKUHFReader_OnReadEPC(object Sender, UHFTagReturnEPC Tag)
        {
            try
            {
                if (!DicTags.ContainsKey(Tag.Epc))
                {
                    DicTags.Add(Tag.Epc, Tag.Epc);
                    Console.WriteLine(Tag.Epc.ToString() + "\n");
                    cont = 0;
                    QuantidadeLida = QuantidadeLida+1;
                }
                //if (cont > 60)
                //{
                //    isReading = false;
                //}
            }
            catch (Exception e)
            {
               _IFileManager.GravarLinhaNoArquivo(PATHARQUIVOLOGPARAENVIO, _LOGLEITOR, $"[{DateTime.Now}] => [OnReadEpc] ERRO: {e.Message}");
            }
        }
        private static void _SDKUHFReader_OnPing(object Sender, MSGStatus Message)
        {
           _IFileManager.GravarLinhaNoArquivo(PATHARQUIVOLOGPARAENVIO, _LOGLEITOR, $"[{DateTime.Now}] => [OnPing] ERRO: {Message.Data}");
        }
        private static void _SDKUHFReader_OnDeviceException(object Sender, MSGStatus Message)
        {
            _IFileManager.GravarLinhaNoArquivo(PATHARQUIVOLOGPARAENVIO, _LOGLEITOR, $"[{DateTime.Now}] => [Device Exception] ERRO: {Message.Data}");
        }
        private static void _SDKUHFReader_OnAgentException(object Sender, MSGStatus Message)
        {
             _IFileManager.GravarLinhaNoArquivo(PATHARQUIVOLOGPARAENVIO, _LOGLEITOR, $"[{DateTime.Now}] => [Agent Exception] ERRO: {Message.Data}");
        }
        private static void READER_OnNoNewTag(object Sender)
        {
            try
            {
                if (!isReading)
                {
                    DicTags = DicTags.Where(c => !String.IsNullOrEmpty(c.Value)).ToDictionary(s => s.Key, x => x.Value);
                    Validatag = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static Dictionary<string, string> Read()
        {
            isReading = true;

            cont = 0;
            isReading = true;
            // Inicia Leitura RFID
            var result = _SDKUHFReader.StartReading();
            if (!result)
            {
                _SDKUHFReader.StopReading();
                _SDKUHFReader.DisConnectDevice();
                return new Dictionary<string, string>();
            }
            DicTags = DicTags.Where(c => !String.IsNullOrEmpty(c.Value)).ToDictionary(s => s.Key, x => x.Value);
            return DicTags;

        }

        public static void iniciaLeitura()
        {
            isReading = true;
            _SDKUHFReader.StartNotagMonitoring(2, true, false, false);
            if (!_SDKUHFReader.StartReading())
            {
                Console.WriteLine($"Não foi possivel conectar ao leitor!\n\n {IPConector} \n\n {_SDKUHFReader.LastError.Data}");
            }
        }
        #endregion

        #region Evento Balança

        public static void InstanceScale()
        {
            
            _BALANCA = machineconfig.Balanca;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _MyScale = new SDKScale($@"./config/{_BALANCA}", $@"config/{_BALANCA}", false);
            }
            else
            {
                _MyScale = new SDKScale($@".\config\{_BALANCA}", $@"config\{_BALANCA}", false);
            }
            _MyScale.OnWeigthGain += _MyScale_OnWeigthGain;
            _MyScale.OnWeigthLoss += _MyScale_OnWeigthLoss;
            _MyScale.OnStableWeigth += _MyScale_OnStableWeigth;
            _MyScale.OnNoWeigth += _MyScale_OnNoWeigth;
            Console.WriteLine("Conectando Balança...");
        
            if (!_MyScale.ConnectDevice())
            {
                _IFileManager.GravarLinhaNoArquivo(PATHARQUIVOLOGPARAENVIO, _LOGSENDNAME, $"[{DateTime.Now}] => [Balança] ERRO: {_MyScale.LastError}");

                Console.WriteLine("Erro ao conectar, consulte o log");
                _MyScale.DisConnectDevice();
            }
            else
            {
                Console.WriteLine("Conectado");
            }
        }

        /// <summary>
        /// Evento acionado quando perde peso
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="Data"></param>
        public static void _MyScale_OnWeigthLoss(object Sender, SoLinkLib.Scale.SCALEData Data)
        {
            try
            {
               
                Console.WriteLine($@"Evento acionado quando perde peso: {Data.NetWeight.ToString()}");
               
            }
            catch (Exception ex)
            {
                //Do Nothing
            }
        }

        /// <summary>
        /// Evento acionado quando ganha peso.
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="Data"></param>
        public static void _MyScale_OnWeigthGain(object Sender, SoLinkLib.Scale.SCALEData Data)
        {
            try
            {
                Console.WriteLine($@"Evento acionado quando ganha peso: {Data.NetWeight.ToString()}");

           
            }
            catch (Exception ex)
            {
                //Do Nothing
            }
        }

        /// <summary>
        /// Evento acionado quando não tem peso.
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="Data"></param>
        public static void _MyScale_OnNoWeigth(object Sender, SoLinkLib.Scale.SCALEData Data)
        {
            try
            {
                Console.WriteLine($@"Evento acionado quando não tem peso: {Data.NetWeight.ToString()}");

            
            }
            catch (Exception ex)
            {
                //Do Nothing
            }
        }

        /// <summary>
        /// Evento acionado quando o peso é estabilizado.
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="Data"></param>
        public static void _MyScale_OnStableWeigth(object Sender, SoLinkLib.Scale.SCALEData Data)
        {
            try
            {
                peso = Data.NetWeight.ToString().Replace(',', '.');
                Console.WriteLine($@"Evento acionado quando o peso é estabilizado: {Data.NetWeight.ToString()}");
            }
            catch (Exception ex)
            {
                //Do Nothing
            }
        }

        #endregion
    }
}
