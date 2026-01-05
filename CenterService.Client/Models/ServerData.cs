using System.ComponentModel;
using System.Runtime.Serialization;

namespace CenterService.Client.Models;

/// <summary>
/// Represents server data information
/// </summary>
[DataContract(Name = "ServerData", Namespace = "http://schemas.datacontract.org/2004/07/Center.Server")]
public class ServerData : IExtensibleDataObject, INotifyPropertyChanged
{
    private ExtensionDataObject? _extensionData;
    private int _id;
    private string? _ip;
    private int _lowestLevel;
    private int _mustLevel;
    private string? _name;
    private int _online;
    private int _port;
    private int _state;

    /// <summary>
    /// Extension data for forward compatibility
    /// </summary>
    [Browsable(false)]
    public ExtensionDataObject? ExtensionData
    {
        get => _extensionData;
        set => _extensionData = value;
    }

    /// <summary>
    /// Server ID
    /// </summary>
    [DataMember]
    public int Id
    {
        get => _id;
        set
        {
            if (_id != value)
            {
                _id = value;
                RaisePropertyChanged(nameof(Id));
            }
        }
    }

    /// <summary>
    /// Server IP address
    /// </summary>
    [DataMember]
    public string? Ip
    {
        get => _ip;
        set
        {
            if (_ip != value)
            {
                _ip = value;
                RaisePropertyChanged(nameof(Ip));
            }
        }
    }

    /// <summary>
    /// Lowest level required
    /// </summary>
    [DataMember]
    public int LowestLevel
    {
        get => _lowestLevel;
        set
        {
            if (_lowestLevel != value)
            {
                _lowestLevel = value;
                RaisePropertyChanged(nameof(LowestLevel));
            }
        }
    }

    /// <summary>
    /// Must level required
    /// </summary>
    [DataMember]
    public int MustLevel
    {
        get => _mustLevel;
        set
        {
            if (_mustLevel != value)
            {
                _mustLevel = value;
                RaisePropertyChanged(nameof(MustLevel));
            }
        }
    }

    /// <summary>
    /// Server name
    /// </summary>
    [DataMember]
    public string? Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }
    }

    /// <summary>
    /// Online player count
    /// </summary>
    [DataMember]
    public int Online
    {
        get => _online;
        set
        {
            if (_online != value)
            {
                _online = value;
                RaisePropertyChanged(nameof(Online));
            }
        }
    }

    /// <summary>
    /// Server port
    /// </summary>
    [DataMember]
    public int Port
    {
        get => _port;
        set
        {
            if (_port != value)
            {
                _port = value;
                RaisePropertyChanged(nameof(Port));
            }
        }
    }

    /// <summary>
    /// Server state
    /// </summary>
    [DataMember]
    public int State
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                _state = value;
                RaisePropertyChanged(nameof(State));
            }
        }
    }

    /// <summary>
    /// Property changed event
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the property changed event
    /// </summary>
    protected void RaisePropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
