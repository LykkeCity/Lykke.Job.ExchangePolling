using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.ExchangeConnector.Client.Models;
using MarginTrading.RiskManagement.HedgingService.Contracts.Models;
using Microsoft.Extensions.Logging;

namespace Lykke.Job.ExchangePolling.Core.Domain
{
    public class Position
    {
        public string Symbol { get; set; }

        public decimal PositionVolume { get; set; }

        public decimal MaintMarginUsed { get; set; }

        public decimal RealisedPnL { get; set; }

        public decimal UnrealisedPnL { get; set; }
        
        public decimal? Value { get; set; }

        public decimal? AvailableMargin { get; set; }

        public decimal InitialMarginRequirement { get; set; }

        public decimal MaintenanceMarginRequirement { get; set; }

        public Position Clone()
        {
            return new Position
            {
                Symbol = this.Symbol,
                PositionVolume = this.PositionVolume,
                MaintMarginUsed = this.MaintMarginUsed,
                RealisedPnL = this.RealisedPnL,
                UnrealisedPnL = this.UnrealisedPnL,
                Value = this.Value,
                AvailableMargin = this.AvailableMargin,
                InitialMarginRequirement = this.InitialMarginRequirement,
                MaintenanceMarginRequirement = this.MaintenanceMarginRequirement
            };
        }

        public static Position Create(PositionModel positionModel)
        {
            return new Position
            {
                Symbol = positionModel.Symbol,
                PositionVolume = (decimal)positionModel.PositionVolume,
                MaintMarginUsed = (decimal)positionModel.MaintMarginUsed,
                RealisedPnL = (decimal)positionModel.RealisedPnL,
                UnrealisedPnL = (decimal)positionModel.UnrealisedPnL,
                Value = (decimal?)positionModel.Value,
                AvailableMargin = (decimal?)positionModel.AvailableMargin,
                InitialMarginRequirement = (decimal)positionModel.InitialMarginRequirement,
                MaintenanceMarginRequirement = (decimal)positionModel.MaintenanceMarginRequirement
            };
        }

        public static Position Create(ExternalPositionModel positionModel)
        {
            return new Position
            {
                Symbol = positionModel.Symbol,
                PositionVolume = (decimal)positionModel.PositionVolume,
                MaintMarginUsed = (decimal)positionModel.MaintMarginUsed,
                RealisedPnL = (decimal)positionModel.RealisedPnL,
                UnrealisedPnL = (decimal)positionModel.UnrealisedPnL,
                Value = (decimal?)positionModel.Value,
                AvailableMargin = (decimal?)positionModel.AvailableMargin,
                InitialMarginRequirement = (decimal)positionModel.InitialMarginRequirement,
                MaintenanceMarginRequirement = (decimal)positionModel.MaintenanceMarginRequirement
            };
        }

        public static Position Merge(Position savedPosition, Position hedgingPosition)
        {
            if (savedPosition == null && hedgingPosition == null)
                return null;
            
            return hedgingPosition?.Clone() ?? new Position
            {
                Symbol = savedPosition?.Symbol
            };
        }
    }
}
