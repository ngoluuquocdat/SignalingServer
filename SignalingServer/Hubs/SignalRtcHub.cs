using Microsoft.AspNetCore.SignalR;
using SignalingServer.Models;
using System.Text.Json;

namespace SignalingServer.Hubs
{
    public class SignalRtcHub : Hub
    {
        private readonly IDictionary<string, string> _users;   // dictionary of usernames (key) - connection IDs (value)

        public SignalRtcHub(IDictionary<string, string> users)
        {
            _users = users;
        }

        // inform the group if one user disconnects
        public override Task OnDisconnectedAsync(Exception exception)
        {
            var username = _users.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if(!String.IsNullOrEmpty(username))
            {
                _users.Remove(username);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task ConnectToSignalRTC(string username, bool isNewlyConnection)
        {
            if(isNewlyConnection)
            {
                if (_users.ContainsKey(username))
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("UsernameExisted");
                    return;
                }
            }
            // add to tracking dictionary
            _users[username] = Context.ConnectionId;

            // add new connection to group
            await Groups.AddToGroupAsync(Context.ConnectionId, username);

            await Clients.Client(Context.ConnectionId).SendAsync("YourID", Context.ConnectionId);
            await Clients.All.SendAsync("AllUsers", _users.Keys);
        }

        public async Task AllUsers()
        {
            await Clients.Caller.SendAsync("AllUsers", _users);
        }

        public async Task CallUser(string toUser, string signal, string fromUser)
        {
            // param fromUser/toUser is a username, or group name
            // param signal is a json string 
            Console.WriteLine("Transfer signaling data: caller to callee");
            await Clients.Groups(toUser).SendAsync("IncomingCall", fromUser, signal);
        }
        public async Task AcceptCall(string caller, string signal)
        {
            // param caller is a username/group name of peer that made the call
            // param signal is a json string 
            Console.WriteLine("Transfer signaling data: callee to caller");
            await Clients.Groups(caller).SendAsync("CallAccepted", signal);
        }
        public async Task ShareScreen(string toUser)
        {
            // param caller is a username/group name of peer that made the call
            // param signal is a json string 
            Console.WriteLine("Transfer signaling data: callee to caller");
            await Clients.Groups(toUser).SendAsync("PartnerSharedScreen");
        }
        public async Task CloseCall(string toUser)
        {
            // param fromUser/toUser is a username/group name
            // param signal is a json string 
            await Clients.Groups(toUser).SendAsync("CloseCall", Context.ConnectionId);
        }
    }
}
