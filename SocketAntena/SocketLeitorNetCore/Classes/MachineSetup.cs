using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketLeitorNetCore.Classes
{
    public class MachineSetup
    {
        public string Filial { get; set; }
        public string Segmento { get; set; }
        public string Ftp { get; set; }
        public string LeitorModelo { get; set; }
        public string LeitorCommDLL { get; set; }
        public string CodigoDaLicenca { get; set; }
        public string Balanca { get; set; }
        public string BalancaComm { get; set; }
        public string TempoLeitura { get; set; }
        public string Modulo { get; set; }
        public string TempoEnvioServidorApiScanIn { get; set; }
        public string CabineUdoor { get; set; }
        public string CabineUdoorFrontDoor { get; set; }
        public string CabineUdoorRearDoor { get; set; }
        public string Chave { get; set; }
        public string QueuServerHostname { get; set; }
        public string DiferencaPeso { get; set; }
        public string valorPorcentagemPeso { get; set; }
        public string MinutosSync { get; set; }
        public string ModoOffline { get; set; }
        public string Contador { get; set; }
        public string NoTagMonitoring { get; set; }
        public string HabilitarSync { get; set; }
        public string[] Segmentos { get; set; }
        public string LerTudo { get; set; }
        public string PararLeitura { get; set; }
        public string TempoAliveDetection { get; set; }
        public string ChaveReaplicacao { get; set; }
        public string FilialReaplicacao { get; set; }
        public string PesoManual { get; set; }
        public string[] Leitores { get; set; }
        public string Leitor { get; set; }
        public string TirarPeso { get; set; }
        public string FilialCNPJ { get; set; }
        public string SegmentoPrincipal { get; set; }
    }
}
