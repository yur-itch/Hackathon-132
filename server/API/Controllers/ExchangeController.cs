using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PlantCare.Api.Dtos;
using PlantCare.Api.Hubs;
using PlantCare.Api.Models;
using PlantCare.Api.Services.Interfaces;

namespace PlantCare.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/exchange")]
public sealed class ExchangeController : ControllerBase
{
    private readonly IExchangeService _exchangeService;
    private readonly IHubContext<ChatHub> _hubContext;

    public ExchangeController(
        IExchangeService exchangeService,
        IHubContext<ChatHub> hubContext)
    {
        _exchangeService = exchangeService;
        _hubContext = hubContext;
    }

    [HttpGet("offers")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyCollection<ExchangeOfferDto>>> GetOffers()
    {
        var offers = await _exchangeService.GetActiveOffersAsync();
        return Ok(offers.Select(ToDto).ToList());
    }

    [HttpGet("offers/{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ExchangeOfferDto>> GetOfferById(Guid id)
    {
        var offer = await _exchangeService.GetOfferByIdAsync(id);
        return offer is null ? NotFound() : Ok(ToDto(offer));
    }

    [HttpPost("offers")]
    public async Task<ActionResult<ExchangeOfferDto>> CreateOffer(
        [FromBody] CreateExchangeOfferDto dto)
    {
        var (result, offer) = await _exchangeService.CreateOfferAsync(
            this.GetOwnerId(),
            dto.Title,
            dto.Description,
            dto.WantedPlantId,
            dto.UserPlantId);

        switch (result)
        {
            case CreateOfferResult.OfferedPlantNotFound:
                return BadRequest("Выбранное растение не найдено в вашей коллекции.");
            case CreateOfferResult.WantedPlantNotFound:
                return BadRequest("Желаемое растение не найдено в справочнике.");
        }

        var offerDto = ToDto(offer!);
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
        // Писать в чат может владелец объявления или тот, у кого есть желаемое растение
        if (!await _exchangeService.CanUserAccessChatAsync(id, this.GetOwnerId()))
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                "Чтобы участвовать в обмене, нужное растение должно быть в вашей коллекции.");
        }

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

        var groupName = ChatHub.GetGroupName(id.ToString(), messageDto.SenderId, messageDto.ReceiverId);
        await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", messageDto);

        return Created($"/api/exchange/offers/{id}/messages/{messageDto.Id}", messageDto);

    }

    [HttpGet("offers/{id:guid}/messages")]
    public async Task<ActionResult<IReadOnlyCollection<ChatMessageDto>>> GetMessages(
        Guid id,
        [FromQuery] string otherUserId)
    {
        if (!await _exchangeService.CanUserAccessChatAsync(id, this.GetOwnerId()))
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                "Чтобы участвовать в обмене, нужное растение должно быть в вашей коллекции.");
        }

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
        var result = await _exchangeService.ConfirmExchangeAsync(
            id,
            this.GetOwnerId(),
            dto.OtherUserId);

        return result switch
        {
            ConfirmExchangeResult.Confirmed => NoContent(),
            ConfirmExchangeResult.OfferNotFound => NotFound("Объявление не найдено или уже закрыто."),
            ConfirmExchangeResult.OfferedPlantMissing => Conflict("Вашего растения больше нет в коллекции — обмен невозможен."),
            ConfirmExchangeResult.WantedPlantMissing => Conflict("У собеседника больше нет нужного растения."),
            _ => NotFound()
        };
    }

    private static ExchangeOfferDto ToDto(ExchangeOffer offer)
    {
        return new ExchangeOfferDto(
            offer.Id,
            offer.OwnerId,
            offer.Title,
            offer.Description,
            offer.UserPlantId,
            offer.UserPlant?.Plant?.Name ?? offer.UserPlant?.Nickname,
            offer.UserPlant?.Plant?.ImageUrl,
            offer.WantedPlantId,
            offer.WantedPlant?.Name,
            offer.WantedPlant?.ImageUrl,
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
