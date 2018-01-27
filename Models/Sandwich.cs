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
        [Describe("Selecciona tu oferta","https://descuentos.ninja/wp-content/uploads/2017/12/white_logo_transparent@2x-2-e1512817509959.png","Mensaje","Titulo","Subtitulo")]
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