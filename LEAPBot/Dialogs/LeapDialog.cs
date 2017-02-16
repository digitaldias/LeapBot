using LEAPBot.Domain.Contracts;
using LEAPBot.Domain.Entities;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LEAPBot.Dialogs
{
    [Serializable] 
    public class LeapDialog : LuisDialog<object>
    {
        private const string helpMessage = "I have been designed to help you with information about our masterclasses. ";

        private string _username;


        private MasterClass SelectedMasterclass = null;


        public LeapDialog(LuisService service)            
            : base(service)
        {            
        }

        protected override Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            if(!context.UserData.TryGetValue("username", out _username))
            {
                PromptDialog.Text(context, ResumeAfterPrompt, "Before we get started, please tell me your name?");
                return Task.FromResult<object>(null);
            }
            return base.MessageReceived(context, item);
        }

        private async Task ResumeAfterPrompt(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var name = await result;
                await context.PostAsync($"Hello there, {name}! {helpMessage}");
                _username = name;                
                context.UserData.SetValue("username", name);
            }
            catch(TooManyAttemptsException)
            {
            }
            context.Wait(MessageReceived);
        }


        [LuisIntent("GetSpeakerTopics")]
        public async Task GetSpeakerTopics(IDialogContext context, LuisResult result)
        {
            EntityRecommendation speakerNameRecommendation;
            var speakerNameFound = result.TryFindEntity("SpeakerReference", out speakerNameRecommendation);


            if (!speakerNameFound)
            {
                await context.PostAsync($"I'm sorry {_username}, but I can't find any match for a speaker named '{speakerNameRecommendation.Entity}' on this years masterclasses");
            }
            else
            {
                var speaker = await WebApiApplication.Container.GetInstance<ILeapRestClient>()
                    .GetSpeakerByPartialName(speakerNameRecommendation.Entity);

                if(speaker == null)
                {
                    await context.PostAsync($"I'm sorry {_username}, but I was unable to find a masterclass speaker named '{speakerNameRecommendation.Entity}' ");
                }
                else
                {
                    var speakerCards = CreateSpeakerCardAttachments(new List<Speaker> { speaker });

                    var reply = context.MakeMessage();
                    reply.Text = "Here is some info about " + speaker.Name + ": ";
                    reply.Attachments.Add(speakerCards.First());

                    await context.PostAsync(reply);
                }
            }

            context.Wait(MessageReceived);
        }

        [LuisIntent("GetMasterclassSpeaker")]
        public async Task GetMasterClassSpeaker(IDialogContext context, LuisResult result)
        {
                var masterClasses = await WebApiApplication.Container.GetInstance<ILeapRestClient>().GetMasterClasses();

                var ro = masterClasses.Select(m => m.Number).ToList().AsReadOnly();
                var rd = masterClasses.Select(m => m.Number + ": " + m.Title).ToList().AsReadOnly();

                var promptOptions = new PromptOptions<int>(
                    "Just to make sure - For which masterclass do you want to know?",
                    "Try again please? Type the number of the Masterclass",
                    "I give up, too many attempts there",
                    ro,
                    3,
                    new PromptStyler(PromptStyle.PerLine), 
                    rd);

                PromptDialog.Choice(context, AfterAskingForMasterClassSpeaker, promptOptions);
        }

        private async Task AfterAskingForMasterClassSpeaker(IDialogContext context, IAwaitable<int> result)
        {
            var masterClassNumber = await result;

            var masterClasses = await WebApiApplication.Container.GetInstance<ILeapRestClient>().GetMasterClasses();

            SelectedMasterclass = masterClasses.FirstOrDefault(m => m.Number == masterClassNumber);

            var speakers = await WebApiApplication.Container.GetInstance<ILeapRestClient>().GetMasterClassSpeakers(masterClassNumber);

            if(speakers.Count() >= 1)
            {
                var reply              = context.MakeMessage();
                reply.Text = $"For masterclass #{masterClassNumber}, we have the following speaker(s) lined up:";
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                reply.Attachments = CreateSpeakerCardAttachments(speakers);

                await context.PostAsync(reply);
            }
            else
            {
                await context.PostAsync($"I'm so sorry, {_username}, but I couldn't find a speaker for masterclass {masterClassNumber}");
            }


            context.Wait(MessageReceived);
        }


        [LuisIntent("ListAllSpeakers")]
        public async Task ListAllSpeakers(IDialogContext context, LuisResult result)
        {
            var speakers = await WebApiApplication.Container.GetInstance<ILeapRestClient>().GetAllSpeakers();
            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Text = "The following speakers are planned for our masterclasses:";
            reply.Attachments = CreateSpeakerCardAttachments(speakers);

            await context.PostAsync(reply);

            context.Wait(MessageReceived);
        }

        private IList<Attachment> CreateSpeakerCardAttachments(IEnumerable<Speaker> speakers)
        {
            var attachments = new List<Attachment>();

            foreach(var speaker in speakers)
            {
                var speakerImages = new List<CardImage> { new CardImage(speaker.Image, "Image of speaker") };
                attachments.Add(new HeroCard(speaker.Name, speaker.IntroHeading, speaker.Bio, speakerImages).ToAttachment());
            }
            return attachments;
        }

        [LuisIntent("Insult")]
        public async Task Insult(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"{_username}, I do my best, but I'm only as smart as my code allows");
            context.Wait(MessageReceived);
        }

        [LuisIntent("BringingValuables")]
        public async Task BringingValuables(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("We have a wardrobe for storing your clothes and luggage. If you're concerned about your valuables, just reach out to one of the members of our team, and we will assist you");
            context.Wait(MessageReceived);
        }

        [LuisIntent("WhereToGetPresentations")]
        public async Task WhereToGetPresentations(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Get your presentations in the LEAP portal by logging in...");
            context.Wait(MessageReceived);
        }

        [LuisIntent("GetParkingInstructions")]
        public async Task GetParkingInstructions(IDialogContext context, LuisResult result)
        {
            var herocard = new HeroCard("Parking for LEAP",
                "Want to drive to get to your LEAP masterclass? No problem!",
                "There is a Q-Park facility just across the road from our Microsoft Office that you can park your vehicle in.",
                new List<CardImage> {
                    new CardImage("https://leap.blob.core.windows.net/other/LEAPParking.png", "Map showing parking location")
                });

            var reply = context.MakeMessage();
            reply.Attachments.Add(herocard.ToAttachment());
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        [LuisIntent("WhatToDoWhenSick")]
        public async Task WhatToDoWhenSick(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("For basic medical stuff, reach out to one of the Microsoft representatives or at our reception");
            context.Wait(MessageReceived);
        }

        [LuisIntent("DoINeedAPC")]
        public async Task DoINeedAPC(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("You will not need a PC to attend the masterclass");

            context.Wait(MessageReceived);
        }


        [LuisIntent("GreetBot")]
        public async Task SayHello(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hello, I'm Leapbot. \nI can help you with information about the LEAP masterclasses. Just give me a try!");
            context.Wait(MessageReceived);
        }


        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I am sorry, but I didn't understand your question.");
            context.Wait(MessageReceived);
        }


        [LuisIntent("GetLastMasterclassDate")]
        public async Task GetLastMasterClassDate(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("The last LEAP masterclass will take place on ###. The topic will be TTTT");
            context.Wait(MessageReceived);
        }


        [LuisIntent("WillMasterclassBeStreamed")]
        public async Task WillMasterClassBeStreamed(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("We will be streaming each of the masterclasses using Skype for business. \nContact us via email (leap@microsoft.com) to get an invitation!");

            context.Wait(MessageReceived);
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
            context.Wait(MessageReceived);

        }


        [LuisIntent("ExplainMasterclasses")]
        public async Task ExplainMasterClasses(IDialogContext context, LuisResult result)
        {
            var cardImage = new CardImage("https://leap.microsoft.no/Content/Images/LeapImage.png", "Leap Logo");
            var heroCard = new HeroCard(
                "What are LEAP Masterclasses?",
                "Masterclasses are monthly in-depth sessions on a specified topic",
                "blablabla",
                new List<CardImage> { cardImage}                
                );

            var replyMessage = context.MakeMessage();
            replyMessage.Attachments.Add(heroCard.ToAttachment());

            await context.PostAsync(replyMessage);
            
            context.Wait(MessageReceived);
        }

        [LuisIntent("GetParticipantCount")]
        public async Task GetParticipantCount(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("There are 3 people on LEAP this year");
            context.Wait(MessageReceived);
        }


        [LuisIntent("ListMasterclasses")]
        public async Task ListMasterClasses(IDialogContext context, LuisResult result)
        {
            
            var allMasterClasses = await WebApiApplication.Container.GetInstance<ILeapRestClient>().GetMasterClasses();

            var receiptCard = new ReceiptCard
            {
                Title = "MasterClasses this season",
                Facts = new List<Fact> {
                    new Fact ("Number of sessions", allMasterClasses.Count().ToString())
                },
                Items = new List<ReceiptItem>(allMasterClasses.Select(m => new ReceiptItem($"{m.Number + 1}: {m.Date.ToString("dd MMM yyyy")}" , m.Title))),
                Buttons = new List<CardAction>
                {
                    new CardAction (
                        ActionTypes.OpenUrl,
                        "More information",
                        null,
                        "https://leap.microsoft.no/Masterclasses"
                        )
                }
            };

            var message = context.MakeMessage();
            message.Attachments.Add(receiptCard.ToAttachment());

            await context.PostAsync(message);

            context.Wait(MessageReceived);
        }

        private async Task OnMasterClassSelected(IDialogContext context, IAwaitable<string> result)
        {
            await context.PostAsync("Yes yes");
            context.Wait(MessageReceived);
        }

        [LuisIntent("GetNextMasterclassDate")]
        public async Task GetNextMasterClass(IDialogContext context, LuisResult result)
        {

            await context.PostAsync("The next LEAP masterclass, entitled 'AAAA' will take place on ###, it will be about XXXX");
            context.Wait(MessageReceived);
        }

        [LuisIntent("SpecialFoodOfferings")]
        public async Task SpecialFoodOfferings(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("The masterclasses will offer a choice of foods, including specials meals for those who have asked for it. If you have a specific dietary requirement, such as halal, kosher, vegetarian/vegan, please send us an email at least a week in advance so that we may accomodate your needs");
            context.Wait(MessageReceived);
        }

        [LuisIntent("GetMasterclassLocation")]
        public async Task GetMasterClassLocation(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Each masterclass will take place in Vika Kino, near the 'Nationaltheateret' train and subway station. \nThere are parking houses nearby.\nWe also ask our speakers to stream the meeting over Skype for Business, in case you're unable to physically attend the masterclass.\n");
            context.Wait(MessageReceived);
        }

        [LuisIntent("GetNextMasterclassTopic")]
        public async Task GetNextMasterTopic(IDialogContext context, LuisResult result)
        {
            var allMasterClasses = await WebApiApplication.Container.GetInstance<ILeapRestClient>().GetMasterClasses();

            var nextMasterClass = allMasterClasses.FirstOrDefault(m => m.Date > DateTime.Now);

            await context.PostAsync($"The topic of the next masterclass is '{nextMasterClass.Title}'\n it will be held on {nextMasterClass.Date.ToShortDateString()}");
            context.Wait(MessageReceived);
        }


        [LuisIntent("Help")]
        public async Task GetHelp(IDialogContext context, LuisResult result)
        {
            await context.PostAsync(helpMessage);
            context.Wait(MessageReceived);
        }
    }
}