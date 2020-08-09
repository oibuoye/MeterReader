using Grpc.Core;
using MeterReaderWeb.Data;
using MeterReaderWeb.Data.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeterReaderWeb.Services
{
    public class MeterService : MeterReadingService.MeterReadingServiceBase
    {
        private readonly ILogger<MeterService> _logger;
        private readonly IReadingRepository _repository;

        public MeterService(ILogger<MeterService> logger, IReadingRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async override Task<StatusMessage> AddReading(ReadingPacket request, ServerCallContext context)
        {
            var result = new StatusMessage
            {
                Success = ReadingStatus.Failure
            };

            if(request.Successful == ReadingStatus.Success)
            {
                try
                {
                    foreach(var r in request.Readings)
                    {
                        //save to the database
                        var reading = new MeterReading
                        {
                            Value = r.ReadingValue,
                            ReadingDate = r.ReadingTime.ToDateTime(),
                            CustomerId = r.CustomerId
                        };

                        _repository.AddEntity(reading);
                    }

                    if(await _repository.SaveAllAsync())
                    {
                        _logger.LogInformation($"Stored {request.Readings.Count} new readings");
                        result.Success = ReadingStatus.Success;
                    }

                }catch(Exception ex)
                {
                    result.Message = "Error!!!";
                    _logger.LogError($"Exception: {ex}");
                }
            }

            return result;
        }
    }
}
