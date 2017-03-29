using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Configuration;
using StructureMap;
using LEAPBot.IoC;
using LEAPBot.Domain.Contracts;
using Microsoft.Bot.Builder.Dialogs;
using LEAPBot.Dialogs;
using Microsoft.Bot.Builder.Luis;

namespace LEAPBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private static Container _container;
        private static readonly LuisService _service;


        static MessagesController()
        {
            _container = new Container(new RuntimeRegistry());
            var settings = _container.GetInstance<ISettingsReader>();

            var appId   = settings["LUIS:AppId"];
            var appKey  = settings["LUIS:AppKey"];
            var model   = new LuisModelAttribute(appId, appKey);
            _service    = new LuisService(model);
        }


        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new LeapDialog(_service));
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }


        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels                
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}