/* The game was developed by: Bálint Farkas (balint.farkas@windowslive.com), MatroIT Systems Kft.
 * Commissioned by Microsoft Hungary. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AndromedaScaffold.FunctionApp;
using AndromedaScaffold.WorkerRole.AndromedaServiceReference;

namespace AndromedaScaffold
{
    public static class SpaceshipController
    {

        /// <summary>
        /// This method is called automatically when your ship lands on a planet.
        /// You can use this method to purchase commodities, equip your ship and launch it towards another star system.
        /// All you have to do is implement this method and the ShipFlying() method - you don't
        /// need to touch the other files in the solution.
        /// 
        /// Don't forget: post any issues you have on the forum! {FORUM URL HERE}
        /// The starmap and all other information can be found on: {GAME URL HERE}
        /// </summary>
        public static async Task ShipLandedAsync(Guid currentShip)
        {
            //Get the status of our ship
            var ship = await NavigationComputer.GetSpaceshipStatusAsync();
            //Get our current star
            var currentStar = await NavigationComputer.GetCurrentStarAsync();
            //Get the stars we can see
            var stars = await NavigationComputer.GetVisibleStarsAsync();

            //Please note that you can own multiple ships. You can differentiate between ships using the
            //currentShip parameter and provide separate logic for each ship. This is not checked in this example,
            //so all ships behave in the same way after landing. See more details in the ShipFlying() method at the 
            //bottom!

            //If we can, buy a larger ship model!
            //This costs a lot of credits, but increases storage space.
            if (ship.Money > 1100000 && ship.TotalCapacity == 100)
            {
                await NavigationComputer.UpgradeShipCapacityTo200Async();
            }else if (ship.Money > 11000000 && ship.TotalCapacity == 200)
            {
                await NavigationComputer.UpgradeShipCapacityTo300Async();
            }

            if (ship.TotalCapacity == 200 && ship.CannonCount < 1)
            {
                await NavigationComputer.AddCannonAsync();
            }

            if(ship.FreeCapacity > 200)
            {
                await NavigationComputer.AddDriveAsync();
            }

            if(ship.DriveCount > 2)
            {
                await NavigationComputer.RemoveDriveAsync();
            }

           
            if (ship.SensorCount < 3)
            {
               
                await NavigationComputer.AddSensorAsync();
                
                ship = await NavigationComputer.GetSpaceshipStatusAsync();
                stars = await NavigationComputer.GetVisibleStarsAsync();

            }

            //Sell everything we've brought.
            foreach (var cargoItem in ship.Cargo)
            {
                //These method calls do not throw exceptions, to avoid disrupting program flow.
                //Instead they return any error as a string.
                await NavigationComputer.SellAsync(cargoItem.Name, cargoItem.Stock);
            }

            //Let's trade water!
            //Check if any of the nearby stars have a higher price level for water than this star system.
            //If we find such a star we can make a profit by buying water here and selling it there.
            /* int localPrice = currentStar.Commodities.Min(i => i.Price);
             var stuffToBuy = currentStar.Commodities.Where(i => i.Price == localPrice).ToList()[0].Name;
             int maxPrice = stars.Max(i => i.Commodities.Single(j => j.Name == stuffToBuy).Price);

             */
            
            var target = TradeHelper.GetPriceDiff(currentStar, stars);
            if(target != null)
            {
                await NavigationComputer.BuyMaximumAsync(target.Item1);
                await NavigationComputer.LaunchSpaceshipAsync(target.Item2);
            }
            else
            {
                var rnd = new Random((int)DateTime.Now.Ticks);
                var randomStar = stars[rnd.Next(0, stars.Length)];
                await NavigationComputer.LaunchSpaceshipAsync(randomStar);
            }
            //Profit can be made by trading water - buy as much as we can, and go to that star!
            /*if (maxPrice > localPrice)
            {
                await NavigationComputer.BuyMaximumAsync(stuffToBuy);
                var targetStar = stars.First(i => i.Commodities.Single(j => j.Name == stuffToBuy).Price == maxPrice);
                await NavigationComputer.LaunchSpaceshipAsync(targetStar);
            }
           */
        }

        /// <summary>
        /// This method is called automatically every few seconds while your ship is flying.
        /// Trading, equipment modification etc. can be done only when landed on a planet,
        /// but you can use this method to attack others!
        /// If you wish to be a peaceful trader, then you can ignore this method, since only pirates
        /// can make any use of it.
        /// 
        /// All you have to do is implement this method and the ShipLanded() method - you don't
        /// need to touch the other files in the solution.
        /// Don't forget: post any issues you have on the forum! {FORUM URL HERE}
        /// The starmap and all other information can be found on: {GAME URL HERE}
        /// </summary>
        public static async Task ShipFlying(Guid currentShip)
        {
            //Let's try and buy another ship, if we have the money! The first additional ship costs 10 million credits.
            var ship = await NavigationComputer.GetSpaceshipStatusAsync();
            var ownedShips = await NavigationComputer.GetOwnedShipsAsync();

            if (ship.Money > 10100000 && ownedShips.ToList().Count == 1)
            {
                await NavigationComputer.BuyNewShipAsync();
            }

            //We can have multiple ships. By checking the value of the currentShip parameter, you can differentiate
            //between your ships and give each of the separate orders.
            //In this example, if this is not the first ship, we'll use it for piracy.
            
            //if (currentShip != ownedShips.First())
            //{
                //Let's attack the first ship that gets in our sight!
                //Caution: this only makes sense when you have some cannons equipped.
                var raidableShips = await NavigationComputer.GetRaidableShipsAsync();

                if (raidableShips.Length > 0 && ship.CannonCount > 0 && raidableShips.First().Distance < 40)
                {
                    await NavigationComputer.RaidAsync(raidableShips.First());
                }
            //}
        }
    }
}
