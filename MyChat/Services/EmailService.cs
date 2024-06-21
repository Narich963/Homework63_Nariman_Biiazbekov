namespace MyChat.Services;

public class EmailService
{
    public void SendEmail(string email, string subject, string text)
    {
        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

        string server = "smtp.gmail.com";
        int port = 465;
        bool enableSsl = true;
        string from = "nariman13.02.2005@gmail.com";
        string password = "yajx ryli gdfb rnnm";
        string to = email;

        var message = new MimeKit.MimeMessage();
        message.From.Add(new MimeKit.MailboxAddress(name: "Nariman", from));
        message.To.Add(new MimeKit.MailboxAddress("Nariman", to));
        message.Subject = subject;
        message.Body = new MimeKit.TextPart("plain")
        {
            Text = text
        };

        using (var client = new MailKit.Net.Smtp.SmtpClient())
        {
            client.Connect(server, port, enableSsl);
            client.Authenticate(from, password);
            client.Send(message);
            client.Disconnect(true);
        }
    }
}
