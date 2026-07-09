using SensorReadingService.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorReadingService.Domain.Entities
{
    public class Reading
    {
        public Guid Id { get; private set; }

        public string DeviceId { get; private set; }

        public string Metric { get; private set; }

        public DateTime Timestamp { get; private set; }

        public double Value { get; private set; }

        public int Sequence { get; private set; }

        public ReadingIdentity Identity =>
    new(DeviceId, Metric, Timestamp, Sequence);

        private Reading()
        {
        }

        public Reading(
            string deviceId,
            string metric,
            DateTime timestamp,
            double value,
            int sequence)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("DeviceId is required.");

            if (string.IsNullOrWhiteSpace(metric))
                throw new ArgumentException("Metric is required.");

            if (sequence < 0)
                throw new ArgumentException("Sequence cannot be negative.");


            Id = Guid.NewGuid();

            DeviceId = deviceId;

            Metric = metric;

            Timestamp = timestamp;

            Value = value;

            Sequence = sequence;
        }
    }
}