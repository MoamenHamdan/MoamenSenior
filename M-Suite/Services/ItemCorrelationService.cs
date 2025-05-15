using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using M_Suite.Data;
using M_Suite.Models;
using M_Suite.Models.AI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace M_Suite.Services
{
    public class ItemCorrelationService
    {
        private readonly MSuiteContext _context;
        private readonly MLContext _mlContext;
        private ITransformer _model = null!;
        private readonly string _modelPath = Path.Combine("wwwroot", "models", "item_correlation_model.zip");

        public ItemCorrelationService(MSuiteContext context)
        {
            _context = context;
            _mlContext = new MLContext(seed: 0);
            
            // Create directory if it doesn't exist
            Directory.CreateDirectory(Path.Combine("wwwroot", "models"));
            
            // Load or train the model
            if (File.Exists(_modelPath))
            {
                LoadModel();
            }
        }

        public async System.Threading.Tasks.Task TrainModelAsync()
        {
            // Get transaction data from the database
            var transactionItems = await _context.TransactionItems
                .AsNoTracking()
                .Select(ti => new { ti.TsiTsId, ti.TsiItId })
                .ToListAsync();

            // Group items by transaction
            var itemsByTransaction = transactionItems
                .GroupBy(ti => ti.TsiTsId)
                .Select(g => g.Select(ti => ti.TsiItId).Distinct().ToList())
                .Where(items => items.Count > 1) // Only consider transactions with more than one item
                .ToList();

            // Create item pairs from each transaction
            var itemPairs = new List<TransactionItemPair>();
            
            foreach (var transaction in itemsByTransaction)
            {
                for (int i = 0; i < transaction.Count; i++)
                {
                    for (int j = i + 1; j < transaction.Count; j++)
                    {
                        // Order item IDs to avoid duplicates (item1, item2) and (item2, item1)
                        int item1 = Math.Min(transaction[i] ?? 0, transaction[j] ?? 0);
                        int item2 = Math.Max(transaction[i] ?? 0, transaction[j] ?? 0);

                        var existingPair = itemPairs.FirstOrDefault(p => p.ItemId1 == (byte)item1 && p.ItemId2 == (byte)item2);
                        
                        if (existingPair != null)
                        {
                            existingPair.Frequency += 1;
                        }
                        else
                        {
                            itemPairs.Add(new TransactionItemPair
                            {
                                ItemId1 = (byte)item1,
                                ItemId2 = (byte)item2,
                                Frequency = 1
                            });
                        }
                    }
                }
            }

            // Prepare data for training
            var data = _mlContext.Data.LoadFromEnumerable(itemPairs);

            // Define pipeline
            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = nameof(TransactionItemPair.ItemId1),
                MatrixRowIndexColumnName = nameof(TransactionItemPair.ItemId2),
                LabelColumnName = nameof(TransactionItemPair.Frequency),
                NumberOfIterations = 20,
                ApproximationRank = 100
            };

            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(
                    inputColumnName: nameof(TransactionItemPair.ItemId1),
                    outputColumnName: "ItemId1Encoded")
                .Append(_mlContext.Transforms.Conversion.MapValueToKey(
                    inputColumnName: nameof(TransactionItemPair.ItemId2),
                    outputColumnName: "ItemId2Encoded"))
                .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(options));

            // Train model
            _model = pipeline.Fit(data);

            // Save model
            _mlContext.Model.Save(_model, data.Schema, _modelPath);
        }

        private void LoadModel()
        {
            _model = _mlContext.Model.Load(_modelPath, out _);
        }

        public async System.Threading.Tasks.Task<List<ItemCorrelationResult>> GetTopCorrelatedItemsAsync(int itemId, int topN = 5)
        {
            if (_model == null)
            {
                await TrainModelAsync();
            }

            // Get all items except the one we're finding correlations for
            var allItems = await _context.Items
                .AsNoTracking()
                .Where(i => i.ItId != itemId && i.ItActive == 1)
                .Select(i => i.ItId)
                .ToListAsync();

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<TransactionItemPair, ItemCorrelationPrediction>(_model);

            var predictions = new List<(int ItemId, float Score)>();

            // Make predictions for all possible pairs
            foreach (var otherItemId in allItems)
            {
                // Order item IDs to match the training data pattern
                int item1 = Math.Min(itemId, otherItemId);
                int item2 = Math.Max(itemId, otherItemId);

                var prediction = predictionEngine.Predict(new TransactionItemPair
                {
                    ItemId1 = (byte)item1,
                    ItemId2 = (byte)item2
                });

                predictions.Add((otherItemId, prediction.Score));
            }

            // Get top N correlated items
            var topItems = predictions
                .OrderByDescending(p => p.Score)
                .Take(topN)
                .ToList();

            // Get item details
            var itemDetails = await _context.Items
                .AsNoTracking()
                .Where(i => i.ItId == itemId || topItems.Select(t => t.ItemId).Contains(i.ItId))
                .Select(i => new { i.ItId, i.ItDescriptionLan1 })
                .ToListAsync();

            var mainItem = itemDetails.First(i => i.ItId == itemId);

            // Format results
            return topItems.Select(t => new ItemCorrelationResult
            {
                ItemId1 = itemId,
                ItemName1 = mainItem.ItDescriptionLan1,
                ItemId2 = t.ItemId,
                ItemName2 = itemDetails.First(i => i.ItId == t.ItemId).ItDescriptionLan1,
                CorrelationScore = t.Score
            }).ToList();
        }
    }
} 