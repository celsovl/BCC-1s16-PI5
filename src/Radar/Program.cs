using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Comum;

namespace Radar
{
    class Program
    {
        const int porta = 10800;
        static Aviao aviao;
        static Canhao canhao;
        static byte[] pacote;
        static Vetor alvo = new Vetor(50000, 50000, 0);
        static Stopwatch sw;
        static bool alvoDestruido;
        static bool aviaoAbatido;
        static bool emLoop;
        static Socket cliente = null;
        static Thread threadIO;

        static void Main(string[] args)
        {
            IniciarThreadIO();

            aviao = new Aviao();
            aviao.Iniciar();

            canhao = new Canhao();
            canhao.Iniciar();

            sw = Stopwatch.StartNew();
            LoopRadar();
        }

        private static void LoopRadar()
        {
            double tempo = sw.Elapsed.TotalSeconds;
            Vetor posicaoAviao = aviao.PosicaoEm(tempo);

            while (!alvoDestruido && !aviaoAbatido && canhao.TirosRestantes > 0 && (alvo - posicaoAviao).Mag() < 20000)
            {
                if ((posicaoAviao - alvo).Mag() < 10000)
                {
                    if (cliente != null && cliente.Connected)
                        cliente.Send(ObterPacotePosicaoAviao(tempo, posicaoAviao));

                    Console.WriteLine("{0}: {1}", tempo, posicaoAviao);
                }

                Thread.Sleep(100);

                tempo = sw.Elapsed.TotalSeconds;
                posicaoAviao = aviao.PosicaoEm(tempo);
            }

            if (cliente != null && cliente.Connected)
                cliente.Close();
            emLoop = false;
        }

        private static void IniciarThreadIO()
        {
            emLoop = true;
            threadIO = new Thread(LoopIO);
            threadIO.IsBackground = true;
            threadIO.Start();
        }

        private static void LoopIO(object obj)
        {
            while (emLoop)
            {
                if (cliente != null && cliente.Connected)
                {
                    //TODO: Verificar se cliente.Available >= pacote para ler pacote
                }
                else
                {
                    TcpListener listerner = new TcpListener(new IPEndPoint(IPAddress.Any, porta));
                    listerner.Start();

                    cliente = listerner.AcceptSocket();
                    cliente.NoDelay = true;
                    listerner.Stop();
                }

                Thread.Sleep(100);
            }
        }

        private static byte[] ObterPacotePosicaoAviao(double tempo, Vetor posicao)
        {
            byte[] buf = new byte[8 * 4];
            BitConverter.GetBytes(tempo).CopyTo(buf, 0);
            BitConverter.GetBytes(posicao.X).CopyTo(buf, 8);
            BitConverter.GetBytes(posicao.Y).CopyTo(buf, 16);
            BitConverter.GetBytes(posicao.Z).CopyTo(buf, 24);

            return buf;
        }
    }
}
