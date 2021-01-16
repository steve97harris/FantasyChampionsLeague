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

namespace DefaultNamespace
{
    public class ErrorLogger : MonoBehaviour
    {
        public static ErrorLogger Instance;

        private void OnEnable()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                if (Instance != this)
                {
                    Destroy(gameObject);
                }
            }
            DontDestroyOnLoad(gameObject.transform.parent.gameObject);
            
            if (EventPointsRetriever.EventDate != null)
                StartCoroutine(SendLog(EventPointsRetriever.EventDate));
            else
            {
                Debug.LogError("Event date returned null");
                StartCoroutine(SendLog("[unknown date]"));
            }
        }

        private IEnumerator SendLog(string date)
        {
            yield return new WaitForSeconds(7f);
            
            var logFilePath = SaveErrorLog();
            if (logFilePath != null)
                SendEmail("steve97harris@hotmail.co.uk", "_password_here_" ,"steve97harris@hotmail.co.uk", "FCL Log", "Error Log: " + date, logFilePath);
        }

        private string SaveErrorLog()
        {
            var logPathA = CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low", Application.companyName, Application.productName, "Player.log");
            Debug.Log(logPathA);
            
            var logPathB = @"C:\Users\Steve\Documents\ErrorLogs\FCL\logs" + @"\log-" + $"{DateTime.Now:dd-MMM-yyyy}" + ".log";
            Debug.Log(logPathB);

            if (!File.Exists(logPathA))
            {
                Debug.LogError("Log file not found, no log file sent");
                return null;
            }
            
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