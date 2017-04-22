using LEAPBot.Domain.Contracts;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LEAPBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private const string helpMessage = "I have been designed to help you with information about our LEAP masterclasses";
        private readonly LuisService _luisService;


        public RootDialog()
        {
            var settings = WebApiApplication.Container.GetInstance<ISettingsReader>();

            var luisModel = new LuisModelAttribute(settings["LUIS:Topics:AppId"], settings["LUIS:Topics:AppKey"]);
            _luisService = new LuisService(luisModel);
        }


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
            }

            var luisResult = await _luisService.QueryAsync(messageActivity.Text, CancellationToken.None);                
            await HandleTopScoringIntentAsync(context, luisResult, messageActivity);                
        }


        public async Task HandleTopScoringIntentAsync(IDialogContext context, LuisResult luisResult, IMessageActivity messageActivity)
        {
            var intent = luisResult.TopScoringIntent;

            switch(intent.Intent)
            {
                case "LEAP":
                    await HandleLEAPQuestionAsync(context, luisResult, messageActivity);
                    break;

                case "Greeting":
                    await SayHello(context, luisResult);
                    break;
            }
        }


        private async Task AfterIntentDialogAsync(IDialogContext context, IAwaitable<object> result)
        {
            var final = await result;
            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent(""), LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I am sorry, but I didn't understand your question.");
            context.Wait(MessageReceivedAsync);
        }


        public async Task HandleLEAPQuestionAsync(IDialogContext context, LuisResult luisResult, IMessageActivity messageActivity)
        {
            var intent = luisResult.TopScoringIntent;
            if (intent.Intent == "LEAP")
            {
                var entity = luisResult.Entities.OrderByDescending(e => e.Score).FirstOrDefault();
                if (entity == null)
                {
                    await context.PostAsync("Oh, I didn't quite catch what topic you wanted to adress there!");
                    return;
                }
                switch (entity.Entity)
                {
                    case "leap":
                        break;
                    case "masterclass":
                    case "masterclasses":
                        await context.Forward(new MasterClassDialog(), AfterIntentDialogAsync, messageActivity, CancellationToken.None);
                        break;
                }
            }            
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
            await context.PostAsync($"Hey! I do my best, but I'm only as smart as my code allows.");
            context.Wait(MessageReceivedAsync);
        }
      

        private async Task ShowHelpAsync(IDialogContext context)
        {
            await context.PostAsync(helpMessage);
            context.Wait(MessageReceivedAsync);
        }
    }
}