using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LEAPBot.Dialogs
{
    public class RootDialog : IDialog<object>
    {
        private const string helpMessage = "I have been designed to help you with information about our LEAP masterclasses";

        public async Task StartAsync(IDialogContext context)
        {
            await Task.FromResult<object>(null);
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var messageActivity = await result;

            if (messageActivity.Text.Contains("help"))
            {
                await ShowHelpAsync(context);
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                await context.PostAsync("Hello, I'm Leapbot. \nI can help you with information about the LEAP masterclasses. Just give me a try!");
            }
            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent(""), LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I am sorry, but I didn't understand your question.");
            context.Wait(MessageReceivedAsync);
        }

        [LuisIntent("GreetBot")]
        public async Task SayHello(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hello, I'm Leapbot. \nI can help you with information about the LEAP masterclasses. Just give me a try!");
            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent("ThankYou")]
        public async Task ThankYou(IDialogContext context, LuisResult result)
        {
            var replies = new[] { "You're welcome!", "No problemo!", "I live to serve :-)", "De nada, Amigo", "Sure thing", "No problem man", "Pleasure was all mine", "Happy to oblige", "It's cool bro!" };
            var random = new Random((int)DateTime.Now.Ticks);

            int choice = random.Next(0, replies.Length - 1);

            await context.PostAsync(replies[choice]);

            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent("Insult")]
        public async Task Insult(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"{_username}, I do my best, but I'm only as smart as my code allows.");
            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent("ExplainMasterclasses")]
        public async Task ExplainMasterClasses(IDialogContext context, LuisResult result)
        {
            var cardImage = new CardImage("https://leap.microsoft.no/Content/Images/LeapImage.png", "Leap Logo");
            var heroCard = new HeroCard(
                "What are LEAP Masterclasses?",
                "Masterclasses are monthly in-depth sessions on a specified topic",
                "",
                new List<CardImage> { cardImage }
                );

            var replyMessage = context.MakeMessage();
            replyMessage.Attachments.Add(heroCard.ToAttachment());

            await context.PostAsync(replyMessage);
            context.Wait(MessageReceivedAsync);
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


        private async Task ShowHelpAsync(IDialogContext context)
        {
            await context.PostAsync(helpMessage);
            context.Wait(MessageReceivedAsync);
        }
    }
}