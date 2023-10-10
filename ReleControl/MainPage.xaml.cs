using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System.Text;

namespace ReleControl;

public partial class MainPage : ContentPage
{
    IAdapter adapter;
    IBluetoothLE ble;
    private readonly List<IDevice> _gattDevices = new List<IDevice>();
    private readonly List<IService> _servicesList = new List<IService>();
    private IService _selectedService;
    private ICharacteristic _char;
    private readonly List<ICharacteristic> _charList = new List<ICharacteristic>();
    private bool isOn = false;
    private bool isOff = false;
    public string ButtonText => !isOff ? "Apagar" : "Encender";

    public string TextButonConnect = "Presiona para Conectar Dispositivo";
    public Color ButtonBackgroundColor => !isOff ? Color.FromRgba("#ff0000") : Color.FromRgba("#008f39");
    public MainPage()
    {
        InitializeComponent();
        adapter = CrossBluetoothLE.Current.Adapter;
        adapter.DeviceDiscovered += (sender, foundBleDevice) =>   // When a BLE Device is found, run the small function below to add it to our list
        {
            if (foundBleDevice.Device != null && !string.IsNullOrEmpty(foundBleDevice.Device.Name))
                _gattDevices.Add(foundBleDevice.Device);
        };
        CounterBtn.IsVisible = isOn;
        LabelText.Text = TextButonConnect;
        ble = CrossBluetoothLE.Current;
    }

    private async void ScannAndConnect(object sender, EventArgs e)
    {

        if (!ble.IsOn)
        {
            await DisplayAlert("Requerido", "La aplicación necesita encender el bluetooth", "Aceptar");
            return;
        }
        activityIndicator.IsRunning = true;
        activityIndicator.IsVisible = true;
        ConnectBtn.IsEnabled = false;
        if (!await PermissionsGrantedAsync())
        {
            await DisplayAlert("Perimiso requerido", "La aplicación necesita permisos de ubicación y bluetooth", "Aceptar");
            return;
        }
        IDevice deviceConnected = null;
        _gattDevices.Clear();

        if (!adapter.IsScanning)
        {
            await adapter.StartScanningForDevicesAsync();
        }

        foreach (var device in adapter.ConnectedDevices)
            _gattDevices.Add(device);

        if (_gattDevices.Count <= 0)
        {
            await DisplayAlert("Error", "No se encontró el dispositivo, por favor intente de nuevo.", "Aceptar");
            return;
        }
        deviceConnected = _gattDevices.FirstOrDefault(c => c.Name.ToLower().Equals("hmsoft"));
        await adapter.StopScanningForDevicesAsync();
        
        if (deviceConnected != null)
        {
            LabelText.Text = "Conectado a: " + deviceConnected.Name;
            activityIndicator.IsRunning = false;
            activityIndicator.IsVisible = false;
            ConnectBtn.IsVisible = false;
            ConnectBtn.IsEnabled = true;
            CounterBtn.IsVisible = true;
            CounterBtn.Text = "Encender";
            CounterBtn.BackgroundColor = Color.FromRgba("#008f39");
            await adapter.ConnectToDeviceAsync(deviceConnected);

            var servicesListReadOnly = await deviceConnected.GetServicesAsync();

            _servicesList.Clear();
            var servicesListStr = new List<String>();
            for (int i = 0; i < servicesListReadOnly.Count; i++)
            {
                _servicesList.Add(servicesListReadOnly[i]);
                servicesListStr.Add(servicesListReadOnly[i].Name + ", UUID: " + servicesListReadOnly[i].Id.ToString());
            }
            _selectedService = _servicesList[3];
            if (_selectedService != null)
            {
                var charListReadOnly = await _selectedService.GetCharacteristicsAsync();

                _charList.Clear();
                var charListStr = new List<String>();
                for (int i = 0; i < charListReadOnly.Count; i++)
                {
                    _charList.Add(charListReadOnly[i]);
                    charListStr.Add(i.ToString() + ": " + charListReadOnly[i].Name);
                }
                _char = charListReadOnly.FirstOrDefault();
                return;
            }
            else
            {
                activityIndicator.IsRunning = false;
                activityIndicator.IsVisible = false;
                ConnectBtn.IsVisible = true;
                ConnectBtn.IsEnabled = true;
                CounterBtn.IsVisible = false;
                await DisplayAlert("Error", "No se encontró el servicio asociado al dispositivo, por favor intente de nuevo reiniciando la APP", "Aceptar");
                return;
            }
        }
        else
        {
            activityIndicator.IsRunning = false;
            activityIndicator.IsVisible = false;
            ConnectBtn.IsVisible = true;
            ConnectBtn.IsEnabled = true;
            CounterBtn.IsVisible = false;
            await DisplayAlert("Error de escaneo", "No se encontró el dispositivo cercano, por favor intente de nuevo", "Aceptar");
            return;
        }
    }

    private async Task<bool> PermissionsGrantedAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        return status == PermissionStatus.Granted;
    }

    private async void OnCounterClicked(object sender, EventArgs e)
    {
        try
        {
            string command = !isOn ? "at+name=b123 on\r\n" : "at+name=b123 off\r\n";
            if (_char != null)
            {
                if (_char.CanWrite)
                {

                    byte[] array = Encoding.UTF8.GetBytes(command);
                    var ts = await _char.WriteAsync(array);
                    if (ts)
                    {
                        CounterBtn.Text = ButtonText;
                        CounterBtn.BackgroundColor = ButtonBackgroundColor;
                        isOff = !isOff;
                        isOn = !isOn;
                        return;
                    }
                    else
                    {
                        await DisplayAlert("Error", "Ocurrió un error al enviar el comando, por favor intente de nuevo reiniciando la APP.", "Aceptar");
                        return;
                    }
                   
                }
                else
                {
                    await DisplayAlert("Error", "Ocurrió un error al leer la característica, por favor intente de nuevo reiniciando la APP.", "Aceptar");
                    return;
                }
            }
        }
        catch
        {
            //ErrorLabel.Text = GetTimeNow() + ": Error receiving Characteristic.";
        }
    }
}

