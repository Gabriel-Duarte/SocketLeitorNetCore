using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketLeitorNetCore.Interfaces
{
    public interface IFileManager
    {

        bool VerificaArquivo(string path, string nome, string obj);

        bool CriarArquivo(string path, string nome, string obj);

        bool GravarLinhaNoArquivo(string path, string nome, string obj);

        List<string> LerArquivo(string path, string nome);

        string LerArquivoInteiro(string path, string nome);

        bool DeletarAquivo(string path, string nome);

        bool MoverAquivo(string pathOrigem, string nome, string pathDestino);

        bool SobreescreverArquivo(string path, string nome, string obj);
    }
}
