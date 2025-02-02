using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;

namespace l99.driver.fanuc.handlers
{
    public class Native: Handler
    {
        private readonly Formatting jsonFormatting = Formatting.None;
        
        private Dictionary<string,string> _publishedSchemas = new Dictionary<string, string>();
        
        public Native(Machine machine) : base(machine)
        {
            
        }

        public override async Task<dynamic?> OnDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? beforeArrival)
        {
            dynamic payload = new
            {
                observation = new
                {
                    time =  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                    machine = veneers.Machine.Id,
                    type = veneer.IsInternal ? "internal" :
                            veneer.IsCompound ? "compound" :
                                "source",
                    name = veneer.Name,
                    marker = veneer.Marker
                },
                source = new
                {
                    method = (veneer.IsInternal || veneer.IsCompound) ? "" : veneer.LastArrivedInput.method,
                    invocationMs = (veneer.IsInternal || veneer.IsCompound) ? -1 : veneer.LastArrivedInput.invocationMs,
                    data = (veneer.IsInternal || veneer.IsCompound) ? new { } : veneer.LastArrivedInput.request.GetType().GetProperty(veneer.LastArrivedInput.method).GetValue(veneer.LastArrivedInput.request, null)
                },
                state = new
                {
                    time = veneer.ArrivalDelta,
                    data = veneer.LastArrivedValue
                }
            };

            return payload;
        }
        
        protected override async Task afterDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? onArrival)
        {
            var topic = $"fanuc/{veneers.Machine.Id}-all/{veneer.Name}{(veneer.SliceKey == null ? string.Empty : "/" + veneer.SliceKey.ToString())}";
            string payload = JObject.FromObject(onArrival).ToString(jsonFormatting);
            await veneers.Machine.Broker.PublishArrivalAsync(topic, payload);
        }
        
        public override async Task<dynamic?> OnDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? beforeChange)
        {
            dynamic payload = new
            {
                observation = new
                {
                    time =  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                    machine = veneers.Machine.Id,
                    type = veneer.IsInternal ? "internal" :
                        veneer.IsCompound ? "compound" :
                        "source",
                    name = veneer.Name,
                    marker = veneer.Marker
                },
                source = new
                {
                    method = (veneer.IsInternal || veneer.IsCompound) ? "" : veneer.LastChangedInput.method,
                    invocationMs = (veneer.IsInternal || veneer.IsCompound) ? -1 : veneer.LastChangedInput.invocationMs,
                    data = (veneer.IsInternal || veneer.IsCompound) ? new { } : veneer.LastChangedInput.request.GetType().GetProperty(veneer.LastChangedInput.method).GetValue(veneer.LastChangedInput.request, null)
                },
                state = new
                {
                    time = veneer.ChangeDelta,
                    data = veneer.LastChangedValue
                }
            };

            return payload;
        }

        protected override async Task afterDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? onChange)
        {
            var topic = $"fanuc/{veneers.Machine.Id}/{veneer.Name}{(veneer.SliceKey == null ? string.Empty : "/" + veneer.SliceKey.ToString())}";
            string payload = JObject.FromObject(onChange).ToString(jsonFormatting);
            await veneers.Machine.Broker.PublishChangeAsync(topic, payload);

            if (!_publishedSchemas.ContainsKey(veneer.Name))
            {
                var topic_schema = $"fanuc/{veneers.Machine.Id}/{veneer.Name}/$schema";
                string schema = JsonSchema.FromSampleJson(payload).ToJson();
                await veneers.Machine.Broker.PublishAsync(topic_schema, schema, true);
                _publishedSchemas.Add(veneer.Name, schema);
            }
        }
        
        public override async Task<dynamic?> OnCollectorSweepCompleteAsync(Machine machine, dynamic? beforeSweepComplete)
        {
            dynamic payload = new
            {
                observation = new
                {
                    time =  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                    machine = machine.Id,
                    name = "ping"
                },
                source = new
                {
                    data = machine.Info
                },
                state = new
                {
                    data = machine.CollectorSuccess ? "OK" : "NOK"
                }
            };
            
            return payload;
        }
        
        protected override async Task afterSweepCompleteAsync(Machine machine, dynamic? onSweepComplete)
        {
            string topic_all = $"fanuc/{machine.Id}-all/ping";
            string topic = $"fanuc/{machine.Id}/ping";
            string payload = JObject.FromObject(onSweepComplete).ToString(jsonFormatting);
            
            await machine.Broker.PublishArrivalStatusAsync(topic_all, payload);
            await machine.Broker.PublishChangeStatusAsync(topic, payload);
        }

        public override async Task OnGenerateIntermediateModel(string json)
        {
            string topic_all = $"fanuc/{machine.Id}-all/$model";
            var topic = $"fanuc/{machine.Id}/$model";
            
            // TODO: conditional -all
            await machine.Broker.PublishAsync(topic_all, json, true);
            await machine.Broker.PublishAsync(topic, json, true);
        }
    }
}