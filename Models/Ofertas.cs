using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using Microsoft.Bot.Connector;

#pragma warning disable 649

// The SandwichOrder is the simple form you want to fill out.  It must be serializable so the bot can be stateless.
// The order of fields defines the default order in which questions will be asked.
// Enumerations shows the legal options for each field in the SandwichOrder and the order is the order values will be presented 
// in a conversation.
namespace Microsoft.Bot.Sample.FormBot
{
    public enum OpcionesDeOferta
    {
      Videojuegos,Libros, Hogar,Musica, Deportes, Ropa, Bebes
    };
    /*public enum LengthOptions { M, FootLong };
    public enum BreadOptions { NineGrainWheat, NineGrainHoneyOat, Italian, ItalianHerbsAndCheese, Flatbread };
    public enum CheeseOptions { American, MontereyCheddar, Pepperjack };
    public enum ToppingOptions
    {
        Avocado, BananaPeppers, Cucumbers, GreenBellPeppers, Jalapenos,
        Lettuce, Olives, Pickles, RedOnion, Spinach, Tomatoes
    };
    public enum SauceOptions
    {
        ChipotleSouthwest, HoneyMustard, LightMayonnaise, RegularMayonnaise,
        Mustard, Oil, Pepper, Ranch, SweetOnion, Vinegar
    };*/

    [Serializable]
    public class AfiliadosAmazon
    {
       // [Prompt("Selecciona tu oferta"), Terms("Videojuegos,Libros, Hogar,Musica, Deportes, Ropa, Bebes")]
        [Prompt("Por favor selecciona tu oferta: {||}"),Describe("Oferta",null,"Mensaje","Elige una categoria:","Mas opciones en http://descuentos.ninja")]
        public OpcionesDeOferta? Oferta;
       /* public LengthOptions? Length;
        public BreadOptions? Bread;
        public CheeseOptions? Cheese;
        public List<ToppingOptions> Toppings;
        public List<SauceOptions> Sauce;*/



        private static Attachment GetHeroCard()
        {
            var heroCard = new HeroCard
            {
                Title = "Tus ofertas",
                Subtitle = "Hemos encontrado estas ofertas para ti",
                Text = "",
                Images = new List<CardImage> { new CardImage("https://images-eu.ssl-images-amazon.com/images/I/510sFxlZHhL._AC_US218_.jpg"), new CardImage("https://images-eu.ssl-images-amazon.com/images/I/51JXRcvJXlL._AC_US218_.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Vamos!", value: "https://www.amazon.com.mx") }
            };

            return heroCard.ToAttachment();
        }

          

        public static IForm<AfiliadosAmazon> BuildForm()
        {
            OnCompletionAsyncDelegate<AfiliadosAmazon> processOrder = async (context, state) =>
            {

                var messageReturn = context.MakeMessage();

                var attachment = GetHeroCard();
                messageReturn.Attachments.Add(attachment);

                await context.PostAsync(messageReturn);

               
            };

            return new FormBuilder<AfiliadosAmazon>()
                    .Message("Bienvenido")
                    .Confirm("No verification will be shown", state => false)
                    .OnCompletion(processOrder)
                .Build();
        }
    };
}