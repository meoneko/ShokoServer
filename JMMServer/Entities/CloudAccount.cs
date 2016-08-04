﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using NutzCode.CloudFileSystem;
using NutzCode.CloudFileSystem.OAuth.Windows.WPF;
using AuthProvider = JMMServer.UI.AuthProvider;

namespace JMMServer.Entities
{
    public class CloudAccount
    {
        public int CloudID { get; private set; }
        public string ConnectionString { get; set; }
        public string Provider 
        {
            get { return _provider; }
            set
            {
                _provider = value;
                if (!string.IsNullOrEmpty(_provider))
                {
                    _plugin = CloudFileSystemPluginFactory.Instance.List.FirstOrDefault(a => a.Name == _provider);
                    if (_plugin != null)
                        Icon = _plugin.CreateIconImage();
                }
            }
        }
        public string Name { get; set; }
        public BitmapImage Icon { get;  set; }


        private string _provider;
        private ICloudPlugin _plugin;


        public virtual IFileSystem FileSystem
        {
            get
            {
                if (!ServerState.Instance.ConnectedFileSystems.ContainsKey(Name))
                    ServerState.Instance.ConnectedFileSystems[Name]=Connect(MainWindow.Instance);
                return ServerState.Instance.ConnectedFileSystems[Name];
            }
            set
            {
                if (value!=null)
                    ServerState.Instance.ConnectedFileSystems[Name] = value;
                else if (ServerState.Instance.ConnectedFileSystems.ContainsKey(Name))
                    ServerState.Instance.ConnectedFileSystems.Remove(Name);
                    
            }
        }
        public bool IsConnected => ServerState.Instance.ConnectedFileSystems.ContainsKey(Name ?? string.Empty);




        public CloudAccount()
        {

        }


        public IFileSystem Connect(Window owner)
        {
            if (string.IsNullOrEmpty(Provider))
                throw new Exception("Empty provider supplied");
            Dictionary<string, object> auth = AuthorizationFactory.Instance.AuthorizationProvider.Get(Provider);
            if (auth == null)
                throw new Exception("Application Authorization Not Found");
            _plugin = CloudFileSystemPluginFactory.Instance.List.FirstOrDefault(a => a.Name == Provider);
            if (_plugin == null)
                throw new Exception("Cannot find cloud provider '" + Provider + "'");
            Icon = _plugin.CreateIconImage();
            FileSystemResult<IFileSystem> res = _plugin.Init(Name, new AuthProvider(owner), auth, ConnectionString);            
            if (!res.IsOk)
                throw new Exception("Unable to connect to '" + Provider + "'");
            string userauth = res.Result.GetUserAuthorization();
            if (ConnectionString != userauth)
                ConnectionString = userauth;
            return res.Result;
        }



    }
}