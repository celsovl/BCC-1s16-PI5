using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Comum;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Radar
{
    class Program : GameWindow
    {
        const int porta = 10800;
        const double fps = 60.0f;
        private double tempoAnterior;
        private int curretFrame;
        private Aviao aviao;
        private Canhao canhao;
        private Vetor posicaoAlvo = new Vetor(50000, 50000, 0);
        private Vetor posicaoCanhao = new Vetor(55000, 55000, 0);
        private Vetor posicaoAviao;
        private Vetor posicaoAviaoAnterior;
        private Stopwatch sw;
        private bool alvoDestruido;
        private bool aviaoAbatido;
        private bool emLoop;
        private Socket cliente = null;
        private Thread threadIO;

        private Model3D modeloAlvo;
        private Model3D modeloAviao;
        private Model3D modeloTiro;
        private Model3D modeloChao;
        private Model3D modeloCanhao;

        const float aceleracao = 10/180f;
        OpenTK.Input.Key tecla;
        DateTime? tempoInicio;
        float angle;
        float angle2;
        float zoom = 500f;

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            using (Program program = new Program())
            {
                program.Title = "Radar";
                program.Run(fps, 0.0);
            }
        }

        protected override void OnUnload(EventArgs e)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(Color.LightBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.ColorMaterial);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.Normalize);
            GL.Enable(EnableCap.Texture2D);

            GL.ShadeModel(ShadingModel.Smooth);
            GL.Light(LightName.Light0, LightParameter.Position, new float[] { (float)posicaoAlvo.X-50000, (float)posicaoAlvo.Y, 1000 });

            GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { 0.2f, 0.2f, 0.0f, 1.0f });
            GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
            GL.Light(LightName.Light0, LightParameter.Specular, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });

            GL.Material(MaterialFace.Front, MaterialParameter.Specular, new float[] { 1, 1, 1, 1 });
            GL.Material(MaterialFace.Front, MaterialParameter.Shininess, new float[] { 1000 });
            GL.ColorMaterial(MaterialFace.Front, ColorMaterialParameter.AmbientAndDiffuse);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            modeloAlvo = Model3D.FromFile("Castle.obj");
            modeloAlvo.Texture = new Model3DTexture2D("tower.jpg");
            modeloAlvo.Translate = new float[] { (float)posicaoAlvo.X, (float)posicaoAlvo.Y, (float)posicaoAlvo.Z + 1 };
            modeloAlvo.Scale = new float[] { 1000, 1000, 1000 };

            modeloAviao = Model3D.FromFile("Airplane HORNET.obj");
            modeloAviao.Texture = new Model3DTexture2D("aviao2.jpg");
            modeloAviao.Scale = new float[] { 100, 100, 100 };

            modeloTiro = Model3D.FromFile("ball.obj");
            modeloTiro.Texture = new Model3DTexture2D("balacanhao.jpg");
            modeloTiro.Scale = new float[] { 50, 50, 50 };

            modeloChao = Model3D.FromFile("mountain.obj");
            modeloChao.Texture = new Model3DTexture2D("ground_grass.jpg");
            modeloChao.Texture.Scale = new float[] { 20, 20 };
            modeloChao.Scale = new float[] { 100000, 100000, 100 };
            modeloChao.Translate = new float[] { (float)posicaoAlvo.X, (float)posicaoAlvo.Y, 0 };

            modeloCanhao = Model3D.FromFile("Tower.obj");
            modeloCanhao.Texture = new Model3DTexture2D("tower.jpg");
            modeloCanhao.Translate = new float[] { (float)posicaoCanhao.X, (float)posicaoCanhao.Y, (float)posicaoCanhao.Z + 1 };
            modeloCanhao.Scale = new float[] { 500, 500, 500 };

            Iniciar();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Width, Height);

            double aspect_ratio = Width / (double)Height;

            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver6, (float)aspect_ratio, 1, int.MaxValue);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perspective);
        }

        protected override void OnKeyDown(OpenTK.Input.KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!tempoInicio.HasValue)
            {
                tempoInicio = DateTime.Now;
                tecla = e.Key;
            }
        }

        protected override void OnKeyUp(OpenTK.Input.KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);
            
            if (e.Key == tecla)
                tempoInicio = null;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            LoopRadar();

            float tempo = (float)(DateTime.Now - (tempoInicio.HasValue ? tempoInicio : DateTime.Now).Value).TotalSeconds;

            var keyboard = OpenTK.Input.Keyboard.GetState();
            if (keyboard[OpenTK.Input.Key.Escape])
            {
                this.Exit();
            }
            else if (keyboard[OpenTK.Input.Key.Right])
            {
                angle += 1/180f + aceleracao * tempo;
            }
            else if (keyboard[OpenTK.Input.Key.Left])
            {
                angle -= 1/180f + aceleracao * tempo;
            }
            else if (keyboard[OpenTK.Input.Key.Up] && keyboard[OpenTK.Input.Key.ControlLeft])
            {
                zoom *= 1.1f;
            }
            else if (keyboard[OpenTK.Input.Key.Down] && keyboard[OpenTK.Input.Key.ControlLeft])
            {
                zoom /= 1.1f;
            }
            else if (keyboard[OpenTK.Input.Key.Up])
            {
                angle2 += 1/180f + aceleracao * tempo;
            }
            else if (keyboard[OpenTK.Input.Key.Down])
            {
                angle2 -= 1/180f + aceleracao * tempo;
            }

            curretFrame++;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);

            Matrix4 lookat = Matrix4.LookAt(
                (float)(posicaoAviao.X + zoom * Math.Cos(angle2) * Math.Cos(angle)),
                (float)(posicaoAviao.Y + zoom * Math.Cos(angle2) * Math.Sin(angle)), 
                (float)(posicaoAviao.Z + zoom * Math.Sin(angle2)),
                (float)posicaoAviao.X,
                (float)posicaoAviao.Y,
                (float)posicaoAviao.Z,
                0, 0, 1);
            GL.LoadMatrix(ref lookat);

            DrawBackground();
            DrawScene();

            this.SwapBuffers();
        }

        private void DrawBackground()
        {
        }

        private void DrawScene()
        {
            DrawChao();
            DrawAlvo();
            DrawCanhao();
            DrawAviao();
            DrawTiros();
        }

        private void DrawTiros()
        {
            for (int i = 0; i < canhao.Tiros.Length - canhao.TirosRestantes; i++)
            {
                if (canhao.Tiros[i].Viajando(sw.Elapsed.TotalSeconds))
                {
                    var posicao = canhao.Tiros[i].PosicaoEm(sw.Elapsed.TotalSeconds);
                    modeloTiro.Translate = new float[] { (float)posicao.X, (float)posicao.Y, (float)posicao.Z };
                    modeloTiro.Draw();
                }
            }
        }

        private void DrawAviao()
        {
            Vetor velocidade = aviao.Trajetoria.Velociade;
            double magXY = Math.Sqrt(velocidade.X * velocidade.X + velocidade.Y * velocidade.Y);

            double anguloAzimute = Math.Acos(velocidade.X / magXY);
            if (velocidade.Y < 0)
                anguloAzimute *= -1;

            modeloAviao.Translate = new float[] { (float)posicaoAviao.X, (float)posicaoAviao.Y, (float)posicaoAviao.Z };
            modeloAviao.Rotate = new float[] { 0, 0, (float)(90 + anguloAzimute * 180 / Math.PI) };
            modeloAviao.Draw();
        }

        private void DrawCanhao()
        {
            modeloCanhao.Draw();
        }

        private void DrawAlvo()
        {
            modeloAlvo.Draw();
        }

        private void DrawChao()
        {
            modeloChao.Draw();
        }

        private void Iniciar()
        {
            IniciarThreadIO();
            aviao = new Aviao();
            aviao.Iniciar();

            canhao = new Canhao(posicaoCanhao);
            canhao.Iniciar();

            sw = Stopwatch.StartNew();
        }

        private void LoopRadar()
        {
            double tempo = sw.Elapsed.TotalSeconds;
            posicaoAviao = aviao.PosicaoEm(tempo);

            if (!alvoDestruido && !aviaoAbatido && (posicaoAlvo - posicaoAviao).Mag() < 20000)
            {
                for (int i = 0; i < canhao.Tiros.Length && canhao.Tiros[i] != null; i++)
                {
                    if (canhao.Tiros[i].Viajando(tempo) || canhao.Tiros[i].Viajando(tempoAnterior))
                    {
                        if (AbateuAviao2(i, tempo, canhao.Tiros[i]))
                        {
                            aviaoAbatido = true;
                            Console.WriteLine("AVIAO ABATIDO");

                            EnviarParaCliente(new Pacote(TipoPacote.AviaoAbatido));

                            sw.Stop();
                            emLoop = false;
                        }

                    }
                }

                double distanciaAviaoAlvo = (posicaoAviao - posicaoAlvo).Mag();

                if (distanciaAviaoAlvo < 10000 && curretFrame % ((int)fps / 4) == 0)
                {
                    EnviarParaCliente(ObterPacotePosicaoAviao(tempo, posicaoAviao));
                    Console.WriteLine("{0}: {1}", tempo, posicaoAviao);
                }

                if (distanciaAviaoAlvo < 1000 && curretFrame % ((int)fps / 4) == 0)
                {
                    alvoDestruido = true;
                    Console.WriteLine("ALVO DESTRUIDO");
                    
                    EnviarParaCliente(new Pacote(TipoPacote.AlvoDestruido));

                    emLoop = false;
                    sw.Stop();
                }

                tempo = sw.Elapsed.TotalSeconds;
                posicaoAviao = aviao.PosicaoEm(tempo);
            }

            tempoAnterior = tempo;
            posicaoAviaoAnterior = posicaoAviao;
        }

        private bool AbateuAviao(int i, double tempo, Tiro tiro)
        {
            Vetor posicaoTiro = tiro.PosicaoEm(tempo);
            Console.WriteLine("  {0}: {1} {2}", i, posicaoTiro, (posicaoAviao - posicaoTiro).Mag());

            return (tiro.PosicaoEm(tempo) - posicaoAviao).Mag() < 7;
        }

        private bool AbateuAviao2(int i, double tempo, Tiro tiro)
        {
            Vetor posicaoTiro = tiro.PosicaoEm(tempo);
            Vetor posicaoTiroAnterior = tiro.PosicaoEm(tempoAnterior);

            Console.WriteLine("  {0}: {1} {2}", i, posicaoTiro, (posicaoAviao - posicaoTiro).Mag());

            for (double t = tempoAnterior; t < tempo; t += 3 / Tiro.VELOCIDADEMEDIA)
            {
                if ((aviao.PosicaoEm(t) - tiro.PosicaoEm(t)).Mag() < 2)
                    return true;
            }

            return false;
        }

        private void EnviarParaCliente(Pacote pacote)
        {
            if (cliente != null && cliente.Connected)
            {
                try
                {
                    cliente.Send(pacote.ToBytes());
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

                        if (pacote.Tipo == TipoPacote.Ping)
                        {
                            EnviarParaCliente(
                                new Pacote(
                                    TipoPacote.Pong,
                                    pingPong: new PacotePingPong(pacote.PingPong.Tempo)));
                        }
                        else if (pacote.Tipo == TipoPacote.Tiro)
                        {
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

        private Pacote ObterPacotePosicaoAviao(double tempo, Vetor posicao)
        {
            return new Pacote(TipoPacote.Posicao,
                posicao: new PacotePosicao(
                    tempo,
                    posicao.X,
                    posicao.Y,
                    posicao.Z));
        }
    }
}
