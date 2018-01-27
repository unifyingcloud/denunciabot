using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
#pragma warning disable 649

// The SandwichOrder is the simple form you want to fill out.  It must be serializable so the bot can be stateless.
// The order of fields defines the default order in which questions will be asked.
// Enumerations shows the legal options for each field in the SandwichOrder and the order is the order values will be presented 
// in a conversation.
namespace Microsoft.Bot.Sample.FormBot
{
    public enum SandwichOptions
    {
      Videojuegos,Libros, Hogar,Musica, Deportes, Ropa, Bebes
    };
    public enum LengthOptions { M, FootLong };
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
    };

    [Serializable]
    public class SandwichOrder
    {
       // [Prompt("Selecciona tu oferta"), Terms("Videojuegos,Libros, Hogar,Musica, Deportes, Ropa, Bebes")]
        [Prompt("Por favor selecciona tu oferta: {||}"),Describe("Oferta","https://images-eu.ssl-images-amazon.com/images/G/30/associates/network/revamp/logo/logo_1.png","Mensaje","Descuentos.ninja","Siempre puedes encontrar mas opciones en http://descuentos.ninja")]
        public SandwichOptions? Oferta;
       /* public LengthOptions? Length;
        public BreadOptions? Bread;
        public CheeseOptions? Cheese;
        public List<ToppingOptions> Toppings;
        public List<SauceOptions> Sauce;*/

        public static IForm<SandwichOrder> BuildForm()
        {
            OnCompletionAsyncDelegate<SandwichOrder> processOrder = async (context, state) =>
            {
                await context.PostAsync("Este es el final de la forma");
            };

            return new FormBuilder<SandwichOrder>()
                .Message("hola, hoy estas de suerte!, te presentaré tus ofertas preferidas")
                    .OnCompletion(processOrder)
                    .Build();
        }
    };
}