﻿using com.Goval.FacturaDigital.Amazon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace com.Goval.FacturaDigital.Pages.Client
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ClientDetail : ContentPage
    {
        Model.Client ActualClient;
        public ClientDetail(Model.Client pClient)
        {
            InitializeComponent();
            ActualClient = pClient;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await SetProducts();
            this.BindingContext = ActualClient;
        }

        private async void SaveClient_Clicked(object sender, EventArgs e)
        {
            var NewClient = this.BindingContext as Model.Client;
            if (NewClient != null && !string.IsNullOrEmpty(NewClient.ClientId) && !string.IsNullOrEmpty(NewClient.Name))
            {
                try
                {
                    NewClient.RemoveOnUsedProducts();
                    if (await DynamoDBManager.GetInstance().SaveAsync<Model.Client>(
                     NewClient
                    ))
                    {
                        await DisplayAlert("Sistema", "Se han guardado los cambios", "ok");
                        this.SendBackButtonPressed();
                    }
                    else
                    {
                        await DisplayAlert("Sistema", "Se ha producido un error al contactar el servicio", "ok");
                    }
                    
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Sistema", ex.Message, "ok");
                }

            }
            else
            {
                await DisplayAlert("Sistema", "Alguno de los datos falta por rellenar", "ok");
            }

        }

        private async void DeleteClient_Clicked(object sender, EventArgs e)
        {
            var deleteClient = this.BindingContext as Model.Client;
            var answer = await DisplayAlert("Sistema", "Estas seguro que deseas eliminar el item", "Si", "No");
            if (deleteClient != null && answer)
            {
                if (await DynamoDBManager.GetInstance().Delete<Model.Client>(
                     deleteClient
                    ))
                {
                    await DisplayAlert("Sistema", "Se ha eliminado el item", "ok");
                    this.SendBackButtonPressed();
                }
                else
                {
                    await DisplayAlert("Sistema", "Se ha producido un error al contactar el servicio", "ok");
                }
                
            }
        }


        private async Task SetProducts()
        {
            var productList = await DynamoDBManager.GetInstance().GetItemsAsync<Model.Product>();
            if (productList != null || productList.Count != 0)
            {
                foreach (var product in ActualClient.Products)
                {
                    var findedProduct = productList.First(d => d.Code == product.Code);
                    if (findedProduct != null)
                    {
                        findedProduct.IsUsed = product.IsUsed;
                    }
                    else
                    {
                        await DisplayAlert("Sistema", "El item " + product.Description + " ha sido elimado, y ya era referencia a este cliente", "ok");
                    }
                }
                ActualClient.Products = productList;
            }
            else
            {
                await DisplayAlert("Sistema", "No se han podido traer los productos del server", "ok");
                
            }
        }


        

        private void ProductListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ProductListView.SelectedItem = null;
        }
    }
}