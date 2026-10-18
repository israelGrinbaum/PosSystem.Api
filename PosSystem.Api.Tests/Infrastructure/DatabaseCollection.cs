using System;
using System.Collections.Generic;
using System.Text;

namespace PosSystem.Api.Tests.Infrastructure
{
    [CollectionDefinition("Database")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }
}
