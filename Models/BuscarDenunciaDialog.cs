
namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class BuscardenunciaQuery
    {
      
        [Prompt("Por favor digame el {&} que desea consultar")]
        public string folio { get; set; }
 

    }


    [Serializable]
    public class BuscarDenunciaDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Realizaremos su busqueda al confirmar su correo electrónico.");

            var DenunciaDialog = FormDialog.FromForm(this.BuildForm, FormOptions.PromptInStart);

         context.Call(DenunciaDialog, this.ResumeAfterFormDialog);
        }

        private IForm<BuscardenunciaQuery> BuildForm()
        {
            OnCompletionAsyncDelegate<BuscardenunciaQuery> processSearch = async (context, state) =>
           {

           
                await context.PostAsync($"Gracias por introducir su informacion");
           };

            return new FormBuilder<BuscardenunciaQuery>()
             .AddRemainingFields()
             .OnCompletion(processSearch)
             .Build();

        }

       private async Task ResumeAfterFormDialog(IDialogContext context, IAwaitable<BuscardenunciaQuery> result)
        {
            try
            {

                //aqui se hace la conexion con la api de busqueda

                //Buscar en la api usando correo electronico

                //context.UserData to get email
               // context.


                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();
                HeroCard heroCard = new HeroCard()
                {
                    Title = "Denuncia FEPADE",
                    Subtitle = "Hemos encontrado su denuncia",
                    Images = new List<CardImage>()
                        {
                        new CardImage() { Url = "https://app.cedac.pgr.gob.mx/ATENCIONPGR/img/LogoAtenci%C3%B3nPGR-02.jpg" }
                        },
                    Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "Ver estado de la denuncia",
                                Type = ActionTypes.OpenUrl,
                            Value = $"https://www.fepadenet.gob.mx/folio.aspx?numfolio=00002420"
                            }
                        }
                };

                resultMessage.Attachments.Add(heroCard.ToAttachment());

                await context.PostAsync(resultMessage);
            }
            catch (Exception ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "Operacion cancelada";
                }
                else
                {
                    reply = $"Error: {ex.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Done<object>(null);
            }
        }
 
   /*     private async Task<IEnumerable<Hotel>> GetMujeresAsync(MujeresQuery searchQuery)
        {
            var hotels = new List<Hotel>();

            // Filling the hotels results manually just for demo purposes
            for (int i = 1; i <= 5; i++)
            {
                var random = new Random(i);
                Hotel hotel = new Hotel()
                {
                    Name = $"{searchQuery.Destination} Hotel {i}",
                    Location = searchQuery.Destination,
                    Rating = random.Next(1, 5),
                    NumberOfReviews = random.Next(0, 5000),
                    PriceStarting = random.Next(80, 450),
                    Image = $"https://placeholdit.imgix.net/~text?txtsize=35&txt=Hotel+{i}&w=500&h=260"
                };

                hotels.Add(hotel);
            }

            hotels.Sort((h1, h2) => h1.PriceStarting.CompareTo(h2.PriceStarting));

            return hotels;
        }*/
    }
}
