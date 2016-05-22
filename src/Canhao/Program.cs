using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Comum;

namespace Canhao
{
    class Program
    {
        const int PORTA = 10800;
        private Vetor posicaoAlvo = new Vetor(50000, 50000, 0);
        private Vetor posicaoCanhao = new Vetor(75000, 75000, 0);
        private int tirosDisponiveis = 4;

        private TcpClient cliente;
        private Stopwatch sw;
        private double tempoAnterior;
        private Vetor posicaoAnterior;
        private Vetor velocidadeAnterior;
        private int contadorEstabilidade;
        private double tempoBase = 5;
        private double tempoAtraso = 0.5;

        static void Main(string[] args)
        {
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
                if (pacote.HasValue)
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
                        DispararTiro();
                    }
                    else if (pacote.Value.Tipo == TipoPacote.AlvoDestruido)
                    {
                        Console.WriteLine("ALVO DESTRUIDO");
                        break;
                    }
                    else if (pacote.Value.Tipo == TipoPacote.AviaoAbatido)
                    {
                        Console.WriteLine("Acertamos motherf....r");
                        break;
                    }
                }
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

        private void DispararTiro()
        {
            const double GRAVIDADE = 9.80665;

            if (tirosDisponiveis > 0 && contadorEstabilidade >= 20)
            {
                // projetar posicao aviao
                Vetor posicaoEstimada = posicaoAnterior + velocidadeAnterior * tempoBase;
                Vetor distanciaCanhaoAviao = posicaoEstimada - posicaoCanhao;

                double magXY = Math.Sqrt(distanciaCanhaoAviao.X * distanciaCanhaoAviao.X + distanciaCanhaoAviao.Y * distanciaCanhaoAviao.Y);
                double anguloAzimute = new Vetor(1,0,0).AnguloEntre(distanciaCanhaoAviao);
                if (distanciaCanhaoAviao.X < 0)
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

                if (tempo2 < tempo1)
                {
                    anguloElevacao = anguloElevacao2;
                    tempo = tempo2;
                }

                if (Math.Abs(tempo - tempoBase) > tempoBase * 1.1)
                {
                    tempoBase = tempo + tempoAtraso;
                    Console.WriteLine(
                        "Tempo: {0}, Elevacao: {1}, Azimute: {2}",
                        tempo,
                        anguloElevacao * 180 / Math.PI,
                        anguloAzimute * 180 / Math.PI);
                    return;
                }

                Console.WriteLine(
                    "Atirou => Tempo: {0}, Elevacao: {1}, Azimute: {2}",
                    tempo,
                    anguloElevacao * 180 / Math.PI,
                    anguloAzimute * 180 / Math.PI);

                cliente.Client.Send(
                    new Pacote(
                        TipoPacote.Tiro,
                        new PacotePosicao(),
                        new PacoteTiro(anguloAzimute, anguloElevacao))
                    .ToBytes());

                tirosDisponiveis--;
                contadorEstabilidade = 0;
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

        private void ConectarSeAoRadar(string host)
        {
            cliente = new TcpClient();
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
