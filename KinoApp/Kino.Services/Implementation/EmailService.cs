﻿using Kino.Domain;
using Kino.Domain.DomainModels;
using Kino.Services.Interface;
using System.Net.Mail;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kino.Services.Implementation
{
    public class EmailService:IEmailService
    {
        private readonly EmailSettings settings;
        public EmailService(EmailSettings settings)
        {
            this.settings = settings;
        }

        public async Task SendEmailAsync(List<EmailMessage> allMails)
        {
            List<MimeMessage> messages = new List<MimeMessage>();
            foreach(var item in allMails)
            {
                var emailMessage = new MimeMessage
                {
                    Sender = new MailboxAddress(settings.SenderName, settings.SmtpUserName),
                    Subject = item.Subject
                };
                emailMessage.From.Add(new MailboxAddress(settings.EmailDisplayName, settings.SmtpUserName));
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Plain) { Text = item.Body };
                emailMessage.To.Add(new MailboxAddress(item.MailTo,item.MailTo));
                messages.Add(emailMessage);
            }
            try
            {
                using(var smtp=new MailKit.Net.Smtp.SmtpClient())
                {
                    var socketOpiton = settings.EnableSSL ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
                    await smtp.ConnectAsync(settings.SmtpServer, settings.SmtpServerPort, socketOpiton);
                    if (!string.IsNullOrEmpty(settings.SmtpUserName))
                    {
                        await smtp.AuthenticateAsync(settings.SenderName, settings.SmtpPassword);
                    }
                    foreach(var item in messages)
                    {
                        await smtp.SendAsync(item);
                    }
                    await smtp.DisconnectAsync(true);
                }
            }
            catch(SmtpException ex)
            {
                throw ex;
            }
        }
    }
}