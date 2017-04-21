using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Threading.Tasks;

namespace LEAPBot.Dialogs
{
    [Serializable]
    public class LeapDialog : LuisDialog<object>
    {
        public LeapDialog(LuisService service)
            : base(service)
        {
        }


        protected async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            await base.MessageReceived(context, item);
        }


        [LuisIntent("SendingColleagues")]
        public async Task SendingColleagues(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("You can send a colleague if you're not able to attend yourself, or if you feel that he/she would have more use of the current topic. Make sure to contact us on [leap@microsoft.no](mailto:leap@microsoft.no) before doing so.");
            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent("WhereToGetPresentations")]
        public async Task WhereToGetPresentations(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("You can find the presentations that the speakers have shared with us in the LEAP portal. You'll need to be signed in to download them");
            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent("WillMasterclassBeStreamed")]
        public async Task WillMasterClassBeStreamed(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("We will be streaming each of the masterclasses using Skype for business. \nContact us via email (leap@microsoft.com) to get an invitation!");

            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent("GetParticipantCount")]
        public async Task GetParticipantCount(IDialogContext context, LuisResult result)
        {            
            await context.PostAsync($"I'm sorry, that is classified :-)");
            context.Wait(MessageReceivedAsync);
        }

    }
}