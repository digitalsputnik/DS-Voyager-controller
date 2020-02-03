using System.Collections.Generic;
using VoyagerApp.Lamps;
using VoyagerApp.Networking.Voyager;
using VoyagerApp.Utilities;

namespace VoyagerApp.Commands
{
    public class CommandQueue
    {
        List<ICommand> _history = new List<ICommand>();

        public void Enqueue(ICommand command)
        {
            command.Execute();
            _history.Add(command);
        }
    }

    public class SendPacketCommand : ICommand
    {
        Packet _send;
        Packet _undo;
        int _port;
        Lamp _destination;

        public SendPacketCommand(Packet send, Packet undo, int port, Lamp destination)
        {
            _send = send;
            _undo = undo;
            _port = port;
            _destination = destination;
        }

        public void Execute()
        {
            NetUtils.VoyagerClient.SendPacket(_destination, _send, _port);
        }

        public void Reverse()
        {
            NetUtils.VoyagerClient.SendPacket(_destination, _undo, _port);
        }
    }
}