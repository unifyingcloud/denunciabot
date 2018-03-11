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
    public class MujeresQuery
    {
        [Prompt("Por favor digame su {&} (Nombre del denunciante)")]
        public string Nombre { get; set; }

      /*  [Prompt("Por favor digame el {&}")]
        public string NombreDeLaPersonaDenunciada { get; set; }


        [Prompt("Cuando ocurrio la {&}?")]
        public DateTime fechaDelIncidente { get; set; }


        [Prompt("Por favor digame su {&}")]
        public string correoElectronico { get; set; }

        [Prompt("Por favor envie una imagen del incidente {&}")]
        public Attachment imagen { get; set; }*/


    }


    [Serializable]
    public class MujeresDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Agradecemos su apoyo, su informacion sera usada de forma confidencial.");

            var mujeresFormDialog = FormDialog.FromForm(this.BuildMujeresForm, FormOptions.PromptInStart);

            context.Call(mujeresFormDialog, this.ResumeAfterMujeresFormDialog);
        }

     private IForm<MujeresQuery> BuildMujeresForm()
        {
           /* OnCompletionAsyncDelegate<MujeresQuery> processMujeresSearch = async (context, state) =>
            {
                await context.PostAsync($"Gracias por introducir su informacion");
            };*/

             return new FormBuilder<MujeresQuery>()
               // .Field(nameof(MujeresQuery.correoElectronico))
              //  .Message("Su informacion esta siendo registrada")
               // .AddRemainingFields()
               // .OnCompletion(processMujeresSearch)
                .Build();
            /*   return new FormBuilder<MujeresQuery>()
                .Field(nameof(MujeresQuery.Nombre))
              .Field(nameof(MujeresQuery.correoElectronico))
                .Field(nameof(MujeresQuery.fechaDelIncidente))
                .Field(nameof(MujeresQuery.NombreDeLaPersonaDenunciada))
                .Message("Su informacion esta siendo registrada")
                .AddRemainingFields()
                .OnCompletion(processMujeresSearch)
                .Build();
*/

} 

       private async Task ResumeAfterMujeresFormDialog(IDialogContext context, IAwaitable<MujeresQuery> result)
        {
            try
            {
                
                await context.PostAsync($"Hemos registrado su denuncia.");

                var resultMessage = context.MakeMessage();
                /*   resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();



              foreach (var hotel in hotels)
                {
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = hotel.Name,
                        Subtitle = $"{hotel.Rating} starts. {hotel.NumberOfReviews} reviews. From ${hotel.PriceStarting} per night.",
                        Images = new List<CardImage>()
                        {
                            new CardImage() { Url = hotel.Image }
                        },
                        Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "More details",
                                Type = ActionTypes.OpenUrl,
                                Value = $"https://www.bing.com/search?q=hotels+in+" + HttpUtility.UrlEncode(hotel.Location)
                            }
                        }
                    };

                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                }*/

                await context.PostAsync(resultMessage);
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation. Quitting from the MujeresDialog";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
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