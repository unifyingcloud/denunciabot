
namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Script.Serialization;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class BuscardenunciaQuery
    {
      
        [Prompt("Por favor digame el {&} que desea consultar")]
        public string folio { get; set; }
 

        [Prompt("Por favor digame la contraseña")]
        public string Contrasenia { get; set; }

    }


    [Serializable]
    public class BuscarDenunciaDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
           // await context.PostAsync("Realizaremos su busqueda al confirmar su correo electrónico.");

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
            BuscardenunciaQuery res = (BuscardenunciaQuery)result;

            try
            {

                String WEBSERVICE_URL = "https://fepade-web.azurewebsites.net/api/v2/pde/denuncia?folioDenuncia="  + res.folio +  "&password="  + res.Contrasenia +  "&esFepadeTel=false";
                try
                {
                    var webRequest = System.Net.WebRequest.Create(WEBSERVICE_URL);
                    if (webRequest != null)
                    {
                        webRequest.Method = "GET";
                        webRequest.Timeout = 12000;
                        webRequest.ContentType = "application/json";
                        webRequest.Headers.Add("Authorization", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InN0cmluZyIsIm5iZiI6MTUyMjUzNjAxNSwiZXhwIjoxNTIyNTQzMjE1LCJpYXQiOjE1MjI1MzYwMTUsImlzcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTEwOTAiLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjUxMDkwIn0.VdPvE3vD2tkJFEJoOfZirXAp2qCCGN5SFDGi07IwNa4");

                        using (System.IO.Stream s = webRequest.GetResponse().GetResponseStream())
                        {
                            using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                            {
                                var jsonResponse = sr.ReadToEnd();
                             

                                JavaScriptSerializer ser = new JavaScriptSerializer();
                               

                                var JSONObj = ser.Deserialize<Dictionary<string, string>>(jsonResponse);
                                foreach(var JsonVal in JSONObj)
                                {
                                    await context.PostAsync(JsonVal.Key + ", " + JsonVal.Value );

                                }
                              
                               
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    await context.PostAsync(ex.Message);
                }

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();
                HeroCard heroCard = new HeroCard()
                {
                    Title = "Ver estado de la denuncia",
                    Subtitle = "Gracias por su apoyo",
                    Images = new List<CardImage>()
                        {
                        new CardImage() { Url = "http://despertardeoaxaca.com/wp-content/uploads/2015/11/PGR-LOGO-770x470.png" }
                        },
                    Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "FEPADE",
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
