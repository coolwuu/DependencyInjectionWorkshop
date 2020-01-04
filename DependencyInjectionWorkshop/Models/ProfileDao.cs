﻿using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public interface IProfile
    {
        string GetPasswordFromDb(string accountId);
    }

    public class ProfileDao : IProfile
    {
        public ProfileDao()
        {
        }

        public string GetPasswordFromDb(string accountId)
        {
            string dbPassword;
            using (var connection = new SqlConnection("my connection string"))
            {
                dbPassword = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return dbPassword;
        }
    }
}