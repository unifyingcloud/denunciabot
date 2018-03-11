namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private const string ElectoralOption = "Delito Electoral";

        private const string TorturaOption = "Delito de Tortura";

        private const string MujeresOption = "Delito Contra las mujeres";


        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Text.ToLower().Contains("ayuda") || message.Text.ToLower().Contains("soporte") || message.Text.ToLower().Contains("problema"))
            {
                await context.Forward(new SupportDialog(), this.ResumeAfterSupportDialog, message, CancellationToken.None);
            }
            else
            {
                this.ShowOptions(context);
            }
        }

        private void ShowOptions(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() { ElectoralOption, MujeresOption, TorturaOption }, "Desea denunciar un delito?", "Seleccione una opcion valida", 3);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case MujeresOption:
                        context.Call(new MujeresDialog(), this.ResumeAfterOptionDialog);
                        break;

                    case ElectoralOption:
                        //context.Call(new HotelsDialog(), this.ResumeAfterOptionDialog);
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Por favor selecciona o escribe una opcion!. Escribe ayuda o soporte y tu mensaje para registrar tu solicitud de otra manera.");

                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task ResumeAfterSupportDialog(IDialogContext context, IAwaitable<int> result)
        {
            var ticketNumber = await result;

            await context.PostAsync($"Gracias por contactar al equipo de soporte. Su numero de incidencia es {ticketNumber}.");
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var message = await result;
            }
            catch (Exception ex)
            {
                await context.PostAsync($"Algo ha fallado: {ex.Message}");
            }
            finally
            {
                context.Wait(this.MessageReceivedAsync);
            }
        }
    }
}