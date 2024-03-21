using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Interface
{
    public interface ITestDynamoDBConnection
    {
        public Task TestConnection();
    }
}