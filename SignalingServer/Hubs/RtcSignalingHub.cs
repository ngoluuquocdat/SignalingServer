using Microsoft.AspNetCore.SignalR;
using SignalingServer.Models;

namespace SignalingServer.Hubs
{
    public class RtcSignalingHub : Hub
    {
        private readonly IDictionary<string, string> _users;   // dictionary of usernames (key) - connection IDs (value)

        public RtcSignalingHub(IDictionary<string, string> users)
        {
            _users = users;
        }

        // inform the group if one user disconnects
        public override Task OnDisconnectedAsync(Exception exception)
        {
            var username = _users.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (!String.IsNullOrEmpty(username))
            {
                _users.Remove(username);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task ConnectToSignalRTC(string username, bool isNewlyConnection)
        {
            if (isNewlyConnection)
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

        public async Task SendOffer(RtcMessage rtcMessage)
        {
            // param fromUser/toUser is a username, or group name
            // param signal is a json string 
            Console.WriteLine("Transfer offer data: caller to callee");
            await Clients.Groups(rtcMessage.Target).SendAsync("ReceiveOffer", rtcMessage.From, rtcMessage.SDP);
        }

        public async Task SendAnswer(RtcMessage rtcMessage)
        {
            // param fromUser/toUser is a username, or group name
            // param signal is a json string 
            Console.WriteLine("Transfer answer data: callee to caller");
            await Clients.Groups(rtcMessage.Target).SendAsync("ReceiveAnswer", rtcMessage.From, rtcMessage.SDP);
        }

        public async Task SendICECandidate(RtcMessage rtcMessage)
        {
            // param fromUser/toUser is a username, or group name
            // param signal is a json string 
            Console.WriteLine("Transfer answer data: callee to caller");
            await Clients.Groups(rtcMessage.Target).SendAsync("ReceiveICECandidate", rtcMessage.From, rtcMessage.SDP);
        }
        public async Task CloseCall(string toUser)
        {
            // param fromUser/toUser is a username/group name
            // param signal is a json string 
            await Clients.Groups(toUser).SendAsync("CloseCall", Context.ConnectionId);
        }
    }
}
