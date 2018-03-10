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
    public enum OpcionesDenuncias
    {
        DelitoUno, DelitoDos,DelitoTres
    };
 

    [Serializable]
    public class Denuncias
    {
        public OpcionesDenuncias? Denuncia;
       

        public static IForm<Denuncias> BuildForm()
        {
            OnCompletionAsyncDelegate<Denuncias> processOrder = async (context, state) =>
            {
                await context.PostAsync("Gracias por tu informacion, es importante reportar delitos");
            };

            return new FormBuilder<Denuncias>()
                    .Message("Bienvenido al sistema privado inteligente de denuncias")
                    .OnCompletion(processOrder)
                    .Build();
        }
    };
}