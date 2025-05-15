using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace M_Suite.Models.AI
{
    public class TransactionItemPair
    {
        [LoadColumn(0)]
        public byte ItemId1 { get; set; }

        [LoadColumn(1)]
        public byte ItemId2 { get; set; }

        [LoadColumn(2)]
        public float Frequency { get; set; }
    }

    public class ItemCorrelationPrediction
    {
        public float Score { get; set; }
    }

    public class ItemCorrelationResult
    {
        public int ItemId1 { get; set; }
        public string ItemName1 { get; set; }
        public int ItemId2 { get; set; }
        public string ItemName2 { get; set; }
        public float CorrelationScore { get; set; }
    }
} 