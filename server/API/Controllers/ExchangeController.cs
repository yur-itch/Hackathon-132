using Microsoft.AspNetCore.Mvc;
using PlantCare.Api.Dtos;
using PlantCare.Api.Models;
using PlantCare.Api.Services.Interfaces;

namespace PlantCare.Api.Controllers;

[ApiController]
[Route("api/exchange")]
public sealed class ExchangeController : ControllerBase
{
    private readonly IExchangeService _exchangeService;

    public ExchangeController(IExchangeService exchangeService)
    {
        _exchangeService = exchangeService;
    }

    [HttpGet("offers")]
    public async Task<ActionResult<IReadOnlyCollection<ExchangeOfferDto>>> GetOffers(
        [FromQuery] string? search)
    {
        var offers = await _exchangeService.GetActiveOffersAsync(search);
        return Ok(offers.Select(ToDto).ToList());
    }

    [HttpGet("offers/{id:guid}")]
    public async Task<ActionResult<ExchangeOfferDto>> GetOfferById(Guid id)
    {
        var offer = await _exchangeService.GetOfferByIdAsync(id);
        return offer is null ? NotFound() : Ok(ToDto(offer));
    }

    [HttpPost("offers")]
    public async Task<ActionResult<ExchangeOfferDto>> CreateOffer(
        [FromBody] CreateExchangeOfferDto dto)
    {
        var offer = await _exchangeService.CreateOfferAsync(
            this.GetOwnerId(),
            dto.Title,
            dto.Description,
            dto.WantedPlantDescription,
            dto.UserPlantId);

        var offerDto = ToDto(offer);
        return CreatedAtAction(nameof(GetOfferById), new { id = offerDto.Id }, offerDto);
    }

    [HttpDelete("offers/{id:guid}")]
    public async Task<IActionResult> CloseOffer(Guid id)
    {
        var closed = await _exchangeService.CloseOfferAsync(id, this.GetOwnerId());
        return closed ? NoContent() : NotFound();
    }

    [HttpPost("offers/{id:guid}/messages")]
    public async Task<ActionResult<ChatMessageDto>> SendMessage(
        Guid id,
        [FromBody] SendChatMessageDto dto)
    {
        var message = await _exchangeService.SendMessageAsync(
            this.GetOwnerId(),
            id,
            dto.ReceiverId,
            dto.Text);

        if (message is null)
        {
            return NotFound("Exchange offer not found.");
        }

        var messageDto = ToDto(message);
        return Created($"/api/exchange/offers/{id}/messages/{messageDto.Id}", messageDto);
    }

    [HttpGet("offers/{id:guid}/messages")]
    public async Task<ActionResult<IReadOnlyCollection<ChatMessageDto>>> GetMessages(
        Guid id,
        [FromQuery] string otherUserId)
    {
        var messages = await _exchangeService.GetChatMessagesAsync(
            id,
            otherUserId,
            this.GetOwnerId());

        return Ok(messages.Select(ToDto).ToList());
    }

    [HttpGet("chats")]
    public async Task<ActionResult<IReadOnlyCollection<ChatDto>>> GetMyChats()
    {
        var chats = await _exchangeService.GetMyChatsAsync(this.GetOwnerId());

        return Ok(chats.Select(chat => new ChatDto(
            chat.Offer.Id,
            chat.Offer.Title,
            chat.OtherUserId,
            chat.OtherUserDisplayName,
            chat.LastMessage.SentAt,
            chat.LastMessage.Text)).ToList());
    }

    [HttpPost("offers/{id:guid}/confirm")]
    public async Task<IActionResult> ConfirmExchange(
        Guid id,
        [FromBody] ConfirmExchangeDto dto)
    {
        var confirmed = await _exchangeService.ConfirmExchangeAsync(
            id,
            this.GetOwnerId(),
            dto.OtherUserId);

        return confirmed ? NoContent() : NotFound();
    }

    private static ExchangeOfferDto ToDto(ExchangeOffer offer)
    {
        return new ExchangeOfferDto(
            offer.Id,
            offer.OwnerId,
            offer.Title,
            offer.Description,
            offer.WantedPlantDescription,
            offer.UserPlantId,
            offer.UserPlant?.Plant?.Name ?? offer.UserPlant?.Nickname,
            offer.UserPlant?.Plant?.ImageUrl,
            offer.CreatedAt,
            offer.IsActive);
    }

    private static ChatMessageDto ToDto(ChatMessage message)
    {
        return new ChatMessageDto(
            message.Id,
            message.ExchangeOfferId,
            message.SenderId,
            message.ReceiverId,
            message.Text,
            message.SentAt,
            message.IsRead);
    }
}
