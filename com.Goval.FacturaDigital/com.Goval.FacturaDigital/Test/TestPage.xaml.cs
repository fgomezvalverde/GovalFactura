﻿using com.Goval.FacturaDigital.Abstraction.DependencyServices;
using com.Goval.FacturaDigital.BusinessProxy.Authentication;
using com.Goval.FacturaDigital.BusinessProxy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace com.Goval.FacturaDigital.Test
{
    //[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage : ContentPage
    {
        public TestPage()
        {
            InitializeComponent();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            /*DependencyService.Get<IMailService>().SendMail("Prueba", "Esto es una prueba",
                new List<string>() { "fabiangomezvalverde@gmail.com" });*/
            App.ShowLoading(true);
            try
            {
                ValidateUserClient client = new ValidateUserClient();
                var request = new LoginRequest {UserName="fgomezvalverde",Password="fgomezvalverde" };

                var result = await client.GetDataAsync(request, "http://192.168.1.104:8081");

                /*var billList = await DynamoDBManager.GetInstance().GetItemsAsync<Model.Bill>();
                foreach (var bill in billList)
                {
                    bill.UserId = 302680516;
                    await DynamoDBManager.GetInstance().SaveAsync<Model.Bill>(bill);
                }
                var clientList = await DynamoDBManager.GetInstance().GetItemsAsync<Model.Client>();
                foreach (var client in clientList)
                {
                    client.UserId = 302680516;
                    await DynamoDBManager.GetInstance().SaveAsync<Model.Client>(client);
                }
                var productList = await DynamoDBManager.GetInstance().GetItemsAsync<Model.Product>();
                foreach (var product in productList)
                {
                    product.UserId = 302680516;
                    await DynamoDBManager.GetInstance().SaveAsync<Model.Product>(product);
                }*/
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
            App.ShowLoading(false);
        }
    }
}