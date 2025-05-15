using Microsoft.ML.Data;
using System;
using System.Collections.Generic;

namespace M_Suite.Models.AI
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string UserMessage { get; set; }
        public string BotResponse { get; set; }
        public DateTime Timestamp { get; set; }
        public int UserId { get; set; }
        public string Intent { get; set; }
        public float Confidence { get; set; }
    }

    public class ChatIntent
    {
        [LoadColumn(0)]
        public string Query { get; set; }

        [LoadColumn(1)]
        public string Intent { get; set; }
        
        public ChatIntent() { }
        
        public ChatIntent(string query, string intent)
        {
            Query = query;
            Intent = intent;
        }
    }

    public class ChatIntentPrediction
    {
        public string Intent { get; set; }
        public float[] Score { get; set; }
    }

    public class BotResponse
    {
        public string Intent { get; set; }
        public string Response { get; set; }
        public string[] RequiredEntities { get; set; }
        public Func<Dictionary<string, string>, string> DynamicResponseGenerator { get; set; }
    }
} 