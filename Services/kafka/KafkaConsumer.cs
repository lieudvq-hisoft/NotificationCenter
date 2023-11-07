using Confluent.Kafka;
using Data.Models;
using Services.Core;

namespace Services.kafka
{
    public class KafkaConsumer : BackgroundService
    {
        private ConsumerConfig _config;
        private readonly IUserService _userService;
        public KafkaConsumer(ConsumerConfig config, IUserService userService)
        {
            _config = config;
            _userService = userService;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() => Start(stoppingToken));
            return Task.CompletedTask;
        }

        private void Start(CancellationToken stoppingToken)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };
            using (var c = new ConsumerBuilder<Ignore, string>(_config).Build())
            {
                var topics = new List<string>() { "user-create-new", "user-update" };
                c.Subscribe(topics);
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = c.Consume(cts.Token);
                    var data = "";
                    switch (cr.Topic)
                    {
                        case "user-create-new":
                            var userCreate = Newtonsoft.Json.JsonConvert.DeserializeObject<UserFromKafka>(cr.Value);
                            _userService.AddFromKafka(userCreate);
                            break;
                        case "user-update":
                            var userUpdate = Newtonsoft.Json.JsonConvert.DeserializeObject<UserFromKafka>(cr.Value);
                            _userService.UpdateFromKafka(userUpdate);
                            break;
                        default:
                            break;
                    }
                }
            }


        }
    }

}

