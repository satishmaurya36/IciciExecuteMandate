using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace IciciExecuteMandate
{   
    public class MandateRecord
    {
        public string SmCode { get; set; }
        public string MerchantTranId { get; set; }
        public decimal DebitAmount { get; set; }
        public string UMN { get; set; }
        public string Status { get; set; }
        public DateTime ExecutionDate { get; set; }
    }

    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
    }

    public class SmCodeWrapper
    {
        public string SmCode { get; set; }
    }

    public class MandateExecutor
    {
        public async Task ExecuteAsync(Action<string> log)
        {
            using (HttpClient client = new HttpClient())
            {
                string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJJZCI6IjE1OSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJTQVRJU0ggTUFVUllBIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoiZG90bmV0ZGV2MUBwYWlzYWxvLmluIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxNTkiLCJDcmVhdG9yIjoiQWdyYSIsIkVtcENvZGUiOiJQRExBMDAwMTAxIiwidG9rZW5WZXJzaW9uIjoiMTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL2V4cGlyYXRpb24iOiJEZWMgVHVlIDE2IDIwMjUgMDQ6Mjc6MTIgQU0iLCJuYmYiOjE3NjU3NzI4MzIsImV4cCI6MTc2NTc4MDAzMiwiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NzE4OCIsImF1ZCI6Imh0dHBzOi8vbG9jYWxob3N0OjcxODgifQ.tf9dZIIhIA4X4B_Fq3YQ80mvlRqX37zQ9EN8SaXgHok";

                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                //string notificationUrl = $"https://apiuat.paisalo.in:4015/collection/api/ICICIMandate/GetNotificationExcelData";
                string notificationUrl = $"https://localhost:7170/api/ICICIMandate/GetNotificationExcelData";
                try
                {
                    log("Fetching SMcodes for notification...");
                    var notifResponse = await client.GetAsync(notificationUrl);
                    notifResponse.EnsureSuccessStatusCode();

                    var notifContent = await notifResponse.Content.ReadAsStringAsync();
                    var notifApiResponse = JsonConvert.DeserializeObject<ApiResponse>(notifContent);
                    var smcodeList = JsonConvert.DeserializeObject<List<SmCodeWrapper>>(notifApiResponse?.Data ?? "[]");

                    log($"Found {smcodeList.Count} SMcodes for notification.");

                    foreach (var sm in smcodeList)
                    {
                        if (string.IsNullOrWhiteSpace(sm.SmCode)) continue;

                        //string notifyUrl = $"https://apiuat.paisalo.in:4015/collection/api/ICICIMandate/MandateNotification?smcode={sm.SmCode}";
                        string notifyUrl = $"https://localhost:7170/api/ICICIMandate/MandateNotification?smcode={sm.SmCode}";
                        log($"Sending notification for SMcode: {sm.SmCode}");

                        var notifyResponse = await client.GetAsync(notifyUrl);
                        string notifyResult = await notifyResponse.Content.ReadAsStringAsync();
                        log($"Notification Response: {notifyResult}");
                    }
                }
                catch (Exception ex)
                {
                    log("Error during notification:");
                    log(ex.Message);
                }

                try
                {
                    //string executeMandateUrl = "https://apiuat.paisalo.in:4015/collection/api/ICICIMandate/SendForExecuteMandate"; 
                    string executeMandateUrl = "https://localhost:7170/api/ICICIMandate/SendForExecuteMandate"; 
                    log("Executing mandates (after sending notifications)...");
                    var execResponse = await client.GetAsync(executeMandateUrl);
                    execResponse.EnsureSuccessStatusCode();

                    var execContent = await execResponse.Content.ReadAsStringAsync();
                    log($"ExecuteMandate API Response:\n{execContent}");
                }
                catch (Exception ex)
                {
                    log("Error during mandate execution:");
                    log(ex.Message);
                }

                try
                {
                    log("Fetching yesterday's failed SMcodes for retry...");
                   //string getFailedUrl = $"https://apiuat.paisalo.in:4015/collection/api/ICICIMandate/GetSmforReExecute";
                   string getFailedUrl = $"https://localhost:7170/api/ICICIMandate/GetSmforReExecute";

                    var failedResponse = await client.GetAsync(getFailedUrl);
                    failedResponse.EnsureSuccessStatusCode();

                    var failedContent = await failedResponse.Content.ReadAsStringAsync();
                    var failedList = JsonConvert.DeserializeObject<List<SmCodeWrapper>>(failedContent);

                    log($"Found {failedList.Count} failed SMcodes from yesterday.");

                    foreach (var sm in failedList)
                    {
                        if (string.IsNullOrWhiteSpace(sm.SmCode)) continue;

                        //string retryUrl = $"https://apiuat.paisalo.in:4015/collection/api/ICICIMandate/ExecuteMandate?SmCode={sm.SmCode}";
                        string retryUrl = $"https://localhost:7170/api/ICICIMandate/ExecuteMandate?SmCode={sm.SmCode}";
                        log($"Calling RetryExecuteMandate for SmCode: {sm.SmCode}");

                        var retryResponse = await client.PostAsync(retryUrl, null);
                        retryResponse.EnsureSuccessStatusCode();

                        string retryContent = await retryResponse.Content.ReadAsStringAsync();
                        log($"RetryExecuteMandate Response:\n{retryContent}");
                    }
                }
                catch (Exception ex)
                {
                    log("Error during batch RetryExecuteMandate call:");
                    log(ex.Message);
                }

            }
        }
    }
}
