using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using DefaultNamespace;
using TMPro;
using UnityEngine;

namespace ErrorManagement
{
    public class ErrorLogger : MonoBehaviour
    {
        public static ErrorLogger Instance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            StartCoroutine(SendLog());
        }

        private IEnumerator SendLog()
        {
            yield return new WaitForSeconds(5f);
            
            var logFilePath = SaveErrorLog();
            SendEmail("steve97harris@hotmail.co.uk", "T0ttenham!" ,"steve97harris@hotmail.co.uk", "FCL Log", "Error Log: " + $"{DateTime.Now:dd-MMM-yyyy}", logFilePath);
        }

        private string SaveErrorLog()
        {
            var logPathA = CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low", Application.companyName, Application.productName, "Player.log");
            
            var logPathB = @"C:\Users\Steve\Documents\ErrorLogs\FCL\logs" + @"\log-" + $"{DateTime.Now:dd-MMM-yyyy}" + ".log";
            Debug.Log(logPathB);
            
            File.Copy(logPathA, logPathB, true);
            return logPathB;
        }

        private void SendEmail(string senderEmail,string senderPassword, string receiverEmail,string subject, string content, string attachmentPath)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail),
                To = { receiverEmail },
                Subject = subject,
                Body = content,
                Attachments = { new Attachment(attachmentPath)}
            };

            var smtpClient = new SmtpClient("smtp.outlook.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(senderEmail, senderPassword) as ICredentialsByHost,
                EnableSsl = true
            };
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
            
            smtpClient.Send(mail);
            Debug.Log("Email Sent!");
        }
        
        private string CombinePaths(params string[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }
            return paths.Aggregate(Path.Combine);
        }
    }
}