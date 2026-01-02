using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IciciExecuteMandate
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
            this.ServiceName = "IciciExecuteMandate";

        }
        private System.Threading.Timer dailyTimer;
        private static readonly HttpClient httpClient = new HttpClient();

        protected override void OnStart(string[] args)
        {
            WriteToFile("Service is started at " + DateTime.Now);
            //timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            //timer.Interval = 5000; //number in milisecinds
            //timer.Enabled = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Set Bearer Token here if needed
            string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJJZCI6IjE1OSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJTQVRJU0ggTUFVUllBIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoiZG90bmV0ZGV2MUBwYWlzYWxvLmluIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxNTkiLCJDcmVhdG9yIjoiQWdyYSIsIkVtcENvZGUiOiJQRExBMDAwMTAxIiwidG9rZW5WZXJzaW9uIjoiMTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL2V4cGlyYXRpb24iOiJEZWMgRnJpIDEyIDIwMjUgMTI6MDc6NTQgUE0iLCJuYmYiOjE3NjU0NTQ4NzQsImV4cCI6MTc2NTQ2MjA3NCwiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NzE4OCIsImF1ZCI6Imh0dHBzOi8vbG9jYWxob3N0OjcxODgifQ.U08Cz8Y7mJwFOBKhViyg4eYNLiBhloQJTBSdaQmbky4";
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);



            ScheduleDailyTask();
        }

        protected override void OnStop()
        {
            WriteToFile("Service is stopped at" + DateTime.Now);

            dailyTimer?.Dispose();

        }
        //private void ScheduleDailyTask()
        //{
        //    // 11:00 AM Today
        //    DateTime now = DateTime.Now;
        //   // DateTime scheduledTime = DateTime.Today.AddHours(15); // 11:00 AM
        //    DateTime scheduledTime = DateTime.Today.AddMinutes(5); //Only Testing

        //    // If current time is past 11 AM, schedule for tomorrow
        //    if (now > scheduledTime)
        //        scheduledTime = scheduledTime.AddDays(1);

        //    TimeSpan timeToGo = scheduledTime - now;

        //    WriteToFile($"Next run scheduled at: {scheduledTime} (in {timeToGo.TotalMinutes:F0} minutes)");

        //    dailyTimer = new System.Threading.Timer(async state =>
        //    {
        //        await RunDailyTask();

        //        // After the task runs, reschedule for the next day
        //        ScheduleDailyTask();

        //    }, null, timeToGo, Timeout.InfiniteTimeSpan); // run once
        //}

        //private async Task RunDailyTask()
        //{
        //    WriteToFile("Task started at " + DateTime.Now);
        //    try
        //    {
        //        var executor = new MandateExecutor();
        //        await executor.ExecuteAsync(WriteToFile); // You pass the logger
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteToFile("Error: " + ex.ToString());
        //    }
        //    WriteToFile("Task finished at " + DateTime.Now);
        //}
        private void ScheduleDailyTask()
        {
            WriteToFile($"Service scheduled to run every 1 minutes. First run at: {DateTime.Now.AddMinutes(1)}");

            dailyTimer = new System.Threading.Timer(async state =>
            {
                await RunDailyTask();

            },
            null,
            TimeSpan.FromMinutes(1),       // Delay before first run
            TimeSpan.FromMinutes(1));      // Repeat frequency
        }

        private async Task RunDailyTask()
        {
            WriteToFile("Task started at " + DateTime.Now);
            try
            {
                var executor = new MandateExecutor();
                await executor.ExecuteAsync(WriteToFile);
            }
            catch (Exception ex)
            {
                WriteToFile("Error: " + ex.ToString());
            }
            WriteToFile("Task finished at " + DateTime.Now);
        }
        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\LOSDOC";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\LOSDOC\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}
