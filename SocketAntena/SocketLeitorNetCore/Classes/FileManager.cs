using SocketLeitorNetCore.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketLeitorNetCore.Classes
{
    public class FileManager : IFileManager
    {

        public FileManager()
        {
        }

        public bool MoverAquivo(string pathOrigem, string nome, string pathDestino)
        {
            string fileOrigem = pathOrigem + nome;

            string data = (DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "" + DateTime.Now.Hour + "" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Millisecond).ToString();

            // @".\BKP\"
            string fileDestino = pathDestino + data + "_" + nome;

            if (VerificaArquivo(pathOrigem, nome, ""))
            {
                File.Move(fileOrigem, @"" + fileDestino);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool VerificaArquivo(string path, string nome, string obj)
        {
            try
            {
                string file = path + nome;

                if (File.Exists(file))
                    return true;

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<string> LerArquivo(string path, string nome)
        {
            string file = path + nome;

            List<string> lista = new List<string>();

            using (StreamReader sr = new StreamReader(file))
            {
                string linha = null;
                //Lê linha por linha até o final do arquivo
                while ((linha = sr.ReadLine()) != null)
                {
                    lista.Add(linha);
                }
            }
            return lista;
        }


        public string LerArquivoInteiro(string path, string nome)
        {
            string file = path + nome;
            string result;
            using (StreamReader sr = new StreamReader(file))
            {
                string linha = null;
                result = sr.ReadToEnd();
            }
            return result;
        }



        public bool CriarArquivo(string path, string nome, string obj)
        {
            string file = path + nome;

            try
            {
                CriaEscreveNoArquivo(file, obj);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool GravarLinhaNoArquivo(string path, string nome, string obj)
        {
            //@".\storage.scanin"
            string file = path + nome;

            try
            {
                CriaEscreveNoArquivo(file, obj);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void CriaEscreveNoArquivo(string path, string obj)
        {
            using (var writer = new StreamWriter(path, true))
            {
                writer.WriteLine(obj);
            }
        }

        public bool SobreescreverArquivo(string path, string nome, string obj)
        {
            string file = path + nome;

            try
            {
                using (var writer = new StreamWriter(file, false))
                {
                    writer.WriteLine(obj);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeletarAquivo(string path, string nome)
        {
            throw new NotImplementedException();
        }
    }
}
