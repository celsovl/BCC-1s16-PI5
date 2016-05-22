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
        private Aviao aviao;
        private Canhao canhao;
        private byte[] pacote;
        private Vetor posicaoAlvo = new Vetor(50000, 50000, 0);
        private Vetor posicaoCanhao = new Vetor(75000, 75000, 0);
        private Stopwatch sw;
        private bool alvoDestruido;
        private bool aviaoAbatido;
        private bool emLoop;
        private Socket cliente = null;
        private Thread threadIO;

        static void Main(string[] args)
        {
            bool executar = true;

            while (executar)
            {
                Program p = new Program();
                p.Rodar();

                bool ok = false;
                while (!ok)
                {
                    Console.Write("\nExecutar novamente (S/n)");
                    var read = Console.ReadKey();
                    if (read.Key == ConsoleKey.S || read.Key == ConsoleKey.Enter)
                        ok = true;
                    else if (read.Key == ConsoleKey.N)
                    {
                        ok = true;
                        executar = false;
                    }
                }

                Console.WriteLine();
            }
        }

        private void Rodar()
        {
            IniciarThreadIO();
            aviao = new Aviao();
            aviao.Iniciar();

            canhao = new Canhao(posicaoCanhao);
            canhao.Iniciar();

            sw = Stopwatch.StartNew();
            LoopRadar();
        }

        private void LoopRadar()
        {
            double tempo = sw.Elapsed.TotalSeconds;
            Vetor posicaoAviao = aviao.PosicaoEm(tempo);

            while (!alvoDestruido && !aviaoAbatido && (posicaoAlvo - posicaoAviao).Mag() < 20000)
            {
                double distanciaAviaoAlvo = (posicaoAviao - posicaoAlvo).Mag();
                if (distanciaAviaoAlvo < 10000)
                {
                    EnviarParaCliente(ObterPacotePosicaoAviao(tempo, posicaoAviao));
                    Console.WriteLine("{0}: {1}", tempo, posicaoAviao);

                    for (int i = 0; i < canhao.Tiros.Length && canhao.Tiros[i] != null; i++)
                    {
                        if (canhao.Tiros[i].Viajando(tempo))
                        {
                            Vetor posicaoTiro = canhao.Tiros[i].PosicaoEm(tempo);
                            Console.WriteLine("  {0}: {1} {2}", i, posicaoTiro, (posicaoAviao - posicaoTiro).Mag());
                        }
                    }
                }

                if (distanciaAviaoAlvo < 1000)
                {
                    alvoDestruido = true;
                    Console.WriteLine("ALVO DESTRUIDO");
                    
                    EnviarParaCliente(new Pacote(
                        TipoPacote.AlvoDestruido, 
                        new PacotePosicao(),
                        new PacoteTiro()).ToBytes());
                }
                else
                {
                    Thread.Sleep(1000 / 4);
                }

                tempo = sw.Elapsed.TotalSeconds;
                posicaoAviao = aviao.PosicaoEm(tempo);
            }

            emLoop = false;
        }

        private void EnviarParaCliente(byte[] buf)
        {
            if (cliente != null && cliente.Connected)
            {
                try
                {
                    cliente.Send(buf);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void IniciarThreadIO()
        {
            emLoop = true;
            threadIO = new Thread(LoopIO);
            threadIO.IsBackground = true;
            threadIO.Start();
        }

        private void LoopIO(object obj)
        {
            TcpListener listerner = new TcpListener(new IPEndPoint(IPAddress.Any, porta));
            listerner.Start();

            while (emLoop)
            {
                if (cliente != null && cliente.Connected)
                {
                    if (cliente.Available >= Pacote.Tamanho)
                    {
                        byte[] buf = new byte[Pacote.Tamanho];
                        cliente.Receive(buf);
                        Pacote pacote = Pacote.FromBytes(buf);

                        Console.WriteLine(
                            "Tiro dado: {0:f2}, {1:f2}",
                            pacote.Tiro.AnguloAzimute * 180 / Math.PI,
                            pacote.Tiro.AnguloElevacao * 180 / Math.PI);

                        canhao.Atirar(
                            sw.Elapsed.TotalSeconds,
                            pacote.Tiro.AnguloAzimute,
                            pacote.Tiro.AnguloElevacao);
                    }
                }
                else
                {
                    if (listerner.Pending())
                    {
                        cliente = listerner.AcceptSocket();
                        cliente.NoDelay = true;
                    }
                }

                Thread.Sleep(1000/4);
            }

            listerner.Stop();

            if (cliente != null && cliente.Connected)
                cliente.Close();
        }

        private byte[] ObterPacotePosicaoAviao(double tempo, Vetor posicao)
        {
            return new Pacote(TipoPacote.Posicao,
                new PacotePosicao(
                    tempo,
                    posicao.X,
                    posicao.Y,
                    posicao.Z),
                new PacoteTiro())
                .ToBytes();
        }
    }
}
