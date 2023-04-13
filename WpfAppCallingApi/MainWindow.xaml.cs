using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfAppCallingApi.Models;

namespace WpfAppCallingApi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HttpClient httpClient = new HttpClient();
        string _message;

        public MainWindow()
        {
            httpClient.BaseAddress = new Uri("https://localhost:44317/api/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
                );
            InitializeComponent();
            lblMessage.Content = "";
            GetClients();
            GetTypes();
            GetSituations();
        }

        public class ResponseModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Cpf { get; set; }
            public int Type { get; set; }
            public int Gender { get; set; }
            public int Situation { get; set; }
            public string Message { get; set; }
        }
        ResponseModel _responseModel = new ResponseModel();

        #region API Methods
        private async void GetClients()
        {
            var responseClient = await httpClient.GetStringAsync("Client");
            var responseType = await httpClient.GetStringAsync("Type");
            var responseSituation = await httpClient.GetStringAsync("Situation");
            var clients = JsonConvert.DeserializeObject<List<Client>>(responseClient);
            var types = JsonConvert.DeserializeObject<List<Type_client>>(responseType);
            var situations = JsonConvert.DeserializeObject<List<Situation_client>>(responseSituation);
            List<ClientGridModel>clientGrid = new List<ClientGridModel>();

            foreach (var client in clients)
            {
                ClientGridModel clientGridObject = new ClientGridModel();
                clientGridObject.Id = client.Id;
                clientGridObject.Name = client.Name;
                clientGridObject.Cpf = client.Cpf;
                
                switch (client.Gender)
                {
                    case 1:
                        clientGridObject.Gender = "Masculino";
                        break;
                    case 2:
                        clientGridObject.Gender = "Feminino";
                        break;
                    case 3:
                        clientGridObject.Gender = "Outros";
                        break;
                    default:
                        clientGridObject.Gender = "NaN";
                        break;
                }


                var type = types.Where(x => x.Id == client.Type);
                clientGridObject.Type = type.FirstOrDefault().Type_des;

                var situation = situations.Where(x => x.Id == client.Situation);
                clientGridObject.Situation = situation.FirstOrDefault().Situation_des;

                clientGrid.Add(clientGridObject);
            }

            dgClient.DataContext = clientGrid;
            lblMessage.Content = _message;
        }

        private async void GetTypes()
        {
            var response = await httpClient.GetStringAsync("Type");
            var types = JsonConvert.DeserializeObject<List<Type_client>>(response);
            foreach ( var type in types )
            {
                cbType.Items.Add(type.Type_des);
            }
        }

        private async void GetSituations()
        {
            var response = await httpClient.GetStringAsync("Situation");
            var situations = JsonConvert.DeserializeObject<List<Situation_client>>(response);
            foreach (var situation in situations)
            {
                cbSituation.Items.Add(situation.Situation_des);
            }
        }

        private async void GetClientForUpdate(ClientGridModel clientGrid)
        {
            var response = await httpClient.GetStringAsync("Client/" + clientGrid.Id);
            var client = JsonConvert.DeserializeObject<Client>(response);

            txtClientId.Text = client.Id.ToString();
            txtName.Text = client.Name;
            txtCpf.Text = client.Cpf;
            cbType.SelectedIndex = client.Type - 1;
            cbGender.SelectedIndex = client.Gender - 1;
            cbSituation.SelectedIndex = client.Situation -1;
        }

        private async void InsertClient(Client client)
        {
            var response = await httpClient.PostAsJsonAsync("Client", client);
            _responseModel = JsonConvert.DeserializeObject<ResponseModel>(response.Content.ReadAsStringAsync().Result);
            _message = _responseModel.Message;
            GetClients();
        }
        private async void UpdateClient(Client client)
        {
            var response = await httpClient.PutAsJsonAsync("Client/" + client.Id, client);
            _responseModel = JsonConvert.DeserializeObject<ResponseModel>(response.Content.ReadAsStringAsync().Result);
            _message = _responseModel.Message;
            GetClients();
        }
        private async void DeleteClient(int clientId)
        {
            var response = await httpClient.DeleteAsync("Client/" + clientId);
            _message = response.Content.ReadAsStringAsync().Result;
            GetClients();
        }
        #endregion API Methods

        #region Button Methods
        private void btnInsertClient_Click(object sender, RoutedEventArgs e)
        {

            var client = new Client()
            {
                Id = Convert.ToInt32(txtClientId.Text),
                Name = txtName.Text,
                Cpf = txtCpf.Text.Trim().Replace(".", "").Replace("-", ""),
                Type = cbType.SelectedIndex + 1,
                Gender = cbGender.SelectedIndex + 1,
                Situation = cbSituation.SelectedIndex + 1
            };

            if (client.Id == 0)
            {
                this.InsertClient(client);
                lblMessage.Content = _message;
            }
            else
            {
                this.UpdateClient(client);
                lblMessage.Content = _message;
            }

            txtClientId.Text = 0.ToString();
            txtName.Clear();
            txtCpf.Clear();
            
        }

        void btnEditClient(object sender, RoutedEventArgs e)
        {
            ClientGridModel client = ((FrameworkElement)sender).DataContext as ClientGridModel;
            this.GetClientForUpdate(client);
        }

        void btnDeleteClient(object sender, RoutedEventArgs e)
        {
            ClientGridModel client = ((FrameworkElement)sender).DataContext as ClientGridModel;
            this.DeleteClient(client.Id);
        }

        #endregion Button Methods
        
    }
}
