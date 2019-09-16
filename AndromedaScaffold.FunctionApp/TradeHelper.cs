using System;
using System.Collections.Generic;
using System.Linq;
using AndromedaScaffold.WorkerRole.AndromedaServiceReference;

namespace AndromedaScaffold.FunctionApp
{
    public static class TradeHelper
    {
        public static Tuple<string, Star> GetPriceDiff(Star current, Star[] targets)
        {
            List<Tuple<TradeWrapper, Star>> maxList = new List<Tuple<TradeWrapper, Star>>();

            foreach(var star in targets)
            {
                if(GetMaxDiff(current, star) != null)
                {
                    maxList.Add(GetMaxDiff(current, star));
                }
            }

            if(maxList.Count > 0)
            {
                var maxList2 = maxList.OrderByDescending(x => x.Item1.DiffPrice).ToList().FirstOrDefault();

                try
                {
                    return new Tuple<string, Star>(maxList2.Item1.ProductName, maxList2.Item2);

                }
                catch (Exception e)
                {
                    var a = e.StackTrace;
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private static Tuple<TradeWrapper, Star> GetMaxDiff(Star current, Star target)
        {
            List<TradeWrapper> maxList = new List<TradeWrapper>();

            foreach(var prod in target?.Commodities)
            {
                foreach(var prod2 in current?.Commodities)
                {
                    if(prod2.Name == prod.Name && prod.Price - prod2.Price > 0 && prod.Stock > 0 && prod2.Stock > 50)
                    {
                        maxList.Add(new TradeWrapper(prod.Price - prod2.Price, prod2.Price, prod2.Name));
                    }
                }
            }

            if(maxList.Count > 0)
            {
                var value = maxList.OrderByDescending(x => x.DiffPrice).FirstOrDefault();

                return new Tuple<TradeWrapper, Star>(value, target);
            }
            else
            {
                return null;
            }
            
        }

    }

    public class TradeWrapper
    {
        public int DiffPrice;

        public int OriginalPrice;

        public string ProductName;

        public TradeWrapper(int diffPrice, int originalPrice, string productName)
        {
            DiffPrice = diffPrice;
            OriginalPrice = originalPrice;
            ProductName = productName;
        }
    }

}
