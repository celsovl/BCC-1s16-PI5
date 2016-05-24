using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Comum;

namespace Canhao
{
    class Program
    {
        const int PORTA = 10800;
        const double tax = 1/10.0;
        private Vetor posicaoAlvo = new Vetor(50000, 50000, 0);
        private Vetor posicaoCanhao = new Vetor(55000, 55000, 0);
        private int tirosDisponiveis = 4;

        private TcpClient cliente;
        private Stopwatch sw;
        private double tempoAnterior;
        private Vetor posicaoAnterior;
        private Vetor velocidadeAnterior;
        private int contadorEstabilidade;
        private int contadorIteracoes;
        private double tempoMedioThroughput;
        private double tempoBase = 5;
        private double tempoAtraso = 0.0;
        private double dispararEm = -1;
        private Pacote pacoteDisparo;

        static void Main(string[] args)
        {
            if (args.Length == 0)
                args = new string[] { "localhost" };

            Program p = new Program();
            p.Rodar(args[0]);
        }

        private void Rodar(string host)
        {
            ConectarSeAoRadar(host);
            sw = Stopwatch.StartNew();

            while (EstaConectado())
            {
                Pacote? pacote = ReceberPacote();
                while (pacote.HasValue)
                {
                    if (pacote.Value.Tipo == TipoPacote.Posicao)
                    {
                        Vetor posicao = new Vetor(
                               pacote.Value.Posicao.X,
                               pacote.Value.Posicao.Y,
                               pacote.Value.Posicao.Z);

                        Console.WriteLine("{0} {1} -> {2}",
                            pacote.Value.Posicao.Tempo,
                            posicao,
                            sw.Elapsed.TotalSeconds);

                        CalcularTrajetoria(pacote.Value.Posicao.Tempo, posicao);
                    }
                    else if (pacote.Value.Tipo == TipoPacote.AlvoDestruido)
                    {
                        Console.WriteLine("ALVO DESTRUIDO");
                        return;
                    }
                    else if (pacote.Value.Tipo == TipoPacote.AviaoAbatido)
                    {
                        Console.WriteLine("Acertamos motherf....r");
                        return;
                    }
                    else if (pacote.Value.Tipo == TipoPacote.Pong)
                    {
                        double dif = sw.Elapsed.TotalSeconds - pacote.Value.PingPong.Tempo;
                        if (tempoMedioThroughput == 0)
                            tempoMedioThroughput = dif;
                        else
                            tempoMedioThroughput = tax * dif + (1 - tax) * tempoMedioThroughput;

                        Console.WriteLine("Throughput: {0}", tempoMedioThroughput);
                    }

                    pacote = ReceberPacote();
                }

                CalcularTiro();
                DispararTiro();
                
                contadorIteracoes++;
                if (contadorIteracoes % 30 == 0)
                {
                    EnviarParaCliente(
                        new Pacote(
                            TipoPacote.Ping, 
                            pingPong: new PacotePingPong(sw.Elapsed.TotalSeconds)));
                }

                Thread.Sleep(100 / 6);
            }
        }

        private void DispararTiro()
        {
            if (dispararEm > -1 && sw.Elapsed.TotalSeconds > dispararEm)
            {
                EnviarParaCliente(pacoteDisparo);

                Console.WriteLine(
                    "Atirou => Tempo: {0}, Elevacao: {1}, Azimute: {2}",
                    sw.Elapsed.TotalSeconds,
                    pacoteDisparo.Tiro.AnguloElevacao * 180 / Math.PI,
                    pacoteDisparo.Tiro.AnguloAzimute * 180 / Math.PI);

                tirosDisponiveis--;
                contadorEstabilidade = 0;
                dispararEm = -1;
            }
        }

        private void CalcularTrajetoria(double tempo, Vetor posicao)
        {
            var dif = (posicao - posicaoAnterior) / (tempo - tempoAnterior);

            if (Math.Abs((velocidadeAnterior - dif).Mag()) < 1e-10)
                contadorEstabilidade++;
            else
                contadorEstabilidade = 0;

            tempoAnterior = tempo;
            posicaoAnterior = posicao;
            velocidadeAnterior = dif;
        }

        private void CalcularTiro()
        {
            const double GRAVIDADE = 9.80665;

            if (tirosDisponiveis > 0 && contadorEstabilidade >= 30 && dispararEm == -1)
            {
                // projetar posicao aviao
                Vetor posicaoEstimada = posicaoAnterior + velocidadeAnterior * tempoBase;
                Vetor distanciaCanhaoAviao = posicaoEstimada - posicaoCanhao;

                double magXY = Math.Sqrt(distanciaCanhaoAviao.X * distanciaCanhaoAviao.X + distanciaCanhaoAviao.Y * distanciaCanhaoAviao.Y);

                double anguloAzimute = Math.Acos(distanciaCanhaoAviao.X / magXY);
                if (distanciaCanhaoAviao.Y < 0)
                    anguloAzimute *= -1;

                double v2 = Tiro.VELOCIDADEMEDIA * Tiro.VELOCIDADEMEDIA;
                double anguloElevacao1 = Math.Atan(
                    (v2 + Math.Sqrt(v2 * v2 - GRAVIDADE * (GRAVIDADE * magXY * magXY + 2 * distanciaCanhaoAviao.Z * v2))) / (GRAVIDADE * magXY)
                    );
                double tempo1 = magXY / (Math.Cos(anguloElevacao1) * Tiro.VELOCIDADEMEDIA);

                double anguloElevacao2 = Math.Atan(
                    (v2 - Math.Sqrt(v2 * v2 - GRAVIDADE * (GRAVIDADE * magXY * magXY + 2 * distanciaCanhaoAviao.Z * v2))) / (GRAVIDADE * magXY)
                    );
                double tempo2 = magXY / (Math.Cos(anguloElevacao2) * Tiro.VELOCIDADEMEDIA);

                double anguloElevacao = anguloElevacao1;
                double tempo = tempo1;

                Console.WriteLine(Tiro.VELOCIDADEMEDIA * Math.Sin(anguloElevacao1) * tempo1 - GRAVIDADE * tempo1 * tempo1 / 2);
                Console.WriteLine(Tiro.VELOCIDADEMEDIA * Math.Sin(anguloElevacao2) * tempo2 - GRAVIDADE * tempo2 * tempo2 / 2);

                if (tempo2 < tempo1)
                {
                    anguloElevacao = anguloElevacao2;
                    tempo = tempo2;
                }

                if (tempo > tempoBase)
                {
                    tempoBase = tempo + 2;
                    Console.WriteLine(
                        "Estimado Ruim Tempo: {0}, Elevacao: {1}, Azimute: {2}",
                        tempo,
                        anguloElevacao * 180 / Math.PI,
                        anguloAzimute * 180 / Math.PI);

                    return;
                }

                contadorEstabilidade = 0;

                Console.WriteLine(new Tiro(posicaoCanhao, anguloAzimute, anguloElevacao, 0).PosicaoEm(tempo));
                Console.WriteLine(posicaoEstimada);
                Console.WriteLine((new Tiro(posicaoCanhao, anguloAzimute, anguloElevacao, 0).PosicaoEm(tempo) - posicaoEstimada).Mag());

                Debug.Assert((new Tiro(posicaoCanhao, anguloAzimute, anguloElevacao, 0).PosicaoEm(tempo) - posicaoEstimada).Mag() < 1e-10);

                dispararEm = sw.Elapsed.TotalSeconds + (tempoBase - tempo - tempoMedioThroughput/2);
                pacoteDisparo = new Pacote(
                    TipoPacote.Tiro,
                    tiro: new PacoteTiro(
                        anguloAzimute,
                        anguloElevacao));
            }
        }

        private Pacote? ReceberPacote()
        {
            if (cliente.Available >= Pacote.Tamanho)
            {
                byte[] buf = new byte[Pacote.Tamanho];
                cliente.Client.Receive(buf);
                return Pacote.FromBytes(buf);
            }

            return null;
        }

        private bool EstaConectado()
        {
            return cliente != null && cliente.Connected;
        }

        private void EnviarParaCliente(Pacote pacote)
        {
            if (cliente != null && cliente.Connected)
            {
                try
                {
                    cliente.Client.Send(pacote.ToBytes());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void ConectarSeAoRadar(string host)
        {
            cliente = new TcpClient();
            cliente.NoDelay = true;
            while (!cliente.Connected)
            {
                try
                {
                    Console.Write("Tentando conexao em {0}:{1} -> ", host, PORTA);
                    cliente.Connect(host, PORTA);
                    Console.WriteLine("OK");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
