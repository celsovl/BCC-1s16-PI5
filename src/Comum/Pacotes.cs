using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Comum
{
    public enum TipoPacote : byte
    {
        Posicao = 1,
        Tiro,
        AlvoDestruido,
        AviaoAbatido
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Pacote
    {
        public static int Tamanho { get { return Marshal.SizeOf(typeof(Pacote)); } }
        
        public TipoPacote Tipo;
        public PacotePosicao Posicao;
        public PacoteTiro Tiro;

        public Pacote(TipoPacote tipo, PacotePosicao posicao, PacoteTiro tiro)
        {
            Tipo = tipo;
            Posicao = posicao;
            Tiro = tiro;
        }

        public byte[] ToBytes()
        {
            byte[] buf = new byte[Tamanho];
            IntPtr ptr = Marshal.AllocHGlobal(Tamanho);

            try
            {
                Marshal.StructureToPtr(this, ptr, false);
                Marshal.Copy(ptr, buf, 0, buf.Length);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return buf;
        }

        public static Pacote FromBytes(byte[] buf)
        {
            Pacote p;
            GCHandle handle = GCHandle.Alloc(buf, GCHandleType.Pinned);

            try
            {
                p = (Pacote)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Pacote));
            }
            finally
            {
                handle.Free();
            }

            return p;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PacotePosicao
    {
        public PacotePosicao(double tempo, double x, double y, double z)
        {
            Tempo = tempo;
            X = x;
            Y = y;
            Z = z;
        }

        public double Tempo;
        public double X;
        public double Y;
        public double Z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PacoteTiro
    {
        public PacoteTiro(double anguloAzimute, double anguloElevacao)
        {
            AnguloAzimute = anguloAzimute;
            AnguloElevacao = anguloElevacao;
        }

        public double AnguloAzimute;
        public double AnguloElevacao;
    }
}
