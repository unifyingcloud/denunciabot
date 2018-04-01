
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
    using Microsoft.Bot.Builder.Location;

    [Serializable]
    public class Denuncia
    {
      
        [Prompt("Por favor digame su {&}")]
        public string correoElectronico { get; set; }

        [Prompt("Por favor digame qué es lo que pasó")]
        public string descripcion { get; set; }
        [Prompt("Por favor digame la {&}")]
        public string fecha { get; set; }
        [Prompt("Por favor digame la dirección más aproximada de dónde fueron los hechos")]
        public string direccion { get; set; }

    }


    [Serializable]
    public class MyLocationDialog : IDialog<string>
    {
        private readonly string channelId;

        public MyLocationDialog(string channelId)
        {
            this.channelId = channelId;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var apiKey = "";// WebConfigurationManager.AppSettings["BingMapsApiKey"];
            var options = LocationOptions.UseNativeControl | LocationOptions.ReverseGeocode;

            var requiredFields = LocationRequiredFields.StreetAddress | LocationRequiredFields.Locality |
                                 LocationRequiredFields.Region | LocationRequiredFields.Country |
                                 LocationRequiredFields.PostalCode;

            var prompt = "Cual es la direccion aproximanada donde sucedieron los hechos?";

            var locationDialog = new LocationDialog(apiKey, this.channelId, prompt, options, requiredFields);

            context.Call(locationDialog, this.ResumeAfterLocationDialogAsync);
        }

        private async Task ResumeAfterLocationDialogAsync(IDialogContext context, IAwaitable<Place> result)
        {
            var place = await result;

            if (place != null)
            {
                var address = place.GetPostalAddress();
                var formatteAddress = string.Join(", ", new[]
                {
                        address.StreetAddress,
                        address.Locality,
                        address.Region,
                        address.PostalCode,
                        address.Country
                    }.Where(x => !string.IsNullOrEmpty(x)));

                await context.PostAsync("TGracias, hemos anotado la direccion " + formatteAddress);
            }

            context.Done<string>(null);
        }
    }


    [Serializable]
    public class DenunciaDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Ha decidido reportar una nueva denuncia.");

            var DenunciaDialog = FormDialog.FromForm(this.BuildForm, FormOptions.PromptInStart);

              context.Call(DenunciaDialog, this.ResumeAfterFormDialog);

           
        }

     private IForm<Denuncia> BuildForm()
        {
             OnCompletionAsyncDelegate<Denuncia> processSearch = async (context, state) =>
            {
                await context.PostAsync($"Gracias por introducir su informacion");
            }; 

               return new FormBuilder<Denuncia>()
                .AddRemainingFields()
                .OnCompletion(processSearch)
                .Build();
 

} 

       private async Task ResumeAfterFormDialog(IDialogContext context, IAwaitable<Denuncia> result)
        {
            try
            {
                
           //     await context.PostAsync($"Hemos registrado su denuncia.");

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();
                HeroCard heroCard = new HeroCard()
                {
                    Title = "Denuncia FEPADE",
                    Subtitle = "Hemos generado su denuncia",
                    Images = new List<CardImage>()
                        {
                        new CardImage() { Url = "https://app.cedac.pgr.gob.mx/ATENCIONPGR/img/LogoAtenci%C3%B3nPGR-02.jpg" }
                        },
                    Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "Gracias por contribuir",
                                Type = ActionTypes.OpenUrl,
                            Value = $"https://www.gob.mx/pgr"
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