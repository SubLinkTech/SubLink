using System;
using System.Net;
using System.Text;

namespace xyz.yewnyx.SubLink.Joystick.Client.OAuth;

internal sealed class WebServer(ushort port, string state) : IDisposable {
    private readonly string _state = state;
    private readonly string _redirectUri = $"http://localhost:{port}/authorize/";
    private HttpListener? _listener;

    public bool HasResponse { get; private set; } = false;
    public string TokenAuthCode { get; private set; } = string.Empty;

    public void Dispose() =>
        Stop();

    public void Start() {
        if (null != _listener) return;

        _listener = new();
        _listener.Prefixes.Add(_redirectUri);
        _listener.Start();
        ReceiveNext();
    }

    public void Stop() {
        if (null == _listener) return;

        _listener.Stop();
        _listener = null;
    }

    private void ReceiveNext() =>
        _listener?.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);

    private void ListenerCallback(IAsyncResult result) {
        if (null == _listener || !_listener.IsListening) return;
        string replyState = string.Empty;
        string authCode = string.Empty;
        bool hasCode = false;

        var context = _listener.EndGetContext(result);

        string requestString = context.Request.Url?.ToString() ?? string.Empty;
        int n = requestString.IndexOf('?') + 1;
        string queryString = requestString[n..];

        foreach (string query in queryString.Split('&')) {
            int isIdx = query.IndexOf('=');
            string value = query[..isIdx];
            string data = query[(isIdx + 1)..];

            switch (value) {
                case "code": {
                    authCode = data;
                    hasCode = true;
                    break;
                }
                case "state": {
                    replyState = data;
                    break;
                }
            }
        }

        bool statesMatch = replyState.Equals(_state, StringComparison.OrdinalIgnoreCase);
        var response = context.Response;
        response.ContentType = "text/plain";

        if (hasCode && !string.IsNullOrWhiteSpace(authCode) && statesMatch) {
            response.StatusCode = (int)HttpStatusCode.OK;
            response.OutputStream.Write(Encoding.ASCII.GetBytes("Authorised, you may now close this tab"));
            response.OutputStream.Close();

            TokenAuthCode = authCode;
            HasResponse = true;

            Stop();
            return;
        }

        string Result = "";

        if (!hasCode || string.IsNullOrWhiteSpace(authCode))
            Result += "Auth code not received\n";

        if (!statesMatch)
            Result += "States did not match.\n";

        response.StatusCode = (int)HttpStatusCode.InternalServerError;
        response.OutputStream.Write(Encoding.ASCII.GetBytes(Result));
        response.OutputStream.Close();
        ReceiveNext();
    }
}
