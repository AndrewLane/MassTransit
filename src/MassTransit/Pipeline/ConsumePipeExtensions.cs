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
namespace MassTransit.Pipeline
{
    using System;
    using ConsumeConnectors;
    using ConsumerFactories;
    using PipeConfigurators;


    public static class ConsumePipeExtensions
    {
        public static ConnectHandle ConnectHandler<T>(this IConsumePipe filter, MessageHandler<T> handler)
            where T : class
        {
            return HandlerConnectorCache<T>.Connector.Connect(filter, handler);
        }

        public static ConnectHandle ConnectConsumer<T>(this IConsumePipeConnector connector, IConsumerFactory<T> consumerFactory,
            params IPipeSpecification<ConsumerConsumeContext<T>>[] pipeSpecifications)
            where T : class
        {
            return ConsumerConnectorCache<T>.Connector.Connect(connector, consumerFactory, pipeSpecifications);
        }

        public static ConnectHandle ConnectConsumer<T>(this IConsumePipeConnector filter,
            params IPipeSpecification<ConsumerConsumeContext<T>>[] pipeSpecifications)
            where T : class, new()
        {
            var consumerFactory = new DefaultConstructorConsumerFactory<T>();

            ConsumerConnector connector = ConsumerConnectorCache.GetConsumerConnector<T>();

            return connector.Connect(filter, consumerFactory, pipeSpecifications);
        }

        public static ConnectHandle ConnectConsumer<T>(this IConsumePipeConnector filter, Func<T> factoryMethod)
            where T : class
        {
            var consumerFactory = new DelegateConsumerFactory<T>(factoryMethod);

            ConsumerConnector connector = ConsumerConnectorCache.GetConsumerConnector<T>();

            return connector.Connect(filter, consumerFactory);
        }

        public static ConnectHandle ConnectConsumer(this IConsumePipeConnector filter, Type consumerType, Func<Type, object> objectFactory)
        {
            return ConsumerConnectorCache.Connect(filter, consumerType, objectFactory);
        }

        public static ConnectHandle ConnectInstance<T>(this IConsumePipeConnector filter, T instance)
            where T : class
        {
            return InstanceConnectorCache<T>.Connector.Connect(filter, instance);
        }

        public static ConnectHandle ConnectInstance(this IConsumePipeConnector filter, object instance)
        {
            return InstanceConnectorCache.GetInstanceConnector(instance.GetType()).Connect(filter, instance);
        }
    }
}