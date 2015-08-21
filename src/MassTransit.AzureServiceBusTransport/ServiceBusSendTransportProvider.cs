// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.AzureServiceBusTransport
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using Transports;


    public class ServiceBusSendTransportProvider :
        ISendTransportProvider
    {
        readonly IServiceBusHost[] _hosts;

        public ServiceBusSendTransportProvider(IServiceBusHost[] hosts)
        {
            _hosts = hosts;
        }

        public async Task<ISendTransport> GetSendTransport(Uri address)
        {
            IServiceBusHost host = _hosts.FirstOrDefault(
                x => address.ToString().StartsWith(x.Settings.ServiceUri.ToString(), StringComparison.OrdinalIgnoreCase));
            if (host == null)
                throw new EndpointNotFoundException("The endpoint address specified an unknown host: " + address);

            var queueDescription = address.GetQueueDescription();

            var namespaceManager = await host.NamespaceManager;

            var namespacePath = namespaceManager.Address.AbsolutePath.Trim('/');
            if (IsInNamespace(queueDescription, namespacePath))
            {
                queueDescription.Path = queueDescription.Path.Replace(namespacePath, "").Trim('/');
                queueDescription = await namespaceManager.CreateQueueSafeAsync(queueDescription);
            }
            else
            {
                namespaceManager = await host.RootNamespaceManager;

                queueDescription = await namespaceManager.CreateQueueSafeAsync(queueDescription);
            }

            MessagingFactory messagingFactory = await host.MessagingFactory;

            string queuePath = host.GetQueuePath(queueDescription);

            MessageSender messageSender = await messagingFactory.CreateMessageSenderAsync(queuePath);

            var sendTransport = new ServiceBusSendTransport(messageSender);
            return sendTransport;
        }

        static bool IsInNamespace(QueueDescription queueDescription, string namespacePath)
        {
            return queueDescription.Path.StartsWith(namespacePath);
        }
    }
}