using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using M_Suite.Services;
using M_Suite.Models.AI;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace M_Suite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly ItemCorrelationService _itemCorrelationService;
        private readonly ChatbotService _chatbotService;

        public AIController(ItemCorrelationService itemCorrelationService, ChatbotService chatbotService)
        {
            _itemCorrelationService = itemCorrelationService;
            _chatbotService = chatbotService;
        }

        [HttpGet("correlations/{itemId}")]
        public async System.Threading.Tasks.Task<ActionResult<List<ItemCorrelationResult>>> GetItemCorrelations(int itemId, [FromQuery] int topN = 5)
        {
            if (itemId <= 0)
            {
                return BadRequest("Invalid item ID");
            }

            var correlations = await _itemCorrelationService.GetTopCorrelatedItemsAsync(itemId, topN);
            return Ok(correlations);
        }

        [HttpPost("chatbot")]
        public async System.Threading.Tasks.Task<ActionResult<ChatMessage>> ProcessChatMessage([FromBody] ChatMessageRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            var response = await _chatbotService.ProcessMessageAsync(request.Message, request.UserId);
            return Ok(response);
        }

        [HttpPost("correlations/train")]
        [Authorize(Roles = "Admin")]
        public async System.Threading.Tasks.Task<ActionResult> TrainCorrelationModel()
        {
            await _itemCorrelationService.TrainModelAsync();
            return Ok(new { message = "Item correlation model trained successfully" });
        }
    }

    public class ChatMessageRequest
    {
        public string Message { get; set; }
        public int UserId { get; set; }
    }
} 