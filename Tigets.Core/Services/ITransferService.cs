﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tigets.Core.Models;

namespace Tigets.Core.Services
{
    public interface ITransferService
    {
        Task Create(string buyerId, string? ownerId, string ticketId, decimal cost);
        Task<IEnumerable<Transfer>> GetTransfers(string ticketId);
    }
}