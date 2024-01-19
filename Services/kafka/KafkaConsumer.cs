using Confluent.Kafka;
using Data.Models;
using Services.Core;

namespace Services.kafka
{
    public class KafkaConsumer : BackgroundService
    {
        private ConsumerConfig _config;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        public KafkaConsumer(ConsumerConfig config, IUserService userService, INotificationService notificationService)
        {
            _config = config;
            _userService = userService;
            _notificationService = notificationService;
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
                var topics = new List<string>() { "user-create-new", "user-update", "receipt-create-new",
                    "inventory-threshold-warning",
                    "pickingrequest-assign-user",
                    "pickingrequest-complete",
                    "order-complete"
                };
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
                        case "receipt-create-new":
                            var receiptModel = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.CreateReceipt(receiptModel);
                            break;
                        case "inventory-threshold-warning":
                            var productModel = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.InventoryThresholdWarning(productModel);
                            break;
                        case "pickingrequest-assign-user":
                            var pickingRequestModel = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.PickingRequestAssignUser(pickingRequestModel);
                            break;
                        case "pickingrequest-complete":
                            var pickingRequestCompleteModel = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.PickingRequestComplete(pickingRequestCompleteModel);
                            break;
                        case "order-complete":
                            var orderCompleteModel = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.OrderComplete(orderCompleteModel);
                            break;
                        default:
                            break;
                    }
                }
            }


        }
    }

}

