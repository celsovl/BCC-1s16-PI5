using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Radar
{
    class Program
    {
        static void Main(string[] args)
        {
            int porta = 10800;
            string ip = "172.30.177.82";
            string mensagem = "ACK-ENTENDI";
            byte[] pacote = new byte[64];

            TcpClient servidorCanhao = new TcpClient();
            servidorCanhao.Connect(IPAddress.Parse(ip), porta);

            int offset = 0;
            while (offset < pacote.Length)
                offset += servidorCanhao.Client.Receive(pacote, offset, pacote.Length - offset, SocketFlags.None);

            byte[] dados = Encoding.ASCII.GetBytes(mensagem);
            pacote[0] = (byte)dados.Length;
            dados.CopyTo(pacote, 1);
            servidorCanhao.Client.Send(pacote);
            servidorCanhao.Close();
        }
    }
}
