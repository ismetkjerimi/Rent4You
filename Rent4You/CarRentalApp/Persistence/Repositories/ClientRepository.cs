﻿using System;
using CarRentalApp.Core.domain;
using CarRentalApp.Core.Repositories;

namespace CarRentalApp.Persistence.Repositories
{
    public class ClientRepository : Repository<Client, Guid> , IClientRepository
    {
        public ClientRepository(CarRentalAppContext context) : base(context)
        {
        }

        public CarRentalAppContext CarRentalAppContext => Context as CarRentalAppContext;

        public Client GetByResume(string resume)
        {
            var cin = Client.CinFromResume(resume);
           return SingleOrDefault(c => c.Cin == cin);
        }

    }

}