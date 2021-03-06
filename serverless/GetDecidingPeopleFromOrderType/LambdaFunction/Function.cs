using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using CodeMash.Membership.Services;
using CodeMash.Repository;
using LambdaFunction.Inputs;
using HrApp.Entities;
using Isidos.CodeMash.ServiceContracts;
using MongoDB.Driver;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace LambdaFunction
{
    public class Function
    {
        public APIGatewayProxyResponse Handler(CustomEventRequest<CollectionTriggerInput> lambdaEvent)
        {
            var formerRecord = JsonConvert.DeserializeObject<WishlistEntity>(lambdaEvent.Input.FormerRecord);
            var newRecord = JsonConvert.DeserializeObject<WishlistEntity>(lambdaEvent.Input.NewRecord);

            var wishlistDb = new CodeMashRepository<WishlistEntity>(HrApp.Settings.Client);
            var orderRulesDb = new CodeMashRepository<WishlistDecisionRule>(HrApp.Settings.Client);
            var usersDb = new CodeMashMembershipService(HrApp.Settings.Client);

            var orderRules = orderRulesDb.Find().Items;
            var allUsers = usersDb.GetUsersList(new GetUsersRequest());
            var orderType = newRecord.OrderType;

            var roles = orderRules.First(x => x.Type == orderType).Roles;

            var shouldBeApprovedByList = (from role in roles
                                          from user in allUsers.Result
                                          where user.Roles.Any(x => x.Name == role)
                                          select user.Id).ToList();

            newRecord.ShouldBeApprovedBy = shouldBeApprovedByList.ToList();

            wishlistDb.ReplaceOne(x => x.Id == formerRecord.Id, newRecord);

            return new APIGatewayProxyResponse
            {
                Body = JsonConvert.SerializeObject(newRecord.ShouldBeApprovedBy),
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}