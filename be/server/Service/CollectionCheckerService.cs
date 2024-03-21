using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.Interface;

// namespace server.Service
// {
//     public class CollectionCheckerService : ICollectionChecker
//     {
//         public async Task<bool> CollectionExistsAsync(IMongoDatabase database, string collectionName)
//         {
//             var filter = new BsonDocument("name", collectionName);
//             var options = new ListCollectionNamesOptions { Filter = filter };
//             return (await database.ListCollectionNamesAsync(options)).Any();
//         }
//     }
// }