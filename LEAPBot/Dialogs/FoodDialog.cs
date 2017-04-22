using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Threading.Tasks;

namespace LEAPBot.Dialogs
{
    [Serializable]
    public class FoodDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await Task.FromResult<object>(null);
            context.Wait(MessageReceivedAsync);
        }


        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            await Task.FromResult<object>(null);
        }

        [LuisIntent("SpecialFoodOfferings")]
        public async Task SpecialFoodOfferings(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("The masterclasses will offer a choice of foods, including specials meals for those who have asked for it. If you have a specific dietary requirement, such as halal, kosher, vegetarian/vegan, please send us an email at least a week in advance so that we may accomodate your needs");
            context.Wait(MessageReceivedAsync);
        }

    }
}