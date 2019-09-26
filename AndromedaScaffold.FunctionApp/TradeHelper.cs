using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AndromedaScaffold.WorkerRole.AndromedaServiceReference;

namespace AndromedaScaffold.FunctionApp
{
    public static class TradeHelper
    {
        public static async Task<Tuple<string, Star>> GetPriceDiff(Star current, Star[] targets)
        {
            try
            {
                List<Tuple<TradeWrapper, Star>> maxList = new List<Tuple<TradeWrapper, Star>>();

                foreach (var star in targets)
                {
                    if (await GetMaxDiff(current, star) != null)
                    {
                        maxList.Add(await GetMaxDiff(current, star));
                    }
                }

                if (maxList.Count > 0)
                {
                    var ship = await NavigationComputer.GetSpaceshipStatusAsync();
                    var maxList2 = maxList.OrderByDescending(x => x.Item1.Profit).ToList().FirstOrDefault();
                    if(maxList2.Item1.Profit > 10000)
                    {
                        return new Tuple<string, Star>(maxList2.Item1.ProductName, maxList2.Item2);
                    }
                    else if(ship.Money < 100000)
                    {
                        return new Tuple<string, Star>(maxList2.Item1.ProductName, maxList2.Item2);
                    }
                    else
                    {
                        return null;
                    }
                    
                }
                else
                {
                    return null;
                }
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                throw e;
            }
        }

        public static async Task<Tuple<TradeWrapper, Star>> GetSecondMaxDiff(Star current, Star target)
        {
            try
            {
                List<TradeWrapper> maxList = new List<TradeWrapper>();

                foreach (var prod in target?.Commodities)
                {
                    foreach (var prod2 in current?.Commodities)
                    {
                        if (prod2.Name == prod.Name && prod.Price - prod2.Price > 0 && prod2.Stock > 0)
                        {
                            var cargoSize = await AvailableCargoSpace();

                            var stockSize = prod2.Stock >= cargoSize ? cargoSize : prod2.Stock;

                            var profit = ((prod.Price - prod2.Price) * stockSize) / target.DistanceInLightYears;

                            maxList.Add(new TradeWrapper(profit, prod2.Price, prod2.Name));
                        }
                    }
                }

                if (maxList.Count >= 2)
                {
                    var value = maxList.OrderByDescending(x => x.Profit).ToList()[1];

                    return new Tuple<TradeWrapper, Star>(value, target);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                throw e;
            }
        }

        private static async Task<Tuple<TradeWrapper, Star>> GetMaxDiff(Star current, Star target)
        {
            try
            {
                List<TradeWrapper> maxList = new List<TradeWrapper>();

                foreach (var prod in target?.Commodities)
                {
                    foreach (var prod2 in current?.Commodities)
                    {
                        if (prod2.Name == prod.Name && prod.Price - prod2.Price > 0 && prod2.Stock > 0)
                        {
                            var cargoSize = await AvailableCargoSpace();

                            var stockSize = prod2.Stock >= cargoSize ? cargoSize : prod2.Stock;

                            var profit = ((prod.Price - prod2.Price) * stockSize) / target.DistanceInLightYears;

                            if(profit > 0)
                            {
                                maxList.Add(new TradeWrapper(profit, prod2.Price, prod2.Name));
                            }
                        }
                    }
                }

                if (maxList.Count > 0)
                {
                    var value = maxList.OrderByDescending(x => x.Profit).FirstOrDefault();

                    return new Tuple<TradeWrapper, Star>(value, target);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                throw e;
            }
        }

        public static async Task<int> UsedCargoSpace()
        {
            try
            {
                var ship = await NavigationComputer.GetSpaceshipStatusAsync();

                int usedCargo = 0;

                foreach (var item in ship.Cargo)
                {
                    usedCargo += item.Stock;
                }

                return usedCargo;
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);

                throw e;
            }
           
        }

        public static async Task<int> AvailableCargoSpace()
        {
            try
            {
                var ship = await NavigationComputer.GetSpaceshipStatusAsync();

                return ship.TotalCapacity - await EquipementSpace();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);

                throw e;
            }
            
        }

        private static async Task<int> EquipementSpace()
        {
            try
            {
                var ship = await NavigationComputer.GetSpaceshipStatusAsync();
                return (ship.CannonCount + ship.DriveCount + ship.SensorCount + ship.ShieldCount) * 20;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);

                throw e;
            }
            
        }

        public static async Task<int> CannonRange()
        {
            try
            {
                var ship = await NavigationComputer.GetSpaceshipStatusAsync();
                return ship.CannonCount * 10;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);

                throw e;
            }

        }

    }

    public class TradeWrapper
    {
        public double Profit;

        public int OriginalPrice;

        public string ProductName;

        public TradeWrapper(double profit, int originalPrice, string productName)
        {
            Profit = profit;
            OriginalPrice = originalPrice;
            ProductName = productName;
        }
    }
    
}
