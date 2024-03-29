﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Tigets.Core.Models;
using Tigets.Core.Repositories;
using Tigets.Core.Specifications;


namespace Tigets.Core.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ITransferService _transferService;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public TicketService(
            ITicketRepository ticketRepository,
            ITransferService transferService,
            UserManager<User> userManager,
            IMapper mapper
        )
        {
            _ticketRepository = ticketRepository;
            _transferService = transferService;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task Import(string username, TicketPostModel ticketPostModel)
        {
            if (username is null)
                throw new ArgumentNullException($"{nameof(username)}");

            if (ticketPostModel is null)
                throw new ArgumentNullException($"{nameof(ticketPostModel)}");
            ValidateTicketPostModel(ticketPostModel);

            var user = await _userManager.FindByNameAsync(username);

            if (user is null)
                throw new Exception($"Cannot find user {username}");

            var ticket = _mapper.Map<Ticket>(ticketPostModel);
            ticket.Id = Guid.NewGuid().ToString();
            ticket.UserId = user.Id;

            await _transferService.Create(user.Id, user.Id, ticket.Id, ticket.Cost);
            await _ticketRepository.AddAsync(ticket);
        }

        public async Task Buy(string username, string ticketId)
        {
            if (username is null)
                throw new ArgumentNullException($"{nameof(username)}");

            if (ticketId is null)
                throw new ArgumentNullException($"{nameof(ticketId)}");

            var ticket = await _ticketRepository.GetByIdAsync(ticketId) ?? throw new Exception("Ticket does not exist.");

            if (ticket.State == TicketState.OffMarket)
                throw new Exception("User cannot buy a ticket that is off the market.");

            var now = DateTime.UtcNow;
            if (now >= ticket.ValidTo)
                throw new Exception("This event has already ended.");

            var owner = await _userManager.FindByIdAsync(ticket.UserId) ?? throw new Exception("Owner does not exist.");
            var user = await _userManager.FindByNameAsync(username) ?? throw new Exception("User does not exist.");

            if (owner.Id == user.Id)
                throw new Exception("User cannot buy a ticket that already belongs to him.");

            if (user.Balance < ticket.Cost)
                throw new Exception("User does not have enough money to buy this ticket.");

            ticket.UserId = user.Id;
            user.Balance -= ticket.Cost;
            owner.Balance += ticket.Cost;

            // Useless cast to fullfil requirements :)
            int newState = (int)TicketState.OffMarket;
            ticket.State = (TicketState)newState;

            await _transferService.Create(user.Id, owner.Id, ticket.Id, ticket.Cost);
        }

        public async Task Move(string username, string ticketId, TicketState state)
        {
            ArgumentNullException.ThrowIfNull(username, nameof(username));
            ArgumentNullException.ThrowIfNull(ticketId, nameof(ticketId));

            var ticket = await _ticketRepository.GetByIdAsync(ticketId) ?? throw new Exception("Ticket does not exist.");
            var user = await _userManager.FindByNameAsync(username) ?? throw new Exception("User does not exist.");

            if (ticket.UserId != user.Id)
                throw new Exception("User does not own this ticket.");

            ticket.State = state;

            await _ticketRepository.SaveChangesAsync();
        }

        private void ValidateTicketPostModel(TicketPostModel model)
        {
            if (model.ValidFrom > model.ValidTo)
                throw new Exception("Ticket time interval is not valid.");

            var now = DateTime.UtcNow;
            if (model.ValidTo <= now)
                throw new Exception("Ticket has expired.");

            if (model.Cost < 0)
                throw new Exception("Ticket cost must be positive.");
        }

        public async Task<IEnumerable<Ticket>> GetTicketsOnTheMarket(string username)
        {

            if (username is null)
                throw new ArgumentNullException($"{nameof(username)}");

            User user = await _userManager.FindByNameAsync(username);

            if (user is null)
                throw new Exception("User does not exist.");

            var tickets = await _ticketRepository.ListAsync(new TicketByOnTheMarketSpec(user.Id));

            return tickets;
        }

        public async Task<IEnumerable<Ticket>> GetUserTickets(string username)
        {
           if (username is null)
                throw new ArgumentNullException($"{nameof(username)}");

            User user = await _userManager.FindByNameAsync(username);

            if (user is null)
                throw new Exception("User does not exist.");
                    
            var tickets = from ticket in await _ticketRepository.ListAsync()
                          where ticket.UserId == user.Id 
                          select ticket;

            return tickets;
        }
    }
}