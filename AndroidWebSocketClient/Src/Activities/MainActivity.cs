using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Util;
using System.Collections.Generic;

namespace AndroidWebSocketClient.Src.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = true,
        LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Activity
    {
        public const string UriString = "ws://10.0.0.103:8181";

        private Button BtnConnect { get; set; }
        private Button BtnSend { get; set; }
        private Button BtnDisconnect { get; set; }

        private ClientWebSocket WebSocketClient { get; set; }
        private CancellationTokenSource CancelationToken { get; set; }

        private readonly Dictionary<WebSocketState, Tuple<bool, bool, bool>> StateButtons = new Dictionary<WebSocketState, Tuple<bool, bool, bool>>
        {
            { WebSocketState.Aborted, new Tuple<bool, bool, bool>(true, false, false) },
            { WebSocketState.Closed, new Tuple<bool, bool, bool>(true, false, false) },
            { WebSocketState.CloseReceived, new Tuple<bool, bool, bool>(true, false, false) },
            { WebSocketState.CloseSent, new Tuple<bool, bool, bool>(true, false, false) },
            { WebSocketState.Connecting, new Tuple<bool, bool, bool>(false, false, false) },
            { WebSocketState.None, new Tuple<bool, bool, bool>(true, false, false) },
            { WebSocketState.Open, new Tuple<bool, bool, bool>(false, true, true) }
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            BtnConnect = FindViewById<Button>(Resource.Id.btn_connect);
            BtnConnect.Click += BtnConnectClick;

            BtnDisconnect = FindViewById<Button>(Resource.Id.btn_disconnect);
            BtnDisconnect.Click += BtnDisconnectClick;

            BtnSend = FindViewById<Button>(Resource.Id.btn_send);
            BtnSend.Click += BtnSendClick;

            CancelationToken = new CancellationTokenSource();
            UpdateStateButtons();
        }

        private async void BtnConnectClick(object sender, EventArgs e)
        {
            try
            {
                if (WebSocketClient == null || WebSocketClient.State != WebSocketState.Open)
                {
                    WebSocketClient = new ClientWebSocket();
                }
                await WebSocketClient.ConnectAsync(new Uri(UriString), CancelationToken.Token);
                Listening();
                UpdateStateButtons();
            }
            catch (Exception ex)
            {
                Log.Debug(nameof(BtnConnectClick), ex.Message);
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                WebSocketClient.Dispose();
                WebSocketClient = null;
                UpdateStateButtons();
            }
        }

        private async void BtnDisconnectClick(object sender, EventArgs e)
        {
            try
            {

                if (WebSocketClient.IsConnected())
                {
                    await WebSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by user", CancelationToken.Token);
                    if (WebSocketClient.CloseStatus.HasValue)
                    {
                        Log.Debug(nameof(BtnDisconnectClick), WebSocketClient.CloseStatus.ToString());
                        Toast.MakeText(this, WebSocketClient.CloseStatus.ToString(), ToastLength.Long).Show();
                    }
                    BtnSend.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Log.Debug(nameof(BtnDisconnectClick), ex.Message);
            }
            finally
            {
                WebSocketClient.Dispose();
                WebSocketClient = null;
                UpdateStateButtons();
            }
        }

        private async void BtnSendClick(object sender, EventArgs e)
        {
            try
            {
                if (WebSocketClient.IsConnected())
                {
                    var message = $"{DateTime.Now.ToString("G")} {Guid.NewGuid()}";
                    await WebSocketClient.SendStringAsync(message, CancelationToken.Token);
                }
            }
            catch (Exception ex)
            {
                Log.Debug(nameof(BtnSendClick), ex.Message);
                WebSocketClient.Dispose();
                WebSocketClient = null;
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
            UpdateStateButtons();
        }

        private void Listening()
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    var message = await WebSocketClient.ReadStringAsync(CancelationToken.Token);
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        Log.Debug(nameof(Listening), "Message is null or white space");
                    }
                    else
                    {
                        RunOnUiThread(() => Toast.MakeText(this, message, ToastLength.Long).Show());
                    }
                }
            }, CancelationToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void UpdateStateButtons()
        {
            var states = WebSocketClient != null ?
                StateButtons[WebSocketClient.State] : StateButtons[WebSocketState.None];
            SetStateButtons(states);
        }

        private void SetStateButtons(Tuple<bool, bool, bool> states)
        {
            BtnConnect.Enabled = states.Item1;
            BtnSend.Enabled = states.Item2;
            BtnDisconnect.Enabled = states.Item3;
        }
    }
}