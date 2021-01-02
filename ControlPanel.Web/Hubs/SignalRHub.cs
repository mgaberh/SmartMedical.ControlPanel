using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ControlPanel.Web.Hubs
{
    public class SignalRHub : Hub
    {
        public async Task SendMessage(string message, string status)
        {
            await Clients.All.SendAsync("ReceiveMessage",message, status);
        }
    }
}
