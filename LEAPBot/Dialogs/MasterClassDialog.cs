using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using LEAPBot.Domain.Entities;
using LEAPBot.Domain.Contracts;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Luis;
using System.Globalization;

namespace LEAPBot.Dialogs
{
    [Serializable]
    public class MasterClassDialog : IDialog<object>
    {
        private MasterClass SelectedMasterclass = null;


        public async Task StartAsync(IDialogContext context)
        {
            await Task.FromResult<object>(null);

            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent("GetMasterclassSpeaker")]
        public async Task GetMasterClassSpeaker(IDialogContext context, LuisResult result)
        {
            var masterClasses = await WebApiApplication.Container.GetInstance<ILeapRestClient>().GetMasterClasses();

            var ro = masterClasses.Select(m => m.Number).ToList().AsReadOnly();
            var rd = masterClasses.Select(m => (m.Number + 1) + ": " + m.Title + " (" + m.Date.ToString("dddd, MMMM dd", new CultureInfo("en-US")) + ")").ToList().AsReadOnly();

            var promptOptions = new PromptOptions<int>(
                "Just to make sure - For which masterclass do you want to know? (give me the number)",
                "Try again please? Type the number of the Masterclass",
                "I give up, too many attempts there",
                ro,
                3,
                new PromptStyler(PromptStyle.PerLine),
                rd);

            PromptDialog.Choice(context, AfterAskingForMasterClassSpeaker, promptOptions);
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
            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent("GetSpeakerTopics")]
        public async Task GetSpeakerTopics(IDialogContext context, LuisResult result)
        {
            EntityRecommendation speakerNameRecommendation;
            var speakerNameFound = result.TryFindEntity("SpeakerReference", out speakerNameRecommendation);

            if (!speakerNameFound)
            {
                await context.PostAsync($"I'm sorry but I can't find any match for a speaker named '{speakerNameRecommendation.Entity}' on this years masterclasses");
            }
            else
            {
                var speaker = await WebApiApplication.Container.GetInstance<ILeapRestClient>()
                    .GetSpeakerByPartialName(speakerNameRecommendation.Entity);

                if (speaker == null)
                {
                    await context.PostAsync($"I'm sorry but I was unable to find a masterclass speaker named '{speakerNameRecommendation.Entity}' ");
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
            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent("GetMasterclassCost")]
        public async Task GetMasterclassCost(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("LEAP masterclasses are part of the LEAP program which is a paid program, however, if you are already signed up on LEAP, there is no additional cost for attending our masterclasses - naturally!");
            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent("GetMasterclassLocation")]
        public async Task GetMasterClassLocation(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Each masterclass will take place in our Microsoft Office, near the 'Lysaker' train and subway station. \nThere are parking houses nearby.\nWe also ask our speakers to stream the meeting over Skype for Business, in case you're unable to physically attend the masterclass.\n");
            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent("GetNextMasterclassDate")]
        public async Task GetNextMasterClass(IDialogContext context, LuisResult result)
        {
            var masterClasses = await WebApiApplication.Container.GetInstance<ILeapRestClient>()
                .GetMasterClasses();

            var nextMasterClass = masterClasses.OrderBy(m => m.Date).FirstOrDefault(m => m.Date >= DateTime.Now.Date);

            var reply = context.MakeMessage();
            reply.Text = "Here is the next masterclass:";
            reply.Attachments.Add(ConvertMasterclassToHeroCardAttachment(nextMasterClass));

            await context.PostAsync(reply);
            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent("GetNextMasterclassTopic")]
        public async Task GetNextMasterTopic(IDialogContext context, LuisResult result)
        {
            var allMasterClasses = await WebApiApplication.Container.GetInstance<ILeapRestClient>().GetMasterClasses();

            var nextMasterClass = allMasterClasses.FirstOrDefault(m => m.Date > DateTime.Now);

            await context.PostAsync($"The topic of the next masterclass is '{nextMasterClass.Title}'\n it will be held on {nextMasterClass.Date.ToShortDateString()}");
            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent("ListMasterclasses")]
        public async Task ListMasterClasses(IDialogContext context, LuisResult result)
        {
            var allMasterClasses = await WebApiApplication.Container.GetInstance<ILeapRestClient>().GetMasterClasses();
            var reply = context.MakeMessage();
            reply.Text = "Here are the planned masterclasses";
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            foreach (var masterclass in allMasterClasses)
                reply.Attachments.Add(ConvertMasterclassToHeroCardAttachment(masterclass));

            await context.PostAsync(reply);
            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent("GetLastMasterclassDate")]
        public async Task GetLastMasterClassDate(IDialogContext context, LuisResult result)
        {
            var allMasterClasses = await WebApiApplication.Container.GetInstance<ILeapRestClient>()
                .GetMasterClasses();

            var lastMasterClass = allMasterClasses.OrderBy(mc => mc.Number).Last();

            var reply = context.MakeMessage();
            reply.Text = "This is our last masterclass:";
            reply.Attachments.Add(ConvertMasterclassToHeroCardAttachment(lastMasterClass));

            await context.PostAsync(reply);
            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent("GetMasterclassByTopic")]
        public async Task GetMasterclassByTopic(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Let me see...");
            EntityRecommendation entityRecommendation;
            MasterClass masterClass = null;
            if (result.TryFindEntity("TopicReference", out entityRecommendation) == false)
            {
                await context.PostAsync($"I'm sorry, but I couldn't figure out the topic you asked for.");
            }
            else
            {
                masterClass = await WebApiApplication
                    .Container
                    .GetInstance<ILeapRestClient>()
                    .GetMasterClassByTopic(entityRecommendation.Entity);

                if (masterClass == null)
                {
                    await context.PostAsync($"Sorry but I couldn't find any masterclasses covering '{entityRecommendation.Entity}'");
                }
                else
                {
                    var reply = context.MakeMessage();
                    reply.Text = "I found this masterclass for you: ";
                    reply.Attachments.Add(ConvertMasterclassToHeroCardAttachment(masterClass));

                    await context.PostAsync(reply);
                }
            }
            context.Wait(MessageReceivedAsync);
        }


        private Attachment ConvertMasterclassToHeroCardAttachment(MasterClass masterClass)
        {
            var currentYear = GetCurrentYear();


            var heroCard = new HeroCard
            {
                Title = masterClass.Title,
                Subtitle = masterClass.Date.ToString("dddd, dd MMMM"),
                Text = masterClass.Description,
                Images = new List<CardImage> { new CardImage($"https://leap.microsoft.no/Content/images/MasterClasses/{currentYear}/{masterClass.Number + 1}.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "View online", null, "https://leap.microsoft.no/masterclasses") }
            };
            return heroCard.ToAttachment();
        }


        private int GetCurrentYear()
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;

            if (month >= 6 && month <= 12)
                return year + 1;
            else
                return year;
        }


        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            await Task.FromResult<object>(null);
        }


        private async Task AfterAskingForMasterClassSpeaker(IDialogContext context, IAwaitable<int> result)
        {
            var masterClassNumber = await result;
            var masterClasses = await WebApiApplication.Container.GetInstance<ILeapRestClient>().GetMasterClasses();
            SelectedMasterclass = masterClasses.FirstOrDefault(m => m.Number == masterClassNumber);
            var speakers = await WebApiApplication.Container.GetInstance<ILeapRestClient>().GetMasterClassSpeakers(masterClassNumber);

            if (speakers.Count() >= 1)
            {
                var reply = context.MakeMessage();
                reply.Text = $"For masterclass #{masterClassNumber}, we have the following speaker(s) lined up:";
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                reply.Attachments = CreateSpeakerCardAttachments(speakers);

                await context.PostAsync(reply);
            }
            else
            {
                await context.PostAsync($"I'm so sorry, but I couldn't find a speaker for masterclass {masterClassNumber}");
            }
            context.Wait(MessageReceivedAsync);
        }


        private IList<Attachment> CreateSpeakerCardAttachments(IEnumerable<Speaker> speakers)
        {
            var attachments = new List<Attachment>();

            foreach (var speaker in speakers)
            {
                var speakerImages = new List<CardImage> { new CardImage(speaker.Image, "Image of speaker") };
                attachments.Add(new HeroCard(speaker.Name, speaker.IntroHeading, speaker.Bio, speakerImages).ToAttachment());
            }
            return attachments;
        }
    }
}