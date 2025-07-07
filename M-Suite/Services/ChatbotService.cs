using Microsoft.ML;
using M_Suite.Models.AI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using M_Suite.Data;
using Microsoft.AspNetCore.Hosting;

namespace M_Suite.Services
{
    public class ChatbotService
    {
        private readonly MSuiteContext _context;
        private readonly MLContext _mlContext;
        private ITransformer _model = null!;
        private readonly string _modelPath;
        private readonly Dictionary<string, BotResponse> _responses;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ChatbotService(MSuiteContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _mlContext = new MLContext(seed: 0);
            
            // Use WebRootPath to get the absolute path to wwwroot folder
            string wwwrootPath = _webHostEnvironment.WebRootPath;
            string modelsDirectory = Path.Combine(wwwrootPath, "models");
            _modelPath = Path.Combine(modelsDirectory, "chatbot_intent_model.zip");
            
            // Create directory if it doesn't exist
            try 
            {
                if (!Directory.Exists(modelsDirectory))
                {
                    Directory.CreateDirectory(modelsDirectory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating model directory: {ex.Message}");
            }
            
            // Initialize responses
            _responses = InitializeResponses();
            
            // Train or load model
            try
            {
                if (File.Exists(_modelPath))
                {
                    LoadModel();
                }
                else
                {
                    TrainModel();
                }

                // Ensure _model is initialized
                if (_model == null)
                {
                    TrainModel();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing chatbot model: {ex.Message}");
                // Continue without crashing - we'll handle null model in the methods
            }
        }

        private Dictionary<string, BotResponse> InitializeResponses()
        {
            return new Dictionary<string, BotResponse>
            {
                ["greeting"] = new BotResponse 
                { 
                    Intent = "greeting", 
                    Response = "Hello! How can I help you with M-Suite today?",
                    RequiredEntities = Array.Empty<string>()
                },
                ["help"] = new BotResponse 
                { 
                    Intent = "help", 
                    Response = "I can help you with: creating transactions, checking inventory, finding related items, and navigating the system.",
                    RequiredEntities = Array.Empty<string>()
                },
                ["inventory_check"] = new BotResponse 
                { 
                    Intent = "inventory_check", 
                    Response = "To check inventory, please specify an item name or code.",
                    RequiredEntities = new[] { "item" },
                    DynamicResponseGenerator = (entities) => 
                    {
                        if (entities.ContainsKey("item"))
                        {
                            return $"I'll check the inventory for {entities["item"]} for you.";
                        }
                        return "Please specify which item you want to check inventory for.";
                    }
                },
                ["create_transaction"] = new BotResponse 
                { 
                    Intent = "create_transaction", 
                    Response = "To create a transaction, go to Transactions > New Transaction, or I can help guide you through the process.",
                    RequiredEntities = Array.Empty<string>()
                },
                ["item_info"] = new BotResponse 
                { 
                    Intent = "item_info", 
                    Response = "Please specify which item you need information about.",
                    RequiredEntities = new[] { "item" },
                    DynamicResponseGenerator = (entities) => 
                    {
                        if (entities.ContainsKey("item"))
                        {
                            return $"Let me get information about {entities["item"]} for you.";
                        }
                        return "Please specify which item you want information about.";
                    }
                },
                ["related_items"] = new BotResponse 
                { 
                    Intent = "related_items", 
                    Response = "I can show you items that are frequently purchased together. Please specify an item.",
                    RequiredEntities = new[] { "item" },
                    DynamicResponseGenerator = (entities) => 
                    {
                        if (entities.ContainsKey("item"))
                        {
                            return $"Let me find items that are frequently purchased with {entities["item"]}.";
                        }
                        return "Please specify which item you want to find related products for.";
                    }
                },
                ["goodbye"] = new BotResponse 
                { 
                    Intent = "goodbye", 
                    Response = "Goodbye! Let me know if you need help with anything else.",
                    RequiredEntities = Array.Empty<string>()
                },
                ["thanks"] = new BotResponse
                {
                    Intent = "thanks",
                    Response = "You're welcome! Is there anything else I can help you with?",
                    RequiredEntities = Array.Empty<string>()
                },
                ["unknown"] = new BotResponse
                {
                    Intent = "unknown",
                    Response = "I'm not sure I understand. Can you rephrase your question?",
                    RequiredEntities = Array.Empty<string>()
                }
            };
        }

        private void TrainModel()
        {
            // Training data
            var trainingData = new List<ChatIntent>
            {
                // Greetings
                new ChatIntent("hello", "greeting"),
                new ChatIntent("hi", "greeting"),
                new ChatIntent("hey", "greeting"),
                new ChatIntent("good morning", "greeting"),
                new ChatIntent("good afternoon", "greeting"),
                new ChatIntent("good evening", "greeting"),
                
                // Help
                new ChatIntent("help", "help"),
                new ChatIntent("what can you do", "help"),
                new ChatIntent("how can you help me", "help"),
                new ChatIntent("what are your features", "help"),
                new ChatIntent("I need assistance", "help"),
                
                // Inventory
                new ChatIntent("check inventory", "inventory_check"),
                new ChatIntent("how many items do we have", "inventory_check"),
                new ChatIntent("check stock", "inventory_check"),
                new ChatIntent("stock levels", "inventory_check"),
                new ChatIntent("available inventory", "inventory_check"),
                new ChatIntent("warehouse stock", "inventory_check"),
                
                // Transactions
                new ChatIntent("create transaction", "create_transaction"),
                new ChatIntent("new transaction", "create_transaction"),
                new ChatIntent("start order", "create_transaction"),
                new ChatIntent("create order", "create_transaction"),
                new ChatIntent("make sale", "create_transaction"),
                new ChatIntent("new sale", "create_transaction"),
                
                // Item info
                new ChatIntent("item information", "item_info"),
                new ChatIntent("product details", "item_info"),
                new ChatIntent("tell me about item", "item_info"),
                new ChatIntent("item details", "item_info"),
                new ChatIntent("product info", "item_info"),
                
                // Related items
                new ChatIntent("related items", "related_items"),
                new ChatIntent("similar products", "related_items"),
                new ChatIntent("what items go together", "related_items"),
                new ChatIntent("frequently bought together", "related_items"),
                new ChatIntent("product recommendations", "related_items"),
                new ChatIntent("suggest complementary items", "related_items"),
                
                // Goodbye
                new ChatIntent("goodbye", "goodbye"),
                new ChatIntent("bye", "goodbye"),
                new ChatIntent("see you", "goodbye"),
                new ChatIntent("talk to you later", "goodbye"),
                
                // Thanks
                new ChatIntent("thank you", "thanks"),
                new ChatIntent("thanks", "thanks"),
                new ChatIntent("appreciate it", "thanks"),
                new ChatIntent("thank you very much", "thanks")
            };

            // Create and train the model
            var data = _mlContext.Data.LoadFromEnumerable(trainingData);
            
            var pipeline = _mlContext.Transforms.Text.NormalizeText("NormalizedText", "Query")
                .Append(_mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "NormalizedText"))
                .Append(_mlContext.Transforms.Text.RemoveDefaultStopWords("Tokens"))
                .Append(_mlContext.Transforms.Text.ApplyWordEmbedding("Features", "Tokens", Microsoft.ML.Transforms.Text.WordEmbeddingEstimator.PretrainedModelKind.GloVe50D))
                .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", "Intent"))
                .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "PredictedLabel"));

            _model = pipeline.Fit(data);
            
            // Save the model
            _mlContext.Model.Save(_model, data.Schema, _modelPath);
        }

        private void LoadModel()
        {
            _model = _mlContext.Model.Load(_modelPath, out _);
        }

        public string GetIntent(string query)
        {
            try
            {
                if (_model == null)
                {
                    return "unknown";
                }
                
                var predictEngine = _mlContext.Model.CreatePredictionEngine<ChatIntent, ChatIntentPrediction>(_model);
                var prediction = predictEngine.Predict(new ChatIntent { Query = query });
                
                return prediction.Intent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting intent: {ex.Message}");
                return "unknown";
            }
        }

        public async System.Threading.Tasks.Task<ChatMessage> ProcessMessageAsync(string message, int userId)
        {
            try
            {
                // Identify intent
                string intent = GetIntent(message);
                
                // If uncertain about intent, use unknown
                if (string.IsNullOrEmpty(intent))
                {
                    intent = "unknown";
                }
                
                // Find response for intent
                if (!_responses.TryGetValue(intent, out var botResponse))
                {
                    botResponse = _responses["unknown"];
                }
                
                // Extract entities (simplified for now)
                var entities = ExtractEntities(message);
                
                // Generate response
                string response = botResponse.DynamicResponseGenerator != null && entities.Any() 
                    ? botResponse.DynamicResponseGenerator(entities) 
                    : botResponse.Response;
                
                // Create chat message
                var chatMessage = new ChatMessage
                {
                    UserMessage = message,
                    BotResponse = response,
                    Timestamp = DateTime.Now,
                    UserId = userId,
                    Intent = intent,
                    Confidence = 1.0f // Simplified, would normally use model confidence
                };
                
                // In a real implementation, you might save the message to the database here
                
                return chatMessage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
                return new ChatMessage
                {
                    UserMessage = message,
                    BotResponse = "Sorry, I'm having trouble understanding right now. Please try again later.",
                    Timestamp = DateTime.Now,
                    UserId = userId,
                    Intent = "error",
                    Confidence = 0.0f
                };
            }
        }

        private Dictionary<string, string> ExtractEntities(string message)
        {
            // Simplified entity extraction
            // In a real implementation, this would be more sophisticated
            var entities = new Dictionary<string, string>();
            
            // Very basic item entity extraction
            var lowerMessage = message.ToLower();
            
            // Check for specific words followed by potential item names
            string[] itemPrefixes = { "item", "product", "check", "about", "for", "with" };
            
            foreach (var prefix in itemPrefixes)
            {
                int idx = lowerMessage.IndexOf(prefix + " ");
                if (idx >= 0 && idx + prefix.Length + 1 < lowerMessage.Length)
                {
                    // Extract rest of the sentence after the prefix as potential item name
                    var rest = lowerMessage.Substring(idx + prefix.Length + 1);
                    
                    // Remove common stopwords at the beginning if they exist
                    foreach (var stopword in new[] { "the", "a", "an" })
                    {
                        if (rest.StartsWith(stopword + " "))
                        {
                            rest = rest.Substring(stopword.Length + 1);
                            break;
                        }
                    }
                    
                    // Get the first few words as the item name
                    var words = rest.Split(new[] { ' ', '.', ',', '?', '!' }, StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length > 0)
                    {
                        // Take up to 3 words as the item name
                        var itemName = string.Join(" ", words.Take(Math.Min(3, words.Length)));
                        entities["item"] = itemName;
                        break;
                    }
                }
            }
            
            return entities;
        }
    }
} 