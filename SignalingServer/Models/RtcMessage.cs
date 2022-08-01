namespace SignalingServer.Models
{
    public class RtcMessage
    {
        public string From { get; set; }
        public string Target { get; set; }
        public string Type { get; set; }
        public string SDP { get; set; }
    }
}
