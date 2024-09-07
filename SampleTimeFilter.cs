#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Collections.ObjectModel;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class OneR : Strategy
	{
		private double boll;
		private double currentHigh;
		private double currentLow;
		private double previousHigh;
		private double previousLow;
		private double previousHigh2;
		private double previousLow2;
		private double twoBarPreviousHigh;
        private double twoBarPreviousLow;
		private double candleRange;
		private double breakevenRange;
		private double beStop;
		private double initialAccountValue;
	    private double profitTarget = 1500; // Example profit target
	    private double lossLimit = -1100; // Example loss limit
	    private bool tradingEnabled = true;
		private bool slbe;
		private Bollinger bollingerBands;
		private double prevCandleLow;
		private double prevCandleHigh;
		private double currentCandleHigh;
		private double currentCandleLow;
		private double nextCandleHigh ;
		private double nextCandleLow;
		private double stopLoss;
		private double takeProfit;
		private int contracts;
		Collection<Cbi.Instrument> instrumentsToClose = new Collection<Instrument>();
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
                Description = @"Automatic Strategy for APlus Setups";
                Name = "APlus1R";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 4;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 0;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 3; // Need at least two bars to compare
                IsInstantiatedOnEachOptimizationIteration = true;
				RealtimeErrorHandling = RealtimeErrorHandling.StopCancelCloseIgnoreRejects;
			}
			else if (State == State.DataLoaded)
        	{
            	// Calculate initial account value
            	initialAccountValue = Account.Get(AccountItem.CashValue, Currency.UsDollar);
       	 	}
			
			else if (State == State.Configure)
			{
				RealtimeErrorHandling = RealtimeErrorHandling.StopCancelCloseIgnoreRejects;
				AddDataSeries("NQ", Data.BarsPeriodType.Minute, 5);
				AddDataSeries("YM", Data.BarsPeriodType.Minute, 5);
    			AddDataSeries("RTY", Data.BarsPeriodType.Minute, 5);
				bollingerBands = Bollinger(Close, 2, 20);
			}
		}
		
		private int DefaultQuantity = 3;
		
		protected override void OnBarUpdate()
		{
			
			if (ToTime(Time[0]) >= 70000 && ToTime(Time[0]) <= 150000)
			{
				//Handles daily profit/loss limit
				if (tradingEnabled && Account.Get(AccountItem.CashValue, Currency.UsDollar) - initialAccountValue >= profitTarget)
	        	{
		            // Close all positions
					instrumentsToClose.Add(Instrument.GetInstrument("NQ"));
					instrumentsToClose.Add(Instrument.GetInstrument("MNQ"));
		            Account.Flatten(instrumentsToClose);
		            tradingEnabled = false;
		            Print("Profit target reached. Trading disabled.");
					return;
		        }
		        else if (tradingEnabled && Account.Get(AccountItem.CashValue, Currency.UsDollar) - initialAccountValue <= lossLimit)
		        {
		            // Close all positions
					instrumentsToClose.Add(Instrument.GetInstrument("NQ"));
					instrumentsToClose.Add(Instrument.GetInstrument("MNQ"));
		            Account.Flatten(instrumentsToClose);
		            tradingEnabled = false;
		            Print("Loss limit reached. Trading disabled.");
					return;
		        }
				
	            if (BarsInProgress == 0 && CurrentBar > 2)
	            {
					//Variables
					prevCandleLow = Low[2];
				    prevCandleHigh = High[2];
				    currentCandleHigh = High[1];
				    currentCandleLow = Low[1];
				    nextCandleHigh = High[0];
				    nextCandleLow = Low[0];
					currentHigh = High[0];
					currentLow = Low[0];
					previousHigh = High[1];
	        		previousLow = Low[1];
					previousHigh2 = High[2];
	        		previousLow2 = Low[2];
	       		 	twoBarPreviousHigh = High[2];
	        		twoBarPreviousLow = Low[2];
					boll = bollingerBands.Middle[0];
					stopLoss = 30;
					takeProfit = 10;
					contracts = 1;
					
					//Checks if high or low of previous candle is broken
				 	bool highNotBreached = High[0] < previousHigh;
	        		bool lowNotBreached = Low[0] > previousLow;
					
					bool highNotBreached2 = High[1] > previousHigh2;
	        		bool lowNotBreached2 = Low[1] < previousLow2;
					
					//Checks to see if the current candle closed red or green
					bool isGreenCandle = Close[0] > Open[0];
	        		bool isRedCandle = Close[0] < Open[0];
					
					//Checks if we have any open positions
	        		bool isLong = Position.MarketPosition == MarketPosition.Long;
	        		bool isShort = Position.MarketPosition == MarketPosition.Short;
					
					if (highNotBreached && lowNotBreached) //Checks if the current candle is a 1bar
					{
						Print("current candle is 1bar at time: " + Time[1]); //For testing purposes
						double candleRange = currentHigh - currentLow; // Calculate the range of the current candle
						
						if (highNotBreached2 && !lowNotBreached2) //Checks if candle before 1bar is a 2bar
						{
							Print("previous candle is a 2bar at time: " + Time[2]);
							
							if (isGreenCandle && !isLong && !isShort) //Checks to see if candle is bullish and there are no active trades
							{
								Print("test");
								if (currentLow <= boll && currentHigh >= boll) //Checks to see if candle is within Bollinger Band
								{
									bool isShortFVG = (nextCandleHigh < prevCandleLow) && (currentCandleHigh < prevCandleLow);
									Print("boll" + bollingerBands.Middle[0]);
									Print(currentLow);
									Print(currentHigh);
									candleRange = Math.Abs(currentHigh - currentLow);
									breakevenRange = candleRange;
									double stopPrice = currentLow;
									double limitPrice = currentHigh;
									beStop = limitPrice;
									
									Print(currentHigh);
									Print(currentLow);
									Print(currentHigh - currentLow);
									
									//if (candleRange < 20)
									//{
										//Print("25");
										//SetProfitTarget(@"BuyMarketOrder", CalculationMode.Price, candleRange + currentHigh);
										//SetStopLoss(@"BuyMarketOrder", CalculationMode.Price, stopPrice, false);
										//EnterLongStopLimit(3, limitPrice, limitPrice, @"BuyMarketOrder");
									
										//SetProfitTarget(@"BuyMarketOrder2", CalculationMode.Price, 2*candleRange + currentHigh);
		    							//SetStopLoss(@"BuyMarketOrder2", CalculationMode.Price, stopPrice, false);
										//EnterLongStopLimit(3, limitPrice, limitPrice, @"BuyMarketOrder2");
									//}
									
									
									slbe = true;
									SetProfitTarget(@"BuyMarketOrder", CalculationMode.Price, takeProfit + currentHigh);
									SetStopLoss(@"BuyMarketOrder", CalculationMode.Price, limitPrice - stopLoss, false);
									EnterLongStopLimit(contracts, limitPrice, limitPrice, @"BuyMarketOrder");
									
										//SetProfitTarget(@"BuyMarketOrder2", CalculationMode.Price, 50 + currentHigh);
		    							//SetStopLoss(@"BuyMarketOrder2", CalculationMode.Price, limitPrice - 20, false);
										//EnterLongStopLimit(3, limitPrice, limitPrice, @"BuyMarketOrder2");
									
								}
							}
							
							else if (isRedCandle && !isLong && !isShort)//(isRedCandle && !isLong && !isShort) //Checks to see if candle is bearish and there are no active trades
							{
								Print("test");
								if (currentLow <= bollingerBands.Middle[0] && currentHigh >= bollingerBands.Middle[0]) //Checks to see if candle is within Bollinger Band
								{
									Print("boll" + bollingerBands.Middle[0]);
									Print(currentLow);
									Print(currentHigh);
									candleRange = Math.Abs(currentHigh - currentLow);
									breakevenRange = candleRange;
									double stopPrice = currentHigh;
									double limitPrice = currentLow;
									beStop = limitPrice;
									

									slbe = true;
									SetProfitTarget(@"SellMarketOrder", CalculationMode.Price, currentLow - takeProfit);
		    						SetStopLoss(@"SellMarketOrder", CalculationMode.Price, stopLoss + limitPrice, false);
									EnterShortStopLimit(contracts, limitPrice, limitPrice, @"SellMarketOrder");
									

									
								}
							}
							
						}
						
						else if (!highNotBreached2 && lowNotBreached2) //Checks if candle before 1bar is a 2bar
						{
							Print("previous candle is a 2bar at time: " + Time[2]);
							
							if (isGreenCandle && !isLong && !isShort) //Checks to see if candle is bullish and flat
							{
								Print("test");
								boll = bollingerBands.Middle[0];
								if (currentLow <= boll && currentHigh >= boll) //Checks to see if candle is within Bollinger Band
								{
									Print("boll" + bollingerBands.Middle[0]);
									Print(currentLow);
									Print(currentHigh);
									candleRange = Math.Abs(currentHigh - currentLow);
									breakevenRange = candleRange;
									double stopPrice = currentLow;
									double limitPrice = currentHigh;
									beStop = limitPrice;
									
									Print(currentHigh);
									Print(currentLow);
									Print(currentHigh - currentLow);
									

									slbe = true;
									SetProfitTarget(@"BuyMarketOrder", CalculationMode.Price, takeProfit + currentHigh);
									SetStopLoss(@"BuyMarketOrder", CalculationMode.Price, limitPrice - stopLoss, false);
									EnterLongStopLimit(contracts, limitPrice, limitPrice, @"BuyMarketOrder");
									

								}
							}
							
							else if (isRedCandle && !isLong && !isShort) //Checks to see if candle is bearish and flat
							{
								Print("test");
								if (currentLow <= bollingerBands.Middle[0] && currentHigh >= bollingerBands.Middle[0]) //Checks to see if candle is within Bollinger Band
								{
									Print("boll" + bollingerBands.Middle[0]);
									Print(currentLow);
									Print(currentHigh);
									candleRange = Math.Abs(currentHigh - currentLow);
									breakevenRange = candleRange;
									double stopPrice = currentHigh;
									double limitPrice = currentLow;
									beStop = limitPrice;
									

									slbe = true;
									SetProfitTarget(@"SellMarketOrder", CalculationMode.Price, currentLow - takeProfit);
		    						SetStopLoss(@"SellMarketOrder", CalculationMode.Price, stopLoss + limitPrice, false);
									EnterShortStopLimit(contracts, limitPrice, limitPrice, @"SellMarketOrder");
									
								}
							}
						}
					}
					
					else if (!highNotBreached && lowNotBreached) //Checks if the current candle is a 2up
					{
						if (isRedCandle && !isLong && !isShort) //Checks if 2up failed and flat
						{
							Print("test");
							if (currentLow <= bollingerBands.Middle[0] && currentHigh >= bollingerBands.Middle[0]) //Checks to see if candle is within Bollinger Band
							{
								Print("boll" + bollingerBands.Middle[0]);
								Print(currentLow);
								Print(currentHigh);
								Print("failed 2up at" + Time[0]);
								candleRange = Math.Abs(currentHigh - currentLow);
								breakevenRange = candleRange;
								double stopPrice = currentHigh;
								double limitPrice = currentLow;
								beStop = limitPrice;
									

								slbe = true;
								SetProfitTarget(@"SellMarketOrder", CalculationMode.Price, currentLow - takeProfit);
		    					SetStopLoss(@"SellMarketOrder", CalculationMode.Price, stopLoss + limitPrice, false);
								EnterShortStopLimit(contracts, limitPrice, limitPrice, @"SellMarketOrder");
									

							}
						}
					}
					
					else if (highNotBreached && !lowNotBreached) //Checks if current candle is a 2down
					{
						if (isGreenCandle && !isLong && !isShort) //Checks if 2down failed and flat
						{
							Print("test");
							if (currentLow <= bollingerBands.Middle[0] && currentHigh >= bollingerBands.Middle[0]) //Checks to see if candle is within Bollinger Band
							{
								Print("boll" + bollingerBands.Middle[0]);
								Print(currentLow);
								Print(currentHigh);
								candleRange = Math.Abs(currentHigh - currentLow);
								breakevenRange = candleRange;
								double stopPrice = currentLow;
								double limitPrice = currentHigh;
								beStop = limitPrice;
									

								slbe = true;
								SetProfitTarget(@"BuyMarketOrder", CalculationMode.Price, takeProfit + currentHigh);
								SetStopLoss(@"BuyMarketOrder", CalculationMode.Price, limitPrice - stopLoss, false);
								EnterLongStopLimit(contracts, limitPrice, limitPrice, @"BuyMarketOrder");
								

							}
						}
					}
					
					//Print(currentLow);
					//Print(currentHigh);
					//Print(beStop);
					//Print(breakevenRange);
					//Stoploss Breakeven Logic
					if (isLong && slbe)
					{
						if (currentHigh >= 5 + beStop)
						{
							Print("high crossed");
							SetStopLoss(@"BuyMarketOrder", CalculationMode.Price, beStop, true);
							SetStopLoss(@"BuyMarketOrder2", CalculationMode.Price, beStop, true);
						}
					}
					
					else if (isLong)
					{
						if (currentHigh >= breakevenRange + beStop)
						{
							Print("high crossed");
							SetStopLoss(@"BuyMarketOrder", CalculationMode.Price, beStop, true);
							SetStopLoss(@"BuyMarketOrder2", CalculationMode.Price, beStop, true);
						}
					}
							
					if (isShort)
					{
						if (currentLow <= beStop - breakevenRange)
						{
							Print("low crossed");
							SetStopLoss(@"SellMarketOrder", CalculationMode.Price, beStop, true);
							SetStopLoss(@"SellMarketOrder2", CalculationMode.Price, beStop, true);
						}
					}
					
					else if (isShort && slbe)
					{
						if (currentLow <= beStop - 5)
						{
							Print("low crossed");
							SetStopLoss(@"SellMarketOrder", CalculationMode.Price, beStop, true);
							SetStopLoss(@"SellMarketOrder2", CalculationMode.Price, beStop, true);
						}
					} 

	            }

	            if (BarsInProgress == 0 && CurrentBar >= 3)
	            {
	                previousHigh = High[1];
	                previousLow = Low[1];
	                twoBarPreviousHigh = High[2];
	                twoBarPreviousLow = Low[2];
	            }
			}
		}
	}
}
