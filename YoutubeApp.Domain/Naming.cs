using System;
using System.Collections.Generic;
using System.Text;
using JustSaying;
using JustSaying.AwsTools.QueueCreation;

namespace YoutubeApp.Domain
{
    public class Naming : INamingStrategy
    {
        private readonly string _env;

        public Naming(string env)
        {
            _env = env;
        }

        public string GetTopicName(string topicName, string messageType)
        {
            return $"{topicName}-{_env}";
        }

        public string GetQueueName(SqsReadConfiguration sqsConfig, string messageType)
        {
            return $"{messageType}-{_env}";
        }
    }
}
