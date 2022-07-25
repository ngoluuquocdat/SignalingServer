using Microsoft.AspNetCore.SignalR;
using SignalingServer.Models;
using System.Text.Json;

namespace SignalingServer.Hubs
{
    public class SignalRtcHub : Hub
    {
        private readonly List<string> _users;   // list of connection IDs

        public SignalRtcHub(List<string> users)
        {
            _users = users;
        }

        // invoked when new user comes
        public async Task NewUser(string username)
        {
            var userInfo = new UserInfo() { UserName = username, ConnectionId = Context.ConnectionId };
            await Clients.Others.SendAsync("NewUserArrived", JsonSerializer.Serialize(userInfo));
        }

        // existed users use this to inform their existence to the new user
        // param newUser is the connectionId of new user
        public async Task HelloUser(string userName, string newUser)
        {
            var userInfo = new UserInfo() { UserName = userName, ConnectionId = Context.ConnectionId };
            await Clients.Client(newUser).SendAsync("UserSaidHello", JsonSerializer.Serialize(userInfo));
        }

        // used to send signal for WebRTC p2p system
        // param user is the connectionId
        public async Task SendSignal(string signal, string user)
        {
            await Clients.Client(user).SendAsync("SendSignal", Context.ConnectionId, signal);
        }

        public override async Task OnConnectedAsync()
        {
            if(!_users.Contains(Context.ConnectionId))
            {
                _users.Add(Context.ConnectionId);
            }
            await base.OnConnectedAsync();
        }

        // inform the group if one user disconnects
        public override Task OnDisconnectedAsync(Exception exception)
        {
            _users.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task YourID()
        {
            await Clients.Client(Context.ConnectionId).SendAsync("YourId", Context.ConnectionId);
        }

        public async Task AllUsers()
        {
            await Clients.Client(Context.ConnectionId).SendAsync("AllUsers", _users);
        }

        public async Task CallUser(string toUser, string signal, string fromUser)
        {
            // param fromUser/toUser is a connection Id
            // param signal is a json string 
            await Clients.Client(toUser).SendAsync("Hey", fromUser, signal);
        }
        public async Task AcceptCall(string toUser, string signal)
        {
            // param fromUser/toUser is a connection Id
            // param signal is a json string 
            await Clients.Client(toUser).SendAsync("CallAccepted", signal);
        }
    }
}
