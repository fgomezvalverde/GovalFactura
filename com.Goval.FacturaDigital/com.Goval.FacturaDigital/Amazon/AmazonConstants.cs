using Amazon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.Amazon
{
    class AmazonConstants
    {
        public const string FB_APP_ID = "";
        public const string FB_APP_NAME = "My Contacts";

        public const string PROVIDER_NAME = "graph.facebook.com";
        public const string COGNITO_IDENTITY_POOL_ID = "us-east-1:3c10aa00-9040-4bae-923b-5175f77e76db";
        public static RegionEndpoint COGNITO_REGION = RegionEndpoint.USEast1;

        public static RegionEndpoint DYNAMODB_REGION = RegionEndpoint.USEast1;
    }
}
