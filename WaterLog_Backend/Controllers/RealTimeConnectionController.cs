using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterLog_Backend.Models;

namespace WaterLog_Backend.Controllers
{
    public class RealTimeConnectionController : Hub
    {
        public async Task SendNewAlert(GetAlerts newAlert)
        {
            await Clients.All.SendAsync(JsonConvert.SerializeObject(newAlert));
        }
    }
}
