using BusinessLayer.Models.Outbound;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface IBookingService<Inbound, Outbound> 
        : IGenericService<Inbound, Outbound> where Inbound : class where Outbound : class
    {   

    }
}