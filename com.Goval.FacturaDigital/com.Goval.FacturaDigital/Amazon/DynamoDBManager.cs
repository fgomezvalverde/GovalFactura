using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace com.Goval.FacturaDigital.Amazon
{
    public class DynamoDBManager
    {
        private static DynamoDBManager INSTANCE = new DynamoDBManager();
        private static CognitoAWSCredentials CREDENTIALS;
        private static AmazonDynamoDBClient DYNAMO_DB_CLIENT;
        private static DynamoDBContext DDB_CONTEXT;
        private DynamoDBManager()
        {
            CREDENTIALS = new CognitoAWSCredentials(AmazonConstants.COGNITO_IDENTITY_POOL_ID,
                AmazonConstants.COGNITO_REGION);
            DYNAMO_DB_CLIENT = new AmazonDynamoDBClient(CREDENTIALS, AmazonConstants.DYNAMODB_REGION);
            DDB_CONTEXT = new DynamoDBContext(DYNAMO_DB_CLIENT);
        }

        public static DynamoDBManager GetInstance()
        {
            if (INSTANCE == null)
            {
                INSTANCE = new DynamoDBManager();
            }
            return INSTANCE;
        }

        public async Task SaveAsync<T>(T pNewObject)
        {
            await DDB_CONTEXT.SaveAsync<T>(pNewObject);
        }

        public void Save<T>(T pNewObject)
        {
             DDB_CONTEXT.SaveAsync<T>(pNewObject);
        }

        public async Task Delete<T>(T pDeleteObject)
        {
            await DDB_CONTEXT.DeleteAsync<T>(pDeleteObject);
        }

        public async Task<List<T>> GetItemsAsync<T>()
        {
            List<ScanCondition> conditions = new List<ScanCondition>();
            var search = DDB_CONTEXT.ScanAsync<T>(conditions);
            return await search.GetNextSetAsync();
        }
       
    }
}
