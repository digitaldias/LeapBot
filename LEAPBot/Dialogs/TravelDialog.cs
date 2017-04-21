using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LEAPBot.Dialogs
{
    [Serializable]
    public class TravelDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await Task.FromResult<object>(null);
            context.Wait(MessageReceivedAsync);
        }


        private Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            throw new NotImplementedException();
        }


        [LuisIntent("OrderTaxi")]
        public async Task OrderTaxi(IDialogContext context, LuisResult result)
        {
            var reply = context.MakeMessage();
            reply.Text = "Sure! Getting a Taxi is fairly straigthtforward:";

            var heroCard = new HeroCard
            {
                Title = "Asker og Bærum Taxi",
                Subtitle = "For ordering a regular taxi 1-4 persons. For maxiTaxi, or special needs, call 06710",
                Images = new List<CardImage> { new CardImage("http://www.06710.no/s/defaultweb/AB-taxi-logo.jpg?w=150&bg=ffffff") },
                Buttons = new List<CardAction> {
                    new CardAction {
                        Title = "Order Online",
                        Type = ActionTypes.OpenUrl,
                        Value = "http://www.06710.no/bestill-taxi"
                    }
                }
            };
            reply.Attachments.Add(heroCard.ToAttachment());

            await context.PostAsync(reply);
            context.Wait(MessageReceivedAsync);
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
            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent("GetHotelRecommendations")]
        public async Task GetHotelRecommendations(IDialogContext context, LuisResult result)
        {
            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Text = "I believe these hotels are good choices for LEAP participants:";

            foreach (var hotel in Hotel.Recommended)
            {
                var heroCard = new HeroCard
                {
                    Text = hotel.Name,
                    Subtitle = hotel.Description,
                    Images = new List<CardImage> { new CardImage(hotel.ImageUrl, "Hotel Image") },
                    Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "View more", null, hotel.HomepageUrl) }
                };
                reply.Attachments.Add(heroCard.ToAttachment());
            }
            await context.PostAsync(reply);
            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent("BringingValuables")]
        public async Task BringingValuables(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("We have a wardrobe for storing your clothes and luggage. If you're concerned about your valuables, just reach out to one of the members of our team, and we will assist you - oh! And no pets/children please!");
            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent("WhatToDoWhenSick")]
        public async Task WhatToDoWhenSick(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("For basic medical stuff, reach out to one of the Microsoft representatives or at our reception");
            context.Wait(MessageReceivedAsync);
        }


        [LuisIntent("DoINeedAPC")]
        public async Task DoINeedAPC(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("You will not need a PC to attend the masterclass");
            context.Wait(MessageReceivedAsync);
        }
    }
}