﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;


namespace RackSpaceCloudServersAPI
{
    public class RackSpaceCloudServersAPI
    {
        private AuthInfo _authInfo;
        
        public RackSpaceCloudServersAPI(AuthInfo authInfo)
        {
            this._authInfo = authInfo;
        }

        public List<RackSpaceCloudServer> ListServers(bool details = false)
        {
            List<RackSpaceCloudServer> result = new List<RackSpaceCloudServer>();
            
            var request = new RackSpaceCloudRequest(this._authInfo.ServerManagementUrl, this._authInfo.AuthToken);

            dynamic serverList;
            
            if (details)
            {
                serverList = request.GetRequest("/servers/detail");
            }
            else
            {
                serverList = request.GetRequest("/servers");
            }

            
            foreach (var server in serverList.servers)
            {
                if (details)
                {
                    result.Add(ExpandoToRackSpaceCloudServerObject(server));
                }
                else
                {
                    result.Add(new RackSpaceCloudServer { id = server.id, name = server.name });                    
                }
            }

            
            return result;
        }


        public RackSpaceCloudServer GetServerDetails(string serverId)
        {

            var request = new RackSpaceCloudRequest(this._authInfo.ServerManagementUrl, this._authInfo.AuthToken);

            var serverDetails = request.GetRequest(String.Format("/servers/{0}",serverId));

            var server = ExpandoToRackSpaceCloudServerObject(serverDetails.server);

            return server;
        }


        public RackSpaceCloudServer CreateServer(string serverName, int imageId, RackSpaceCloudServerFlavor flavorId, Dictionary<string, string> metadata = null, List<Personality> personality = null)
        {

            var request = new RackSpaceCloudRequest(this._authInfo.ServerManagementUrl, this._authInfo.AuthToken);
            dynamic data = new ExpandoObject();
            data.server = new ExpandoObject();
            data.server.name = serverName;
            data.server.imageId = imageId;
            data.server.flavorId = flavorId;
            data.server.metadata = metadata;      
            data.server.personality = personality;
            
            
            dynamic response = request.Request("POST", "/servers", data);

            return ExpandoToRackSpaceCloudServerObject(response.server);
        }

        
        public bool DeleteServer(string serverId)
        {
            try
            {
                var request = new RackSpaceCloudRequest(this._authInfo.ServerManagementUrl, this._authInfo.AuthToken);

                dynamic response = request.Request("DELETE", "/servers/" + serverId);
            }
            catch 
            {
                return false;
            }
            
            return true;
        }


        public bool UpdateServerName(string serverId, string newServerName)
        {

            try
            {
                var request = new RackSpaceCloudRequest(this._authInfo.ServerManagementUrl, this._authInfo.AuthToken);
                
                dynamic data = new ExpandoObject();
                data.server = new ExpandoObject();
                data.server.name = newServerName;
                //data.server.adminPass = newPassword;

                dynamic response = request.Request("PUT", "/servers/" + serverId, data);

            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
        }


        public bool UpdateServerPass(string serverId, string newPassword)
        {

            try
            {
                var request = new RackSpaceCloudRequest(this._authInfo.ServerManagementUrl, this._authInfo.AuthToken);

                dynamic data = new ExpandoObject();
                data.server = new ExpandoObject();
                //data.server.name = newServerName;
                data.server.adminPass = newPassword;

                dynamic response = request.Request("PUT", "/servers/" + serverId, data);

            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }



        public List<RackSpaceCloudServerImage> ListImages(bool details)
        {

            List<RackSpaceCloudServerImage> listImages = new List<RackSpaceCloudServerImage>();
            
            var request = new RackSpaceCloudRequest(this._authInfo.ServerManagementUrl, this._authInfo.AuthToken);


            if (!details)
            {
                var images = request.GetRequest("/images");

                foreach (var image in images.images)
                {
                    listImages.Add(new RackSpaceCloudServerImage { id = image.id, name = image.name });
                }
            }
            else
            {
                var images = request.GetRequest("/images/detail");

                foreach (var image in images.images)
                {

                    listImages.Add(ExpandoToRackSpaceCloudImageObject(image));
                }
            }

            return listImages;
        }


        public RackSpaceCloudServerImage GetImageDetails(int imageId)
        {
            
            var request = new RackSpaceCloudRequest(this._authInfo.ServerManagementUrl, this._authInfo.AuthToken);

            var image = request.GetRequest("/images/" + imageId.ToString());

            var result = ExpandoToRackSpaceCloudImageObject(image.image);

            return result;
        }


        private RackSpaceCloudServerImage ExpandoToRackSpaceCloudImageObject(dynamic image)
        {
            var p = image as IDictionary<String, object>;

            object id;
            object name;
            object status;
            object updated;
            object created;
            object serverId;
            object progress;

            if (!p.TryGetValue("id", out id))
            {
                id = "0";
            }

            if (!p.TryGetValue("name", out name))
            {
                name = "Empty value";
            }

            if (!p.TryGetValue("status", out status))
            {
                status = "Empty value";
            }

            if (!p.TryGetValue("updated", out updated))
            {
                updated = "Empty value";
            }


            if (!p.TryGetValue("created", out created))
            {
                created = "Empty value";
            }

            if (!p.TryGetValue("serverId", out serverId))
            {
                serverId = "0";
            }

            if (!p.TryGetValue("progress", out progress))
            {
                progress = "0";
            }

            RackSpaceCloudServerImage imagesDetails = new RackSpaceCloudServerImage
            {
                id = Convert.ToInt32(id.ToString()),
                name = name.ToString(),
                status = status.ToString(),
                updated = updated.ToString(),
                created = created.ToString(),
                serverId = Convert.ToInt32(serverId.ToString()),
                progress = Convert.ToInt32(progress.ToString())
            };
            return imagesDetails;
        }




        private RackSpaceCloudServer ExpandoToRackSpaceCloudServerObject(dynamic server)
        {

            List<string> tempPublicIP = new List<string>();
            List<string> tempPrivateIP = new List<string>();


            if (server.addresses.@public is ICollection<object>)
            {
                foreach (var ip in server.addresses.@public)
                {
                    tempPublicIP.Add(Convert.ToString(ip.Unknown));
                }

            }

            if (server.addresses.@private is ICollection<object>)
            {
                foreach (var ip in server.addresses.@private)
                {
                    tempPrivateIP.Add(Convert.ToString(ip.Unknown));
                }

            }

            var result = new RackSpaceCloudServer
            {
                id = server.id,
                name = server.name,
                imageId = server.imageId,
                flavorId = server.flavorId,
                hostId = server.hostId,
                status = server.status,
                progress = server.progress,
                addresses = new RackSpaceCloudServerIPAdress { privateIP = tempPrivateIP, publicIP = tempPublicIP },
            };

            return result;
        }
       
    }


}
