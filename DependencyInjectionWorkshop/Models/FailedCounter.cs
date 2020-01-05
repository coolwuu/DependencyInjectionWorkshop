﻿using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailedCounter
    {
        void Reset(string accountId);
        void AddFailedCount(string accountId);
        bool GetAccountIsLocked(string accountId);
        int GetFailedCount(string accountId);
    }

    public class FailedCounter : IFailedCounter
    {
        private readonly HttpClient _httpClient;

        public FailedCounter()
        {
            _httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
        }

        public void Reset(string accountId)
        {
            var resetResponse = _httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public void AddFailedCount(string accountId)
        {
            var addFailedCountResponse = _httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public bool GetAccountIsLocked(string accountId)
        {
            var isLockedResponse = _httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            return isLockedResponse.Content.ReadAsAsync<bool>().Result;
        }

        public int GetFailedCount(string accountId)
        {
            var failedCountResponse =
                _httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;
            failedCountResponse.EnsureSuccessStatusCode();
            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }
    }
}