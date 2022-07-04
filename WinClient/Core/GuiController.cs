using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WinClient.Api;
using WinClient.GUI;
using Timer = System.Windows.Forms.Timer;

namespace WinClient
{
    class GuiController
    {
        private readonly Config config;
        private FormStatus form;

        public GuiController(Config config)
        {
            this.config = config;
            createForm();
            if (config.debug_mode) createDebugForm(form);
        }

        private void createForm()
        {
            var waiter = new AutoResetEvent(false);
            new Thread(() =>
            {
                form = new FormStatus(config);
                form.FormClosing += (o, ev) =>
                {
                    if (ev.CloseReason != CloseReason.UserClosing) return;
                    if (config.no_background)
                    {
                        Application.Exit();
                        return;
                    }
                    ev.Cancel = true;
                    form.Hide();
                };
                if (config.no_background) form.Visible = true;
                waiter.Set();
                Application.Run(form);
            }).Start();
            waiter.WaitOne();
        }

        private void createDebugForm(Form targetForm)
        {
            var debugForm = new DebugForm(targetForm, this);
            debugForm.Run();
        }

        public void OnMessage(string text)
        {
            var message = JsonConvert.DeserializeObject<Api.Message>(text);
            try  {  form.Invoke((Action) (() => form.Update(message) )); }
            catch (InvalidOperationException) {}
            catch (NullReferenceException)  {}
        }
    }
}
