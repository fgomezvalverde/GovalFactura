using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace com.Goval.FacturaDigital.Amazon
{
    public class DynamoDBManager
    {
        private static DynamoDBManager INSTANCE = new DynamoDBManager();
        /*private static CognitoAWSCredentials CREDENTIALS;
        private static AmazonDynamoDBClient DYNAMO_DB_CLIENT;
        private static DynamoDBContext DDB_CONTEXT;*/
        private DynamoDBManager()
        {
            /*CREDENTIALS = new CognitoAWSCredentials(AmazonConstants.COGNITO_IDENTITY_POOL_ID,
                AmazonConstants.COGNITO_REGION);
            DYNAMO_DB_CLIENT = new AmazonDynamoDBClient(CREDENTIALS, AmazonConstants.DYNAMODB_REGION);
            DDB_CONTEXT = new DynamoDBContext(DYNAMO_DB_CLIENT);*/
        }

        public static DynamoDBManager GetInstance()
        {
            if (INSTANCE == null)
            {
                INSTANCE = new DynamoDBManager();
            }
            return INSTANCE;
        }

        public async Task<Boolean> SaveAsync<T>(T pNewObject)
        {
            try
            {
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DynamoDBManager.SaveAsync:" + ex.Message);
                return false;
            }
            
        }


        public async Task<Boolean> Delete<T>(T pDeleteObject)
        {
            try
            {
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DynamoDBManager.Delete:"+ex.Message);
                return false;
            }
            
        }

        public async Task<List<T>> GetItemsAsync<T>()
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DynamoDBManager.GetItemsAsync:" + ex.Message);
                return null;
            }
           
        }

    }
}
