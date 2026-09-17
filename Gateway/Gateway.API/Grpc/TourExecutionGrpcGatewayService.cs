using Grpc.Core;
using Grpc.Net.Client;
using TourExecutionRpc;

namespace Gateway.API.Grpc
{
    public class TourExecutionGrpcGatewayService : TourExecutionRpcService.TourExecutionRpcServiceBase
    {
        private const string ToursServiceAddress = "http://tours:9091";

        public override async Task<StartTourResponse> StartTour(
            StartTourRequest request,
            ServerCallContext context)
        {
            using var channel = GrpcChannel.ForAddress(ToursServiceAddress);
            var client = new TourExecutionRpcService.TourExecutionRpcServiceClient(channel);

            return await client.StartTourAsync(request);
        }
    }
}