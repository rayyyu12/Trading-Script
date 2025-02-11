# NinjaTrader Trading Strategies 📈

A collection of automated trading strategies for NinjaTrader 8, implementing market analysis and position management techniques.

## Overview 🔍

This repository contains two main trading strategies:
- **StraightStrat**: A comprehensive trading strategy with dynamic position sizing and risk management
- **OneR**: A modified version with tighter risk controls and Bollinger Band integration

## Features 🚀

### Common Features
- Real-time market analysis
- Automated entry and exit signals
- Built-in risk management
- Session time controls (7:00 AM - 3:00 PM)
- Breakeven stop-loss automation
- Multi-instrument support (NQ, YM, RTY)

### StraightStrat Features
- Multiple entry conditions based on price action
- Dynamic profit targets and stop losses
- Support for up to 8 entries per direction
- Default position size of 3 contracts
- Customizable profit target ($10,000) and loss limit (-$10,000)

### OneR Features
- Bollinger Band integration for trade confirmation
- More conservative position sizing (1 contract)
- Tighter profit target ($1,500) and loss limit (-$1,100)
- Support for up to 4 entries per direction
- Enhanced risk management with smaller stop losses

## Trading Logic 📊

Both strategies implement the following trade setups:
1. One-Bar Pattern Trading
2. Two-Bar Pattern Trading
3. Failed Break Pattern Trading
4. Breakeven Stop Management

### Entry Conditions
- Price action pattern recognition
- Candlestick analysis
- Previous high/low breach detection
- Volume analysis
- Technical indicator confirmation (Bollinger Bands)

### Risk Management
- Automated stop-loss placement
- Dynamic breakeven stops
- Daily profit/loss limits
- Session time restrictions
- Position size management

## Installation 🔧

1. Clone the repository
2. Copy the strategy files to your NinjaTrader custom strategies folder:
```
Documents\NinjaTrader 8\bin\Custom\Strategies\
```
3. Launch NinjaTrader 8
4. Import the strategies through the Strategy Analyzer

## Configuration ⚙️

### Basic Setup
1. Open NinjaTrader 8
2. Navigate to the Strategies tab
3. Select either StraightStrat or OneR
4. Configure the following parameters:
   - Time frame
   - Session times
   - Position size
   - Risk limits

### Risk Parameters
- `profitTarget`: Daily profit target
- `lossLimit`: Daily loss limit
- `stopLoss`: Per-trade stop loss
- `takeProfit`: Per-trade profit target
- `contracts`: Number of contracts per trade

## Usage 📱

1. Apply the strategy to a chart:
   - Right-click on chart
   - Select Strategies
   - Choose StraightStrat or OneR

2. Configure strategy parameters:
   - Set risk parameters
   - Adjust time frames
   - Select instruments

3. Enable strategy automation:
   - Click the "Auto Trade" button
   - Confirm strategy activation

## Requirements 📋

- NinjaTrader 8 platform
- Valid brokerage account
- Market data subscription
- Minimum system requirements:
  - Windows 10 or later
  - 8GB RAM
  - Stable internet connection

## Disclaimer ⚠️

Trading carries significant financial risk. These strategies are for educational and research purposes only. Always test thoroughly in a simulation environment before live trading.
