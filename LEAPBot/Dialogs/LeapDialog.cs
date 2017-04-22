using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
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


        [LuisIntent("GeneralLeapInformation")]
        public async Task GeneralLeapInformation(IDialogContext context, LuisResult result)
        {
            var reply = context.MakeMessage();

            var heroCard = new HeroCard(
                "What is LEAP you say?",
                "Lead Enterprise Architecture Program is a program for software and solution architects who are seeking the best possible professional training, delivered from Microsoft Norway in our headquarters in Redmond, Seattle. ",
                "LEAP addresses the most important Microsoft technologies and the relationship between these technologies and business challenges. Microsoft’s vision, mission, strategy and roadmap are also part of the program. ",
                new List<CardImage> {
                    new CardImage("https://leap.microsoft.no/Content/Images/LeapImage.png", "LEAP Logo")
                }, null,
                new CardAction(ActionTypes.OpenUrl, "Learn more", null, "https://leap.microsoft.no"));

            reply.Attachments.Add(heroCard.ToAttachment());

            await context.PostAsync(reply);
            context.Wait(MessageReceivedAsync);
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




        [LuisIntent("GetParticipantCount")]
        public async Task GetParticipantCount(IDialogContext context, LuisResult result)
        {            
            await context.PostAsync($"I'm sorry, that is classified :-)");
            context.Wait(MessageReceivedAsync);
        }

    }
}