using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace MauiMqtt
{
    public partial class MainPage : ContentPage
    {
        private MqttClient client;
        private StringBuilder messages;
        private string subscribedTopic;

        public MainPage()
        {
            InitializeComponent();
            messages = new StringBuilder();
            Txt_Server.Text = messages.ToString();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (client != null && client.IsConnected)
            {
                client.Disconnect();
                client = null;
            }
        }

        private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.Message);
            if (e.Topic == subscribedTopic)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    messages.AppendLine($"{e.Topic} = {message}");
                    Txt_Server.Text = messages.ToString();
                });
            }
        }

        private void Btn_Publish_Clicked(object sender, EventArgs e)
        {
            if (client != null && client.IsConnected)
            {
                string pubTopic = Txt_Topic.Text + "/pub";
                string message = Txt_Message.Text;

                // Publish the message
                client.Publish(pubTopic, Encoding.UTF8.GetBytes(message));

                // Automatically subscribe to the subscription topic
                subscribedTopic = Txt_Topic.Text + "/sub";
                client.Subscribe(new string[] { subscribedTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

                // Display the published message as if it was received
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    messages.AppendLine($"{pubTopic} = {message}");
                    Txt_Server.Text = messages.ToString();
                });
            }
            else
            {
                DisplayAlert("MQTT", "Client is not connected", "OK");
            }
        }

        private void Btn_Connect_Clicked(object sender, EventArgs e)
        {
            string broker = Txt_Broker.Text;
            int port = int.Parse(Txt_Port.Text);
            string username = Txt_Username.Text;
            string password = Txt_Password.Text;

            try
            {
                client = new MqttClient(broker, port, false, MqttSslProtocols.None, null, null);
                client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

                string clientId = Guid.NewGuid().ToString();
                client.Connect(clientId, username, password);
                if (client.IsConnected)
                {
                    DisplayAlert("MQTT", "Connected to broker", "OK");
                }
                else
                {
                    DisplayAlert("MQTT", "Failed to connect", "OK");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("MQTT", $"Connection failed: {ex.Message}", "OK");
            }
        }
    }
}
