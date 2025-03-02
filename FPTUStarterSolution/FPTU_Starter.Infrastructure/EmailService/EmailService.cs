﻿using FPTU_Starter.Application.IEmailService;
using FPTU_Starter.Domain.EmailModel;
using MailKit.Net.Smtp;
using MimeKit;


namespace FPTU_Starter.Infrastructure.EmailService;

public class EmailService : IEmailService
{
    private readonly EmailConfig _emailConfig;

    public EmailService(EmailConfig emailConfig)
    {
        _emailConfig = emailConfig;
    }
    public void SendEmail(Message email)
    {
        try
        {
            var emailMessage = CreateEmailMessage(email);
            Send(emailMessage);

        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }
    private MimeMessage CreateEmailMessage(Message message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("email", _emailConfig.From));
        emailMessage.To.AddRange(message.To);
        emailMessage.Subject = message.Subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Content };

        return emailMessage;

    }
    private void Send(MimeMessage mailMessage)
    {
        using var client = new SmtpClient();
        try
        {
            client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, true);
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            client.Authenticate(_emailConfig.UserName, _emailConfig.Password);

            client.Send(mailMessage);
        }
        catch
        {
            //log an error message or throw an exception or both.
            throw;
        }
        finally
        {
            client.Disconnect(true);
            client.Dispose();
        }
    }
}
